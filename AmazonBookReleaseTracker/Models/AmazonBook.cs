using HtmlAgilityPack;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazonBookReleaseTracker
{
    public class AmazonBook : IAmazonProduct, IEquatable<AmazonBook>
    {
        public  AmazonId AmazonId { get; set; }
        public string Title { get; set; }
        public DateTime ReleaseDate { get; set; }

        public AmazonBook()
        {

        }

        public AmazonBook(AmazonId amazonId) : this()
        {
            AmazonId = amazonId;
        }

        public Uri GetUri()
        {
            if (AmazonId is not null)
            {
                return new Uri(@"https://www.amazon.com/gp/product/" + AmazonId.Asin);
            }

            return null;
        }

        public Guid GetGuid()
        {
            return Guid.ParseExact(
                Utilities.CreateMD5(AmazonId.Asin + Title + ReleaseDate.ToString("g")),
                "N");
        }

        public bool ProcessHtml(Stream htmlStream)
        {
            return ProcessHtml(htmlStream, Encoding.UTF8);
        }

        public bool ProcessHtml(Stream htmlStream, Encoding encoding)
        {
            HtmlDocument htmlDoc = new();
            htmlDoc.OptionFixNestedTags = true;
            htmlDoc.Load(htmlStream, encoding);
            
            if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Any())
            {
                //return false;
            }

            var titleNode = htmlDoc.GetElementbyId("productTitle");
            if (titleNode is null)
            {
                throw new NotBookException();
            }
            string title = titleNode.InnerText.Trim();

            if (string.IsNullOrWhiteSpace(Title))
            {
                Title = title;
            }
            else if (Title != title)
            {
                Log.Warning($"Title mismatch with { AmazonId.Asin }.\nExpected: { Title }\nFound: { title }");
                Title = title;
            }

            var details = htmlDoc.GetElementbyId("detailBullets_feature_div").Descendants("span")
                .Where(node => node.GetAttributeValue("class", "").Contains("a-list-item")).ToList();

            string releaseDate = string.Empty;
            foreach (var item in details)
            {
                var listSpans = item.ChildNodes
                    .Where(node => node.Name == "span").ToArray();

                if (listSpans[0].InnerText.Trim().Contains("Publication date"))
                {
                    releaseDate = listSpans[1].InnerText.Trim();
                    break;
                }
            }

            ReleaseDate = DateTime.Parse(releaseDate, new CultureInfo("en-US"), DateTimeStyles.None);
            ReleaseDate = ReleaseDate.AddHours(9d);

            return true;
        }

        public bool Equals(AmazonBook other)
        {
            if (other is null)
            {
                return false;
            }

            return AmazonId.Equals(other.AmazonId)
                && Title.Equals(other.Title)
                && ReleaseDate.Equals(other.ReleaseDate);
        }

        public override bool Equals(object obj)
        {
            return obj is AmazonBook objS && Equals(objS);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AmazonId, Title, ReleaseDate);
        }

        public static bool operator ==(AmazonBook amazonBook1, AmazonBook amazonBook2)
        {
            if (amazonBook1 is null || amazonBook2 is null)
            {
                return System.Object.Equals(amazonBook1, amazonBook2);
            }

            return amazonBook1.Equals(amazonBook2);
        }

        public static bool operator !=(AmazonBook amazonBook1, AmazonBook amazonBook2)
        {
            return !(amazonBook1 == amazonBook2);
        }
    }
}
