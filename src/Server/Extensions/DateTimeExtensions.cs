using System;

namespace Vetrina.Server.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dateTime, DayOfWeek weekStartDay = DayOfWeek.Monday)
        {
            int diff = (7 + (dateTime.DayOfWeek - weekStartDay)) % 7;

            return dateTime.AddDays(-1 * diff).Date;
        }

        public static DateTime EndOfWeek(this DateTime dateTime, DayOfWeek weekStartDay = DayOfWeek.Monday)
        {
           return dateTime.StartOfWeek(weekStartDay).AddDays(6);
        }
    }
}
