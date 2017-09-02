using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection.Emit;
using System.Web;
using Matlus.Quartz.Exceptions;

namespace Matlus.Quartz
{
    public static class ViewFactory
    {
        private static readonly Type baseViewType = typeof(BaseView);
        private static readonly Type factoryRegistrationAttributeType = typeof(FactoryIdentifierAttribute);
        private static readonly string viewSuffix = "View";

        private static readonly Type[] constructorMethodArgs1 = new Type[] { typeof(HttpContextBase) };
        private static readonly ConcurrentDictionary<string, Type> viewRegistry = new ConcurrentDictionary<string, Type>();
        private static readonly ConcurrentDictionary<string, ViewConstructorDelegate> viewConstructors = new ConcurrentDictionary<string, ViewConstructorDelegate>();
        private static object viewConstructorsRef;

        static ViewFactory()
        {
            var exportedViews = from b in ExportedTypesProvider.GetExportedTypes()
                                where b.Name.EndsWith(viewSuffix) && b.IsSubclassOf(baseViewType)
                                select b;


            foreach (var type in exportedViews)
                RegisterView(type);
        }

        public static void RegisterView(Type type)
        {
            if (type.Name.EndsWith(viewSuffix) && type.IsSubclassOf(baseViewType))
            {
                object[] factoryRegistrationAttributes = type.GetCustomAttributes(factoryRegistrationAttributeType, false);
                if (factoryRegistrationAttributes.Length > 0)
                {
                    var identifier = ((FactoryIdentifierAttribute)factoryRegistrationAttributes[0]).Identifier;
                    if (String.IsNullOrEmpty(identifier))
                        identifier = type.Name;
                    viewRegistry.TryAdd(identifier, type);
                }
            }
        }

        delegate BaseView ViewConstructorDelegate(HttpContextBase httpContext);
        delegate BaseView ViewConstructor2Delegate<in T>(HttpContextBase httpContext, T model);

        /// <summary>
        /// This method creates an instance of a View that expects strongly typed model
        /// as the second constructor parameter
        /// </summary>
        /// <typeparam name="T">The Type of the Model</typeparam>
        /// <param name="identifier">The identifier or the class name of the view</param>
        /// <param name="httpContext"></param>
        /// <param name="model">The model the view expects as its constructor parameter</param>
        /// <returns></returns>
        public static BaseView Create<T>(string identifier, HttpContextBase httpContext, T model)
        {
            if (model == null)
                return Create(identifier, httpContext);

            if (String.IsNullOrEmpty(identifier))
                throw new ArgumentException("identifier can not be null or empty", identifier);
            if (!viewRegistry.ContainsKey(identifier))
                throw new ArgumentException("No View has been registered with the identifier: " + identifier);

            ConcurrentDictionary<string, ViewConstructor2Delegate<T>> viewConstructorsStronglyTyped;
            if (viewConstructorsRef == null)
            {
                viewConstructorsStronglyTyped = new ConcurrentDictionary<string, ViewConstructor2Delegate<T>>();
                viewConstructorsRef = viewConstructorsStronglyTyped;
            }
            else
                viewConstructorsStronglyTyped = (ConcurrentDictionary<string, ViewConstructor2Delegate<T>>)viewConstructorsRef;

            ViewConstructor2Delegate<T> del;
            Type[] stronglyTypedConstructorMethodArgs = new Type[] { typeof(HttpContextBase), typeof(T) };

            if (viewConstructorsStronglyTyped.TryGetValue(identifier, out del))
                return del(httpContext, model);

            Type viewType = viewRegistry[identifier];
            var constructorInfo = viewType.GetConstructor(stronglyTypedConstructorMethodArgs);
            if (constructorInfo == null)
                throw new ViewFactoryException("No Constructor with matching parameters found");

            DynamicMethod dynamicMethod = new DynamicMethod("CreateBaseViewInstance", viewType, stronglyTypedConstructorMethodArgs, viewType);
            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Newobj, constructorInfo);
            ilGenerator.Emit(OpCodes.Ret);

            del = (ViewConstructor2Delegate<T>)dynamicMethod.CreateDelegate(typeof(ViewConstructor2Delegate<T>));
            viewConstructorsStronglyTyped.TryAdd(identifier, del);
            return del(httpContext, model);
        }

        public static BaseView Create(string identifier, HttpContextBase httpContext)
        {
            if (String.IsNullOrEmpty(identifier))
                throw new ArgumentException("identifier can not be null or empty", identifier);
            if (!viewRegistry.ContainsKey(identifier))
                throw new ArgumentException("No View has been registered with the identifier: " + identifier);


            ViewConstructorDelegate del;
            if (viewConstructors.TryGetValue(identifier, out del))
                return del(httpContext);

            Type viewType = viewRegistry[identifier];
            var constructorInfo = viewType.GetConstructor(constructorMethodArgs1);
            if (constructorInfo == null)
                throw new ViewFactoryException("No Constructor with matching parameters found");


            DynamicMethod dynamicMethod = new DynamicMethod("CreateBaseViewInstance", viewType, constructorMethodArgs1, viewType);
            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Newobj, constructorInfo);
            ilGenerator.Emit(OpCodes.Ret);

            del = (ViewConstructorDelegate)dynamicMethod.CreateDelegate(typeof(ViewConstructorDelegate));
            viewConstructors.TryAdd(identifier, del);
            return del(httpContext);
        }
    }
}
