using System.Net;
using System;
using System.Net.Http;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Xml.Serialization;

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
            Logger.Trace($"GET {url}", this);
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

        public ModernWebClientResult DELETE(string url, string body = null)
        {
            Logger.Trace($"DELETE {url}", this);
            var r = new ModernWebClientResult();
            try
            {
                var buffer = new byte[0];
                var text = string.Empty;
                if (string.IsNullOrEmpty(body))
                    buffer = this.UploadData(url, "DELETE", new byte[0]);
                else
                    buffer = this.UploadData(url, "DELETE", System.Text.Encoding.UTF8.GetBytes(body));
                r.SetText(buffer, this.GetResponseContentType());

            }
            catch (Exception ex)
            {
                r.SetException(ex);
            }
            return r;
        }

        public ModernWebClientResult POST(string url, string fileName, Dictionary<string, string> options = null, string streamName = "file"/*"media"*/)
        {
            Logger.Trace($"POST {url}, fileName:{fileName}", this);

            using (var fileStream = File.OpenRead(fileName))
            {
                var r = new ModernWebClientResult();
                using (HttpClient httpClient = new HttpClient())
                {
                    foreach (var header in this.Headers.AllKeys)
                        if (header.ToLowerInvariant() != "content-type")
                            httpClient.DefaultRequestHeaders.Add(header, this.Headers[header]);

                    var multiPartcontent = new MultipartFormDataContent("----MyGreatBoundary");
                    multiPartcontent.Add(new StreamContent(fileStream), streamName, Path.GetFileName(fileName));

                    if (options != null)
                    {
                        foreach (var option in options)
                        {
                            multiPartcontent.Add(new StringContent(option.Value), option.Key);
                        }
                    }

                    using (HttpResponseMessage response = httpClient.PostAsync(url, multiPartcontent).GetAwaiter().GetResult())
                    {
                        string responseString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        r.SetText(responseString, "application/json");
                    }
                }
                return r;
            }
        }

        public ModernWebClientResult POST(string url, string body)
        {
            Logger.Trace($"POST {url}, body:{body}", this);
            this.InitializeHeaderAsJson();
            var r = new ModernWebClientResult();
            try
            {
                this.Encoding = System.Text.Encoding.UTF8;
                var buffer = this.UploadData(url, "POST", System.Text.Encoding.UTF8.GetBytes(body));
                r.SetResult(buffer, this.GetResponseContentType());
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (var stream = ex.Response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        string errorMessage = reader.ReadToEnd();
                        var newEx = new Exception(errorMessage);
                        r.SetException(newEx);
                        r.SetText(errorMessage, "application/json");
                    }
                }
                else r.SetException(ex);
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

