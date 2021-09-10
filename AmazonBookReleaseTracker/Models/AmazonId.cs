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
    public class AmazonId : IArgumentModel, IEquatable<AmazonId>
    {
        private static readonly Regex _regex = new("^B\\d{2}\\w{7}|\\d{9}(X|\\d)$");

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

            return _regex.IsMatch(asin);
        }

        public static bool operator ==(AmazonId amazonId1, AmazonId amazonId2)
        {
            if ((object)amazonId1 == null || ((object)amazonId2) == null)
            {
                return System.Object.Equals(amazonId1, amazonId2);
            }

            return amazonId1.Equals(amazonId2);
        }

        public static bool operator !=(AmazonId amazonId1, AmazonId amazonId2)
        {
            return !(amazonId1 == amazonId2);
        }
    }

    public enum AmazonProductType
    {
        Unknown,
        Book,
        Series,
    }
}
