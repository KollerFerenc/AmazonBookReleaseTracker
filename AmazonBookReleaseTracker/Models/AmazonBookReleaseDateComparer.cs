using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazonBookReleaseTracker
{
    public class AmazonBookReleaseDateComparer : IComparer, IComparer<AmazonBook>
    {
        public int Compare(AmazonBook x, AmazonBook y)
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

            return DateTime.Compare(x.ReleaseDate, y.ReleaseDate);
        }

        int IComparer.Compare(object x, object y)
        {
            return Compare((AmazonBook)x, (AmazonBook)y);
        }
    }
}
