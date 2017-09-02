using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Matlus.Quartz.Exceptions
{
  [Serializable]
  public class DuplicateBuilderUrlPatternException : BaseQuartzWebBuilderException
  {
    public DuplicateBuilderUrlPatternException()
      : base()
    {
    }
    public DuplicateBuilderUrlPatternException(string message)
      : base(message)
    {
    }

    public DuplicateBuilderUrlPatternException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    protected DuplicateBuilderUrlPatternException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
