using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace IoTVisualization.Utils
{
    /// <summary>
    /// A util class for strings.
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// Replaces all characters after "length" with ... .
        /// </summary>
        /// <param name="source">Device string</param>
        /// <param name="length">Maximum length</param>
        /// <returns>String with maximal "length" characters</returns>
        public static string TruncateWithEllipsis(this string source, int length)
        {
            return Regex.Replace(source, "^(.{" + length + "}).+", "$1...");
        }
    }
}
