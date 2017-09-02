using System;
using System.Runtime.Serialization;

namespace Matlus.Quartz.Exceptions
{
  [Serializable]
  public class BaseQuartzWebBuilderException : Exception
  {
    public BaseQuartzWebBuilderException()
      :base()
    {
    }
    public BaseQuartzWebBuilderException(string message)
      : base(message)
    {
    }

    public BaseQuartzWebBuilderException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    protected BaseQuartzWebBuilderException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
