using System;
using System.Runtime.Serialization;

namespace Matlus.Quartz.Exceptions
{
  [Serializable]
  public class LocatingTemplateException : BaseQuartzWebBuilderException
  {
    public LocatingTemplateException()
      :base()
    {
    }
    public LocatingTemplateException(string message)
      : base(message)
    {
    }

    public LocatingTemplateException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    protected LocatingTemplateException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
