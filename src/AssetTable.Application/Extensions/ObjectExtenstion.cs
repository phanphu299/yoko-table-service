using System;
using System.Linq;
using System.Dynamic;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using AHI.Infrastructure.SharedKernel.Extension;
using AssetTable.Application.Constant;

namespace AssetTable.ApplicationExtension.Extension
{
    public static class ObjectExtension
    {
        public static bool ConvertToLongNumber(this object value, out long result)
        {
            if (value == null)
            {
                result = default;
                return false;
            }

            if (long.TryParse(value.ToString(), out result))
            {
                return true;
            }
            else if (double.TryParse(value.ToString(), out var doubleOut))
            {
                result = Convert.ToInt64(doubleOut);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public static bool ConvertToIntNumber(this object value, out int result)
        {
            if (value == null)
            {
                result = default;
                return false;
            }

            if (int.TryParse(value.ToString(), out result))
            {
                return true;
            }
            else if (double.TryParse(value.ToString(), out var doubleOut))
            {
                result = Convert.ToInt32(doubleOut);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public static bool ConvertToDoubleNumber(this object value, out double result)
        {
            if (value == null)
            {
                result = default;
                return false;
            }

            return double.TryParse(value.ToString(), out result);

            // check in case result is an Infinity number
            //return success & double.IsNormal(result);
        }

        public static bool ConvertToDecimalNumber(this object value, out decimal result)
        {
            if (value == null)
            {
                result = default;
                return false;
            }

            return decimal.TryParse(value.ToString(), out result);
        }

        public static bool ConvertToDateTime(this object value, out DateTime result)
        {
            if (value == null)
            {
                result = default;
                return true;
            }

            // string[] formats =
            // {
            //     "yyyy-MM-dd HH:mm:ss",
            //     "yyyy-MM-dd HH:mm:ss.fff",
            //     "yyyy-MM-ddTHH:mm:ss.fff",
            //     "yyyy/MM/dd HH:mm:ss",
            //     "yyyy/MM/dd HH:mm:ss.fff",
            //     "yyyy/MM/ddTHH:mm:ss.fff",
            //     "yyyy-MM-dd",
            //     "yyyy/MM/dd"
            // };
            return DateTime.TryParseExact(value.ToString(), AHI.Infrastructure.SharedKernel.Extension.Constant.DefaultDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
        }

        public static bool ConvertToBoolean(this object value, out bool result)
        {
            return bool.TryParse(value.ToString(), out result);
        }

        public static bool IsLessThanOrEqualsTo(this object value, int length)
        {
            if (value == null)
            {
                return true;
            }

            return value.ToString().Count() <= length;
        }

        public static bool ParseResultWithDataType(this object value, string dataType)
        {
            switch (dataType)
            {
                case DataTypeConstants.TYPE_BOOLEAN:
                    return value is bool;

                case DataTypeConstants.TYPE_TIMESTAMP:
                    return double.TryParse(value.ToString(), out _);

                case DataTypeConstants.TYPE_DOUBLE:
                    return double.TryParse(value.ToString(), out _);
                case DataTypeConstants.TYPE_INTEGER:
                    return int.TryParse(value.ToString(), out _);
                // case "text":
                //     return value is string || value is char;
                case DataTypeConstants.TYPE_DATETIME:
                    return DateTime.TryParse(value.ToString(), out _);
                default:
                    return true;
            }
        }

        public static object ParseValueWithDataType(this object value, string dataType, string valueText, bool isRawData)
        {
            if (value == null)
            {
                if (valueText == null)
                {
                    return value;
                }
                value = valueText;
            }
            switch (dataType)
            {
                case DataTypeConstants.TYPE_BOOLEAN:
                    if (isRawData) //https://dev.azure.com/AssetHealthInsights/Asset%20Backlogs/_workitems/edit/11775
                    {
                        if (bool.TryParse(value.ToString(), out var boolVal))
                        {
                            return boolVal;
                        }
                        else if (value.ToString() == "1" || value.ToString() == "0")
                        {
                            return value.ToString() == "1";
                        }
                        else
                            return valueText;
                    }
                    else
                    {
                        return double.TryParse(value.ToString(), out var vb) && !double.IsNaN(vb) ? value as object : valueText;
                    }

                case DataTypeConstants.TYPE_DOUBLE:
                case DataTypeConstants.TYPE_INTEGER:
                    //https://dev.azure.com/AssetHealthInsights/Asset%20Backlogs/_workitems/edit/11775
                    return double.TryParse(value.ToString(), out var v) && !double.IsNaN(v) ? value as object : valueText;
                case DataTypeConstants.TYPE_TEXT:
                    return Regex.IsMatch(value.ToString(), "^(?=.{0,255}$)") ? value as object : valueText;
                case DataTypeConstants.TYPE_TIMESTAMP:
                    return double.TryParse(value.ToString(), out _) ? value as object : valueText;
                case DataTypeConstants.TYPE_DATETIME:
                    return DateTime.TryParse(value.ToString(), out _) ? value as object : valueText;
                default:
                    return valueText;
            }
        }

        public static dynamic ToExpandoObject(this object value)
        {
            IDictionary<string, object> dapperRowProperties = value as IDictionary<string, object>;
            IDictionary<string, object> expando = new ExpandoObject();
            foreach (KeyValuePair<string, object> property in dapperRowProperties)
            {
                var valueProperty = property.Value;
                if (valueProperty == null)
                {
                    expando.Add(property.Key, valueProperty);
                    continue;
                }
                if (valueProperty.GetType() == typeof(DateTime))
                {
                    var datetimeValue = Convert.ToDateTime(valueProperty).ToString(Constant.DefaultDateTimeFormat);
                    expando.Add(property.Key, datetimeValue);
                }
                else
                {
                    expando.Add(property.Key, valueProperty);
                }
            }
            return expando as ExpandoObject;
        }
    }
}
