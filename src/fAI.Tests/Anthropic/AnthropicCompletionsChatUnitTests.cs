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
            DS.Assert.Words(response.Text, "True");
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
            Assert.Contains(@"Database Error Page", response.Text);
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
            DS.Assert.Words(response.Text, "(sea & waves) | ( stormy & shipwreck)");
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

