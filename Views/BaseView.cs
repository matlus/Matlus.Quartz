using System.IO;
using System.Web;
using System.Web.Mvc;

namespace Matlus.Quartz
{
  public abstract class BaseView : BaseControl, IView
  {
    public object Model { get; set; }

    protected BaseView(HttpContextBase httpContext)
      :base(httpContext)
    {
    }

    protected BaseView(HttpContextBase httpContext, object model)
      :this(httpContext)
    {
      Model = model;
    }

    /// <summary>
    /// All Views must override and implement this method in order to Render their View.
    /// This method is called by the Builder during the BuildPage phase
    /// </summary>
    /// <param name="viewContext">The ViewContext extracted from the HttpContextBase instance that was handed to the Builder</param>
    /// <param name="writer">The TextWriter that writes directly to the Response.OutputStream</param>
    public abstract void Render(ViewContext viewContext, TextWriter writer);
  }
}
