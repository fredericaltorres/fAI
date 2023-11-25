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

        static void Main(string[] args)
        {
            Generate_Document();
        }

        const string TheHunchbackOfNotreDame_Summary_Sanitized = @"
""The Hunchback of Notre-Dame"" is a tragic tale set in 15th century Paris. 
The story revolves around Quasimodo, a bell-ringer of Notre-Dame Cathedral. 
He falls in love with a beautiful dancer, Esmeralda, who shows him kindness.
However, Esmeralda is also desired by Archdeacon Frollo, Quasimodo's adoptive father, and Captain Phoebus.
";

        const string BugJargal_Summary_Sanitized = @"
\""Bug-Jargal"" is a historical novel by Victor Hugo set during the revolt in Santo Domingo in 1791. 
The protagonist, a French military officer named Leopold d'Auverney, recounts his experiences during the revolt. 
He befriends Bug-Jargal, a noble African who becomes a revolutionary leader.
Despite their differences, they form a deep bond.
The novel explores themes of inequality, friendship, and the struggle for freedom. 
It also highlights the brutality of slavery and the courage of those who fought against it. 
";

        // Victor Hugo's MasterPieces
        public static Dictionary<string, string> Books = new Dictionary<string, string>()
        {
            ["Les Misérables"] = "",
            ["The Hunchback of Notre-Dame"] = TheHunchbackOfNotreDame_Summary_Sanitized,
            ["Ninety-Three"] = "",
            ["The Man Who Laughs"] = "",
            ["Bug-Jargal"] = BugJargal_Summary_Sanitized,
            ["The Toilers of the Sea"] = "",
            ["The Last Day of a Condemned Man"] = "",
            ["Hans of Iceland"] = "",
            ["Les Châtiments"] = "",
            ["Les Contemplations"] = "",
        };

        public static void Generate_Document()
        {
            var generatedDocuments = new GeneratedDocuments();
            generatedDocuments.Properties.Title = "Victor Hugo's MasterPieces";
            generatedDocuments.Properties.Description = "Generate a summary of Victor Hugo's MasterPieces and an image inspired by the book.";
            OpenAI.TraceOn = true;
            OpenAI.Trace($"Generating document about {generatedDocuments.Properties.Title}", new { });

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
Also used the following book's summary as a prompt:
[MORE_DALLE_PROMPT]

The image should depict 3 characters from France only and from the era of the novel.
The image should be a painting, not a photograph.

About the image to be generated:
- Level of Detail: High.
/*
- Artistic Style: Impressionism.
- Color Scheme: Vincent Van Gogh color palette.
*/
".RemoveComment(StringComment.C);

                    var useSanitizedSummaryVersion = !string.IsNullOrEmpty(Books[book]); // Avoid our content policy from openAI, about image generation.
                    if (useSanitizedSummaryVersion)
                        imagePrompt = imagePrompt.Replace("[MORE_DALLE_PROMPT]", Books[book]);
                    else
                        imagePrompt = imagePrompt.Replace("[MORE_DALLE_PROMPT]", $@"Also used the following book's summary as a prompt:\r\n{generatedDocument.Summary}\r\n");

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
            OpenAI.Trace($"Done", new { });
        }


        public static void Generate_HtmlWebSite()
        {
            var generatedDocuments = GeneratedDocuments.Load(@".\VictorHugoPresentation\VictorHugo.Documents.json");
        }
    }
}