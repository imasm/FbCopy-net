using System;
using System.Globalization;

namespace FbCopy.Tests
{
    class SqlFormatProvider : IFormatProvider
    {
        private readonly SqlFormatter _formatter = new SqlFormatter();

        public object GetFormat(Type formatType)
        {
            if (formatType == typeof(ICustomFormatter))
                return _formatter;
            return null;
        }

        class SqlFormatter : ICustomFormatter
        {
            public string Format(string format, object arg, IFormatProvider formatProvider)
            {
                if (arg == null)                
                    return "NULL";
                
                if (arg is string)  
                    return "'" + ((string)arg).Replace("'", "''") + "'";

                if (arg is DateTime)
                {
                    DateTime dt = ((DateTime)arg);
                    if (dt.TimeOfDay == TimeSpan.Zero)
                        return "'" + ((DateTime)arg).ToString("MM/dd/yyyy") + "'";

                    return "'" + ((DateTime)arg).ToString("MM/dd/yyyy HH:mm:ss.fff") + "'";
                }

                if (arg is Boolean)
                    return ((bool)arg) ? "1" : "0";

                if (arg is IFormattable)
                    return ((IFormattable)arg).ToString(format, CultureInfo.InvariantCulture);

                return arg.ToString();
            }
        }
    }
}

