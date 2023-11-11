using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace fAI
{
    public class VoiceDefinitions : List<VoiceDefinition>
    {
        public VoiceDefinitions Clone()
        {
            var r = new VoiceDefinitions();
            r.AddRange(this);
            return r;
        }

        public void Set(List<VoiceDefinition> list)
        {
            this.Clear();
            this.AddRange(list);
        }

        public string GetAzureVoiceDefinitionApiUrl(string region)
        {
            return $"https://{region}.tts.speech.microsoft.com/cognitiveservices/voices/list";
        }

        public static VoiceDefinitions FromJson(string json)
        {
            return JsonConvert.DeserializeObject<VoiceDefinitions>(json);
        }

        public VoiceDefinitions Load(string subscriptionKey, string region)
        {
            if (this.Count == 0)
            {
                var client = new WebClient();
                client.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                var json = client.DownloadString(GetAzureVoiceDefinitionApiUrl(region));
                var voiceDefinitions = FromJson(json);
                this.AddRange(voiceDefinitions);
            }
            return this;
        }

        public List<string> GetLocales(params string[] regExFilters)
        {
            var r = this.Select(x => $"{x.Locale}").Distinct().ToList();
            if (regExFilters.Length > 0)
            {
                var rr = new List<string>();
                foreach (var regExFilter in regExFilters)
                {
                    var filter = new Regex(regExFilter, RegexOptions.IgnoreCase);
                    var rrr = r.Where(s => filter.IsMatch(s)).ToList();
                    rr.AddRange(rrr);
                }
                return rr;
            }
            else return r;
        }

        //public VoiceDefinition GetVoiceFromSpeechPlatformBingVendorId(int vendorId)
        //{
        //    var oldBingVoiceDefinitions = new SpeechPlatformBingVoiceDefinitions();
        //    var oldBingVoiceDefinition = oldBingVoiceDefinitions.GetVoice(vendorId);
        //    return this.GetVoiceById(oldBingVoiceDefinition.MicrosoftAzureServerSpeechShortNameMapping);
        //}

        public VoiceDefinition GetVoice(string voiceDefinition)
        {
            return this.GetVoiceById(this.ExtractShortName(voiceDefinition));
        }

        public VoiceDefinition GetVoiceById(string id)
        {
            var r = this.Find(x => x.Id == id);
            return r;
        }

        public string ExtractShortName(string voiceString)
        {
            var parts = voiceString.Split(',').ToList();
            if (parts.Count != 4)
                throw new ArgumentException($"Unexpected number of items in {nameof(ExtractShortName)}({nameof(voiceString)}:{voiceString})");
            return parts[3].Trim(); // ShortName is a unique identifier and is place in column 3 in a CSV string
        }

        public List<string> GetVoiceDisplayNamesForLocal(string locale)
        {
            var rx = new Regex(locale, RegexOptions.IgnoreCase);
            var r = this.Where(x => rx.IsMatch(x.Locale))
                        .Select(x => x.GetVoiceDefinitionDisplayName())
                        .ToList();
            r.Sort();
            return r;
        }

        public Locals GetLocalsForLanguage(string locale)
        {
            var r = new Locals();
            var rx = new Regex($"{locale}-??", RegexOptions.IgnoreCase);
            foreach (var e in this)
            {
                if (rx.IsMatch(e.Locale))
                {
                    if (r.Where(rr => rr.Locale == e.Locale).ToList().Count == 0)
                    {
                        r.Add(new Local { Locale = e.Locale, LocaleName = e.LocaleName });
                    }
                }
                //var r = this.Where(x => rx.IsMatch(x.Locale))
                //            .Select(x => x.GetVoiceDefinitionDisplayName())
                //            .ToList();
            }
            r.Sort();
            return r;
        }
    }
}

