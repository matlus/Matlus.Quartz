using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;

namespace Matlus.Quartz
{
    public static class ExportedTypesProvider
    {
        private static readonly HashSet<Type> exportedTypes = new HashSet<Type>();
        private static readonly List<Assembly> loadedAssemblies;

        static ExportedTypesProvider()
        {
            loadedAssemblies = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                where
                                  assembly.ManifestModule.Name != "<In Memory Module>"
                                  && !assembly.FullName.StartsWith("mscorlib")
                                  && !assembly.FullName.StartsWith("System")
                                  && !assembly.FullName.StartsWith("Microsoft")
                                  && assembly.Location.IndexOf("App_Web") == -1
                                  && assembly.Location.IndexOf("App_global") == -1
                                  && assembly.FullName.IndexOf("CppCodeProvider") == -1
                                  && assembly.FullName.IndexOf("WebMatrix") == -1
                                  && assembly.FullName.IndexOf("SMDiagnostics") == -1
                                  && !String.IsNullOrEmpty(assembly.Location)
                                select assembly).ToList();

            var binFolderAssemblies = from assem in loadedAssemblies
                                      select assem.CodeBase.Substring(8).Replace('/', '\\').Replace(".DLL", ".dll");

            var additionalAssemblyFilePaths = Directory.EnumerateFiles(AppDomain.CurrentDomain.RelativeSearchPath, "*.dll")
                                              .Except(binFolderAssemblies);

            foreach (var file in additionalAssemblyFilePaths)
                loadedAssemblies.Add(Assembly.LoadFile(file));

            foreach (var assembly in loadedAssemblies)
            {
                Type[] types = assembly.GetExportedTypes();
                foreach (var type in types.AsParallel())
                    exportedTypes.Add(type);
            }
        }

        public static IEnumerable<Assembly> GetLoadedAssemblies()
        {
            return loadedAssemblies;
        }

        public static IEnumerable<Type> GetExportedTypes()
        {
            return exportedTypes;
        }
    }
}
