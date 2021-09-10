using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AmazonBookReleaseTracker
{
    public class AmazonIdJsonConverter : JsonConverter<AmazonId>
    {
        public override AmazonId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string asin = reader.GetString();

            return new AmazonId(asin);
        }

        public override void Write(Utf8JsonWriter writer, AmazonId value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Asin);
        }
    }
}
