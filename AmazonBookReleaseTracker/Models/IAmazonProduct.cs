using System;
using System.IO;

namespace AmazonBookReleaseTracker
{
    public interface IAmazonProduct
    {
        AmazonId AmazonId { get; set; }
        string Title { get; set; }

        Uri GetUri();
        void ProcessHtml(Stream htmlStream);
    }
}