using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AmazonBookReleaseTracker
{
    public class NotBookException : Exception
    {
        public NotBookException()
        {
        }

        public NotBookException(string message) : base(message)
        {
        }

        public NotBookException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NotBookException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
