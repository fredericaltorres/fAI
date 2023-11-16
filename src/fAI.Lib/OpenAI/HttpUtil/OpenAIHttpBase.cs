using System;

namespace fAI
{
    public class OpenAIHttpBase
    {
        private int _timeout = 60 * 4;

        protected string _openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        protected string _openAiOrg = Environment.GetEnvironmentVariable("OPENAI_ORGANIZATION_ID");

        public OpenAIHttpBase(int timeOut = -1, string openAiKey = null, string openAiOrg = null)
        {
            if (timeOut > 0)
                _timeout = timeOut;

            if (openAiKey != null)
                _openAiKey = openAiKey;

            if (openAiOrg != null)
                _openAiOrg = openAiOrg;
        }

        protected ModernWebClient InitWebClient()
        {
            var mc = new ModernWebClient(_timeout);
            mc.AddHeader("Authorization", $"Bearer {_openAiKey}")
              .AddHeader("OpenAI-Organization", _openAiOrg)
              .AddHeader("Content-Type", "application/json")
              .AddHeader("Accept", "application/json");
            return mc;
        }
    }
}

