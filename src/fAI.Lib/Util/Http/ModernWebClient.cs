using System.Net;
using System;
using System.Net.Http;
using System.IO;
using System.Collections.Generic;

namespace fAI
{
    public class ModernWebClient : WebClient
    {
        int _timeOut { get; set; } = 30 * 1000;

        public ModernWebClient(int timeout)
        {
            this._timeOut = timeout * 1000;
        }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = _timeOut;
            return w;
        }

        public void SetCredentials(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
                return;
            this.Credentials = new NetworkCredential(username, password);
        }

        public ModernWebClient AddHeader(string name, string value)
        {
            if (name == null)
                return this;

            if (!string.IsNullOrEmpty(name.Trim()))
                this.Headers.Add(name, value);

            return this;
        }

        private string GetResponseContentType()
        {
            return this.ResponseHeaders["Content-Type"];
        }

        public void InitializeHeaderAsJson()
        {
            this.Headers[HttpRequestHeader.ContentType] = "application/json";
            ///this.Headers[HttpRequestHeader.Accept] = "application/json";
        }

        public ModernWebClientResult GET(string url)
        {
            var r = new ModernWebClientResult();
            try
            {
                var buffer = this.DownloadData(url);
                r.SetText(buffer, this.GetResponseContentType());
            }
            catch (Exception ex)
            {
                r.SetException(ex);
            }
            return r;
        }

        public ModernWebClientResult POST(string url, FileStream fileStream, Dictionary<string, string> options = null)
        {
            var r = new ModernWebClientResult();
            using (HttpClient httpClient = new HttpClient())
            {
                foreach(var header in this.Headers.AllKeys)
                    httpClient.DefaultRequestHeaders.Add(header, this.Headers[header]);

                var content = new MultipartFormDataContent();
                content.Add(new StreamContent(fileStream), "media", "media"); // https://github.com/kbridbur/revai-node-sdk/blob/develop/src/api-client.ts

                if (options != null)
                    foreach (var option in options)
                        content.Add(new StringContent(option.Value), option.Key, option.Key);

                using (HttpResponseMessage response = httpClient.PostAsync(url, content).GetAwaiter().GetResult())
                {
                    string responseString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    r.SetText(responseString, "application/json");
                }
            }
            return r;
        }

        public ModernWebClientResult POST(string url, string body)
        {
            this.InitializeHeaderAsJson();
            var r = new ModernWebClientResult();
            try
            {
                this.Encoding = System.Text.Encoding.UTF8;
                var buffer = this.UploadData(url, "POST", System.Text.Encoding.UTF8.GetBytes(body));
                r.SetResult(buffer, this.GetResponseContentType());
            }
            catch (Exception ex)
            {
                r.SetException(ex);
            }
            return r;
        }
        public string GetResponseImageExtension()
        {
            var parts = GetResponseContentType().Split('/');
            return parts[1];
        }
    }
}
