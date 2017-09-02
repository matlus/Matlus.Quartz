using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Matlus.Quartz.Exceptions
{
  [Serializable]
  public class ViewFactoryException : BaseQuartzWebBuilderException
  {
    public ViewFactoryException()
      : base()
    {
    }
    public ViewFactoryException(string message)
      : base(message)
    {
    }

    public ViewFactoryException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    protected ViewFactoryException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }

}
