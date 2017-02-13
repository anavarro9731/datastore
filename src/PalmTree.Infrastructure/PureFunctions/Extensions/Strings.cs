using System;

namespace PalmTree.Infrastructure.PureFunctions.Extensions
{
    public static class Strings
    {
        /// <summary>
        ///     Return the substring up to but not including the first instance of 'c'.
        ///     If 'c' is not found, the entire string is returned.
        /// </summary>
        public static string SubstringBefore(this string src, char c)
        {
            if (string.IsNullOrEmpty(src)) return string.Empty;

            var idx = Math.Min(src.Length, src.IndexOf(c));
            if (idx < 0) return src;
            return src.Substring(0, idx);
        }

        public static string SubstringBefore(this string src, string s)
        {
            if (string.IsNullOrEmpty(src)) return string.Empty;

            var idx = Math.Min(src.Length, src.IndexOf(s));
            if (idx < 0) return src;
            return src.Substring(0, idx);
        }

        /// <summary>
        ///     Return the substring up to but not including the last instance of 'c'.
        ///     If 'c' is not found, the entire string is returned.
        /// </summary>
        public static string SubstringBeforeLast(this string src, char c)
        {
            if (string.IsNullOrEmpty(src)) return string.Empty;

            var idx = Math.Min(src.Length, src.LastIndexOf(c));
            if (idx < 0) return src;
            return src.Substring(0, idx);
        }

        public static string SubstringBeforeLast(this string src, string s)
        {
            if (string.IsNullOrEmpty(src)) return string.Empty;

            var idx = Math.Min(src.Length, src.LastIndexOf(s));
            if (idx < 0) return src;
            return src.Substring(0, idx);
        }

        /// <summary>
        ///     Return the substring after to but not including the first instance of 'c'.
        ///     If 'c' is not found, the entire string is returned.
        /// </summary>
        public static string SubstringAfter(this string src, char c)
        {
            if (string.IsNullOrEmpty(src)) return string.Empty;

            var idx = Math.Min(src.Length - 1, src.IndexOf(c) + 1);
            if (idx < 0) return string.Empty;
            return src.Substring(idx);
        }

        /// <summary>
        ///     Return the substring after to but not including the first instance of 'c'.
        ///     If 'c' is not found, the entire string is returned.
        /// </summary>
        public static string SubstringAfter(this string src, string s)
        {
            if (string.IsNullOrEmpty(src)) return string.Empty;

            var idx = Math.Min(src.Length - 1, src.IndexOf(s) + s.Length);
            if (idx < 0) return string.Empty;
            return src.Substring(idx);
        }


        /// <summary>
        ///     Return the substring after to but not including the last instance of 'c'.
        ///     If 'c' is not found, an empty string is returned.
        /// </summary>
        public static string SubstringAfterLast(this string src, char c)
        {
            if (string.IsNullOrEmpty(src)) return string.Empty;
            var fidx = src.LastIndexOf(c);
            if (fidx < 0) return string.Empty;

            var idx = Math.Min(src.Length - 1, fidx + 1);
            return src.Substring(idx);
        }

        public static string SubstringAfterLast(this string src, string s)
        {
            if (string.IsNullOrEmpty(src)) return string.Empty;
            var fidx = src.LastIndexOf(s);
            if (fidx < 0) return string.Empty;

            var idx = Math.Min(src.Length - 1, fidx + s.Length);
            return src.Substring(idx);
        }
    }
}