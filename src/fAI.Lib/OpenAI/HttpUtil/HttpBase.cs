using System;
using System.Collections.Generic;

namespace fAI
{
    public class ErrorDetail
    {
        public string message { get; set; }
        public string type { get; set; }
        public object param { get; set; }
        public object code { get; set; }
    }

    public class HttpError
    {
        public ErrorDetail error { get; set; }

        public static HttpError FromJson(string text)
        {
            return JsonUtils.FromJSON<HttpError>(text);
        }
        public static bool IsHttpError(string text)
        {
            try
            {
                return FromJson(text) != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override string ToString()
        {
            return $"Error type:{this.error.type}, msg:{this.error.message}, param:{this.error.param}, code:{this.error.code}";
        }
    }

    public class HttpBase : Logger
    {
        public static int _timeout = 60 * 4;
        public static string _key = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        public static string _openAiOrg = Environment.GetEnvironmentVariable("OPENAI_ORGANIZATION_ID");

        public HttpBase(int timeOut = -1, string openAiKey = null, string openAiOrg = null)
        {
            if (timeOut > 0)
                _timeout = timeOut;

            if (openAiKey != null)
                _key = openAiKey;

            if (openAiOrg != null)
                _openAiOrg = openAiOrg;
        }

        protected virtual ModernWebClient InitWebClient(bool addJsonContentType = true, Dictionary<string, object> extraHeaders = null)
        {
            var mc = new ModernWebClient(_timeout);
            mc.AddHeader("Authorization", $"Bearer {_key}")
              .AddHeader("OpenAI-Organization", _openAiOrg);

            if (addJsonContentType)
                mc.AddHeader("Content-Type", "application/json")
                  .AddHeader("Accept", "application/json");
            return mc;
        }

        protected bool IsError(string text)
        {
            return HttpError.IsHttpError(text);
        }

        protected HttpError GetError(string text)
        {
            return HttpError.FromJson(text);
        }
    }
}

