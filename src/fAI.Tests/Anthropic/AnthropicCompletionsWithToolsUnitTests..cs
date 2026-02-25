using DynamicSugar;
using Markdig;
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
            Assert.Equal(AnthropicLib.StopReason.tool_use, response.StopReason);
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
    }
}

