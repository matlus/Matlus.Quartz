using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Reflection;

namespace Matlus.Quartz
{
  public static class Repeater
  {
    private static readonly string ItemTemplateTag = "ItemTemplate";
    private static readonly string FooterTemplateTag = "FooterTemplate";

    public static void Repeat<T>(BaseView view, TextWriter writer, string headerTemplate,
      string itemTemplate, string footerTemplate, IEnumerable<T> model,
      Action<string, T> missingPropertiesCallback = null)
    {
      if (model == null)
        throw new ArgumentNullException("model");
      Dictionary<string, PropertyInfo> properties = null;
      if (model.First() != null)
        properties = DtoBinder.GetPropertyInfos(model.First());

      view.ParseTemplate(headerTemplate, writer, headerTag =>
        {
          if (String.CompareOrdinal(headerTag, ItemTemplateTag) == 0)
          {
            foreach (var item in model)
            {
              view.ParseTemplate(itemTemplate, writer, itemTag =>
                {
                  if (properties.ContainsKey(itemTag))
                  {
                    var propInfo = properties[itemTag];
                    if (propInfo != null)
                      writer.Write(propInfo.GetValue(item, null));
                  }
                  else if (missingPropertiesCallback != null)
                    missingPropertiesCallback(itemTag, item);
                });
            }
          }
          else if (String.CompareOrdinal(headerTag, FooterTemplateTag) == 0)
          {
            view.ParseTemplate(footerTemplate, writer, footerTag =>
              {
                object propertyValueObj = DtoBinder.GetPropertyValue(view, footerTag);
                if (propertyValueObj != null)
                  writer.Write(propertyValueObj);
                else if (missingPropertiesCallback != null)
                  missingPropertiesCallback(footerTag, default(T));
              });
          }
          else
          {
            object propertyValueObj = DtoBinder.GetPropertyValue(view, headerTag);
            if (propertyValueObj != null)
              writer.Write(propertyValueObj);
            else if (missingPropertiesCallback != null)
              missingPropertiesCallback(headerTag, default(T));
          }
        });
    }
  }
}