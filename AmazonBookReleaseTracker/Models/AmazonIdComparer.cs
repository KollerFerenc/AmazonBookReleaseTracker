using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazonBookReleaseTracker
{
    public class AmazonIdComparer : IComparer, IComparer<AmazonId>
    {
        private static readonly StringComparer _stringComparer = StringComparer.Create(System.Globalization.CultureInfo.CurrentCulture, ignoreCase: true);

        public int Compare(AmazonId x, AmazonId y)
        {
            if (x is null && y is null)
            {
                return 0;
            }
            else if (x is null)
            {
                return -1;
            }
            else if (y is null)
            {
                return 1;
            }

            return _stringComparer.Compare(x.Asin, y.Asin);
        }

        int IComparer.Compare(object x, object y)
        {
            return Compare((AmazonId)x, (AmazonId)y);
        }
    }
}
