using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazonBookReleaseTracker
{
    public static class Extensions
    {
        public static CalendarEvent GetCalendarEvent(
            this AmazonBook amazonBook)
        {
            return GetCalendarEvent(amazonBook, amazonBook.Title);
        }

        public static CalendarEvent GetCalendarEvent(
            this AmazonBook amazonBook,
            IList<string> categories)
        {
            return GetCalendarEvent(amazonBook, amazonBook.Title, categories);
        }

        public static CalendarEvent GetCalendarEvent(
            this AmazonBook amazonBook,
            string seriesTitle)
        {
            return GetCalendarEvent(amazonBook, seriesTitle, Array.Empty<string>());
        }

        public static CalendarEvent GetCalendarEvent(
            this AmazonBook amazonBook,
            string seriesTitle,
            IList<string> categories)
        {
            var outpout = new CalendarEvent
            {
                IsAllDay = true,
                DtStart = new CalDateTime(amazonBook.ReleaseDate.Date),
                DtEnd = new CalDateTime(amazonBook.ReleaseDate.Date.AddDays(1d)),
                DtStamp = new CalDateTime(amazonBook.ReleaseDate),
                Summary = seriesTitle,
                Description = amazonBook.Title,
                Categories = categories,
                Url = amazonBook.GetUri(),
                Uid = amazonBook.GetGuid().ToString("D"),
                Transparency = TransparencyType.Transparent,
            };

            return outpout;
        }
    }
}
