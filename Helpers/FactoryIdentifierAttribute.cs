using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Matlus.Quartz
{
  [AttributeUsage(AttributeTargets.Class)]
  public sealed class FactoryIdentifierAttribute : Attribute
  {
    public string Identifier { get; private set; }

    public FactoryIdentifierAttribute()
    {
    }

    public FactoryIdentifierAttribute(string identifier)
    {
      Identifier = identifier;
    }
  }
}
