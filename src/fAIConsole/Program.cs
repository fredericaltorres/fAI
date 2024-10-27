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
using static DynamicSugar.DS;
using System.Threading;
using fAIConsole.RAG;
using System.Runtime.Remoting.Contexts;
using HtmlAgilityPack;

namespace fAIConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Main_GenerateMultiChoiceQuestionAboutKingsOfFrance(args);
            //Main_GenerateMultiChoiceQuestionAboutText(@"");

            return;

            var people = RandomPeople.FromFile(@".\rag\random_people_data.json");
            var peopleData = RandomPeople.GenerateTextDataForPrompt(people);
            var question = "I am looking for somebody that can repair a car in Florida.";

            var client = new OpenAI();
            while (true)
            {
                Console.Write($"Question: ");
                question = Console.ReadLine();
                if(question == "exit")
                    break;
                var answer = client.CompletionsEx.AnswerQuestionBasedOnText(peopleData, question);
                Console.WriteLine($"Answer: {answer}");
            }
        }

        const string KingOfFrances = @"
            ""Hugh Capet"" was king of France from 987 to 996.
            ""Robert II"" was king of France from 996 to 1031.
            ""Henry I"" was king of France from 1031 to 1060.
            ""Philip I"" was king of France from 1060 to 1108.
            ""Louis VI"" was king of France from 1108 to 1137.
            ""Louis VII"" was king of France from 1137 to 1180.
            ""Philip II"" was king of France from 1180 to 1223.
            ""Louis VIII"" was king of France from 1223 to 1226.
            ""Louis IX (Saint Louis)"" was king of France from 1226 to 1270.
            ""Philip III"" was king of France from 1270 to 1285.
            ""Philip IV"" was king of France from 1285 to 1314.
            ""Louis X"" was king of France from 1314 to 1316.
            ""John I"" was king of France from 1316 to 1316.
            ""Philip V"" was king of France from 1316 to 1322.
            ""Charles IV"" was king of France from 1322 to 1328.
            ""Philip VI (Valois Branch)"" was king of France from 1328 to 1350.
            ""John II"" was king of France from 1350 to 1364.
            ""Charles V"" was king of France from 1364 to 1380.
            ""Charles VI"" was king of France from 1380 to 1422.
            ""Charles VII"" was king of France from 1422 to 1461.
            ""Louis XI"" was king of France from 1461 to 1483.
            ""Charles VIII"" was king of France from 1483 to 1498.
            ""Louis XII"" was king of France from 1498 to 1515.
            ""Francis I"" was king of France from 1515 to 1547.
            ""Henry II"" was king of France from 1547 to 1559.
            ""Francis II"" was king of France from 1559 to 1560.
            ""Charles IX"" was king of France from 1560 to 1574.
            ""Henry III"" was king of France from 1574 to 1589.
            ""Henry IV (Bourbon Branch)"" was king of France from 1589 to 1610.
            ""Louis XIII"" was king of France from 1610 to 1643.
            ""Louis XIV"" was king of France from 1643 to 1715.
            ""Louis XV"" was king of France from 1715 to 1774.
            ""Louis XVI"" was king of France from 1774 to 1792.
        ";

        static void Main_HowManyYearsWasLouisXIVKing(string[] args)
        {
            var question = @"How many years was ""Louis XIV"" king of France?";

            var client = new OpenAI();
            var answer = client.CompletionsEx.AnswerQuestionBasedOnText(KingOfFrances, question);
            Console.WriteLine($"Question: {question}");
            Console.WriteLine($"Answer: {answer}");
            Console.ReadLine();
        }

        static string GetNodePath(HtmlNode node)
        {
            return node.ParentNode == null ? node.Name : GetNodePath(node.ParentNode) + "\\" + node.Name;
        }

        static string ExtractNode(HtmlNode node)
        {
            if (node .ParentNode.Name == "a") // No links
                return "";

            var nodePath = GetNodePath(node); // No header or footer
            if (nodePath.Contains(@"\footer\")|| nodePath.Contains(@"\header\"))
                return "";

            return Environment.NewLine + $"[{GetNodePath(node)}] " + node.InnerText.Trim() + Environment.NewLine ; // + "`.` "
        }

        static void Main_GenerateMultiChoiceQuestionAboutText(string text2)
        {
            var html = @"https://www.bigtincan.com/content/";
            var web = new HtmlWeb();
            var doc = web.Load(html);

            // Remove script and style elements
            var nodestoRemove = doc.DocumentNode.SelectNodes("//script|//style");
            if (nodestoRemove != null)
            {
                foreach (var node in nodestoRemove)
                {
                    node.Remove();
                }
            }

            // Get text content
            var text3 = doc.DocumentNode
                .SelectNodes("//text()[normalize-space()]")
                .Select(node => ExtractNode(node))
                .Where(text => !string.IsNullOrWhiteSpace(text))
                //.Select(text => text.Replace("\t", " ").Replace("\n", " ").Replace("\r", " "))
                .Select(text => System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " "))
                .Aggregate((current, next) => current + $"{Environment.NewLine} " + next);

            text3 = text3.Replace("`.` ", $@".{Environment.NewLine}");
            //text3 = text3.Replace($@".{Environment.NewLine}"+ $@".{Environment.NewLine}", $@".{Environment.NewLine}");
            //text3 = text3.Replace("..", $@".");
            Console.WriteLine(text3);

            //var node = htmlDoc.DocumentNode.SelectSingleNode("//head/title");
            //Console.WriteLine("Node Name: " + node.Name + "\n" + node.InnerText);

            //var bodyNode = htmlDoc.DocumentNode.SelectSingleNode("//body");
            //Console.WriteLine("Node Name: " + node.Name + "\n" + bodyNode.InnerText);
            //Console.WriteLine("Node Name: " + node.Name + "\n" + bodyNode.InnerHtml);
        }


        static void Main_GenerateMultiChoiceQuestionAboutKingsOfFrance(string[] args)
        {
            var questionCount = 3;
            var dbFact = new DBFact();
            dbFact.AddFacts(KingOfFrances, randomizeOrder: true);

            var client = new OpenAI();
            var questions = client.CompletionsEx.GenerateMultiChoiceQuestionAboutText(questionCount, dbFact.GetText());
            var questionIndex = 0;
            foreach (var q in questions)
            {
                Console.WriteLine($"{questionIndex+1} - {q.Text}");
                var answerIndex = 0;
                foreach (var answer in q.Answers)
                {
                    Console.WriteLine($"{answerIndex+1} - {answer} - rightAnswer: {q.CorrectAnswerIndex == answerIndex}");
                    answerIndex++;
                }
                questionIndex++;
            }
            Console.WriteLine();
            Console.ReadLine();
        }


        static void Main_AskQuestion_WhoWasKingOfFrance(string[] args)
        {
            var client = new OpenAI();
            var question = "Who was king of france in 1032?";
            var answer = client.CompletionsEx.AnswerQuestionBasedOnText(KingOfFrances, question);
            Console.WriteLine($"Question: {question}");
            Console.WriteLine($"Answer: {answer}"); // Henry I
            PlayAnswerWhoWasKingOfFrance(client, question, answer, client.CompletionsEx.AnswerNotFoundDefault);

            question = "Who was king of france in 1812?";
            answer = client.CompletionsEx.AnswerQuestionBasedOnText(KingOfFrances, question);
            Console.WriteLine($"Question: {question}");
            Console.WriteLine($"Answer: {answer}");
            PlayAnswerWhoWasKingOfFrance(client, question, answer, client.CompletionsEx.AnswerNotFoundDefault);

            Console.ReadLine();
        }

        //Generate_Document(VictorHugoBooks, VictorHugoName, VictorHugoTitle, VictorHugoDescription);
        //Generate_Document(new FyodorDostoevsky());
        // Generate_HtmlWebSite();

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


        public class AuthorData
        {
            public string Name;
            public string Title;
            public string Description;
            public string Country;

            public Dictionary<string, string> Books = new Dictionary<string, string>();
        }

        public class VictorHugo : AuthorData
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
                    ["Crime and Punishment"] = "",
                    ["The Brothers Karamazov"] = "",
                    ["The Idiot"] = @"Imagine a bustling street in Moscow, Russia, in the year 1880. A man named the muffin-man, with a good heart is walking in main street. The scene is vibrant with the daily life of the era. In the foreground, a horse-drawn carriage is passing by,while pedestrians in period attire - men in long coats and women in flowing dresses - walk along the cobblestone streets. The architecture reflects the historic Russian style, with onion domes visible in the distance. The muffin-man meet 2 women in the street. The muffin-man is viewed by others as an punk. The muffin-man's compassion lead him into a series of sad circumstances. Delves into themes of love and the struggle between good and evil.",
                    ["Notes from Underground"] = "",
                    ["The Possessed"] = "",
                    ["The Gambler"] = "",
                    ["White Nights"] = "",
                    ["Poor Folk"] = "",
                    ["The Eternal Husband"] = @"As Russia literature expert and people expert. Render an image based on the following: Imagine 2 man and one woman in the street in Moscow, Russia, in the year 1880. Explores the relationship between two men man1 and man2, who are connected by a woman1. man1 is married to woman1. woman1 was the former lover of man2. Delves into themes of guilt, jealousy, and the complexities of love and marriage.",
                    ["The House of the Dead"] = "",
                };
            }
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

        private static void PlayAnswerWhoWasKingOfFrance(OpenAI client, string question, string answer, string notFoundAnswer)
        {
            var text = $@"{answer} {question.Replace("Who", "")}.";
            if (answer == notFoundAnswer)
                text = $@"{notFoundAnswer} to the question: {question}.";
            var mp3FileName = client.Audio.Speech.Create(text, OpenAISpeech.Voices.echo);
            AudioUtil.PlayMp3WithWindowsPlayer(mp3FileName);
            Thread.Sleep(1000 * 4);
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