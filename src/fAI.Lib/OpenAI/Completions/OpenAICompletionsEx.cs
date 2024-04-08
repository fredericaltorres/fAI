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
        public OpenAICompletionsEx() : base(-1, null, null)
        {
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

        public Func<object, string> SemanticFunction(string parameterizedPrompt) 
        {
            return new Func<object, string>((poco) =>
            {
                var prompt = new Prompt_GPT_35_TurboInstruct
                {
                    Text = parameterizedPrompt.Template(poco, "[","]" ),
                };

                return Create(prompt).Text?.Trim();
            });
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
            bool gpt35 = false)
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

            if (gpt35)
            {
                p = new Prompt_GPT_35_Turbo// Prompt_GPT_35_Turbo
                {
                    //PrePrompt = context,
                    //Text = dataAndQuestion,
                    Messages = new List<GPTMessage>()
                    {
                        new GPTMessage{ Role =  MessageRole.system, Content = context },
                        new GPTMessage{ Role =  MessageRole.user, Content = dataAndQuestion }
                    }
                };
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
                    Mark the right answer with a character *.
            ")
        {
            var contextPreProcessed = context.Template(new { numberOfQuestions, random = GetRandomWord(RandomSynonym) }, "[", "]");
            var p = GetPromptForGenerateMultiChoiceQuestionAboutText(text, contextPreProcessed);
            var client = new OpenAI();
            var response = client.Completions.Create(p);
            if (response.Success)
                return MultiChoiceQuestion.FromText(response.Text, numberOfQuestions);
            else 
                return null;
        }

    }
}

