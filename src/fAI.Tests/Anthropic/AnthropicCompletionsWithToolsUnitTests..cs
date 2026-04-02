using DynamicSugar;
using fAI.Google;
using Markdig;
using Mistral.SDK.DTOs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        FunctionCallers GetFunctionCallersForUnitTests()
        {
            var f1 = new FunctionCaller()
            {
                Name = "get_weather",
                Type = FunctionCallerType.InProcess,
                Arguments = new Dictionary<string, object>()
                {
                    { "location", "Boston, MA" },
                    { "unit", "celsius" }
                },
                F1 = (arg1) =>
                {
                    return new
                    {
                        requested_location = arg1,
                        temperature_f = 62,
                        condition = "Partly Cloudy",
                        humidity = "75%",
                        wind = "10 mph NW"
                    };
                }
            };

            var functionCallers = new FunctionCallers();
            functionCallers.Add(f1.Name, f1);
            return functionCallers;
        }


        AnthropicTool GetWeatherTool()
        {
            return new AnthropicTool()
            {
                Name = "get_weather",
                Description = "Get the current weather in a given p1Requested",
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
            var tool = ToolFactory.CreateTool(LLMProvider.Anthropic, GetWeatherTool()) as AnthropicTool;
            var functionCallers = GetFunctionCallersForUnitTests();

            var userPrompt = @"What's the weather like in Boston right now?";
            var anthropicClient = new Anthropic();
            var r = anthropicClient.Completions.CreateAgenticLoop(userPrompt, tools: DS.List(tool),  functionCallers: functionCallers);
        }
        
        [Fact()]
        [TestBeforeAfter]
        public void Completion_With_Tools__Google()
        {
            var functionCallers = GetFunctionCallersForUnitTests();
            var userPrompt = @"What's the weather like in Boston right now?";
            var systemPrompt = "";
            var model = "gemini-3-flash-preview";

            var tool = ToolFactory.CreateTool(LLMProvider.Google, GetWeatherTool()) as GoogleTool;
            var sw = Stopwatch.StartNew();
            var googleAIClient = new GoogleAI();
            
            var prompt = googleAIClient.Completions.GetPrompt(userPrompt, systemPrompt, model);
            var url = googleAIClient.Completions.GetUrl(model);

            var r = googleAIClient.Completions.CreateAgenticLoop(prompt, url, model, tools: DS.List(tool), functionCallers: functionCallers);
            var a = r.GetText();
       
            Assert.Contains("partly cloudy", a.ToLowerInvariant());
        }
    }
}

