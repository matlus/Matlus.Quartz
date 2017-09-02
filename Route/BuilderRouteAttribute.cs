using System;

namespace Matlus.Quartz
{
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
  public sealed class BuilderRouteAttribute : Attribute
  {
    public int Id { get; private set; }
    public string Pattern { get; private set; }
    public int Priority { get; private set; }

    public BuilderRouteAttribute(int id, string pattern, int priority = 0)
    {
      Id = id;
      Pattern = pattern;
      Priority = priority;
    }
  }
}
