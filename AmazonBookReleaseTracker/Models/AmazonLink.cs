using CommandDotNet;
using FluentValidation.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AmazonBookReleaseTracker
{
    [Validator(typeof(AmazonLinkValidator))]
    public class AmazonLink : IArgumentModel, IEquatable<AmazonLink>
    {
        private static readonly Regex _regex = new (@"(com\/([^/\\])+\/)(dp\/)(B\d{2}\w{7})|(B\d{9}(X|\d))");

        private AmazonId _amazonId;
        private AmazonProductType _productType = (AmazonProductType)(-1);

        [Operand(
            Name = "link",
            Description = "Amazon link.")]
        public string Link { get; set; }

        public AmazonLink()
        {

        }

        public AmazonLink(string link) : this()
        {
            Link = link;

            SimplifyLink();
            GetProductType();
        }

        public AmazonProductType GetProductType()
        {
            if (!Enum.IsDefined(_productType))
            {
                if (Link.Contains("com/dp/"))
                {
                    _productType = AmazonProductType.Series;
                }
                else if (Link.Contains("com/gp/product/"))
                {
                    _productType = AmazonProductType.Book;
                }
                else if (_regex.IsMatch(Link))
                {
                    _productType = AmazonProductType.Book;
                }
                else
                {
                    _productType = AmazonProductType.Unknown;
                }
            }

            return _productType;
        }

        public bool IsValidUri()
        {
            return IsValidUri(Link);
        }

        public bool TryGetAmazonId(out AmazonId amazonId)
        {
            amazonId = new AmazonId();

            if (_amazonId is null)
            {
                if (IsValidUri())
                {
                    var productType = GetProductType();

                    if (productType == AmazonProductType.Book || productType == AmazonProductType.Series)
                    {
                        SimplifyLink();

                        if (AmazonId.TryGetAmazonId(Link,out amazonId))
                        {
                            if (amazonId.IsValid())
                            {
                                _amazonId = new AmazonId(amazonId.Asin);
                                return true;
                            }
                        }
                    }
                }
            }
            else
            {
                amazonId = _amazonId;
                return true;
            }

            return false;
        }

        public void SimplifyLink()
        {
            int index = Link.IndexOf('?');

            if (index != -1)
            {
                Link = Link.Remove(index);
            }
        }

        public Uri GetUri()
        {
            return new Uri(Link);
        }

        public override string ToString()
        {
            return Link;
        }

        public bool Equals(AmazonLink other)
        {
            if (other is null)
            {
                return false;
            }

            return Link.Equals(other.Link);
        }

        public override bool Equals(object obj)
        {
            return obj is AmazonLink objS && Equals(objS);
        }

        public override int GetHashCode()
        {
            if (Link is null)
            {
                return base.GetHashCode();
            }

            return Link.GetHashCode();
        }

        public static bool IsValidUri(string uriName)
        {
            Uri uriResult;
            return Uri.TryCreate(uriName, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
