using System.Web;
using System.Web.Mvc;

namespace Matlus.Quartz
{
  public abstract class BaseView<TModel> : BaseView
  {
    public new TModel Model { get; set; }

    protected BaseView(HttpContextBase httpContext, TModel model)
      :base(httpContext, model)
    {
      Model = model;
    }
  }  
}