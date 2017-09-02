using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Collections.Specialized;
using System.Web.Routing;

namespace Matlus.Quartz
{
    /// <summary>
    /// This class is the Base Class for All Views and Builders
    /// </summary>
    public class BaseControl
    {
        const int BufferSize = 8192;

        #region Properties

        protected static readonly string AppDomainAppVirtualPath;
        protected static readonly string ServerRootFolder = HttpRuntime.AppDomainAppPath;

        protected HttpContextBase HttpContext { get; private set; }
        protected HttpRequestBase Request { get { return HttpContext.Request; } }
        protected NameValueCollection Form { get { return Request.Form; } }
        protected NameValueCollection QueryString { get { return Request.QueryString; } }
        protected string WebsiteUrl { get; private set; }
        private ITemplateProvider templateprovider;
        protected ITemplateProvider TemplateProvider { get { return templateprovider ?? (templateprovider = MakeTemplateProvider()); } }

        #endregion Properties

        static BaseControl()
        {
            AppDomainAppVirtualPath = HttpRuntime.AppDomainAppVirtualPath;
            if (AppDomainAppVirtualPath[AppDomainAppVirtualPath.Length - 1] != '/')
                AppDomainAppVirtualPath += "/";
        }

        public BaseControl(HttpContextBase httpContext)
        {
            HttpContext = httpContext;
            WebsiteUrl = GetWebsiteRootUrl(httpContext);
        }

        private string GetWebsiteRootUrl(HttpContextBase httpContext)
        {
            var url = httpContext.Request.Url.ToString();
            if (String.Compare(AppDomainAppVirtualPath, "/", StringComparison.OrdinalIgnoreCase) == 0)
                return url.Substring(0, url.IndexOf("/", 8) + 1);
            else
                return url.Substring(0, url.IndexOf(AppDomainAppVirtualPath, StringComparison.OrdinalIgnoreCase) + AppDomainAppVirtualPath.Length);
        }

        #region Factory Methods for aggregated classes

        protected ITemplateProvider MakeTemplateProvider()
        {
            return Matlus.Quartz.TemplateProvider.GetTemplateProvider();
        }

        #endregion Factory Methods for aggregated classes

        #region Protected Methods

        protected NameValueCollection GetRequestNameValueCollection()
        {
            var combinedNameValueCollection = new NameValueCollection();
            foreach (string key in HttpContext.Request.Form.Keys)
                combinedNameValueCollection.Add(key, HttpContext.Request.Form[key]);

            foreach (string key in HttpContext.Request.QueryString.Keys)
                combinedNameValueCollection.Add(key, HttpContext.Request.QueryString[key]);

            return combinedNameValueCollection;
        }

        protected T Bind<T>() where T : class, new()
        {
            Type type = typeof(T);
            var nameValues = GetRequestNameValueCollection();
            if (RequestBinder.Contains(type))
                return (T)RequestBinder.GetBinder(type).Bind(nameValues, type);
            else if (!type.IsValueType)
                return DtoBinder.CreateInstance<T>(nameValues);
            else
                return default(T);
        }

        protected T Bind<T>(string parameterName, T defaultValue = default(T))
        {
            Type type = typeof(T);
            var nameValues = GetRequestNameValueCollection();
            if (RequestBinder.Contains(type))
                return (T)RequestBinder.GetBinder(type).Bind(nameValues, type, parameterName, defaultValue);
            else if (type.IsEnum)
                return (T)RequestBinder.GetBinder(typeof(Enum)).Bind(nameValues, type, parameterName, defaultValue);
            else
                return defaultValue;
        }

        protected T Bind<T>(int enumValue) where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("The Generic Type parameter must be an Enum type");
            T result = (T)Enum.ToObject(typeof(T), enumValue);
            int eValue;
            return !int.TryParse(result.ToString(), out eValue) ? result : default(T);
        }


