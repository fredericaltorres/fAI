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
    public class AuthorData
    {
        public string Name;
        public string Title;
        public string Description;
        public string Country;

        public Dictionary<string, string> Books = new Dictionary<string, string>();
    }

    public class VictorHugo: AuthorData
    {
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

        public VictorHugo()
        {
            this.Name = "Victor Hugo";
            this.Title = $"{this.Name}'s MasterPieces";
            this.Description = $"Generate a summary of {this.Name}'s MasterPieces and an image inspired by the book.";
            this.Country = "France";

            Books = new Dictionary<string, string>()
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
        }
    }

    public class FyodorDostoevsky : AuthorData
    {
        public FyodorDostoevsky()
        {
            this.Name = "Fyodor Dostoevsky";
            this.Title = $"{this.Name}'s MasterPieces";
            this.Description = $"Generate a summary of {this.Name}'s MasterPieces and an image inspired by the book.";
            this.Country = "Russia";

            Books = new Dictionary<string, string>()
            {
                ["Crime and Punishment"]    = "",
                ["The Brothers Karamazov"]  = "",
                ["The Idiot"]               = @"Imagine a bustling street in Moscow, Russia, in the year 1880. A man named the muffin-man, with a good heart is walking in main street. The scene is vibrant with the daily life of the era. In the foreground, a horse-drawn carriage is passing by,while pedestrians in period attire - men in long coats and women in flowing dresses - walk along the cobblestone streets. The architecture reflects the historic Russian style, with onion domes visible in the distance. The muffin-man meet 2 women in the street. The muffin-man is viewed by others as an punk. The muffin-man's compassion lead him into a series of sad circumstances. Delves into themes of love and the struggle between good and evil.",
                ["Notes from Underground"]  = "",
                ["The Possessed"]           = "",
                ["The Gambler"]             = "",
                ["White Nights"]            = "",
                ["Poor Folk"]               = "",
                ["The Eternal Husband"]     = @"As Russia literature expert and people expert. Render an image based on the following: Imagine 2 man and one woman in the street in Moscow, Russia, in the year 1880. Explores the relationship between two men man1 and man2, who are connected by a woman1. man1 is married to woman1. woman1 was the former lover of man2. Delves into themes of guilt, jealousy, and the complexities of love and marriage.",
                ["The House of the Dead"]   = "",
            };
        }   
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            //Generate_Document(VictorHugoBooks, VictorHugoName, VictorHugoTitle, VictorHugoDescription);
           //Generate_Document(new FyodorDostoevsky());
            Generate_HtmlWebSite();
        }

        public static void Generate_Document(AuthorData a)
        {
            var generatedDocuments = new GeneratedDocuments();
            generatedDocuments.Properties.Title = a.Title;
            generatedDocuments.Properties.Description = a.Description;
            OpenAI.TraceOn = true;
            OpenAI.Trace($"Generating document about {generatedDocuments.Properties.Title}", new { });

            var client = new OpenAI();
            foreach (var book in a.Books.Keys)
            {
                Console.WriteLine($"Generating document about {book}");
                var generatedDocument = generatedDocuments.Add(book);
                var prompt = new Prompt_GPT_4
                {
                    Messages = new List<GPTMessage> {
                        new GPTMessage { Role =  MessageRole.system, Content = "As literature expert."},
                        new GPTMessage { Role =  MessageRole.user  , Content =
                            $@"Summarize in 10 lines the book ""{book}"" from author ""{a.Name}""."
                        }
                    }
                };
                generatedDocument._summaryPrompt = prompt.GetPromptString(); // Save the prompt for debugging purposes.
                var response = client.Completions.Create(prompt);
                if (response.Success)
                {
                    generatedDocument.Summary = response.Text;
                    var imagePrompt = $@"
As {a.Country} literature expert and people expert.
Generate an image inspired by {a.Name}'s classic novel, ""{book}"".
[MORE_DALLE_PROMPT]

The image should depict 3 characters from {a.Country} only and from the era of the novel.
The image should be a painting, not a photograph.
".RemoveComment(StringComment.C).Trim();

                    var useSanitizedSummaryVersion = !string.IsNullOrEmpty(a.Books[book]); // Avoid our content policy from openAI, about image generation.
                    if (useSanitizedSummaryVersion)
                        imagePrompt = imagePrompt.Replace("[MORE_DALLE_PROMPT]", a.Books[book]);
                    else
                        imagePrompt = imagePrompt.Replace("[MORE_DALLE_PROMPT]", $@"Also used the following book's summary as a prompt:{Environment.NewLine}{generatedDocument.Summary}{Environment.NewLine}");

                    var promptLength = imagePrompt.Length;
                    generatedDocument._imagePrompt = imagePrompt; // Save the prompt for debugging purposes.

                    var r = client.Image.Generate(imagePrompt, size: ImageSize._1024x1024);
                    if (r.Success)
                    {
                        generatedDocument.LocalImage = r.DownloadImages()[0];
                    }
                }
            }
            generatedDocuments.Save($@"c:\temp\{a.Name}.Documents.json");
            OpenAI.Trace($"Done", new { });
        }


        public static void Generate_HtmlWebSite()
        {
            var jsonInput = @".\VictorHugoPresentation\VictorHugo.Documents.json";
            var htmlOutput = @".\VictorHugoPresentation\VictorHugo.Documents.3.html";

            jsonInput = @".\Fyodor Dostoevsky\Fyodor Dostoevsky.Documents.json";
            htmlOutput = @".\Fyodor Dostoevsky\Fyodor Dostoevsky.Documents.3.html";

            var generatedDocuments = GeneratedDocuments.Load(jsonInput);
            var templateGenerator = new StaticHtmlTemplateGenerator();
            templateGenerator.GenerateFile(generatedDocuments, htmlOutput, OpenAISpeech.Voices.onyx);
        }
    }
}



/*
The text title ""{book}"" is located at the bottom of the image.
About the image to be generated:
- Level of Detail: High.

- Artistic Style: Impressionism.
- Color Scheme: Vincent Van Gogh color palette.
*/