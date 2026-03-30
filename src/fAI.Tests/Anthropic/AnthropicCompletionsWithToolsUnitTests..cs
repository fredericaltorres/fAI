using DynamicSugar;
using fAI.Google;
using Markdig;
using Mistral.SDK.DTOs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using Xunit;

namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public partial class AnthropicCompletionsWithToolsUnitTests : OpenAIUnitTestsBase
    {
        public AnthropicCompletionsWithToolsUnitTests()
        {
        }

        AnthropicTool GetWeatherTool()
        {
            return new AnthropicTool()
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
        }



        [Fact()]
        [TestBeforeAfter]
        public void Completion_With_Tools__Anthropic()
        {
            var tool = GetWeatherTool();

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

            // CALL STEP 1
            var response = new Anthropic().Completions.Create(p);

            // Return an answer which is to call a tool
            Assert.Equal(AnthropicLib.StopReason.tool_use, response.StopReason);
            Assert.True(response.Success);
            Assert.True(response.IsToolUse);
            var toolContent = response.Content.FindToolUse();
            var toolContent2 = response.Content;
            Assert.True(toolContent.IsToolUse);
            Assert.StartsWith("tool", toolContent.Id);
            Assert.Equal("get_weather", toolContent.Name);
            Assert.Equal("Boston, MA", toolContent.Input["location"]);
            //Assert.Equal("fahrenheit", toolContent.Input["unit"]);

            // Pretend to call the tool and return a result
            var toolResult = toolContent.Name == "get_weather" ? "72 degree Fahrenheit, partly cloudy with a light breeze." : "No data available.";

            var p2 = new Anthropic_Prompt_Claude_4_6_Sonnet()
            {
                Messages = new List<AnthropicMessage>()
                {
                    new AnthropicMessage { Role =  MessageRole.user, Content = DS.List<AnthropicContentMessage>(new AnthropicContentText(@"What's the weather like in Boston right now?")) },
                    new AnthropicMessage { Role =  MessageRole.assistant, Content = toolContent2 },
                    new AnthropicMessage { Role =  MessageRole.user, Content = DS.List<AnthropicContentMessage>( new AnthropicContentMessage() { Type = AnthropicContentMessageType.tool_result, toolUseId = toolContent.Id, Content = toolResult })},
                },
            };

            var response2 = new Anthropic().Completions.Create(p2);
            Assert.Equal(AnthropicLib.StopReason.end_turn, response2.StopReason);
            var markDown = response2.Text;
            var htmlMarkDown = MarkdownManager.ConvertToHtmlFile(markDown, true);
        }

        [Fact()]
        [TestBeforeAfter]
        public void Completion_With_Tools__Google()
        {
            var tool = ToolFactory.CreateTool(LLMProvider.Google, GetWeatherTool()) as GoogleTool;
            var userPrompt = @"What's the weather like in Boston right now?";
            var systemPrompt = "";
            var model = "gemini-3-flash-preview";

            var googleAIClient = new GoogleAI();
            var prompt = googleAIClient.Completions.GetPrompt(userPrompt, systemPrompt, model);
            var url = googleAIClient.Completions.GetUrl(model);
            var agenticLoopOn = true;

            while (agenticLoopOn)
            {
                // CALL STEP 1
                var r = googleAIClient.Completions.Create(prompt, url, model, tools: DS.List(tool));
                if (!r.Success)
                {
                    throw new ApplicationException($"Request failed  {DS.Dictionary(new { r.FinishReason }).Format()} ");
                }
                else if (r.Success && !r.HasFunctionCall)
                {
                    var answer = r.GetText();
                    agenticLoopOn = false;
                    break;
                }
                else if (r.Success && r.HasFunctionCall)
                {
                    var func = r.candidates.First().content.GetFunctionCall();
                    if (func.name == "get_weather") // CALL STEP 2 , LLM ASK for a function call
                    {
                        var location = func.args.Get("location", "");
                        var unit = func.args.Get("unit", "celsius");
                        Assert.Equal("Boston, MA", location);
                        Assert.Equal("celsius", unit);
                        
                        var funcData = new // CALL STEP 3 , Call Function and to get result for LLM 
                        {
                            requested_location = location,
                            temperature_f = 62,
                            condition = "Partly Cloudy",
                            humidity = "75%",
                            wind = "10 mph NW"
                        };

                        // CALL STEP 4 , Call LLN with function result and all conversation history to get final answer
                        prompt.contents.Add(r.candidates.First().content);
                        prompt.contents.Add(new GoogleAICompletions.GoogleAICompletionsResponse.Content
                        {
                            role = "function",
                            parts = new List<GoogleAICompletions.GoogleAICompletionsResponse.Part>() 
                            {
                                new GoogleAICompletions.GoogleAICompletionsResponse.Part() 
                                {
                                    functionResponse = new GoogleAICompletions.GoogleAICompletionsResponse.FunctionResponse() 
                                    {
                                        name = func.name,
                                        response = funcData
                                    }
                                }
                            }
                        });
                    }
                }

            } // agenticLoopOn
        }
    }
}

