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

        public List<string> DownloadImages()
        {
            var r = new List<string>();
            foreach (var d in this.data)
            {
                var uri = new Uri(d.url);
                r.Add(DownloadImage(uri));
            }
            return r;
        }

        
    }
}
