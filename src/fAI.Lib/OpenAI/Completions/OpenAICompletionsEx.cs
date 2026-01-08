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
    public partial class OpenAICompletionsEx  : OpenAICompletions
    {
        public OpenAICompletionsEx() : base(-1, null)
        {
        }

        public string GenerateMultiChoiceQuestionAbout(string text,
            string promptCommand = "Generate a multi choice questions about the userPrompt below,  mark with a * the right answer:")
        {
            var prompt = new Prompt_GPT_35_TurboInstruct
            {
                Text = text,
                PrePrompt = $"{promptCommand} \n===\n",
                PostPrompt = "\n===\nSummary:\n",
            };

            return Create(prompt).Text?.Trim();
        }

        //public Func<object, string> SemanticFunction(string parameterizedPrompt) 
        //{
        //    return new Func<object, string>((poco) =>
        //    {
        //        var prompt = new Prompt_GPT_35_TurboInstruct
        //        {
        //            Text = parameterizedPrompt.Template(poco, "[","]" ),
        //        };

        //        return Create(prompt).Text?.Trim();
        //    });
        //}

        public string Summarize(string text, TranslationLanguages sourceLangague, string promptCommand = "Summarize the following userPrompt:") 
        {
            var prompt = new Prompt_GPT_4_Turbo_128k //Prompt_GPT_35_TurboInstruct
            {
                Messages = new List<GPTMessage>()
                {
                    new GPTMessage{ Role =  MessageRole.system, Content = promptCommand },
                    new GPTMessage{ Role =  MessageRole.user, Content = text }
                }
            };

            return Create(prompt).Text?.Trim();
        }


        public const string _answerNotFoundDefault = "I could not find an answer";

        public string AnswerNotFoundDefault => _answerNotFoundDefault;

        // https://platform.openai.com/docs/guides/prompt-engineering/strategy-provide-reference-text
        public string AnswerQuestionBasedOnText(
            string text,
            string question,
            string context = @"
                    Use the provided article delimited by triple quotes to answer the question:
                    ""[question]"".
                    - Answer with a JSON object with the property 'answer'.
                    - If the answer cannot be found in the article, write ""[not_found]""
            ",
            string answerNotFound = _answerNotFoundDefault,
            string model = "gpt-4")
        {

            context = context.Template(new { question, not_found = answerNotFound }, "[", "]");
            var dataAndQuestion = $@"
""""""
{text} 
""""""
";
            var client = new OpenAI();

            GPTPrompt p = new Prompt_GPT_4
            {
                Messages = new List<GPTMessage>()
                {
                    new GPTMessage{ Role =  MessageRole.system, Content = context },
                    new GPTMessage{ Role =  MessageRole.user, Content = dataAndQuestion }
                }
            };

            if (model != "gpt-4")
            {
               p.Model = model;
            }

            var response = client.Completions.Create(p);
            if (response.Success)
            {
                if(response.Text == answerNotFound)
                    return answerNotFound;

                if(response.JsonObject == null)
                    return $"Error parsing JSON answer:{response.Text}";

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

            public MultiChoiceQuestion()
            {
            }

            private void CleanText()
            {
                this.Text = CleanTextReceivedFromGPTForQuestionGeneration(this.Text);
                this.Answers = Answers.Select(a => CleanTextReceivedFromGPTForQuestionGeneration(a)).ToList();
            }

            static string CleanTextReceivedFromGPTForQuestionGeneration(string text)
            {
                text = text.Trim();

                if (text[1] == ')' || text[1] == '.')
                    text = text.Substring(2);
                if (text.StartsWith(@"Question:"))
                    text = text.Substring(9);
                if (text.StartsWith(@"Q:"))
                    text = text.Substring(2);
                if (text.StartsWith(@""""))
                    text = text.Substring(1);
                if (text.EndsWith(@""""))
                    text = text.Substring(0, text.Length - 1);

                text = text.Trim();

                return text;
            }

            public static List<MultiChoiceQuestion> FromText(string text, int questionCount, int maxAnswer = 4)
            {
                var rr = new List<MultiChoiceQuestion>();
                text = text.Replace(Environment.NewLine, "\n").Replace("\n\n", "\n");
                var lines = text.SplitByCRLF(separator: "\n");
                var lineIndex = 0;

                for (var zi = 0; zi < questionCount; zi++)
                {
                    var r = new MultiChoiceQuestion();
                    rr.Add(r);
                    r.Text = lines[lineIndex++];
                    if (r.Text.TrimStart().StartsWith(@""""))
                        r.Text = r.Text.TrimStart().Substring(1);

                    if (r.Text.TrimStart().EndsWith(@""""))
                        r.Text = r.Text.TrimStart().Substring(0, r.Text.Length - 1);

                    if (CompletionResponse.StartsWithANumberSection(r.Text) || CompletionResponse.StartsWithALetterSection(r.Text) || 
                        CompletionResponse.StartsWithWordSection(r.Text, DS.List("Question", "Q")))
                    {
                        r.Text = CompletionResponse.RemoveSection(r.Text);
                    }

                    for (var i = 1; i <= maxAnswer; i++)
                    {
                        if (lines[lineIndex].Contains("*"))
                            r.CorrectAnswerIndex = i - 1;
                        r.Answers.Add(lines[lineIndex].Replace("*", ""));
                        lineIndex += 1;
                    }
                    r.CleanText();
                }

                return rr;
            }
        }

        public static string RandomSynonym = @"Radom,Arbitrary,Randomized,Unpredictable,Sporadic,Accidental,Incidental,Serendipitous,Unplanned,Aleatory,Casual,Creative,Inventive,Innovative,Imaginative,Original,Artistic,Inspired,Ingenious,Visionary,Inventive,Resourceful,Clever,Productive,Inventive,Expressive,Inventive";

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

        public List<MultiChoiceQuestion> GenerateMultiChoiceQuestionAboutText(
           int numberOfQuestions,
           string text,
           string context = @"
                    Use the provided article delimited by triple quotes to 
                    Generate [numberOfQuestions] [random] multi choice question about the article. 
                    Mark the right answer with a character * in the same line of the answer.
            ",
           Func<string, string> preProcessTextReceivedFn = null,
           bool insertRandomizedWord = true)
        {
            var contextPreProcessed = context.Template(new { 
                numberOfQuestions, 
                random =(insertRandomizedWord ?GetRandomWord(RandomSynonym)  : "")
            }, "[", "]");

            var p = GetPromptForGenerateMultiChoiceQuestionAboutText(text, contextPreProcessed);
            var client = new OpenAI();
            var response = client.Completions.Create(p);
            if (response.Success) 
            {
                var responseText = response.Text;
                if (preProcessTextReceivedFn != null)
                {
                    responseText = preProcessTextReceivedFn(response.Text);
                    OpenAI.Trace(new { preProcessTextReceivedFn = responseText }, this);
                }
                return MultiChoiceQuestion.FromText(responseText, numberOfQuestions);
            }
            else 
                return null;
        }

        // "gpt-5.2", gpt-5-mini, gpt-5-nano
        public Prompt_GPT_4 GetPromptForTextImprovement(string userPrompt, string systemPrompt, string model = "gpt-5-mini")
        {
            var data = $@"
""""""
{userPrompt} 
""""""
";
            var client = new OpenAI();
            var p = new Prompt_GPT_4
            {
                Messages = new List<GPTMessage>()
                {
                    new GPTMessage{ Role =  MessageRole.system, Content = systemPrompt },
                    new GPTMessage{ Role =  MessageRole.user, Content = data },
                }
                , Model = model
            };


            return p;
        }

        public string TextImprovement(
           string text,
           string language,
           string systemPrompt = @"
Improve the [language] for the following phrases, in more polished and business-friendly way:
===================================
            ",
            string model = "gpt-5-mini"
           )
        {
            var contextPreProcessed = systemPrompt.Template(new
            {
                language,
            }, "[", "]");

            
            var client = new OpenAI();
            var p = GetPromptForTextImprovement(text, contextPreProcessed, model);
            var response = client.Completions.Create(p);
            if (response.Success)
            {
                var responseText = response.Text;
                return responseText;
            }
            else return null;
        }
    }
}

