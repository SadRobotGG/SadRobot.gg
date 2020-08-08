using System;

namespace SadRobot.Core.Json
{
    public static class DateTimeExtensions
    {
        internal static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        internal static readonly DateTime MilleniumEpoch = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long ToUnixEpoch(this DateTime value)
        {
            return ToEpoch(value, UnixEpoch);
        }

        public static long ToUnixEpoch(this DateTimeOffset value)
        {
            return ToEpoch(value, UnixEpoch);
        }

        public static long ToMilleniumEpoch(this DateTime value)
        {
            return ToEpoch(value, MilleniumEpoch);
        }

        public static long ToEpoch(this DateTime value, DateTime epochBase)
        {
            var seconds = (long)(value.ToUniversalTime() - epochBase).TotalSeconds;
            if (seconds < 0) throw new FormatException("Invalid epoch timestamp (less than 0)");
            return seconds;
        }

        public static long ToEpoch(this DateTimeOffset value, DateTime epochBase)
        {
            var seconds = (long)(value.ToUniversalTime() - epochBase).TotalSeconds;
            if (seconds < 0) throw new FormatException("Invalid epoch timestamp (less than 0)");
            return seconds;
        }
    }
}