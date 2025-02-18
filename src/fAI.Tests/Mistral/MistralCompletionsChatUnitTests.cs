using System.IO;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using DynamicSugar;
using System.Text.Json.Serialization;

namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public partial class MistralCompletionsChatUnitTests : OpenAIUnitTestsBase
    {
        public MistralCompletionsChatUnitTests()
        {
        }

        [Fact()]
        [TestBeforeAfter]
        public void Nusbio_Led_Control()
        {
            var p = new Mistral_Prompt_MistralSmallLatest()
            {
                Messages = new List<global::Mistral.SDK.DTOs.ChatMessage>()
                {
                    new global::Mistral.SDK.DTOs.ChatMessage { 
                        Role = global::Mistral.SDK.DTOs.ChatMessage.RoleEnum.User, 
                        Content = @"
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
"
                    }
                }
            };

            p.SetResponseFormatAsJson();

            var response = new Mistral().Completions.Create(p);
            Assert.True(response.Success);
            var text = response.Text;
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

        
    }
}

