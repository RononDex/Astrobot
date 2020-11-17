using System;

namespace AstroBot.Utilities.Extensions
{
    public static class StringExtensions
    {
        public static string WithMaxLength(this string value, int maxLength)
        {
            return value?.Substring(0, Math.Min(value.Length, maxLength));
        }

        public static string ShortenTo(this string value, int length)
        {
            return value.Length > length
                ? value.Substring(0, length - 4) + " ..."
                : value;
        }
    }
}