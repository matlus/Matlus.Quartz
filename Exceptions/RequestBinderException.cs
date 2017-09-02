using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Matlus.Quartz.Exceptions
{
  [Serializable]
  public class RequestBinderException : BaseQuartzWebBuilderException
  {
    public RequestBinderException()
      : base()
    {
    }
    public RequestBinderException(string message)
      : base(message)
    {
    }

    public RequestBinderException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    protected RequestBinderException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