        /// <summary>
        /// This method renders the contents of a file directly to the Output stream via the writer
        /// that has been provided as a parameter
        /// </summary>
        /// <param name="templateFile">The name of the File to write directly to the output</param>
        /// <param name="viewContext">The ViewContext of the current View</param>
        /// <param name="writer">The TextWriter of the current View</param>
        protected void WriteTemplate(string templateFile, TextWriter writer)
        {
            using (Stream stream = TemplateProvider.GetTemplateStream(templateFile, ServerRootFolder))
            {
                var reader = new StreamReader(stream);
                var buffer = new Char[BufferSize];
                int bytesRead;
                while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) != 0)
                {
                    if (bytesRead == BufferSize)
                        writer.Write(buffer);
                    else
                    {
                        for (int i = 0; i < bytesRead; i++)
                            writer.Write(buffer[i]);
                    }
                }
            }
        }

        /// <summary>
        /// This method locates the specified template file, loads all of its content and returns it
        /// </summary>
        /// <param name="templateFile">This is just the name of a file and does not contain any path information</param>
        /// <returns>Returns the entire content of the file</returns>
        protected string GetTemplateFileContent(string templateFile)
        {
            using (Stream stream = TemplateProvider.GetTemplateStream(templateFile, ServerRootFolder))
            {
                var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// The method binds tags found in the template to property values of the dtoObject parameter.
        /// <para>In order to bind, tag names must match the property names of the dtoObject (case-insensitive)</para>
        /// <para>When a tag does not match any property in the dtoObject the missingPropertiesCallback Action</para>
        /// <para>delegate is called passing in the tag as a parameter so it can be handled in the callback</para>
        /// <para>This method handles resolving rooted path tags such as "@~/forums/@"</para>
        /// </summary>
        /// <param name="templateFile">The name of the Template File to Parse</param>
        /// <param name="writer">The TextWriter of the current View</param>
        /// <param name="dtoObject">An instance of a Dto Object/ViewModel object</param>
        /// <param name="missingPropertiesCallback">A callback Action delegate that will handle any tags that did not match property names of the dtoObject</param>
        protected void BindModelToTagsFromTemplate(string templateFile, TextWriter writer, object dtoObject, Action<string> missingPropertiesCallback = null)
        {
            ParseTemplate(templateFile, writer, tag =>
              {
                  object propertyValueObj = DtoBinder.GetPropertyValue(dtoObject, tag);
                  if (propertyValueObj != null)
                      writer.Write(propertyValueObj);
                  else if (missingPropertiesCallback != null)
                      missingPropertiesCallback(tag);
              });
        }

        /// <summary>
        /// This method parses the given text and returns an <see cref="IEnumerable&lt;String&gt;"/>
        /// <para>Use this method in the Render method of a View</para>
        /// <para>This method handles resolving rooted path tags such as "@~/forums/@"</para>  
        /// </summary>
        /// <param name="textToParse">Text containing PlaceHolderTags to parse</param>
        /// <param name="writer">The TextWriter of the current View</param>
        /// <returns>Returns an <see cref="IEnumerable&lt;String&gt;"/></returns>
        protected IEnumerable<string> GetTagsFromText(string textToParse, TextWriter writer)
        {
            int pos = 0;
            while (pos < textToParse.Length)
            {
                if (textToParse[pos] == '@')
                {
                    var chars = new List<Char>();
                    pos++;
                    while (pos < textToParse.Length)
                    {
                        if (textToParse[pos] == '@')
                        {
                            if (chars.Count == 0)
                            {
                                writer.Write(textToParse[pos]);
                                break;
                            }
                            var tagname = new String(chars.ToArray());
                            if (tagname.StartsWith("~/"))
                                writer.Write(VirtualPathUtility.ToAbsolute(tagname));
                            else
                                yield return tagname;
                            break;
                        }
                        else
                            chars.Add(textToParse[pos]);
                        pos++;
                    }
                }
                else
                    writer.Write(textToParse[pos]);
                pos++;
            }
        }

        /// <summary>
        /// This method parses the given template file and returns an <see cref="IEnumerable&lt;String&gt;"/>
        /// <para>Use this method in the Render method of a View in order to parse templates and snippets</para> 
        /// <para>This method handles resolving rooted path tags such as "@~/forums/@"</para>
        /// </summary> 
        /// <param name="templateFile">The name of the Template File to Parse</param>
        /// <param name="writer">The TextWriter of the current View</param>
        /// <returns>Returns an <see cref="IEnumerable&lt;String&gt;"/></returns>
        protected IEnumerable<string> GetTagsFromTemplate(string templateFile, TextWriter writer)
        {
            using (Stream stream = TemplateProvider.GetTemplateStream(templateFile, ServerRootFolder))
            {
                var reader = new StreamReader(stream);
                char[] buffer = new Char[1];
                while ((reader.Read(buffer, 0, buffer.Length)) != 0)
                {
                    if (buffer[0] == '@')
                    {
                        var chars = new List<Char>();
                        while ((reader.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            if (buffer[0] == '@')
                            {
                                if (chars.Count == 0)
                                {
                                    writer.Write(buffer[0]);
                                    break;
                                }
                                var tagname = new String(chars.ToArray());
                                if (tagname.StartsWith("~/"))
                                    writer.Write(VirtualPathUtility.ToAbsolute(tagname));
                                else
                                    yield return tagname;
                                break;
                            }
                            else
                                chars.Add(buffer[0]);
                        }
                    }
                    else
                        writer.Write(buffer[0]);
                }
            }
        }

        #endregion Protected Methods

        #region Public Methods

        /// <summary>
        /// This method parses the given template file and calls and Action method each time a tag is found.
        /// <para>In the Action method, use the TextWriter to write the replacement text directly to the output stream.</para>
        /// <para>This method handles resolving rooted path tags such as "@~/forums/@"</para>
        /// </summary>
        /// <param name="templateFile">The name of the Template File to Parse</param>
        /// <param name="writer">The TextWriter of the current View</param>
        /// <param name="placeholderTagHandler">The Action callback method to call for each placeholderTag found</param>
        public void ParseTemplate(string templateFile, TextWriter writer, Action<string> placeholderTagHandler)
        {
            using (Stream stream = TemplateProvider.GetTemplateStream(templateFile, ServerRootFolder))
            {
                var reader = new StreamReader(stream);
                char[] buffer = new Char[1];
                while ((reader.Read(buffer, 0, buffer.Length)) != 0)
                {
                    Start:
                    if (buffer[0] == '@')
                    {
                        var chars = new List<Char>();
                        while ((reader.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            if (buffer[0] == '@')
                            {
                                if (chars.Count == 0)
                                {
                                    writer.Write(buffer[0]);
                                    break;
                                }
                                var tagname = new String(chars.ToArray());
                                if (tagname.StartsWith("~/"))
                                    writer.Write(VirtualPathUtility.ToAbsolute(tagname));
                                else
                                    placeholderTagHandler(tagname);
                                break;
                            }
                            else if (buffer[0] == ' ' || buffer[0] == '\r' || buffer[0] == '\n')
                            {
                                if (chars.Count > 1)
                                    chars.Insert(0, '@');
                                writer.Write(chars.ToArray());
                                chars.Clear();
#pragma warning disable S907 // "goto" statement should not be used
                                goto Start;
#pragma warning restore S907 // "goto" statement should not be used
                            }
                            else
                                chars.Add(buffer[0]);
                        }
                    }
                    else
                        writer.Write(buffer[0]);
                }
            }
        }

        /// <summary>
        /// This method parses the given text and calls the placeholderTagHandler Action
        /// method each time it finds a placeHolderTag
        /// <para>This method handles resolving rooted path tags such as "@~/forums/@"</para>
        /// </summary>
        /// <param name="textToParse"></param>
        /// <param name="writer"></param>
        /// <param name="placeholderTagHandler"></param>
        public void ParseText(string textToParse, TextWriter writer, Action<string> placeholderTagHandler)
        {
            int pos = 0;
            while (pos < textToParse.Length)
            {
                if (textToParse[pos] == '\r' || textToParse[pos] == '\n')
                {
                    pos++;
                    continue;
                }
                if (textToParse[pos] == '@')
                {
                    var chars = new List<Char>();
                    pos++;
                    while (pos < textToParse.Length)
                    {
                        if (textToParse[pos] == '@')
                        {
                            if (chars.Count == 0)
                            {
                                writer.Write(textToParse[pos]);
                                break;
                            }
                            var tagname = new String(chars.ToArray());
                            if (tagname.StartsWith("~/"))
                                writer.Write(VirtualPathUtility.ToAbsolute(tagname));
                            else
                                placeholderTagHandler(tagname);
                            break;
                        }
                        else
                            chars.Add(textToParse[pos]);
                        pos++;
                    }
                }
                else
                    writer.Write(textToParse[pos]);
                pos++;
            }
        }

        #endregion Public Methods
    }
}
