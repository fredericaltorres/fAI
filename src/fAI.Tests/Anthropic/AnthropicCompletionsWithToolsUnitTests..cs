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

            var f2 = new FunctionCaller()
            {
                Name = "get_calendar_events",
                Type = FunctionCallerType.InProcess,
                Arguments = new Dictionary<string, object>()
                {
                    { "start_date", "2026-04-03T00:00:00Z" },
                    { "end_date", "2026-04-03T23:59:59Z" }
                },
                F1 = (arg1) =>
                {
                    return new
                    {
                        meeting_title = "team status",
                        participants = DS.List("Alice", "Bob", "Charlie")
                    };
                }
            };

            var functionCallers = new FunctionCallers();
            functionCallers.Add(f1.Name, f1);
            functionCallers.Add(f2.Name, f2);
            return functionCallers;
        }


        AnthropicTool GetCalendarTool()
        {
            return new AnthropicTool()
            {
                Name = "get_calendar_events",
                Description = "Retrieves calendar events for the user within a specified time range. Use this weatherTool when the user asks about their schedule, upcoming meetings, appointments, or events on a specific day or period. Returns a list of events with their title, date, time, location, and attendees.",
                InputSchema = new InputSchema()
                {
                    Type = "object",
                    Properties = new Dictionary<string, SchemaProperty>()
                    {
                        { "start_date", new SchemaProperty() { 
                            Type = "string", 
                            Format = "date-time",
                            Description = "The start of the time range to fetch events from, in ISO 8601 format (e.g. '2026-04-02T00:00:00Z'). Defaults to today if not provided." } 
                        },
                        { "end_date", new SchemaProperty() {
                            Type = "string",
                            Format = "date-time",
                            Description = "The end of the time range to fetch events until, in ISO 8601 format (e.g. '2026-04-02T23:59:59Z'). Defaults to end of the current day if not provided." }
                        },
                        { "calendar_id", new SchemaProperty() {
                            Type = "string",
                            Description = "The ID of a specific calendar to query (e.g. 'work', 'personal'). If omitted, events from all calendars are returned." }
                        }
                    },
                    Required = new List<string>() { "start_date", "end_date" }
                }
            };
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

        const string userPrompt = @"What's the weather like in Boston right now?";

        [Fact()]
        [TestBeforeAfter]
        public void Completion_With_Tools__Anthropic()
        {
            var tool = ToolFactory.CreateTool(LLMProvider.Anthropic, GetWeatherTool()) as AnthropicTool;
            var functionCallers = GetFunctionCallersForUnitTests();
            var anthropicClient = new Anthropic();
            var models = DS.List("claude-opus-4-6", "claude-sonnet-4-5", "claude-haiku-4-5");
            models.ForEach(model =>
            {
                var r = anthropicClient.Completions.CreateAgenticLoop(userPrompt, model, tools: DS.List(tool), functionCallers: functionCallers);
                var a = r.Text;
                Assert.Contains("partly cloudy", a.ToLowerInvariant());
            });
        }
        
        [Fact()]
        [TestBeforeAfter]
        public void Completion_With_Tools__Google()
        {
            var functionCallers = GetFunctionCallersForUnitTests();
            var tool = ToolFactory.CreateTool(LLMProvider.Google, GetWeatherTool()) as AnthropicTool;
            var googleAIClient = new GoogleAI();
            var models = DS.List("gemini-3-flash-preview",
                "gemini-2.0-flash",
                "gemini-2.5-pro", 
                "gemini-2.5-flash");

            models.ForEach(model =>
            {
                var r = googleAIClient.Completions.CreateAgenticLoop(userPrompt, model, tools: DS.List(tool), functionCallers: functionCallers);
                var a = r.GetText();
                Assert.Contains("partly cloudy", a.ToLowerInvariant());
            });
        }


        [Fact()]
        [TestBeforeAfter]
        public void Completion_With_Tools__GenericAI()
        {
            var functionCallers = GetFunctionCallersForUnitTests();
            var tool = ToolFactory.CreateTool(LLMProvider.Anthropic, GetWeatherTool()) as AnthropicTool;
            var genericAIClient = new GenericAI();
            var models = DS.List(
                "claude-opus-4-6", "claude-sonnet-4-5", "claude-haiku-4-5",
                "gemini-3-flash-preview", 
                "gemini-2.0-flash", "gemini-2.5-flash", "gemini-2.5-pro" 
            );

            models.ForEach(model =>
            {
                dynamic r = genericAIClient.Completions.CreateAgenticLoop(userPrompt, model, tools: DS.List(tool), functionCallers: functionCallers);
                var a = r.Text;
                Assert.Contains("partly cloudy", a.ToLowerInvariant());
            });
        }

        [Fact()]
        [TestBeforeAfter]
        public void Completion_With_Tools__GenericAI__gemini_2_0_flash()
        {
            var functionCallers = GetFunctionCallersForUnitTests();
            var weatherTool = ToolFactory.CreateTool(LLMProvider.Anthropic, GetWeatherTool()) as AnthropicTool;
            var calendarTool = ToolFactory.CreateTool(LLMProvider.Anthropic, GetCalendarTool()) as AnthropicTool;
            var genericAIClient = new GenericAI();
            var models = DS.List("gemini-2.0-flash");

            var userPrompt = "What are my meetings for tomorrow Friday 2026/04/03?";

            models.ForEach(model =>
            {
                dynamic r = genericAIClient.Completions.CreateAgenticLoop(userPrompt, model, tools: DS.List(weatherTool, calendarTool), functionCallers: functionCallers);
                var a = r.Text;
                Assert.Contains("partly cloudy", a.ToLowerInvariant());
            });
        }
    }
}

