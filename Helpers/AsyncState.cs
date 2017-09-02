using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Matlus.Quartz
{
  internal class AsyncState
  {
    public Delegate Del { get; private set; }
    public object State { get; private set; }

    public AsyncState(Delegate del, object state)
    {
      Del = del;
      State = state;
    }
  }
}
