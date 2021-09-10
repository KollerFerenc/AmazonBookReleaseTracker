using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AmazonBookReleaseTracker
{
    public class NotSeriesException : Exception
    {
        public NotSeriesException()
        {
        }

        public NotSeriesException(string message) : base(message)
        {
        }

        public NotSeriesException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NotSeriesException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
