using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;
using Matlus.Quartz.Exceptions;
using System.Runtime.CompilerServices;

namespace Matlus.Quartz
{
    public interface IRequestBinder
    {
        object Bind(NameValueCollection nameValues, Type convertToType, string parameterName = null, object defaultValue = null);
        object Bind(string value, Type convertToType, object defaultValue = null);
    }

    public static class RequestBinder
    {
        private static readonly Dictionary<string, RuntimeTypeHandle>
          registeredBinders = new Dictionary<string, RuntimeTypeHandle>();
        private static readonly Dictionary<string, IRequestBinder>
          instantiatedBinders = new Dictionary<string, IRequestBinder>();

        static RequestBinder()
        {
            Add(typeof(String), typeof(StringRequestBinder));
            Add(typeof(Int32), typeof(Int32RequestBinder));
            Add(typeof(Int64), typeof(Int64RequestBinder));
            Add(typeof(Double), typeof(DoubleRequestBinder));
            Add(typeof(DateTime), typeof(DateTimeRequestBinder));
            Add(typeof(Boolean), typeof(BooleanRequestBinder));
            Add(typeof(Decimal), typeof(DecimalRequestBinder));
            Add(typeof(Int32[]), typeof(Int32ArrayRequestBinder));
            Add(typeof(String[]), typeof(StringArrayRequestBinder));
            Add(typeof(Double[]), typeof(DoubleArrayRequestBinder));
            Add(typeof(Enum), typeof(EnumRequestBinder));
        }

        public static void Add(Type binderForType, Type requestBinderType)
        {
            Type interfaceType = requestBinderType.GetInterface("IRequestBinder");
            if (interfaceType != null)
            {
                var fullName = binderForType.FullName;
                if (registeredBinders.ContainsKey(fullName))
                    registeredBinders.Remove(fullName);
                registeredBinders.Add(fullName, requestBinderType.TypeHandle);
            }
        }

        public static bool Contains(Type type)
        {
            return registeredBinders.ContainsKey(type.FullName);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static IRequestBinder GetBinder(Type type)
        {
            var fullName = type.FullName;
            if (!registeredBinders.ContainsKey(fullName))
                throw new RequestBinderException(
                  "No RequestBinder found for Type: " + fullName);
            if (instantiatedBinders.ContainsKey(fullName))
                return instantiatedBinders[fullName];
            else
            {
                var typeHandle = registeredBinders[fullName];
                IRequestBinder binder =
                  (IRequestBinder)Activator.CreateInstance(
                  Type.GetTypeFromHandle(typeHandle));
                instantiatedBinders.Add(fullName, binder);
                return binder;
            }
        }
    }

    public sealed class Int32RequestBinder : IRequestBinder
    {
        public object Bind(NameValueCollection nameValues, Type convertToType, string parameterName = null, object defaultValue = null)
        {
            var value = nameValues[parameterName];
            return Bind(value, convertToType, defaultValue);
        }

        public object Bind(string value, Type convertToType, object defaultValue = null)
        {
            if (!String.IsNullOrEmpty(value))
                return Int32.Parse(value, NumberStyles.Currency ^ NumberStyles.AllowDecimalPoint);
            else
                return defaultValue;
        }
    }

    public sealed class StringRequestBinder : IRequestBinder
    {
        public object Bind(NameValueCollection nameValues, Type convertToType, string parameterName = null, object defaultValue = null)
        {
            var value = nameValues[parameterName];
            return Bind(value, convertToType, defaultValue);
        }

        public object Bind(string value, Type convertToType, object defaultValue = null)
        {
            if (!String.IsNullOrEmpty(value))
                return value;
            else
                return defaultValue;
        }
    }

    public sealed class Int64RequestBinder : IRequestBinder
    {
        public object Bind(NameValueCollection nameValues, Type convertToType, string parameterName = null, object defaultValue = null)
        {
            var value = nameValues[parameterName];
            return Bind(value, convertToType, defaultValue);
        }

        public object Bind(string value, Type convertToType, object defaultValue = null)
        {
            if (!String.IsNullOrEmpty(value))
                return Int64.Parse(value, NumberStyles.Currency ^ NumberStyles.AllowDecimalPoint);
            else
                return defaultValue;
        }
    }

    public sealed class DoubleRequestBinder : IRequestBinder
    {
        public object Bind(NameValueCollection nameValues, Type convertToType, string parameterName = null, object defaultValue = null)
        {
            var value = nameValues[parameterName];
            return Bind(value, convertToType, defaultValue);
        }

        public object Bind(string value, Type convertToType, object defaultValue = null)
        {
            if (!String.IsNullOrEmpty(value))
                return Double.Parse(value, NumberStyles.Currency);
            else
                return defaultValue;
        }
    }

