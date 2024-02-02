using NAudio.SoundFont;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using static fAI.OpenAIImage;

namespace fAI
{
    public partial class LeonardoImage : HttpBase
    {


        public class UserInformation
        {
            public List<UserDetail> user_details { get; set; }
            public Stopwatch Stopwatch { get; set; }

            public static UserInformation FromJson(string text)
            {
                return JsonUtils.FromJSON<UserInformation>(text);
            }
        }

        public class User
        {
            public string id { get; set; }
            public string username { get; set; }
        }

        public class UserDetail
        {
            public User user { get; set; }
            public DateTime tokenRenewalDate { get; set; }
            public int subscriptionTokens { get; set; }
            public int subscriptionGptTokens { get; set; }
            public int subscriptionModelTokens { get; set; }
            public int apiConcurrencySlots { get; set; }
        }











        public LeonardoImage(int timeOut = -1) : base(timeOut)
        {
        }

        const string __urlGetUserInfo = "https://cloud.leonardo.ai/api/rest/v1/me";

        public UserInformation GetUserInformation()
        {
            OpenAI.Trace(new { }, this);

            var sw = Stopwatch.StartNew();
            var response = InitWebClient().GET(__urlGetUserInfo);
            sw.Stop();
            if (response.Success)
            {
                var r = UserInformation.FromJson(response.Text);
                r.Stopwatch = sw;
                return r;
            }
            else
            {
                return null;
            }
        }

        public ImageResponse Generate(string prompt, string model = "dall-e-3", int imageCount = 1, OpenAIImageSize size = OpenAIImageSize._1024x1024)
        {
            OpenAI.Trace(new { prompt, model, size }, this);

            var sw = Stopwatch.StartNew();
            var body = new { prompt, model, n=imageCount, size= size.ToString().Replace("_","") };
            var response = InitWebClient().POST(__urlGetUserInfo, JsonConvert.SerializeObject(body));
            sw.Stop();
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                var r = ImageResponse.FromJson(response.Text);
                r.Stopwatch = sw;
                return r;
            }
            else
            {
                return new ImageResponse
                {
                    Exception = new ChatGPTException($"{response.Exception.Message}", response.Exception)
                };
            }
        }
    }
}

