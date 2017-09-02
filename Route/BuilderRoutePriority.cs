using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Matlus.Quartz
{
  public sealed class BuilderRoutePriority
  {
    public int RouteId { get; set; }
    public Type Builder { get; private set; }
    public string Pattern { get; private set; }
    public int Priority { get; private set; }
    public Regex Regex { get; private set; }

    public BuilderRoutePriority(int routeId, Type builder, string pattern, int priority = 0)
    {
      RouteId = routeId;
      Builder = builder;
      Pattern = pattern;
      Regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Singleline);
      Priority = priority;
    }
  }
}
