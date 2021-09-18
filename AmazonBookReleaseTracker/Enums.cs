using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazonBookReleaseTracker
{
    public enum ExitCode
    {
        Default = 0,
        ConfigNotFound = -1,
        ConfigLoadError = -2,
        ValidationError = -3,
        ConfigExists = -4,
        NoIdFound = -5,
        NoProductType = -6,
        DataFileNotFound = -7,
        DataFileLoadError = -8,
    }

    public enum AmazonProductType
    {
        Unknown,
        Book,
        Series,
    }

    public enum OutputFormat
    {
        console,
        csv,
        calendar,
    }
}
