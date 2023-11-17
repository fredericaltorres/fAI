using System;

namespace fAI
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class ErrorDetail
    {
        public string message { get; set; }
        public string type { get; set; }
        public object param { get; set; }
        public object code { get; set; }
    }

    public class OpenAIHttpError
    {
        public ErrorDetail error { get; set; }

        public static OpenAIHttpError FromJson(string text)
        {
            return JsonUtils.FromJSON<OpenAIHttpError>(text);
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

    public class OpenAIHttpBase
    {
        public static int _timeout = 60 * 4;
        public static string _openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        public static string _openAiOrg = Environment.GetEnvironmentVariable("OPENAI_ORGANIZATION_ID");

        public OpenAIHttpBase(int timeOut = -1, string openAiKey = null, string openAiOrg = null)
        {
            if (timeOut > 0)
                _timeout = timeOut;

            if (openAiKey != null)
                _openAiKey = openAiKey;

            if (openAiOrg != null)
                _openAiOrg = openAiOrg;
        }

        protected ModernWebClient InitWebClient(bool addJsonContentType = true)
        {
            var mc = new ModernWebClient(_timeout);
            mc.AddHeader("Authorization", $"Bearer {_openAiKey}")
              .AddHeader("OpenAI-Organization", _openAiOrg);

            if (addJsonContentType)
                mc.AddHeader("Content-Type", "application/json")
                  .AddHeader("Accept", "application/json");
            return mc;
        }

        protected bool IsError(string text)
        {
            return OpenAIHttpError.IsHttpError(text);
        }

        protected OpenAIHttpError GetError(string text)
        {
            return OpenAIHttpError.FromJson(text);
        }
    }
}

