using System;
using System.Text;
using CacheExample.Helpers;

namespace CacheExample.Extensions
{
    public static class HashCodeExtension
    {
        public static string GetBodyHashCode<TIn, TOut>(this Func<TIn, TOut> predicate, string param = "")
        {
            return GetBodyHashCode(predicate) + $"_{param}";
        }

        public static string GetBodyHashCode<TIn, TOut>(this Func<TIn, TOut> predicate)
        {
            return predicate.Method.GetMethodBody()?.GetILAsByteArray().ToHex(false);
        }

        private static string ToHex(this byte[] bytes, bool upperCase)
        {
            StringBuilder result = new StringBuilder(bytes.Length * 2);

            for (int i = 0; i < bytes.Length; i++)
                result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));

            return result.ToString();
        }
    }
}
