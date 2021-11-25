using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AmazonBookReleaseTracker
{
    public class AmazonContainer
    {
        public List<AmazonSeries> AmazonSeries { get; set; } = new();
        public List<AmazonBook> AmazonBooks { get; set; } = new();

        [JsonIgnore]
        public int BookCount => AmazonSeries.Sum(s => s.Books.Count) + AmazonBooks.Count;

        public AmazonContainer()
        {

        }

        public AmazonContainer(
            IEnumerable<AmazonSeries> amazonSeries,
            IEnumerable<AmazonBook> amazonBooks)
        {
            AmazonSeries = new(amazonSeries);
            AmazonBooks = new(amazonBooks);
        }

        public AmazonContainer GetAfter(DateTime date)
        {
            return GetAfter(this, date);
        }

        public AmazonContainer GetWithin(double days)
        {
            return GetWithin(this, days);
        }

        public IEnumerable<string> ToLines()
        {
            List<string> lines = new(BookCount);

            lines.Add($"Releases: { BookCount }");
            foreach (var book in AmazonBooks)
            {
                lines.Add($"- { book.Title } ({ book.AmazonId.Asin }): { book.ReleaseDate:d}");
            }

            foreach (var series in AmazonSeries)
            {
                if (series.Books.Count > 0)
                {
                    lines.Add($"- { series.Title } ({series.AmazonId.Asin}) ({ series.Books.Count })");
                    foreach (var book in series.Books)
                    {
                        lines.Add($"\t- { book.Title } ({ book.AmazonId.Asin }): { book.ReleaseDate:d}");
                    }
                }
            }

            return lines;
        }

        public static AmazonContainer GetAfter(AmazonContainer amazonContainer, DateTime date)
        {
            AmazonContainer output = new();

            foreach (var series in amazonContainer.AmazonSeries)
            {
                var tempSeries = series;
                tempSeries.Books = series.Books.FindAll(b => b.ReleaseDate >= date);

                if (tempSeries.Books.Count > 0)
                {
                    output.AmazonSeries.Add(tempSeries);
                }
            }
            
            output.AmazonBooks = amazonContainer.AmazonBooks.FindAll(b => b.ReleaseDate >= date);
            
            return output;
        }

        public static AmazonContainer GetWithin(AmazonContainer amazonContainer, double days)
        {
            AmazonContainer output = new();
            var today = DateTime.Now.Date;

            foreach (var series in amazonContainer.AmazonSeries)
            {
                var tempSeries = series;
                tempSeries.Books = series.Books.FindAll(b => b.ReleaseDate.Date.IsBetween(today, today.AddDays(days)));

                if (tempSeries.Books.Count > 0)
                {
                    output.AmazonSeries.Add(tempSeries);
                }
            }

            output.AmazonBooks = amazonContainer.AmazonBooks.FindAll(b => b.ReleaseDate.Date.IsBetween(today, today.AddDays(days)));

            return output;
        }

        public void SortByReleaseDate()
        {
            AmazonSeries.SortBooks(new AmazonBookReleaseDateComparer());
            AmazonBooks.Sort(new AmazonBookReleaseDateComparer());

            var emptySeries = new List<AmazonSeries>();
            var tempList = new List<Tuple<int, DateTime>>();

            for (int i = 0; i < AmazonSeries.Count; i++)
            {
                if (AmazonSeries[i].Books.Count > 0)
                {
                    tempList.Add(new Tuple<int, DateTime>(i, AmazonSeries[i].Books[0].ReleaseDate));
                }
                else
                {
                    emptySeries.Add(AmazonSeries[i]);
                }
            }

            tempList.Sort((x,y) => x.Item2.CompareTo(y.Item2));

            var sortedList = new List<AmazonSeries>(AmazonSeries.Count);
            foreach (var item in tempList)
            {
                sortedList.Add(AmazonSeries[item.Item1]);
            }

            sortedList.AddRange(emptySeries);

            AmazonSeries = sortedList;
        }
    }
}
