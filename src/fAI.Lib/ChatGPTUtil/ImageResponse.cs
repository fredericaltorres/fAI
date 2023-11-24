using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;

namespace fAI
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class DatumImage
    {
        public string revised_prompt { get; set; }
        public string url { get; set; }
    }


    public class ImageResponse : BaseHttpResponse
    {
        public int created { get; set; }
        public List<DatumImage> data { get; set; }
        public static ImageResponse FromJson(string text)
        {
            return JsonUtils.FromJSON<ImageResponse>(text);
        }

        public List<string> DownloadImageLocally()
        {
            var r = new List<string>();
            foreach (var d in this.data)
            {
                var uri = new Uri(d.url);
                r.Add(DownloadImage(uri));
            }
            return r
        }

        private string DownloadImage(Uri uri)
        {
            string fileNameOnly = Path.GetFileName(uri.LocalPath);
            var fullPath = Path.Combine(Path.GetTempPath(), fileNameOnly);
            DownloadImageAsync(this.data[0].url, fullPath).Wait();
            return fullPath;
        }

        public static async Task DownloadImageAsync(string imageUrl, string savePath)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(imageUrl);
                if (response.IsSuccessStatusCode)
                {
                    byte[] imageData = await response.Content.ReadAsByteArrayAsync();
                    File.WriteAllBytes(savePath, imageData);
                }
                else
                {
                }
            }
        }
    }
}
