using System;

namespace AstroBot.Utilities.Extensions
{
    public static class StringExtensions
    {
        public static string WithMaxLength(this string value, int maxLength)
        {
            return value?.Substring(0, Math.Min(value.Length, maxLength));
        }
    }
}