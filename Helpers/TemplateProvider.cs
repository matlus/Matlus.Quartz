using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Concurrent;
using Matlus.Quartz.Exceptions;
using System.Threading;

namespace Matlus.Quartz
{
    public class TemplateProvider : ITemplateProvider
    {
        public static string RootTemplateFolder { get; set; }
        private static ConcurrentDictionary<string, string> locatedFiles = new ConcurrentDictionary<string, string>();

        private static TemplateProvider instance = null;

        private TemplateProvider()
        {
        }

        /// <summary>
        /// If RootTemplateFolder is not null then try and find the template in that folder and all
        /// subfolders of that folder. If RootTemplateFolder is null, then we search for files in rootFolder
        /// and all its sub folders.
        /// </summary>
        /// <param name="templateFile">This is just the name of a file and does not contain any path information</param>
        /// <returns>Returns a File Stream of the located Template file</returns>
        /// <exception cref="ArgumentException">Thrown when the templateFile parameter is null or empty</exception>
        private static string LocateTemplateFile(string templateFile, string rootFolder)
        {
            if (String.IsNullOrEmpty(templateFile))
                throw new ArgumentException("The parameter templateFile can not be null or empty", templateFile);
            if (Path.IsPathRooted(templateFile))
                return templateFile;

            string templateFilePath = null;
            if (locatedFiles.TryGetValue(templateFile, out templateFilePath))
                return templateFilePath;

            string root = rootFolder;
            if (!String.IsNullOrEmpty(RootTemplateFolder))
                root = RootTemplateFolder;

            var files = Directory.EnumerateFiles(root, templateFile, SearchOption.AllDirectories).ToList();

            if (!files.Any())
                throw new LocatingTemplateException("The Template File: " + templateFile + " could not be found. The Root path: " + root +
                  " and all sub folders were searched. Try setting the RootTemplateFolder property to a folder under which your template files can be found.");

            locatedFiles.TryAdd(templateFile, files[0]);

            return files[0];
        }

        /// <summary>
        /// This method returns the singleton instance of TemplateProvider
        /// </summary>
        /// <returns></returns>
        public static TemplateProvider GetTemplateProvider()
        {
            if (instance != null) return instance;

            TemplateProvider temp = new TemplateProvider();
            Interlocked.CompareExchange(ref instance, temp, null);
            return instance;
        }

        #region ITemplateProvider Members

        public Stream GetTemplateStream(string templateFile, string rootFolder)
        {
            return new FileStream(LocateTemplateFile(templateFile, rootFolder), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }

        #endregion
    }
}
