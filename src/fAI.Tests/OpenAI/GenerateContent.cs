using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using fAI;
using Xunit;
using static fAI.OpenAICompletions;
using System.Runtime.InteropServices;
using static fAI.OpenAIImage;
using System.ComponentModel;

namespace fAI.Tests
{
    public class GeneratedDocument 
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public string LocalImage { get; set; }
    }

    public class GeneratedDocuments : List<GeneratedDocument>
    {
        public GeneratedDocument Add(string title)
        {
            var d = new GeneratedDocument { Title = title };
            this.Add(d);
            return d;
        }

        public string ToJson()
        {
            return fAI.JsonUtils.ToJSON(this);
        }

        public void Save(string fileName)
        {
            File.WriteAllText(fileName, ToJson());
        }
    }

    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class GenerateContentTests
    {
        public List<string> Books = new List<string>()
        {
            "Les Misérables",
            "The Hunchback of Notre-Dame",
            "Ninety-Three",
            //"The Man Who Laughs",
            //"The Toilers of the Sea",
            //"The Last Day of a Condemned Man",
            //"Bug-Jargal",
            //"Hans of Iceland",
            //"Les Châtiments",
            //"Les Contemplations",
        };

        [Fact()]
        public void Generate_Document()
        {
            var generatedDocuments = new GeneratedDocuments();

            var client = new OpenAI();
            foreach (var book in Books)
            {
                var generatedDocument = generatedDocuments.Add(book);

                var prompt = new Prompt_GPT_4
                {
                    Messages = new List<GPTMessage> {
                        new GPTMessage { Role =  MessageRole.system, Content = "As French literature expert."},
                        new GPTMessage { Role =  MessageRole.user  , Content = $@"Summarize in 10 lines the book from French author Victor Hugo, ""{book}""." }
                    }
                };
                var response = client.Completions.Create(prompt);
                if(response.Success)
                {
                    generatedDocument.Summary = response.Text;
                    var imagePrompt = $@"
As French literature expert and people expert.
Generate an image inspired by Victor Hugo's classic novel, '{book}'.
The image should depict three typical characters, each with distinct characteristics. 
            ";
                    var r = client.Image.Generate(imagePrompt, size: OpenAIImageSize._1792x1024);

                    generatedDocument.LocalImage = r.DownloadImageLocally()[0];
                }
            }
            generatedDocuments.Save(@"c:\a\VictorHugo.Documents.json");
        }
    }
}