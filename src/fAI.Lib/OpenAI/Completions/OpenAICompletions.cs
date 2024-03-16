using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using static DynamicSugar.DS;
using DynamicSugar;

namespace fAI
{
    public partial class OpenAICompletions  : HttpBase
    {
        public OpenAICompletions(int timeOut = -1, string openAiKey = null, string openAiOrg = null) : base(timeOut,  openAiKey, openAiOrg)
        {
        }

        const string __url = "https://api.openai.com/v1/models";

        public Models GetModels()
        {
            var response = InitWebClient().GET(__url);
            if(response.Success)
            {
                return Models.FromJSON(response.Text);
            }
            else throw new ChatGPTException($"{nameof(GetModels)}() failed - {response.Exception.Message}", response.Exception);
        }





        // https://platform.openai.com/docs/guides/gpt
        public CompletionResponse Create(GPTPrompt p)
        {
            OpenAI.Trace(new { p.Url }, this);
            OpenAI.Trace(new { Prompt = p }, this);
            OpenAI.Trace(new { Body = p.GetPostBody() }, this);

            var sw = Stopwatch.StartNew();
            var response = InitWebClient().POST(p.Url, p.GetPostBody());
            sw.Stop();
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                OpenAI.Trace(new { response.Text }, this);

                var r = CompletionResponse.FromJson(response.Text);
                r.GPTPrompt = p;
                r.Stopwatch = sw;
                return r;
            }
            else
            {
                return new CompletionResponse { Exception = new ChatGPTException($"{response.Exception.Message}", response.Exception) };
            }
        }

        public string GenerateMultiChoiceQuestionAbout(string text,
            string promptCommand = "Generate a multi choice questions about the text below,  mark with a * the right answer:")
        {
            var prompt = new Prompt_GPT_35_TurboInstruct
            {
                Text = text,
                PrePrompt = $"{promptCommand} \n===\n",
                PostPrompt = "\n===\nSummary:\n",
            };

            return Create(prompt).Text?.Trim();
        }

        public string Summarize(string text, TranslationLanguages sourceLangague, string promptCommand = "Summarize the following text:") 
        {
            var prompt = new Prompt_GPT_35_TurboInstruct 
            {
                Text = text,
                PrePrompt = $"{promptCommand} \n===\n",
                PostPrompt = "\n===\nSummary:\n",
            };

            return Create(prompt).Text?.Trim();
        }

        public enum GPT_YesNoResponse
        {
            Yes,
            No,
            Unknown
        }

        public GPT_YesNoResponse IsThis(string systemContent, string yesNoQuestion, string dataText, string forceAnswerToYesNo = ", answer only with yes or no?")
        {
            var prompt = new Prompt_GPT_4
            {
                Messages = new List<GPTMessage>()
                {
                    new GPTMessage{ Role =  MessageRole.system, Content = systemContent },
                    new GPTMessage{ Role =  MessageRole.user, Content = $"{yesNoQuestion}{forceAnswerToYesNo}{Environment.NewLine}{dataText}" }
                },
                Url = "https://api.openai.com/v1/chat/completions"
            };
            var response = this.Create(prompt);
            if (response.Success)
            {
                if(response.Text.ToLowerInvariant().Contains(GPT_YesNoResponse.Yes.ToString().ToLower()))
                    return GPT_YesNoResponse.Yes;
                if (response.Text.ToLowerInvariant().Contains(GPT_YesNoResponse.No.ToString().ToLower()))
                    return GPT_YesNoResponse.No;
                return GPT_YesNoResponse.Unknown;
            }
               
            else 
                throw new ChatGPTException($"{nameof(IsThis)}() failed - {response.ErrorMessage}");
        }

        // https://platform.openai.com/docs/guides/prompt-engineering/strategy-provide-reference-text
        public string AnswerQuestionBasedOnText(
            string text,
            string question,
            string context = @"
                    Use the provided article delimited by triple quotes to answer the question.
                    Answer with a JSON object with the property 'answer'.
                    If the answer cannot be found in the article, write ""[NOT_FOUND]""
            ",
            string answerNotFound = "I could not find an answer.")
        {
            context = context.Template(new { NOT_FOUND = answerNotFound }, "[", "]");

            var dataAndQuestion = $@"
""""""
{text} 
""""""

Question: {question}
";
            var client = new OpenAI();
            var p = new Prompt_GPT_4
            {
                Messages = new List<GPTMessage>()
                {
                    new GPTMessage{ Role =  MessageRole.system, Content = context },
                    new GPTMessage{ Role =  MessageRole.user, Content = dataAndQuestion }
                }
            };
            var response = client.Completions.Create(p);
            if (response.Success)
            {
                var answer = response.JsonObject["answer"];
                return answer.ToString();
            }
            else return response.ErrorMessage;
        }


        public class MultiChoiceQuestion
        {
            public string Text { get; set; }
            public List<string> Answers { get; set; } = new List<string>();
            public int CorrectAnswerIndex { get; set; }

            public static MultiChoiceQuestion FromText(string text)
            {
                var r = new MultiChoiceQuestion();
                text = text.Replace(Environment.NewLine, "\n").Replace("\n\n", "\n");
                var lines = text.Split('\n');
                r.Text = lines[0];
                for (var i = 1; i < lines.Length; i++)
                {
                    if (lines[i].Contains("*"))
                        r.CorrectAnswerIndex = i - 1;
                    r.Answers.Add(lines[i].Replace("*", ""));
                }
                return r;
            }
        }

        public static string RandomSynonym = @"Radom,Arbitrary,Randomized,Unpredictable,Sporadic,Accidental,Incidental,Serendipitous,Unplanned,Aleatory,Indiscriminate,Scattered,Casual,Creative,Inventive,Innovative,Imaginative,Original,Artistic,Inspired,Ingenious,Visionary,Inventive,Resourceful,Clever,Productive,Inventive,Expressive,Inventive";

        public static string GetRandomWord(string words)
        {
            var r = new Random();
            var wordsArray = words.Split(',');
            return wordsArray[r.Next(0, wordsArray.Length)];
        }

        public Prompt_GPT_4 GetPromptForGenerateMultiChoiceQuestionAboutText(
          string text,
          string context)
        {
            var data = $@"
""""""
{text} 
""""""
";
            context = context.Template(new { random = GetRandomWord(RandomSynonym) }, "[", "]");
            var client = new OpenAI();
            var p = new Prompt_GPT_4
            {
                Messages = new List<GPTMessage>()
                {
                    new GPTMessage{ Role =  MessageRole.system, Content = context },
                    new GPTMessage{ Role =  MessageRole.user, Content = data },
                }
            };
            return p;
        }

        public string GenerateMultiChoiceQuestionAboutText(
           int numberOfQuestions,
           string text,
           string context = @"
                    Use the provided article delimited by triple quotes to 
                    Generate [numberOfQuestions] [random] multi choice question about the article. 
                    Mark the right answer with a character *.
            ")
        {
            context = context.Template(new { numberOfQuestions }, "[", "]");
            var p = GetPromptForGenerateMultiChoiceQuestionAboutText(text, context);
            var client = new OpenAI();
            var response = client.Completions.Create(p);
            if (response.Success)
            {
                var answer = response.Text;
                return answer.ToString();
            }
            else return response.ErrorMessage;
        }

        private bool IsValidJson<T>(string json)
        {
            try 
            {                 
                var o = JsonConvert.DeserializeObject<T>(json);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool IsNumeric(string value)
        {
            return value.All(char.IsNumber);
        }

        private bool IsNumeric(List<string> strings)
        {
            return strings.All(x => IsNumeric(x));
        }

    }
}

