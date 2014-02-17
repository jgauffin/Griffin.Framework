using System;

namespace Griffin.Data.Mapper
{
    public static class DateTimeExtensions
    {
        private static readonly DateTime UnixDate = new DateTime(1970, 1, 1);

        public static double ToUnixTime(this DateTime dateUtc)
        {
            return dateUtc.Subtract(UnixDate).TotalSeconds;
        }

        public static DateTime FromUnixTime(this double unixTime)
        {
            return UnixDate.AddSeconds(unixTime);
        }
    }
}