using System.Collections.Generic;
using System.Web.Mvc;

namespace Matlus.Quartz
{
  public sealed class ViewRegistry : IEnumerable<KeyValuePair<string, IView>>
  {
    private readonly Dictionary<string, IView> innerDictionary = new Dictionary<string, IView>();

    public void RegisterView(string placeholderTagname, IView view)
    {
      if (innerDictionary.ContainsKey(placeholderTagname))
        innerDictionary.Remove(placeholderTagname);
      innerDictionary.Add(placeholderTagname, view);
    }

    public bool Contains(string placeholderTagname)
    {
      return innerDictionary.ContainsKey(placeholderTagname);
    }

    public IView this[string placeholderTagname]
    {
      get { return innerDictionary[placeholderTagname]; }
      set
      {
        if (innerDictionary.ContainsKey(placeholderTagname))
          innerDictionary.Remove(placeholderTagname);
        innerDictionary[placeholderTagname] = value;
      }
    }

    #region IEnumerable<KeyValuePair<string,IView>> Members

    public IEnumerator<KeyValuePair<string, IView>> GetEnumerator()
    {
      return innerDictionary.GetEnumerator();
    }

    #endregion

    #region IEnumerable Members

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return innerDictionary.GetEnumerator();
    }

    #endregion
  }
}
