using System.IO;
using System.Web.Mvc;

namespace Matlus.Quartz
{
    public class BuilderActionResult : ActionResult
    {
        private readonly IView view;
        public BuilderActionResult(IView view)
        {
            this.view = view;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            TextWriter writer = context.RequestContext.HttpContext.Response.Output;
            ViewContext viewContext = new ViewContext(context, view, new ViewDataDictionary(), new TempDataDictionary(), writer);
            view.Render(viewContext, writer);
        }
    }
}
