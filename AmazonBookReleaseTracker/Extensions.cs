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
        public static void Add(this Ical.Net.Proxies.IUniqueComponentList<CalendarEvent> events, 
            AmazonSeries amazonSeries)
        {
            Add(events, amazonSeries, Array.Empty<string>());
        }

        public static void Add(this Ical.Net.Proxies.IUniqueComponentList<CalendarEvent> events,
            AmazonSeries amazonSeries,
            IList<string> categories)
        {
            foreach (var book in amazonSeries.Books)
            {
                events.RemoveExisting(book.GetGuid().ToString("D"));

                events.Add(book.GetCalendarEvent(amazonSeries.Title, categories));
            }
        }

        public static void Add(this Ical.Net.Proxies.IUniqueComponentList<CalendarEvent> events,
            AmazonBook amazonBook)
        {
            Add(events, amazonBook, Array.Empty<string>());
        }

        public static void Add(this Ical.Net.Proxies.IUniqueComponentList<CalendarEvent> events,
            AmazonBook amazonBook,
            IList<string> categories)
        {
            events.RemoveExisting(amazonBook.GetGuid().ToString("D"));

            events.Add(amazonBook.GetCalendarEvent(amazonBook.Title, categories));
        }

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

        public static bool RemoveExisting(this Ical.Net.Proxies.IUniqueComponentList<CalendarEvent> events, string uid)
        {
            int index = events.GetIndexOfUid(uid);

            if (index != -1)
            {
                events.Remove(events[index]);
                return true;
            }

            return false;
        }

        public static int GetIndexOfUid(this Ical.Net.Proxies.IUniqueComponentList<CalendarEvent> events, string uid)
        {
            int i = 0;
            bool found = false;

            while (i < events.Count && !found)
            {
                if (events[i].Uid == uid)
                {
                    found = true;
                }

                i++;
            }

            if (found)
            {
                return (i - 1);
            }
            else
            {
                return -1;
            }
        }

        public static bool IsBetween(this DateTime dateTime, DateTime start, DateTime end)
        {
            return ((dateTime >= start) && (dateTime <= end));
        }

        public static void SortBooks(this IEnumerable<AmazonSeries> amazonSerieses, IComparer<AmazonBook> comparer)
        {
            foreach (var item in amazonSerieses)
            {
                item.Books.Sort(comparer);
            }
        }

        public static bool IsOrdered<T>(this IList<T> list, IComparer<T> comparer = null)
        {
            if (comparer == null)
            {
                comparer = Comparer<T>.Default;
            }

            if (list.Count > 1)
            {
                for (int i = 1; i < list.Count; i++)
                {
                    if (comparer.Compare(list[i - 1], list[i]) > 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
