using System.Web;

namespace Matlus.Quartz
{
  /// <summary>
  /// This class specializes in rendering the Content properrty.
  /// Use this class when all you need is to replace the PlaceholderTag this view
  /// is associated with, with the value of the Content property
  /// </summary>
  public class ContentView : BaseView
  {
    public string Content { get; private set; }
    /// <summary>
    /// Use this class when you want to replace the PlaceholderTag with an arbitary string
    /// that you specify as the Content parameter/property
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="content">The content you want rendered</param>
    public ContentView(HttpContextBase httpContext, string content)
      :base(httpContext)
    {
      Content = content;
    }

    public override void Render(System.Web.Mvc.ViewContext viewContext, System.IO.TextWriter writer)
    {
      writer.Write(Content);
    }
  }
}
