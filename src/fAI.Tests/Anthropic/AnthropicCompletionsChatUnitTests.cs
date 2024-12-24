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
            Assert.True(
                response.Text.Contains("The image shows a quiz question") ||
                response.Text.Contains("The image shows an online quiz")  ||
                response.Text.Contains("The image shows a screenshot of an online quiz")
            );

            Assert.Contains("Secure Coding for .NET - Data Protection", response.Text);
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
                        new AnthropicContentText("Answer the question in the image.")
                    )
                )
            };

            var response = new Anthropic().Completions.Create(prompt);
            Assert.True(response.Success);
            Assert.Contains(@"The correct answer to the question ""Use a controlled mechanism like Azure Key Vault to store secrets in the production environment", response.Text);
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
    }
}

