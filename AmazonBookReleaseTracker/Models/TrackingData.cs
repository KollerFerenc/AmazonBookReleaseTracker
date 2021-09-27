using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AmazonBookReleaseTracker
{
    public class TrackingData : AmazonContainer
    {
        public DateTime LastRun { get; set; } = DateTime.MinValue;

        [JsonIgnore]
        public bool IsEmpty => LastRun == DateTime.MinValue;

        public TrackingData() : base()
        {

        }

        public TrackingData(
            IEnumerable<AmazonSeries> amazonSeries,
            IEnumerable<AmazonBook> amazonBooks) : base(amazonSeries, amazonBooks)
        {
            LastRun = DateTime.Now;
        }

        public TrackingData(
            DateTime lastRun,
            IEnumerable<AmazonSeries> amazonSeries,
            IEnumerable<AmazonBook> amazonBooks) : base(amazonSeries, amazonBooks)
        {
            LastRun = lastRun;
        }
    }
}
