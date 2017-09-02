using System.IO;

namespace Matlus.Quartz
{
  public interface ITemplateProvider
  {
    Stream GetTemplateStream(string templateFile, string rootFolder);
  }
}
