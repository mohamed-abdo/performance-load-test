using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Performance.Logger.API.Helper
{
    public static class Utilities
    {
        public static readonly string dateFormat = System.Configuration.ConfigurationManager.AppSettings["dateFormat"];
        public static string ParseTimestamp(string timestamp)
        {
            var culure = CultureInfo.InvariantCulture;
            var retDateTime = DateTime.Now;
            if (DateTime.TryParseExact(timestamp, dateFormat, culure, DateTimeStyles.AssumeLocal, out retDateTime))
                return retDateTime.ToString(dateFormat);
            else
                return null;
        }
        public static DateTime? ParseDateTimestamp(string timestamp)
        {
            var culure = CultureInfo.InvariantCulture;
            var dateTimeFormated = ParseTimestamp(timestamp);
            if (!string.IsNullOrEmpty(dateTimeFormated))
                return DateTime.ParseExact(ParseTimestamp(timestamp), dateFormat, culure);
            else
                return null;
        }
    }
}
