using System.IO;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using DynamicSugar;
using System.Text.Json.Serialization;
using System.Runtime.InteropServices;

namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public partial class AnthropicCompletionsChatUnitTests : OpenAIUnitTestsBase
    {
        public AnthropicCompletionsChatUnitTests()
        {
        }

        [Fact()]
        [TestBeforeAfter]
        public void Nusbio_Led_Control()
        {
            var p = new Anthropic_Prompt_Claude_4_6_Sonnet()
            {
                System = null,
                Messages = new List<AnthropicMessage>()
                {
                    new AnthropicMessage { Role =  MessageRole.user,
                         Content = DS.List<AnthropicContentMessage>(new AnthropicContentText(@"
You are controlling a electronic device displaying 9 LED named:
 LED_0, LED_1, LED_2, LED_3, LED_4, LED_5,LED_6, LED_7.

All output should be in JSON, using a main object containing a ""sequences"" property containing an array of the following properties:
""comment"" which is a string, ""actions"" is an array of object containing the properties ""command"" and ""value"".

To turn on LED LED_0, you must output: LED_0 ON.
To turn off LED LED_0, you must output: LED_0 OFF.
To wait 1 second, you must output: WAIT 1.

For each character of the word ""HELLO""
    Write a sequence which display the character ASCII in binary
    Wait 1 second
"))
                    }
                }
            };

            var response = new Anthropic().Completions.Create(p);
            Assert.True(response.Success);
            var text = response.Text;
            var asJson = response.AsJson;
            var sequence = response.Deserialize<LedSequence>();

            Assert.Equal(5, sequence.Sequences.Count);
            foreach(var s in sequence.Sequences)
            {
                Assert.Equal(9, s.Actions.Count);
                Assert.Single(s.Actions.Where(a => a.Command == "WAIT").ToList());
                Assert.Equal(8, s.Actions.Where(a => a.Command.StartsWith("LED_")).ToList().Count);
                Assert.True(!string.IsNullOrEmpty(s.Comment));
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void Completion_Summarize()
        {
            var p = new Anthropic_Prompt_Claude_4_6_Sonnet()
            {
                System = @"
Your task is to review the provided meeting notes and create a concise summary that captures the essential information, focusing on key takeaways and action items assigned to specific individuals or departments during the meeting. 
Use clear and professional language, 
and organize the summary in a logical manner using appropriate formatting such as headings, subheadings, and bullet points. 
Ensure that the summary is easy to understand and provides a comprehensive but succinct overview of the meeting’s content, 
with a particular focus on clearly indicating who is responsible for each action item.",
                
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
        public void AnalyseTextAndExtractCodeInformation_Airport_Code_Analyst()
        {
            var p = new Anthropic_Prompt_Claude_4_6_Sonnet()
            {
                System = @"Your task is to analyze the provided Text and identify 
                city airport codes and 
                city time zones mentioned within it. 

                Present these airport codes, names and time zone names as a JSON array in the order they appear in the Text.
                Output the data in JSON using the property names: (airportCode, cityName, timeZone). 
                If no airport codes are found, return an empty array.",
                Messages = new List<AnthropicMessage>()
                {
                    new AnthropicMessage { Role =  MessageRole.user,
                         Content = DS.List<AnthropicContentMessage>(new AnthropicContentText(
                             @"My next trip involves flying from Seattle to Amsterdam.
                               I’ll be spending a few days in Amsterdam before heading to Paris for a connecting flight to Rome and finaly by train to marseille."))
                    }
                }
            };

            var response = new Anthropic().Completions.Create(p);
            Assert.True(response.Success);
            var airportCode = response.JsonArray[0]["airportCode"].ToString();
            Assert.Equal("SEA", airportCode);

            var cityName = response.JsonArray[0]["cityName"].ToString();
            Assert.Equal("Seattle", cityName);

            var timeZoneName = response.JsonArray[0]["timeZone"].ToString();
            Assert.Equal("America/Los_Angeles", timeZoneName);
        }

        [Fact()]
        [TestBeforeAfter]
        public void Completion_JsonMode_WorldCup()
        {
            var p = new Anthropic_Prompt_Claude_4_6_Sonnet()
            {
                Messages = new List<AnthropicMessage>()
                {
                    new AnthropicMessage { Role =  MessageRole.user,
                         Content = DS.List<AnthropicContentMessage>(new AnthropicContentText(
                             @"Who won the soccer world cup in 1998?
                               Text the question in JSON only using a property 'winner'."))
                    }
                }
            };

            var response = new Anthropic().Completions.Create(p);
            Assert.True(response.Success);
            var answer = response.JsonObject["winner"].ToString();
            Assert.Equal("France", answer);
        }

        [Fact()]
        [TestBeforeAfter]
        public void Completion_JsonMode_WhatIsLatinForAnt()
        {
            var p = new Anthropic_Prompt_Claude_4_6_Sonnet()
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
            var answer = response.JsonObject["answer"].ToString();
            Assert.Equal("C", answer);
        }
        

        [Fact()]
        [TestBeforeAfter]
        public void Completion_UploadImage_WhatIsInImage()
        {
            var imageFileName = base.GetTestFile("code.question.1.jpg");
            Assert.True(File.Exists(imageFileName));

            var prompt = new Anthropic_Prompt_Claude_4_6_Sonnet()
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
            var prompt = new Anthropic_Prompt_Claude_4_6_Sonnet()
            {
                Messages = new AnthropicMessages(
                    new AnthropicMessage(
                        MessageRole.user,
                        new AnthropicContentImage(imageFileName),
                        new AnthropicContentText("Text the question in the image with True or False only.")
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

            var prompt = new Anthropic_Prompt_Claude_4_6_Sonnet()
            {
                Messages = new AnthropicMessages(
                    new AnthropicMessage(
                        MessageRole.user,
                        new AnthropicContentImage(imageFileName),
                        new AnthropicContentText("Text the question in the image.")
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

            var prompt = new Anthropic_Prompt_Claude_4_6_Sonnet()
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

            var prompt = new Anthropic_Prompt_Claude_4_6_Sonnet()
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



        [Fact()]
        [TestBeforeAfter]
        public void Completion_With_Tools()
        {
            var tool = new AnthropicTool()
            {
                Name = "get_weather",
                Description = "Get the current weather in a given location",
                InputSchema = new InputSchema()
                {
                    Properties = new Dictionary<string, SchemaProperty>()
                    {
                        { "location", new SchemaProperty() { Type = "string", Description = "The city and state, e.g. San Francisco, CA" } },
                        { "unit", new SchemaProperty() { Type = "string", Description = "The unit of temperature to return, either 'celsius' or 'fahrenheit'" } }
                    },
                    Required = new List<string>() { "location" }
                }
            };

            var p = new Anthropic_Prompt_Claude_4_6_Sonnet()
            {
                Messages = new List<AnthropicMessage>()
                {
                    new AnthropicMessage { Role =  MessageRole.user,
                         Content = DS.List<AnthropicContentMessage>(new AnthropicContentText(@"What's the weather like in Boston right now?"))
                    }
                },
                Tools = new List<AnthropicTool>() { tool }
            };

            var response = new Anthropic().Completions.Create(p);
            Assert.True(response.Success);
            Assert.True(response.IsToolUse);
            var toolContent = response.Content.FindToolUse();
            Assert.True(toolContent.IsToolUse);
            Assert.Equal("get_weather", toolContent.Name);
            Assert.Equal("Boston, MA", toolContent.Input["location"]);
            //Assert.Equal("fahrenheit", toolContent.Input["unit"]);

            var answer = response.Text;
        }
    }
}

