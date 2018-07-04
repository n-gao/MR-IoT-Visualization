using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IoTVisualization.Utils
{
    /// <summary>
    /// An util class to simplify DateTime operations.
    /// </summary>
    public static class DateTimeHelper
    {
        /// <summary>
        /// Returns a DateTimeOffset at 1.1.1970 at 00:00:00
        /// </summary>
        public static DateTimeOffset UtcMin
        {
            get { return new DateTimeOffset(1970, 1, 1, 0, 0, 0, new TimeSpan()); }
        }

        /// <summary>
        /// Returns the average DateTime.
        /// </summary>
        /// <param name="dates">Entries</param>
        /// <returns>Average</returns>
        public static DateTimeOffset Average(this IEnumerable<DateTimeOffset> dates)
        {
            double count = dates.Count();
            double avgTicks = 0D;
            foreach (var date in dates)
            {
                avgTicks += date.UtcTicks / count;
            }
            return DateTimeOffset.MinValue.AddTicks((long)avgTicks);
        }
    }
}
