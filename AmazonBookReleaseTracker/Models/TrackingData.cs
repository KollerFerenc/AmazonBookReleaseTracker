using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AmazonBookReleaseTracker
{
    public class TrackingData
    {
        public DateTime LastRun { get; set; } = DateTime.MinValue;
        public List<AmazonSeries> AmazonSeries { get; set; } = new();
        public List<AmazonBook> AmazonBooks { get; set; } = new();

        [JsonIgnore]
        public bool IsEmpty => LastRun == DateTime.MinValue;
        public TrackingData()
        {

        }

        public TrackingData(
            IEnumerable<AmazonSeries> amazonSeries,
            IEnumerable<AmazonBook> amazonBooks)
        {
            LastRun = DateTime.Now;
        }

        public TrackingData(
            DateTime lastRun,
            IEnumerable<AmazonSeries> amazonSeries,
            IEnumerable<AmazonBook> amazonBooks) : this(amazonSeries, amazonBooks)
        {
            LastRun = lastRun;
            AmazonSeries = new(amazonSeries);
            AmazonBooks = new(amazonBooks);
        }
    }
}
