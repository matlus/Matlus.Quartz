using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Matlus.Quartz
{
  [Serializable]
  public sealed class PathDataDictionary : Dictionary<string, string>
  {
    public PathDataDictionary()
    {        
    }

    /// <summary>
    /// Get the key at a certain Index
    /// </summary>
    /// <param name="index">The index value of the key</param>
    /// <returns>The key at the specified index</returns>
    public string this[int index]
    {
      get
      {
        if (index > Count - 1)
          return null;
        int idx = 0;
        foreach (var kvp in this)
        {
          if (idx == index)
            return kvp.Key;
          idx++;
        }
        return null;
      }
    }

    private PathDataDictionary(SerializationInfo info, StreamingContext context)
      :base(info, context)
    {

    }
  }
}
