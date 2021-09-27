using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazonBookReleaseTracker
{
    public class AmazonContainer
    {
        public List<AmazonSeries> AmazonSeries { get; set; } = new();
        public List<AmazonBook> AmazonBooks { get; set; } = new();

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
    }
}
