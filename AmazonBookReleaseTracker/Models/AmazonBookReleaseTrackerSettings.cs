using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazonBookReleaseTracker
{
    public class AmazonBookReleaseTrackerSettings : IAmazonBookReleaseTrackerSettings
    {
        public List<AmazonId> TrackedSeries { get; set; } = new();
        public List<AmazonId> TrackedBooks { get; set; } = new();
        public List<AmazonId> IgnoredIds { get; set; } = new();
        public List<string> IcsCategories { get; set; } = new()
        {
            "Book release",
        };
        public bool IgnoreReleasedBooks { get; set; } = true;
        public int IgnoreAfterReleaseDays { get; set; } = 30;
        public string TimeZoneTZ { get; set; } = "Europe/Budapest";

        public static AmazonBookReleaseTrackerSettings GetSettings(IAmazonBookReleaseTrackerSettings settings)
        {
            AmazonBookReleaseTrackerSettings settings1 = new();

            settings1.TrackedSeries = settings.TrackedSeries;
            settings1.TrackedBooks = settings.TrackedBooks;
            settings1.IgnoredIds = settings.IgnoredIds;
            settings1.IcsCategories = settings.IcsCategories;
            settings1.IgnoreReleasedBooks = settings.IgnoreReleasedBooks;
            settings1.IgnoreAfterReleaseDays = settings.IgnoreAfterReleaseDays;
            settings1.TimeZoneTZ = settings.TimeZoneTZ;

            return settings1;
        }
    }
}
