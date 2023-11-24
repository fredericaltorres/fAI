using fAI;
using System;
using DynamicSugar;
using static DynamicSugar.ExtensionMethods_Format;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static fAI.OpenAIImage;

namespace fAIConsole
{
    internal class Program
    {
        const string EnglishTest01 = @"I am he as you are he as you are me
And we are all together.
See how they run like pigs from a gun
See how they fly.
I'm crying";

        static void Main(string[] args)
        {
            Generate_Document();
            //var mcs = new MicrosoftCognitiveServices();
            //var voiceId = "en-US-DavisNeural";
            //var mp3FileName = Path.Combine(Path.GetTempPath(), "mp3.mp3");
            //mcs.ExecuteTTS(EnglishTest01, voiceId, mp3FileName);
        }

        // Victor Hugo's MasterPieces
        public static Dictionary<string, string> Books = new Dictionary<string, string>()
        {
            ["Les Misérables"] = "",
            ["The Hunchback of Notre-Dame"] = "\"The Hunchback of Notre-Dame\" is a tragic tale set in 15th century Paris",
            ["Ninety-Three"] = "",
            ["The Man Who Laughs"] = "",
            //["The Toilers of the Sea"] = "",
            //["The Last Day of a Condemned Man"] = "",
            //["Bug-Jargal"] = "",
            //["Hans of Iceland"] = "",
            //["Les Châtiments"] = "",
            //["Les Contemplations"] = "",
        };

        public static void Generate_Document()
        {
            var generatedDocuments = new GeneratedDocuments();
            generatedDocuments.Title = "Victor Hugo's MasterPieces";
            generatedDocuments.Description = "Generate a summary of Victor Hugo's MasterPieces and an image inspired by the book.";

            var client = new OpenAI();
            foreach (var book in Books.Keys)
            {
                var generatedDocument = generatedDocuments.Add(book);

                var prompt = new Prompt_GPT_4
                {
                    Messages = new List<GPTMessage> {
                        new GPTMessage { Role =  MessageRole.system, Content = "As French literature expert."},
                        new GPTMessage { Role =  MessageRole.user  , Content =
                               $@"Summarize in 10 lines the book from French author Victor Hugo, ""{book}""."
                        }
                    }
                };
                generatedDocument._summaryPrompt = prompt.GetPromptString(); // Save the prompt for debugging purposes.

                var response = client.Completions.Create(prompt);

                if (response.Success)
                {
                    generatedDocument.Summary = response.Text;
                    var imagePrompt = $@"
As French literature expert and people expert.
Generate an image inspired by Victor Hugo's classic novel, '{book}'.
[MORE_DALLE_PROMPT]

The image should depict 3 characters from France only and from the era of the novel.
The image should be a painting, not a photograph.

About the image to be generated:
/*
- Artistic Style: Impressionism.
- Color Scheme: Vincent Van Gogh color palette.
*/
- Level of Detail: High.".RemoveComment(StringComment.C);

                    // Avoid our content policy from openAI, about image generation.
                    var useBookSummaryInDallePrompt = string.IsNullOrEmpty(Books[book]);
                    if (useBookSummaryInDallePrompt)
                        imagePrompt = imagePrompt.Replace("[MORE_DALLE_PROMPT]", $@"Also used the following book's summary as a prompt:\r\n{generatedDocument.Summary}\r\n");
                    else
                        imagePrompt = imagePrompt.Replace("[MORE_DALLE_PROMPT]", Books[book]);

                    var promptLength = imagePrompt.Length;
                    generatedDocument._imagePrompt = imagePrompt; // Save the prompt for debugging purposes.

                    var r = client.Image.Generate(imagePrompt, size: OpenAIImageSize._1024x1024);
                    if (r.Success)
                    {
                        generatedDocument.LocalImage = r.DownloadImageLocally()[0];
                    }
                }
            }
            generatedDocuments.Save(@"c:\temp\VictorHugo.Documents.json");
        }
    }
}
