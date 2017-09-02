using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Matlus.Quartz.Exceptions;

namespace Matlus.Quartz
{
  public abstract class BaseBuilder : BaseControl, IHttpHandler, IView
  {
    #region Properties

    public int RouteId { get; set; }

    protected PathDataDictionary PathData { get; private set; }    
    /// <summary>
    /// The html file that is the "Master Template".
    /// Rendering of Views is triggered by the processing of this template
    /// </summary>
    protected string MasterTemplateFile { get; set; }

    /// <summary>
    /// If a MasterTeplateFile has not been defined,
    /// you can use simple Text as your MasterTemplate
    /// instead.
    /// </summary>
    public string MasterTemplateText { get; set; }

    private ViewRegistry viewRegistry;
    protected ViewRegistry ViewRegistry
    {
      get
      {
        if (viewRegistry == null)
          viewRegistry = MakeViewRegistry();
        return viewRegistry;
      }
    }

    #endregion Properties

    protected BaseBuilder(HttpContextBase httpContext, PathDataDictionary pathData)
      :base(httpContext)
    {
      PathData = pathData;
    }

    #region Factory Methods for composite classes

    protected ViewRegistry MakeViewRegistry()
    {
      return new ViewRegistry();
    }

    #endregion Factory Methods for composite classes

    /// <summary>
    /// <para>Used Internally.</para>
    /// <para>This method is called from the Render method of Builder classes that in turn have addtional views registered.</para>
    /// <para>This method calls the Render method of registered views when a matching PlaceholderTag is found for a view that has been registered for it.</para>
    /// <para>This method handles resolving rooted path tags such as "@~/forums/@"</para>
    /// </summary>
    /// <param name="templateFile">The name of the Template File to Parse</param>
    /// <param name="viewContext">The ViewContext extracted from the HttpContextBase instance that was handed to the Builder</param>
    /// <param name="writer">The TextWriter that wraps the Response.OutputStream</param>
    private void ParseAndRenderTemplate(string templateFile, ViewContext viewContext, TextWriter writer)
    {
      ParseTemplate(templateFile, writer, tag =>
        {
          if (ViewRegistry.Contains(tag))
          {
            var view = ViewRegistry[tag];
            var baseView = view as BaseView;
            if (baseView != null)
              view.Render(viewContext, writer);
            else
            {
              var baseBuilder = view as BaseBuilder;
              if (baseBuilder != null)
                baseBuilder.BuildPage();
              else
                view.Render(viewContext, writer);
            }
          }
          else if (BuilderFactory.Contains(tag))
            BuilderFactory.Create(tag, HttpContext, PathData).BuildPage();
          else
          {
            /* Swallow the tag */
          }
        });
    }

    /// <summary>
    /// <para>Used Internally.</para>
    /// <para>This method is called from the Render method of Builder classes that in turn have addtional views registered.</para>
    /// <para>This method calls the Render method of registered views when a matching PlaceholderTag is found for a view that has been registered for it.</para>
    /// <para>This method handles resolving rooted path tags such as "@~/forums/@"</para>
    /// </summary>
    /// <param name="templateText">The Template in the form of Text</param>
    /// <param name="viewContext">The ViewContext extracted from the HttpContextBase instance that was handed to the Builder</param>
    /// <param name="writer">The TextWriter that wraps the Response.OutputStream</param>
    private void ParseAndRenderTemplateText(string templateText, ViewContext viewContext, TextWriter writer)
    {
      ParseText(templateText, writer, tag =>
      {
        if (ViewRegistry.Contains(tag))
        {
          var view = ViewRegistry[tag];
          var baseView = view as BaseView;
          if (baseView != null)
            view.Render(viewContext, writer);
          else
          {
            var baseBuilder = view as BaseBuilder;
            if (baseBuilder != null)
              baseBuilder.BuildPage();
            else
              view.Render(viewContext, writer);
          }
        }
        else if (BuilderFactory.Contains(tag))
          BuilderFactory.Create(tag, HttpContext, PathData).BuildPage();
        else
        {
          /* Swallow the tag */
        }
      });
    }
    protected T GetPathDataValue<T>(string key, T defaultValue = default(T))
    {
      if (PathData.ContainsKey(key))
      {
        Type type = typeof(T);
        var value = PathData[key];
        if (RequestBinder.Contains(type))
          return (T)RequestBinder.GetBinder(type).Bind(value, type, defaultValue);
        else if (type.IsEnum)
          return (T)RequestBinder.GetBinder(typeof(Enum)).Bind(value, type, defaultValue);
      }
      return defaultValue;
    }

    protected void RegisterView(string placeholderTagname, IView view)
    {
      ViewRegistry.RegisterView(placeholderTagname, view);
    }

    /// <summary>
    /// This method returns a view identified by the placeholderTagname
    /// </summary>
    /// <param name="placeholderTagname">The PlaceholderTagname that was used to Register the view</param>
    /// <returns>If found, returns the instance otherwise returns null</returns>
    protected IView GetRegisteredView(string placeholderTagname)
    {
      if (ViewRegistry.Contains(placeholderTagname))
        return ViewRegistry[placeholderTagname];
      else
        return null;
    }

    /// <summary>
    /// This method returns the first matching view of a specific type
    /// that has been registered in the ViewRegistry
    /// </summary>
    /// <typeparam name="T">The Type of the view you want returned</typeparam>
    /// <returns>If a View of the Type specified by the Type parameter was found it returns that instance otherwise it returns default(T)</returns>
    protected T GetRegisteredView<T>()
    {
      foreach (var view in ViewRegistry)
        if (view.Value is T)
          return (T)view.Value;
      return default(T);
    }

    public ActionResult GetBuiltPage()
    {
      return new BuilderActionResult(ComposePage());
    }

    public void BuildPage()
    {
      ComposePage();
      var writer = HttpContext.Response.Output;
      var viewContext = new ViewContext();
      viewContext.Writer = writer;
      viewContext.RequestContext = HttpContext.Request.RequestContext;
      Render(viewContext, writer);      
    }

    private IView ComposePage()
    {
      InitializeProperties();
      var model = GetModelForViews();
      HttpContext.Response.ContentType = "text/html; charset=utf-8";
      BuildPage(HttpContext, model);      
      HttpContext.Response.Cache.SetCacheability(HttpCacheability.ServerAndPrivate);
      return this;
    }

    protected abstract void InitializeProperties();    
    protected abstract object GetModelForViews();
    protected abstract void BuildPage(HttpContextBase httpContext, object model);

    #region IHttpHandler Members

    public bool IsReusable
    {
      get { return true; }
    }

    public void ProcessRequest(HttpContext context)
    {
      BuildPage();
    }

    #endregion

    #region IHttpAsyncHandler Members

    public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
    {
      Action del = BuildPage;
      return del.BeginInvoke(cb, new AsyncState(del, extraData));
    }

    public void EndProcessRequest(IAsyncResult result)
    {
      var asyncState = (AsyncState)result.AsyncState;
      var del = (Action)asyncState.Del;
      del.EndInvoke(result);
    }

    #endregion

    #region IView Members

    public virtual void Render(ViewContext viewContext, TextWriter writer)
    {
      if (!String.IsNullOrEmpty(MasterTemplateFile))
        ParseAndRenderTemplate(MasterTemplateFile, viewContext, writer);
      else if (!String.IsNullOrEmpty(MasterTemplateText))
        ParseAndRenderTemplateText(MasterTemplateText, viewContext, writer);
      else
        throw new LocatingTemplateException("No MasterTemplateFile or MasterTemplateText has been defined. Please assign either one in order to continue");
    }

    #endregion
  }
}
