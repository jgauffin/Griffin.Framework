using System;

namespace Griffin.Data.Mapper
{
    /// <summary>
    /// Some databases do not support DateTime columns. For them we can instead store an unix time integer. These extensions handles the conversions for us.
    /// </summary>
    public static class DateTimeExtensions
    {
        public static readonly DateTime UnixDate = new DateTime(1970, 1, 1);

        /// <summary>
        /// Will remove milliseconds from the the date tiime
        /// </summary>
        /// <param name="source">Time with milliseconds.</param>
        /// <returns>Date/Time without ms</returns>
        public static DateTime TruncateMilliseconds(this DateTime source)
        {
            return new DateTime(source.Ticks - (source.Ticks % TimeSpan.TicksPerSecond),source.Kind);
        }

        /// <summary>
        /// Convert a date to unix epoch
        /// </summary>
        /// <param name="dateUtc">MUST be in UTC. Either use <c>DateTime.UtcNow</c> or convert the local time to UTC time.</param>
        /// <returns>Seconds from 1970-01-01</returns>
        public static double ToUnixTime(this DateTime dateUtc)
        {
            return dateUtc.Subtract(UnixDate).TotalSeconds;
        }

        /// <summary>
        /// Convert unix time to a DateTime struct
        /// </summary>
        /// <param name="unixTime">Seconds from 1970-01-01</param>
        /// <returns>Date/time in UTC time zone</returns>
        public static DateTime FromUnixTime(this double unixTime)
        {
            return UnixDate.AddSeconds(unixTime);
        }

        /// <summary>
        /// Convert unix time to a DateTime struct
        /// </summary>
        /// <param name="unixTime">Seconds from 1970-01-01</param>
        /// <returns>Date/time in UTC time zone</returns>
        public static DateTime FromUnixTime(this int unixTime)
        {
            return UnixDate.AddSeconds(unixTime);
        }


    }
}