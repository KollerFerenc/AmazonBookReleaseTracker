using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentValidation.Attributes;
using CommandDotNet;

namespace AmazonBookReleaseTracker
{
    [Validator(typeof(AmazonIdValidator))]
    public class AmazonId : IArgumentModel, IEquatable<AmazonId>, IComparable<AmazonId>
    {
        private static readonly Regex _regexValidation = new("(^B\\d{2}\\w{7})|(^B\\d{9}(X|\\d))$",
                                                             RegexOptions.Compiled,
                                                             TimeSpan.FromMilliseconds(250));
        private static readonly Regex _regexFind = new("(B\\d{2}\\w{7})|(B\\d{9}(X|\\d))$",
                                                       RegexOptions.Compiled,
                                                       TimeSpan.FromMilliseconds(250));
        private static readonly AmazonIdComparer _amazonIdComparer = new();

        private string _asin = string.Empty;
        private Guid _guid = Guid.ParseExact(Utilities.CreateMD5(string.Empty), "N");

        [Operand(
            Name = "asin",
            Description = "Amazon ASIN.")]
        public string Asin
        {
            get => _asin;
            set
            {
                if (value != _asin)
                {
                    _asin = value;
                    _guid = Guid.ParseExact(Utilities.CreateMD5(_asin), "N");
                }
            }
        }

        public AmazonId()
        {

        }

        public AmazonId(string asin) : this()
        {
            Asin = asin;
        }

        public bool IsValid()
        {
            return IsValid(Asin);
        }

        public Guid GetGuid()
        {
            return _guid;
        }

        public override string ToString()
        {
            return Asin;
        }

        public bool Equals(AmazonId other)
        {
            if (other is null)
            {
                return false;
            }

            return Asin.Equals(other.Asin);
        }

        public override bool Equals(object obj)
        {
            return obj is AmazonId objS && Equals(objS);
        }

        public override int GetHashCode()
        {
            if (Asin is null)
            {
                return base.GetHashCode();
            }

            return Asin.GetHashCode();
        }

        public static bool IsValid(string asin)
        {
            if (string.IsNullOrEmpty(asin))
            {
                return false;
            }

            if (asin.Length != 10)
            {
                return false;
            }

            return _regexValidation.IsMatch(asin);
        }

        public static bool TryGetAmazonId(string input, out AmazonId amazonId)
        {
            amazonId = new AmazonId();

            if (_regexFind.IsMatch(input))
            {
                string asin = _regexFind.Match(input).Value;
                amazonId = new AmazonId(asin);
                return true;
            }

            return false;
        }

        public int CompareTo(AmazonId other)
        {
            if (other is null)
            {
                return 1;
            }

            return _amazonIdComparer.Compare(this, other);
        }

        public static bool operator ==(AmazonId amazonId1, AmazonId amazonId2)
        {
            if (amazonId1 is null || amazonId2 is null)
            {
                return System.Object.Equals(amazonId1, amazonId2);
            }

            return amazonId1.Equals(amazonId2);
        }

        public static bool operator !=(AmazonId amazonId1, AmazonId amazonId2)
        {
            return !(amazonId1 == amazonId2);
        }

        public static bool operator <(AmazonId left, AmazonId right)
        {
            return left is null ? right is not null : left.CompareTo(right) < 0;
        }

        public static bool operator <=(AmazonId left, AmazonId right)
        {
            return left is null || left.CompareTo(right) <= 0;
        }

        public static bool operator >(AmazonId left, AmazonId right)
        {
            return left is not null && left.CompareTo(right) > 0;
        }

        public static bool operator >=(AmazonId left, AmazonId right)
        {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }
    }
}
