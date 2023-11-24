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
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class GenerateContentTests
    {
        // Victor Hugo's MasterPieces
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
Also used the following book's summary as a prompt:
{generatedDocument.Summary}

The image should depict 3 characters related to the location of the book, Paris, France.
The image should be a painting, not a photograph.

About the image to be generated:
- Artistic Style: Impressionism.
- Color Scheme: Vincent Van Gogh color palette.
- Level of Detail: High.
            ";
                    var r = client.Image.Generate(imagePrompt, size: OpenAIImageSize._1024x1024);

                    generatedDocument.LocalImage = r.DownloadImageLocally()[0];
                }
            }
            generatedDocuments.Save(@"c:\a\VictorHugo.Documents.json");
        }
    }
}