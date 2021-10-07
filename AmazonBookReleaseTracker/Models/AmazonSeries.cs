using HtmlAgilityPack;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazonBookReleaseTracker
{
    public class AmazonSeries : IAmazonProduct, IEquatable<AmazonSeries>
    {
        public AmazonId AmazonId { get; set; }
        public string Title { get; set; }
        public List<AmazonBook> Books { get; set; } = new();

        public AmazonSeries()
        {

        }

        public AmazonSeries(AmazonId amazonId) : this()
        {
            AmazonId = amazonId;
        }

        public Uri GetUri()
        {
            if (AmazonId is not null)
            {
                return new Uri(@"https://www.amazon.com/dp/" + AmazonId.Asin);
            }

            return null;
        }

        public bool ProcessHtml(Stream htmlStream)
        {
            return ProcessHtml(htmlStream, Encoding.UTF8);
        }

        public bool ProcessHtml(Stream htmlStream, Encoding encoding)
        {
            HtmlDocument htmlDoc = new();
            htmlDoc.Load(htmlStream, encoding);

            if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Any())
            {
                return false;
            }

            var titleNode = htmlDoc.GetElementbyId("collection-title");
            if (titleNode is null)
            {
                throw new NotSeriesException();
            }
            Title = titleNode.InnerText.Trim();

            string count = htmlDoc.GetElementbyId("collection-size")?.InnerText.Trim();
            count = count[1..count.IndexOf(' ')];

            var nodes = htmlDoc.DocumentNode.Descendants("a")
                .Where(node => node.GetAttributeValue("class", "").Contains("a-size-medium a-link-normal itemBookTitle")).ToList();

            Books = new List<AmazonBook>(nodes.Count);
            foreach (var item in nodes)
            {
                string title = item.InnerText.Trim();
                string id = item.GetAttributeValue("href", "").Replace("/gp/product/", "");
                id = id[0..id.IndexOf('?')];

                var tempBook = new AmazonBook(new AmazonId(id))
                {
                    Title = title
                };

                Books.Add(tempBook);
            }

            if (Books.Count.ToString() != count)
            {
                Log.Warning($"Count mismatch with { AmazonId.Asin }. Expected: { count }, found: { Books.Count }.");
            }

            return true;
        }

        public bool Equals(AmazonSeries other)
        {
            if (other is null)
            {
                return false;
            }

            return AmazonId.Equals(other.AmazonId)
                && Title.Equals(other.Title);
        }

        public override bool Equals(object obj)
        {
            return obj is AmazonSeries objS && Equals(objS);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AmazonId, Title);
        }

        public static bool operator ==(AmazonSeries amazonSeries1, AmazonSeries amazonSeries2)
        {
            if (amazonSeries1 is null || amazonSeries2 is null)
            {
                return System.Object.Equals(amazonSeries1, amazonSeries2);
            }

            return amazonSeries1.Equals(amazonSeries2);
        }

        public static bool operator !=(AmazonSeries amazonSeries1, AmazonSeries amazonSeries2)
        {
            return !(amazonSeries1 == amazonSeries2);
        }
    }
}
