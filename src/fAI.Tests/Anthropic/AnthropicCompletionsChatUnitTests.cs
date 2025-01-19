using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using fAI;
using Xunit;
using static fAI.OpenAICompletions;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Newtonsoft.Json;
using DynamicSugar;
using static System.Net.Mime.MediaTypeNames;
using MimeTypes;

namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class AnthropicCompletionsChatUnitTests : OpenAIUnitTestsBase
    {
        public AnthropicCompletionsChatUnitTests()
        {
        }

        [Fact()]
        [TestBeforeAfter]
        public void Completion_Summarize()
        {
            var p = new Anthropic_Prompt_Claude_3_Opus()
            {
                System = @"Your task is to review the provided meeting notes and create a concise summary that captures the essential information, focusing on key takeaways and action items assigned to specific individuals or departments during the meeting. 
Use clear and professional language, and organize the summary in a logical manner using appropriate formatting such as headings, subheadings, and bullet points. 
Ensure that the summary is easy to understand and provides a comprehensive but succinct overview of the meeting’s content, with a particular focus on clearly indicating who is responsible for each action item.",
                
                Messages = new List<AnthropicMessage>()
                {
                    new AnthropicMessage { Role =  MessageRole.user,
                         Content = DS.List<AnthropicContentMessage>(new AnthropicContentText(
@"Meeting notes:

Date: Verona, Italy - Late 16th century

Attendees:
- Lord Capulet (Head of the Capulet family)
- Lord Montague (Head of the Montague family)
- Prince Escalus (Ruler of Verona)
- Friar Laurence (Religious advisor)

Agenda:
1. Address the ongoing feud between the Capulet and Montague families
2. Discuss the secret marriage of Romeo Montague and Juliet Capulet
3. Develop a plan to bring peace to Verona
4. Address the tragic deaths of Romeo and Juliet

Discussion:
- Prince Escalus opened the meeting by expressing his grave concern over the long-standing feud between the Capulet and Montague families. He admonished both Lord Capulet and Lord Montague for the recent violent clashes that have disturbed the peace in Verona’s streets. The Prince warned that further violence would result in severe consequences, including heavy fines and potential exile for the perpetrators.
- Friar Laurence then broached the topic of the between Romeo Montague and Juliet Capulet, which had taken place under his guidance. Lord Capulet and Lord Montague evidently had not known about it, and reacted with anger and disbelief. However, Friar Laurence urged them to consider the profound and tragic love shared by their children and the potential for this love to heal the rift between the families going forward.
- Prince Escalus proposed a formal truce between the Capulet and Montague families. He demanded that both sides lay down their arms and cease all hostile actions against one another. The Prince declared that any violation of the truce would result in severe punishments, including the possibility of exile or even execution. Lord Capulet and Lord Montague, recognizing the wisdom in the Prince’s words and the necessity of peace for the well-being of their families and the city, grudgingly agreed to the terms of the truce.
- The meeting took a somber turn as the tragic deaths of Romeo and Juliet were addressed. Friar Laurence recounted the unfortunate series of events that led to the young lovers taking their own lives, emphasizing the devastating impact of the families’ hatred on their innocent children. Lord Capulet and Lord Montague, overcome with grief and remorse, acknowledged that their blind hatred had ultimately caused the loss of their beloved children.
- Prince Escalus called upon the families to learn from this heartbreaking tragedy and to embrace forgiveness and unity in honor of Romeo and Juliet’s memory. He urged them to work together to create a lasting peace in Verona, setting aside their long-standing animosity. Friar Laurence offered his support in mediating any future disputes and providing spiritual guidance to help the families heal and move forward.
- As the meeting drew to a close, Lord Capulet and Lord Montague pledged to put an end to their feud and work towards reconciliation. Prince Escalus reaffirmed his commitment to ensuring that the truce would be upheld, promising swift justice for any who dared to break it.
- The attendees agreed to meet regularly to discuss the progress of their reconciliation efforts and to address any challenges that may arise."))
                    }
                }
            };

            var response = new Anthropic().Completions.Create(p);
            Assert.True(response.Success);
            var answer = response.Text;
            //Assert.Equal("France", answer);
        }

        [Fact()]
        [TestBeforeAfter]
        public void Completion_JsonMode_WorldCup()
        {
            var p = new Anthropic_Prompt_Claude_3_Opus()
            {
                Messages = new List<AnthropicMessage>()
                {
                    new AnthropicMessage { Role =  MessageRole.user,
                         Content = DS.List<AnthropicContentMessage>(new AnthropicContentText(
                             @"Who won the soccer world cup in 1998?
                               Answer the question in JSON only using a property 'winner'."))
                    }
                }
            };

            var response = new Anthropic().Completions.Create(p);
            Assert.True(response.Success);
            var answer = response.JsonObject["winner"];
            Assert.Equal("France", answer);
        }

        [Fact()]
        [TestBeforeAfter]
        public void Completion_JsonMode_WhatIsLatinForAnt()
        {
            var p = new Anthropic_Prompt_Claude_3_Opus()
            {
                Messages = new List<AnthropicMessage>()
                {
                    new AnthropicMessage { Role =  MessageRole.user,
                         Content = DS.List<AnthropicContentMessage>(new AnthropicContentText(
                             @"What is latin for Ant? (A) Apoidea, (B) Rhopalocera, (C) Formicidae? Please respond with just the letter of the correct answer in JSON format."))
                    }
                }
            };

            var response = new Anthropic().Completions.Create(p);
            Assert.True(response.Success);
            var answer = response.JsonObject["answer"];
            Assert.Equal("C", answer);
        }

        [Fact()]
        [TestBeforeAfter]
        public void Completion_UploadImage_WhatIsInImage()
        {
            var imageFileName = base.GetTestFile("code.question.1.jpg");
            Assert.True(File.Exists(imageFileName));

            var prompt = new Anthropic_Image_Prompt_Claude_3_Opus()
            {
                Messages = new AnthropicMessages(
                    new AnthropicMessage(
                        MessageRole.user,
                        new AnthropicContentImage(imageFileName),
                        new AnthropicContentText("What's in this image?")
                    )
                )
            };

            var response = new Anthropic().Completions.Create(prompt);
            Assert.True(response.Success);
            DS.Assert.Words(response.Text, "image & knowledge & .NET & Key & Vault & environment");
        }

        [Fact()]
        [TestBeforeAfter]
        public void Completion_UploadImage_AskToAnswerTheQuestion01InImage()
        {
            var imageFileName = base.GetTestFile("code.question.1.jpg");
            Assert.True(File.Exists(imageFileName));

            var prompt = new Anthropic_Image_Prompt_Claude_3_Opus()
            {
                Messages = new AnthropicMessages(
                    new AnthropicMessage(
                        MessageRole.user,
                        new AnthropicContentImage(imageFileName),
                        new AnthropicContentText("Answer the question in the image with True or False only.")
                    )
                )
            };

            var response = new Anthropic().Completions.Create(prompt);
            Assert.True(response.Success);
            DS.Assert.Words(response.Text.ToLower(), "true");
        }

        [Fact()]
        [TestBeforeAfter]
        public void Completion_UploadImage_AskToAnswerTheQuestion02InImage()
        {
            var imageFileName = base.GetTestFile("code.question.2.jpg");
            Assert.True(File.Exists(imageFileName));

            var prompt = new Anthropic_Image_Prompt_Claude_3_Opus()
            {
                Messages = new AnthropicMessages(
                    new AnthropicMessage(
                        MessageRole.user,
                        new AnthropicContentImage(imageFileName),
                        new AnthropicContentText("Answer the question in the image.")
                    )
                )
            };

            var response = new Anthropic().Completions.Create(prompt);
            Assert.True(response.Success);
            Assert.Contains(@"Developer Exception", response.Text);
            // ISSUE WITH AI Assert.Contains(@"Database Error Page", response.Text);
        }


        // C:\DVT\fAI\src\fAIConsole\VictorHugoPresentation\images

        [Fact()]
        [TestBeforeAfter]
        public void Completion_UploadImage_AskToDescribeImage()
        {
            var imageFileName = base.GetTestFile("ManAndBoartInStorm.png");
            Assert.True(File.Exists(imageFileName));

            var prompt = new Anthropic_Image_Prompt_Claude_3_Opus()
            {
                Messages = new AnthropicMessages(
                    new AnthropicMessage(
                        MessageRole.user,
                        new AnthropicContentImage(imageFileName),
                        new AnthropicContentText("Describe image.")
                    )
                )
            };

            var response = new Anthropic().Completions.Create(prompt);
            Assert.True(response.Success);
            //DS.Assert.Words(response.Text, "(sea & waves) | ( stormy & shipwreck)");
        }

        [Fact()]
        [TestBeforeAfter]
        public void Completion_UploadImage_WroteShortStoryBasedOnImage()
        {
            var imageFileName = base.GetTestFile("ManAndBoartInStorm.png");
            Assert.True(File.Exists(imageFileName));

            var prompt = new Anthropic_Image_Prompt_Claude_3_Opus()
            {
                Messages = new AnthropicMessages(
                    new AnthropicMessage(
                        MessageRole.user,
                        new AnthropicContentImage(imageFileName),
                        new AnthropicContentText("Write a short story based on the image.")
                    )
                )
            };

            var response = new Anthropic().Completions.Create(prompt);
            Assert.True(response.Success);
            //DS.Assert.Words(response.Text, "sea & waves & sailor");
        }
    }
}

