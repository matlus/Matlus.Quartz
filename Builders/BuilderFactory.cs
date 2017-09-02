using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection.Emit;
using System.Web;
using System.Web.Routing;

namespace Matlus.Quartz
{
  public static class BuilderFactory
  {
    private static readonly Type baseBuilderType = typeof(BaseBuilder);
    private static readonly Type[] constructorMethodArgs = new Type[] { typeof(HttpContextBase), typeof(PathDataDictionary) };
    private static readonly ConcurrentDictionary<string, Type> builderRegistry = new ConcurrentDictionary<string, Type>();
    private static readonly ConcurrentDictionary<string, BuilderConstructorDelegate> builderConstructors = new ConcurrentDictionary<string, BuilderConstructorDelegate>();

    delegate BaseBuilder BuilderConstructorDelegate(HttpContextBase httpContext, PathDataDictionary pathData);

    static BuilderFactory()
    {
      var sw = System.Diagnostics.Stopwatch.StartNew();
      
      var exportedBuilders = from b in ExportedTypesProvider.GetExportedTypes()
                             where b.IsSubclassOf(baseBuilderType)
                             select b;

      foreach(var type in exportedBuilders)
      {
        object[] factoryRegistrationAttributes = type.GetCustomAttributes(typeof(FactoryIdentifierAttribute), false);
        if (factoryRegistrationAttributes.Length > 0)
        {
          var identifier = ((FactoryIdentifierAttribute)factoryRegistrationAttributes[0]).Identifier;
          if (String.IsNullOrEmpty(identifier))
            identifier = type.Name;
          builderRegistry.TryAdd(identifier, type);
        }
      }

      System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds);
    }

    public static bool Contains(string identifier)
    {
      return builderRegistry.ContainsKey(identifier);
    }

    public static BaseBuilder Create(string identifier, HttpContextBase httpContext, PathDataDictionary pathData)
    {
      if (String.IsNullOrEmpty(identifier))
        throw new ArgumentException("identifier can not be null or empty", identifier);
      if (!builderRegistry.ContainsKey(identifier))
        throw new ArgumentException("No Builder has been registered with the identifier: " + identifier);

      return Create(builderRegistry[identifier], httpContext, pathData);
    }

    public static BaseBuilder Create(Type builderType, HttpContextBase httpContext, PathDataDictionary pathData)
    {
      if (!builderType.IsSubclassOf(baseBuilderType)) return null;
      
      BuilderConstructorDelegate del;
      if (builderConstructors.TryGetValue(builderType.FullName, out del))
        return del(httpContext, pathData);
      
      DynamicMethod dynamicMethod = new DynamicMethod("CreateBaseBuilderInstance", builderType, constructorMethodArgs, builderType);
      ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
      ilGenerator.Emit(OpCodes.Ldarg_0);
      ilGenerator.Emit(OpCodes.Ldarg_1);
      ilGenerator.Emit(OpCodes.Newobj, builderType.GetConstructor(constructorMethodArgs));
      ilGenerator.Emit(OpCodes.Ret);

      del = (BuilderConstructorDelegate)dynamicMethod.CreateDelegate(typeof(BuilderConstructorDelegate));
      builderConstructors.TryAdd(builderType.FullName, del);
      return del(httpContext, pathData);
    }
  } 

}
