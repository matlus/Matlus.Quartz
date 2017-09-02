using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Matlus.Quartz.Exceptions;

namespace Matlus.Quartz
{
    public sealed class BuilderRouteHandler : IRouteHandler
    {
        private static readonly Type baseBuilderType = typeof(BaseBuilder);
        private static readonly Type builderRouteAttributeType = typeof(BuilderRouteAttribute);
        private static readonly string builderSuffix = "Builder";
        private static readonly string homeBuilderName = "HomeBuilder";

        private static readonly HashSet<string> patternHashSet = new HashSet<string>();
        private static readonly List<BuilderRoutePriority> builderRegistry = new List<BuilderRoutePriority>();

        public Type RouteNotFoundBuilder { get; private set; }

        static BuilderRouteHandler()
        {
            var exportedBuilders = from b in ExportedTypesProvider.GetExportedTypes()
                                   where b.Name.EndsWith(builderSuffix) && b.IsSubclassOf(baseBuilderType)
                                   select b;

            foreach (var type in exportedBuilders)
                RegisterBuilder(type);

            builderRegistry = builderRegistry.OrderByDescending(bpp => bpp.Priority).ThenByDescending(bpp => bpp.Pattern).ToList();
        }

        public BuilderRouteHandler()
        {
        }

        /// <summary>
        /// Use this constructor if you want to define a Route Not Found Builder Type that will be instantiated
        /// when your site receives a Request for a Route for which no Builder has been Registered.    /// 
        /// </summary>
        /// <param name="routeNotFoundBuilder">The Type of the Builder class that will be instantiated when no Builder that maches the requested route was found</param>
        public BuilderRouteHandler(Type routeNotFoundBuilder)
          : this()
        {
            if (!routeNotFoundBuilder.IsSubclassOf(typeof(BaseBuilder)))
                throw new BaseQuartzWebBuilderException("The Type of the routeNotFoundBuilder Must be a Subclass of BaseBuilder");
            RouteNotFoundBuilder = routeNotFoundBuilder;
        }

        public static IEnumerable<BuilderRoutePriority> GetRegisteredRoutes()
        {
            foreach (var pattern in builderRegistry)
                yield return pattern;
        }

        public static void RegisterBuilder(Type type)
        {
            if (!type.Name.EndsWith(builderSuffix) && !type.IsSubclassOf(baseBuilderType)) return;

            int idx = type.Name.IndexOf(builderSuffix, StringComparison.OrdinalIgnoreCase);
            var builderClassNamePrefix = type.Name.Substring(0, idx).ToLower();

            object[] builderPatternAttributes = type.GetCustomAttributes(builderRouteAttributeType, false);

            if (builderPatternAttributes.Length > 0)
            {
                foreach (var attribute in builderPatternAttributes)
                {
                    var routeAttribute = (BuilderRouteAttribute)attribute;
                    string pattern = null;
                    if (routeAttribute.Pattern.StartsWith("/"))
                        pattern = "^~/" + builderClassNamePrefix + routeAttribute.Pattern;
                    else
                        pattern = routeAttribute.Pattern;

                    VerifyPatternIsUnique(type.Name, pattern);
                    builderRegistry.Add(new BuilderRoutePriority(routeAttribute.Id, type, pattern, routeAttribute.Priority));
                }
            }
            else /* if no BuilderRouteAttribute has been defined */
            {
                /* Special case for "Home" */
                if (String.Compare(type.Name, homeBuilderName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    var pattern = "^~/$";
                    VerifyPatternIsUnique(type.Name, pattern);
                    builderRegistry.Add(new BuilderRoutePriority(0, type, pattern, 10));
                }
                else
                {
                    var pattern = "^~/" + builderClassNamePrefix + "/?$";
                    VerifyPatternIsUnique(type.Name, pattern);
                    builderRegistry.Add(new BuilderRoutePriority(0, type, pattern, 9));
                }
            }
        }

        private static void VerifyPatternIsUnique(string typeName, string pattern)
        {
            if (patternHashSet.Contains(pattern))
                throw new DuplicateBuilderUrlPatternException("The Builder: " + typeName + " has registered a BuilderUrlPatternAttribute: " + pattern + " that has already been registered");
            patternHashSet.Add(pattern);
        }

        #region IRouteHandler Members

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var appPathLower = requestContext.HttpContext.Request.AppRelativeCurrentExecutionFilePath.ToLower();
            for (int i = 0; i < builderRegistry.Count; i++)
            {
                var brp = builderRegistry[i];
                if (brp.Regex.IsMatch(appPathLower))
                {
                    var pathData = new PathDataDictionary();
                    var match = brp.Regex.Match(appPathLower);
                    if (match.Groups.Count > 1)
                    {
                        for (int j = 1; j < match.Groups.Count; j++)
                            pathData.Add(brp.Regex.GroupNameFromNumber(j), match.Groups[j].Value);
                    }

                    var builderInstance = BuilderFactory.Create(brp.Builder, requestContext.HttpContext, pathData);
                    builderInstance.RouteId = brp.RouteId;
                    return builderInstance;
                }
            }

            if (RouteNotFoundBuilder != null)
                return BuilderFactory.Create(RouteNotFoundBuilder, requestContext.HttpContext, new PathDataDictionary());
            return null;
        }

        #endregion
    }
}
