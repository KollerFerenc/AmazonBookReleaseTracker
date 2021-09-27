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
                tempSeries.Books = series.Books.FindAll(b => b.ReleaseDate.IsBetween(today, today.AddDays(days)));

                if (tempSeries.Books.Count > 0)
                {
                    output.AmazonSeries.Add(tempSeries);
                }
            }

            output.AmazonBooks = amazonContainer.AmazonBooks.FindAll(b => b.ReleaseDate.IsBetween(today, today.AddDays(days)));

            return output;
        }
    }
}
