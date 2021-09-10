using System.Collections.Generic;

namespace AmazonBookReleaseTracker
{
    public interface IAmazonBookReleaseTrackerSettings
    {
        List<AmazonId> TrackedSeries { get; set; }
        List<AmazonId> TrackedBooks { get; set; }
        List<AmazonId> IgnoredIds { get; set; }
        List<string> IcsCategories { get; set; }
        bool IgnoreReleasedBooks { get; set; }
        int IgnoreAfterReleaseDays { get; set; }
        string TimeZoneTZ { get; set; }
        string ExportNameScheme { get; set; }
        string ExportDirectory { get; set; }

        string GetExportName(string extension);
        string GetExportPath(string extension);
    }
}