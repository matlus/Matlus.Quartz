using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Web;
using System.Collections.Specialized;

namespace Matlus.Quartz
{
    public static class DtoBinder
    {
        private static readonly BindingFlags PROP_BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase;
        private static readonly BindingFlags PROP_SET_BINDING_FLAGS = BindingFlags.SetProperty | BindingFlags.SetField | BindingFlags.IgnoreCase;

        public static object GetPropertyValue(object dtoObject, string propertyName)
        {
            PropertyInfo propInfo = dtoObject.GetType().GetProperty(propertyName, PROP_BINDING_FLAGS);
            if (propInfo != null)
                return propInfo.GetValue(dtoObject, null);
            else
                return null;
        }

        public static Dictionary<string, PropertyInfo> GetPropertyInfos(object dtoObject)
        {
            var properties = new Dictionary<string, PropertyInfo>();
            foreach (var propInfo in dtoObject.GetType().GetProperties(PROP_BINDING_FLAGS))
                properties.Add(propInfo.Name, propInfo);
            return properties;
        }

        public static T CreateInstance<T>(NameValueCollection nameValues) where T : new()
        {
            Type t = typeof(T);
            var propertyInfos = t.GetProperties(PROP_BINDING_FLAGS);
            var instance = new T();

            foreach (var propInfo in propertyInfos)
            {
                var value = nameValues[propInfo.Name];
                if (value != null)
                    SetPropertyValue(instance, propInfo, value);
            }
            return instance;
        }

        public static void SetPropertyValue(object dtoObject, string propertyName, object propertyValue)
        {
            var propInfo = dtoObject.GetType().GetProperty(propertyName, PROP_BINDING_FLAGS);
            SetPropertyValue(dtoObject, propInfo, propertyValue);
        }

        private static void CleanUpPropertyValue<TResult>(ref object propertyValue) where TResult : struct
        {
            var value = propertyValue as String;
            if (value != null)
            {
                var trimmed = value.Trim();
                if (trimmed.Length == 0)
                    propertyValue = default(TResult);
                else
                {
                    Type resultType = typeof(TResult);

                    if (resultType == typeof(Int32))
                        propertyValue = ConvertToInt32(trimmed);
                    else if (resultType == typeof(Int64))
                        propertyValue = ConvertToInt64(trimmed);
                    else if (resultType == typeof(Double))
                        propertyValue = ConvertToDouble(trimmed);
                    else if (resultType == typeof(Decimal))
                        propertyValue = ConvertToDecimal(trimmed);
                    else
                        propertyValue = trimmed;
                }
            }
        }

        public static void SetPropertyValue(object dtoObject, PropertyInfo propInfo, object propertyValue)
        {
            Type propType = propInfo.PropertyType;

            if (propType == typeof(String))
                propInfo.SetValue(dtoObject, propertyValue, PROP_SET_BINDING_FLAGS, null, null, CultureInfo.CurrentCulture);
            else if (propType == typeof(Int32))
            {
                CleanUpPropertyValue<Int32>(ref propertyValue);
                propInfo.SetValue(dtoObject, propertyValue, PROP_SET_BINDING_FLAGS, null, null, CultureInfo.CurrentCulture);
            }
            else if (propType == typeof(Int64))
            {
                CleanUpPropertyValue<Int64>(ref propertyValue);
                propInfo.SetValue(dtoObject, propertyValue, PROP_SET_BINDING_FLAGS, null, null, CultureInfo.CurrentCulture);
            }
            else if (propType == typeof(Boolean))
                propInfo.SetValue(dtoObject, ConvertToBool(propertyValue), PROP_SET_BINDING_FLAGS, null, null, CultureInfo.CurrentCulture);
            else if (propType == typeof(DateTime))
            {
                CleanUpPropertyValue<DateTime>(ref propertyValue);
                propInfo.SetValue(dtoObject, Convert.ToDateTime(propertyValue), PROP_SET_BINDING_FLAGS, null, null, CultureInfo.CurrentCulture);
            }
            else if (propType == typeof(Double))
            {
                CleanUpPropertyValue<Double>(ref propertyValue);
                propInfo.SetValue(dtoObject, propertyValue, PROP_SET_BINDING_FLAGS, null, null, CultureInfo.CurrentCulture);
            }
            else if (propType == typeof(Decimal))
            {
                CleanUpPropertyValue<Decimal>(ref propertyValue);
                propInfo.SetValue(dtoObject, Convert.ToDecimal(propertyValue), PROP_SET_BINDING_FLAGS, null, null, CultureInfo.CurrentCulture);
            }
            else if (propType.IsEnum)
                propInfo.SetValue(dtoObject, Enum.Parse(propType, Convert.ToString(propertyValue), true), PROP_SET_BINDING_FLAGS, null, null, CultureInfo.CurrentCulture);
            else if (propType == typeof(Array))
                propInfo.SetValue(dtoObject, propertyValue, PROP_SET_BINDING_FLAGS, null, null, CultureInfo.CurrentCulture);
            else if (propType == typeof(String[]))
            {
                if (propertyValue is String)
                {
                    var propertyValueAsString = propertyValue.ToString().Trim();
                    if (propertyValueAsString.Length == 0)
                        propertyValue = new string[0];
                    else
                        propertyValue = propertyValueAsString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                }
                propInfo.SetValue(dtoObject, propertyValue, PROP_SET_BINDING_FLAGS, null, null, CultureInfo.CurrentCulture);
            }
            else if (propType == typeof(Int32[]) || propType == typeof(Int64[]))
            {
                if (propertyValue is String)
                {
                    var propertyValueAsString = propertyValue.ToString().Trim();
                    if (propertyValueAsString.Length == 0)
                        propertyValue = Activator.CreateInstance(propType, new object[] { 0 });
                    else
                    {
                        var values = propertyValueAsString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (propType == typeof(Int32[]))
                        {
                            var intArray = new Int32[values.Length];
                            for (int i = 0; i < values.Length; i++)
                                intArray[i] = Int32.Parse(values[i]);
                            propertyValue = intArray;
                        }
                        else
                        {
                            var intArray = new Int64[values.Length];
                            for (int i = 0; i < values.Length; i++)
                                intArray[i] = Int64.Parse(values[i]);
                            propertyValue = intArray;
                        }
                    }
                }
                propInfo.SetValue(dtoObject, propertyValue, PROP_SET_BINDING_FLAGS, null, null, CultureInfo.CurrentCulture);
            }
        }

        private static Int32 ConvertToInt32(string value)
        {
            return Int32.Parse(value, NumberStyles.Currency ^ NumberStyles.AllowDecimalPoint);
        }

        private static Int64 ConvertToInt64(string value)
        {
            return Int64.Parse(value, NumberStyles.Currency ^ NumberStyles.AllowDecimalPoint);
        }

        private static Double ConvertToDouble(string value)
        {
            return Double.Parse(value, NumberStyles.Currency);
        }

        private static Decimal ConvertToDecimal(string value)
        {
            return Decimal.Parse(value, NumberStyles.Currency);
        }

        private static bool ConvertToBool(object objBool)
        {
            switch (objBool.ToString().Trim())
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
    }
}
