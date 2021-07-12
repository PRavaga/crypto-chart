using System;
using Windows.ApplicationModel.Appointments;
using CryptoCoins.UWP.Helpers;

namespace CryptoCoins.UWP.Views.Formatter
{
    public static class DateTime
    {
        public static string TimeAgo(System.DateTime dateTime, bool capitalize)
        {
            string result = string.Empty;
            var timeSpan = System.DateTime.Now.Subtract(dateTime);

            if (timeSpan <= TimeSpan.FromSeconds(60))
            {
                result = "DateTime_JustNow".GetLocalized();
            }
            else if (timeSpan <= TimeSpan.FromMinutes(60))
            {
                result = timeSpan.Minutes > 1 ?
                    String.Format("DateTime_MinutesAgo".GetLocalized(), timeSpan.Minutes) :
                    "DateTime_MinuteAgo".GetLocalized();
            }
            else if (timeSpan <= TimeSpan.FromHours(24))
            {
                result = timeSpan.Hours > 1 ?
                    String.Format("DateTime_HoursAgo".GetLocalized(), timeSpan.Hours) :
                    "DateTime_HourAgo".GetLocalized();
            }
            else if (timeSpan <= TimeSpan.FromDays(30))
            {
                result = timeSpan.Days > 1 ?
                    String.Format("DateTime_DaysAgo".GetLocalized(), timeSpan.Days) :
                    "DateTime_DayAgo".GetLocalized();
            }
            else if (timeSpan <= TimeSpan.FromDays(365))
            {
                result = timeSpan.Days > 30 ?
                    String.Format("DateTime_MongthsAgo".GetLocalized(), timeSpan.Days / 30) :
                    "DateTime_MongthAgo".GetLocalized();
            }
            else
            {
                result = timeSpan.Days > 365 ?
                    String.Format("DateTime_YearsAgo".GetLocalized(), timeSpan.Days / 365) :
                    "DateTime_YearAgo".GetLocalized();
            }
            if (capitalize)
            {
                result = result.ToUpper();
            }

            return result;
        }
    }
}
