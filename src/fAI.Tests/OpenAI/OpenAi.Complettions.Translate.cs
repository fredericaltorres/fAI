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

namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class OpenAiCompletionsTranslate : OpenAIUnitTestsBase
    {
        public OpenAiCompletionsTranslate()
        {
            OpenAI.TraceOn = true;
        }

        [Fact()]
        [TestBeforeAfter]
        public void Translate_EnglishToFrench_SpecialRules()
        {
            var client = new OpenAI();
            var text = "10%";
            Assert.False(client.Completions.NeedToBeTranslated(text, TranslationLanguages.English, TranslationLanguages.French), $"NeedToBeTranslated text:{text}");

            text = "10% cheaper";
            Assert.True(client.Completions.NeedToBeTranslated(text, TranslationLanguages.English, TranslationLanguages.French), $"NeedToBeTranslated text:{text}");

            text = "200K";
            Assert.False(client.Completions.NeedToBeTranslated(text, TranslationLanguages.English, TranslationLanguages.French), $"NeedToBeTranslated text:{text}");

            text = "$22M"; // Do not translate because text is to long, '$1M' to '1 million de dollars'
            Assert.False(client.Completions.NeedToBeTranslated(text, TranslationLanguages.English, TranslationLanguages.French), $"NeedToBeTranslated text:{text}");

            text = "200K cheaper";
            Assert.True(client.Completions.NeedToBeTranslated(text, TranslationLanguages.English, TranslationLanguages.French), $"NeedToBeTranslated text:{text}");

            text = "$200K";
            Assert.True (client.Completions.NeedToBeTranslated(text, TranslationLanguages.English, TranslationLanguages.French), $"NeedToBeTranslated text:{text}");

            text = "3,948";
            Assert.True(client.Completions.NeedToBeTranslated(text, TranslationLanguages.English, TranslationLanguages.French), $"NeedToBeTranslated text:{text}");

            text = "3,948,123";
            Assert.True(client.Completions.NeedToBeTranslated(text, TranslationLanguages.English, TranslationLanguages.French), $"NeedToBeTranslated text:{text}");
        }


        [Fact()]
        [TestBeforeAfter]
        public void Translate_EnglishToFrench_TranslationOverRide()
        {
            var client = new OpenAI();
            var text = "3,948";
            var translation = client.Completions.Translate(text, TranslationLanguages.English, TranslationLanguages.French);
            Assert.True(FlexStrCompare("3 948") == FlexStrCompare(translation));
        }

        [Fact()]
        [TestBeforeAfter]
        public void Translate_EnglishToFrench_SpecialCase_NumberWithCommaAsThousandSeparator()
        {
            var client = new OpenAI();
            var text = "3,948";
            var translation = client.Completions.Translate(text, TranslationLanguages.English, TranslationLanguages.French);
            Assert.True(FlexStrCompare("3 948") == FlexStrCompare(translation));
        }

        [Fact()]
        [TestBeforeAfter]
        public void Translate_EnglishToFrench_SpecialCase_Dollars200K()
        {
            var client = new OpenAI();
            var text = "$200K";
            var translation = client.Completions.Translate(text, TranslationLanguages.English, TranslationLanguages.French);
            Assert.True(FlexStrCompare("200 000 $") == FlexStrCompare(translation));
        }

        [Fact()]
        [TestBeforeAfter]
        public void Translate_EnglishToFrench_SpecialCase_200K()
        {
            var client = new OpenAI();
            var text = "200K";
            var translation = client.Completions.Translate(text, TranslationLanguages.English, TranslationLanguages.French);
            Assert.True(FlexStrCompare(text) == FlexStrCompare(translation));
        }

        [Fact()]
        [TestBeforeAfter]
        public void Translate_EnglishToFrench()
        {
            var client = new OpenAI();
            var translation = client.Completions.Translate(ReferenceEnglishSentence, TranslationLanguages.English, TranslationLanguages.French);
            Assert.True(FlexStrCompare("Bonjour monde.") == FlexStrCompare(translation) ||
                        FlexStrCompare("Bonjour le monde.") == FlexStrCompare(translation));
        }

        [Fact()]
        [TestBeforeAfter]
        public void Translate_EnglishToFrench_Dictionary()
        {
            var client = new OpenAI();
            var inputDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReferenceEnglishJsonDictionary);

            var outputDictionary = client.Completions.Translate(inputDictionary, TranslationLanguages.English, TranslationLanguages.French);

            Assert.Equal(6, outputDictionary.Keys.Count);
            inputDictionary.Keys.ToList().ForEach(k => Assert.True(outputDictionary.ContainsKey(k)));

            Assert.Equal("Éducation", outputDictionary["2"]);
            Assert.Equal("Salle de classe 01", outputDictionary["3"]);
        }

        [Fact()]
        [TestBeforeAfter]
        public void Translate_EnglishToFrench_List()
        {
            var client = new OpenAI();
            var inputDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReferenceEnglishJsonDictionary);
            var inputList = inputDictionary.Values.ToList();

            var outputList = client.Completions.Translate(inputList, TranslationLanguages.English, TranslationLanguages.French);
            Assert.Equal(6, outputList.Count);

            DS.Assert.Words(outputList[0], "personnes & important & domaine & activité");
        }

        [Fact()]
        [TestBeforeAfter]
        public void Translate_EnglishToFrench_OverideValue()
        {
            var client = new OpenAI();
            var text =  "Sales";
            var translationOverrideRules = new List<TranslationOverrideRule>
            {
                new TranslationOverrideRule
                {
                    sourceLangague = TranslationLanguages.English,
                    targetLanguage = TranslationLanguages.French,
                    InputTextRegex = new Regex("Sales"),
                    OutputTextRegex = new Regex("Vend"),
                    Replacement = "Ventes"
                }
            };
            
            var outputText = client.Completions.Translate(text, TranslationLanguages.English, TranslationLanguages.French,  translationOverrideRules: translationOverrideRules);
            Assert.Equal("Ventes", outputText);
        }

        [Fact()]
        [TestBeforeAfter]
        public void Translate_EnglishToFrench_List_WithNonTranslatableValue()
        {
            var client = new OpenAI();
            var inputList = new List<string> { "Strawberry", "200K", "Raspberry", "10%", "$1M" };
            var outputList = client.Completions.Translate(inputList, TranslationLanguages.English, TranslationLanguages.French);

            DS.Assert.Words(string.Join(" ", outputList), "Fraise & 200K & Framboise & 10% & $1M");
        }

        [Fact()]
        [TestBeforeAfter]
        public void Translate_EnglishToFrench_ListOfNumber()
        {
            var client = new OpenAI();
            var inputList = DS.List(1, 2, 3, 4).Select(i => i.ToString()).ToList();

            var outputList = client.Completions.Translate(inputList, TranslationLanguages.English, TranslationLanguages.French);
            Assert.Equal(4, outputList.Count);
            DS.Range(4).ForEach(i => Assert.Contains((i+1).ToString(), outputList[i]));
        }

        [Fact()]
        [TestBeforeAfter]
        public void Translate_EnglishToSpanish()
        {
            var client = new OpenAI();
            var translation = client.Completions.Translate(ReferenceEnglishSentence, TranslationLanguages.English, TranslationLanguages.Spanish);
            Assert.Equal("'Hola mundo.'", translation);
        }

        const string ReferenceEnglishTextForSummarization = @"Hey there, everyone! I'm Jordan Lee, and I'm super excited to be here with you today because 
I've got somethin to share with you that is going to blow your mind!
 Introducing the all-new ""SwiftGadget X"" – the gadget of your dreams! This little marvel is not just a device; 
it's your personal assistant, your entertainment hub, and your productivity powerhouse, all rolled into one. 
Trust me, folks, this isn't your ordinary gadget – this is a game-changer. ";

       
    }
}