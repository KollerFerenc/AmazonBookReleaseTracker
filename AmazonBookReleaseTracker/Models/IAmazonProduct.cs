using System;
using System.IO;
using System.Text;

namespace AmazonBookReleaseTracker
{
    public interface IAmazonProduct
    {
        AmazonId AmazonId { get; set; }
        string Title { get; set; }

        Uri GetUri();
        bool ProcessHtml(Stream htmlStream);
        bool ProcessHtml(Stream htmlStream, Encoding encoding);
    }
}