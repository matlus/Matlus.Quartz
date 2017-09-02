using System;
using System.Runtime.Serialization;

namespace Matlus.Quartz.Exceptions
{
  [Serializable]
  public class HandlerNotFoundException : BaseQuartzWebBuilderException
  {
    public HandlerNotFoundException()
      : base()
    {
    }
    public HandlerNotFoundException(string message)
      : base(message)
    {
    }

    public HandlerNotFoundException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    protected HandlerNotFoundException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
