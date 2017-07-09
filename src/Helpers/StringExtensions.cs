
namespace OcrStatement
{
    using System;
    using System.ComponentModel;

    public static class StringExtensions
    {
        public static T As<T>(this string s)
        {
            return s.As<T>(default(T));
        }

        public static T As<T>(this string s, T defaultValue)
        {
            T result = defaultValue;

            if (string.IsNullOrEmpty(s) == false)
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                result = (T)converter.ConvertFromString(s);
            }
            return result;
        }

        public static Nullable<Decimal> TryAsDecimal(this string s)
        {
            Decimal? result = null;
            if (Decimal.TryParse(s, out Decimal amount))
            {
                result = amount;
            }
            return result;
        }
    }
}
