using System.IO;
using System.Web;
using System.Web.Mvc;

namespace Matlus.Quartz
{
  /// <summary>
  /// This is a specialized Matlus.Quartz.View that can Render a View using a TemplateFile.
  /// It simply replaces the PlaceholderTag with content from the TemplateFile withhout any modification
  /// </summary>
  public class TemplateFileView : BaseView
  {
    public string TemplateFile { get; private set; }

    /// <summary> 
    /// Use this class when you want to replace the PlaceholderTag with Html
    /// loaded from an Html template file and render it without any modification
    /// </summary>
    /// <param name="templateFile">The Html Template File that this view will render without any modification</param>
    public TemplateFileView(HttpContextBase httpContext, string templateFile)
      :base(httpContext)
    {
      TemplateFile = templateFile;
    }
    
    public override void Render(ViewContext viewContext, TextWriter writer)
    {
      WriteTemplate(TemplateFile, writer);
    }
  }
}
