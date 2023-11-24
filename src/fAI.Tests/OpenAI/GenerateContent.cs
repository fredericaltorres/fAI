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
using fAI.Util.Strings;

namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class GenerateContentTests
    {
        // Victor Hugo's MasterPieces
        public Dictionary<string, string> Books = new Dictionary<string, string>()
        {
            ["Les Misérables"] = "" ,
            ["The Hunchback of Notre-Dame"] = "\"The Hunchback of Notre-Dame\" is a tragic tale set in 15th century Paris",
            ["Ninety-Three"] = "",
            ["The Man Who Laughs"] = "",
            ["The Toilers of the Sea"] = "",
            ["The Last Day of a Condemned Man"] = "",
            ["Bug-Jargal"] = "",
            ["Hans of Iceland"] = "",
            ["Les Châtiments"] = "",
            ["Les Contemplations"] = "",
        };

        // https://platform.openai.com/tokenizer
        [Fact()]
        public void Generate_Document()
        {
            var generatedDocuments = new GeneratedDocuments();

            var client = new OpenAI();
            foreach (var book in Books.Keys)
            {
                var generatedDocument = generatedDocuments.Add(book);

                var prompt = new Prompt_GPT_4
                {
                    Messages = new List<GPTMessage> {
                        new GPTMessage { Role =  MessageRole.system, Content = "As French literature expert."},
                        new GPTMessage { Role =  MessageRole.user  , Content = StringUtil.RemoveMultiLineComment(
                               $@"Summarize in 10 lines the book from French author Victor Hugo, ""{book}"".") 
                        }
                    }
                };
                generatedDocument._summaryPrompt = prompt.GetPromptString(); // Save the prompt for debugging purposes.

                var response = client.Completions.Create(prompt);

                if(response.Success)
                {
                    generatedDocument.Summary = response.Text;
                    var imagePrompt = StringUtil.RemoveMultiLineComment($@"
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
- Level of Detail: High.");

                    // Avoid our content policy from openAI, about image generation.
                    var useBookSummaryInDallePrompt = !string.IsNullOrEmpty(Books[book]);
                    if(useBookSummaryInDallePrompt)
                        imagePrompt = imagePrompt.Replace("[MORE_DALLE_PROMPT]", $@"Also used the following book's summary as a prompt:\r\n{generatedDocument.Summary}\r\n");
                    else
                        imagePrompt = imagePrompt.Replace("[MORE_DALLE_PROMPT]", Books[book]);

                    var promptLength = imagePrompt.Length;
                    generatedDocument._imagePrompt = imagePrompt; // Save the prompt for debugging purposes.

                    var r = client.Image.Generate(imagePrompt, size: OpenAIImageSize._1024x1024);
                    if(r.Success)
                    {
                        generatedDocument.LocalImage = r.DownloadImageLocally()[0];
                    }
                }
            }
            generatedDocuments.Save(@"c:\a\VictorHugo.Documents.json");
        }
    }
}