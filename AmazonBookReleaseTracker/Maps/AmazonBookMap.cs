using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazonBookReleaseTracker
{
    public class AmazonBookMap : ClassMap<AmazonBook>
    {
        public AmazonBookMap()
        {
            Map(m => m.AmazonId).Name("asin");
            Map(m => m.AmazonId).Index(0);
            Map(m => m.Title).Name("title");
            Map(m => m.Title).Index(1);
            Map(m => m.ReleaseDate).Name("releasedate");
            Map(m => m.ReleaseDate).Index(2);
            Map(m => m.ReleaseDate).TypeConverterOption.Format("yyyy.MM.dd.");
        }
    }
}