    public sealed class DateTimeRequestBinder : IRequestBinder
    {
        public object Bind(NameValueCollection nameValues, Type convertToType, string parameterName = null, object defaultValue = null)
        {
            var value = nameValues[parameterName];
            return Bind(value, convertToType, defaultValue);
        }

        public object Bind(string value, Type convertToType, object defaultValue = null)
        {
            if (!String.IsNullOrEmpty(value))
                return DateTime.Parse(value);
            else
                return defaultValue;
        }
    }

    public sealed class DecimalRequestBinder : IRequestBinder
    {
        public object Bind(NameValueCollection nameValues, Type convertToType, string parameterName = null, object defaultValue = null)
        {
            var value = nameValues[parameterName];
            return Bind(value, convertToType, defaultValue);
        }

        public object Bind(string value, Type convertToType, object defaultValue = null)
        {
            if (!String.IsNullOrEmpty(value))
                return Decimal.Parse(value, NumberStyles.Currency);
            else
                return defaultValue;
        }
    }

    public sealed class BooleanRequestBinder : IRequestBinder
    {
        public object Bind(NameValueCollection nameValues, Type convertToType, string parameterName = null, object defaultValue = null)
        {
            var value = nameValues[parameterName];
            return Bind(value, convertToType, defaultValue);
        }

        public object Bind(string value, Type convertToType, object defaultValue = null)
        {
            if (!String.IsNullOrEmpty(value))
            {
                switch (value.ToString().Trim())
                {
                    case "False":
                    case "false":
                    case "0":
                    case "off":
                    case "":
                    default:
                        return false;
                    case "True":
                    case "true":
                    case "1":
                    case "on":
                        return true;
                }
            }
            else
                return defaultValue;
        }
    }

    public sealed class StringArrayRequestBinder : IRequestBinder
    {
        public object Bind(NameValueCollection nameValues, Type convertToType, string parameterName = null, object defaultValue = null)
        {
            var value = nameValues[parameterName];
            return Bind(value, convertToType, defaultValue);
        }

        public object Bind(string value, Type convertToType, object defaultValue = null)
        {
            if (!String.IsNullOrEmpty(value))
            {
                value = value.Trim();
                if (value.Length == 0)
                    return Activator.CreateInstance(convertToType, new object[] { 0 });
                else
                {
                    var values = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var strArray = new String[values.Length];
                    for (int i = 0; i < values.Length; i++)
                        strArray[i] = values[i];
                    return strArray;
                }
            }
            return defaultValue;
        }
    }

    public sealed class Int32ArrayRequestBinder : IRequestBinder
    {
        public object Bind(NameValueCollection nameValues, Type convertToType, string parameterName = null, object defaultValue = null)
        {
            var value = nameValues[parameterName];
            return Bind(value, convertToType, defaultValue);
        }

        public object Bind(string value, Type convertToType, object defaultValue = null)
        {
            if (!String.IsNullOrEmpty(value))
            {
                value = value.Trim();
                if (value.Length == 0)
                    return Activator.CreateInstance(convertToType, new object[] { 0 });
                else
                {
                    var values = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var intArray = new Int32[values.Length];
                    for (int i = 0; i < values.Length; i++)
                        intArray[i] = Int32.Parse(values[i]);
                    return intArray;
                }
            }
            return defaultValue;
        }
    }

    public sealed class DoubleArrayRequestBinder : IRequestBinder
    {
        public object Bind(NameValueCollection nameValues, Type convertToType, string parameterName = null, object defaultValue = null)
        {
            var value = nameValues[parameterName];
            return Bind(value, convertToType, defaultValue);
        }

        public object Bind(string value, Type convertToType, object defaultValue = null)
        {
            if (!String.IsNullOrEmpty(value))
            {
                value = value.Trim();
                if (value.Length == 0)
                    return Activator.CreateInstance(convertToType, new object[] { 0 });
                else
                {
                    var values = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var dblArray = new Double[values.Length];
                    for (int i = 0; i < values.Length; i++)
                        dblArray[i] = Double.Parse(values[i]);
                    return dblArray;
                }
            }
            return defaultValue;
        }
    }

    public sealed class EnumRequestBinder : IRequestBinder
    {
        public object Bind(NameValueCollection nameValues, Type convertToType, string parameterName = null, object defaultValue = null)
        {
            var value = nameValues[parameterName];
            return Bind(value, convertToType, defaultValue);
        }

        public object Bind(string value, Type convertToType, object defaultValue = null)
        {
            if (!String.IsNullOrEmpty(value))
                return Enum.Parse(convertToType, value, true);
            else
                return defaultValue;
        }
    }

}
