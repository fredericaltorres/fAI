using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public bool Success => Exception == null;
        public Exception Exception { get; set; }
        public int created { get; set; }
        public List<DatumImage> data { get; set; }
        public static ImageResponse FromJson(string text)
        {
            return JsonUtils.FromJSON<ImageResponse>(text);
        }

        public List<string> GetUrls()
        {
            return this.data.Select(z => z.url).ToList();
        }

        public List<string> DownloadImages(List<string> images = null)
        {
            if (images == null)
            {
                images = new List<string>();
                foreach (var d in this.data)
                    images.Add(null);
            }

            var r = new List<string>();
            var x = 0;
            foreach (var d in this.data)
            {
                var uri = new Uri(d.url);
                r.Add(DownloadImage(uri, images[x]));
                x += 1;
            }
            return r;
        }
    }
}
