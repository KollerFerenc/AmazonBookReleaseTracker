using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AmazonBookReleaseTracker
{
    internal static class Utilities
    {
        static Utilities()
        {
            ConfigureHttpClient();
        }

        public static readonly string pathToConfig = Path.Combine(Program.baseDirectory, @"config.json");
        public static readonly string pathToDataNew = Path.Combine(Program.baseDirectory, @"data.new.json");
        public static readonly string pathToDataOld = Path.Combine(Program.baseDirectory, @"data.old.json");

        public static readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            WriteIndented = true,
            Converters = { new AmazonIdJsonConverter() },
        };
        public static readonly CsvConfiguration csvConfig = new(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        };
        public static readonly HttpClientHandler clientHandler = new()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        };
        public static readonly HttpClient client = new(clientHandler);

        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            if (input is null)
            {
                input = string.Empty;
            }

            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            StringBuilder sb = new();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }

        private static void ConfigureHttpClient()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
            Utilities.client.DefaultRequestHeaders.Accept.Clear();
        }

        public static async Task<Stream> GetHtml(Uri uri)
        {
            return await Utilities.client.GetStreamAsync(uri);
        }
    }
}
