using DynamicSugar;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class GenericAI__TTS_UnitTests : OpenAIUnitTestsBase
    {
        
        public GenericAI__TTS_UnitTests()
        {
            OpenAI.TraceOn = true;
        }

        const string TTS_TEXT = @"
diagnose the following patient as a spine orthopedic surgeon:
Jane Doe, a 55-year-old female, presents with extremely painful lower back pain and in the left legs.
MRI scan shows fracture at L4.
Find root cause.
";

        [Fact()]
        [TestBeforeAfter]
        public void GenericAISpeech_Create()
        {
            var client = new GenericAI();
            client.Speech.TTSVoiceInfos.Take(2).ToList().ForEach(request =>
            {
                foreach(var testVoice in request.TestsVoices)
                {
                    var inputFile = client.Speech.Create(TTS_TEXT, testVoice, request.Model, cost: request.ComputeCost(TTS_TEXT), useOpenAI: !request.OpenRouterSupported);
                    Assert.True(File.Exists(inputFile));
                }
            });
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenericAISpeech_Create___fish_audio_s2_1_pro()
        {
            var TTS_TEXT = @"
diagnose the following patient as a spine orthopedic surgeon:
Jane Doe, a 55-year-old female, presents with extremely painful lower back pain and in the left legs.
MRI scan shows fracture at L4.
Find root cause.
";

            // https://fish.audio/app/default-voices/
            var client = new GenericAI();
            client.Speech.TTSVoiceInfos.Where(v => v.Model== "fish-audio/s2.1-pro").ToList().ForEach(request =>
            {
                foreach (var testVoice in request.TestsVoices)
                {
                    var inputFile = client.Speech.Create(TTS_TEXT, testVoice, request.Model, cost: request.ComputeCost(TTS_TEXT), useOpenAI: !request.OpenRouterSupported);
                    Assert.True(File.Exists(inputFile));
                }
            });
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenericAISpeech_Create_WithCustomVoice()
        {
            var inputVoiceFileName = base.GetTestFile("Fred Voice Sample - With Compressor - I am Jordan Lee.wav");
            var inputVoiceText = @"Hey there everyone. I am Jordan Lee, and I'm super excited to be here with you today because I've got something to share with you that's going to blow your mind. Introducing the all new Swift Gadget X, the gadget of your dreams. This little marvel is not just a device, it's your personal assistant you're entertaining and HOB and your productivity powerhouse all rolled into one. Trust me folks, this isn't just your ordinary gadget. This is a game changer. Imagine having the world at your fingertips with lighting, fast performance, crystal clear display and a battery life that seems to go on forever. You won't miss a bit with Swift Gadget XD by your side. Now I know what you might be thinking, Jordan. This is too good to be true. But let me tell you. But let me tell you, we put Swift Gadget X through the ringer. We've tested it in extreme conditions, push it to limits, and it came out on top every single time. We believe in this product so much that we are offering an exclusive deal just for you, our online community. ";
            var client = new GenericAI();
            var modelSupportingCustomVoice = client.Speech.TTSVoiceInfos.Where(v => v.SupportVoiceCloning).ToList();
            modelSupportingCustomVoice.ForEach(request =>
            {
                try
                {
                    var inputFile = client.Speech.Create(TTS_TEXT, null, request.Model, cost: request.ComputeCost(TTS_TEXT), useOpenAI: !request.OpenRouterSupported,
                                                            inputVoiceFileName: inputVoiceFileName, inputVoiceText: inputVoiceText);
                    Assert.True(File.Exists(inputFile));
                }
                catch (Exception ex)
                {
                    HttpBase.Trace($"[ERROR] Exception: {ex.Message}", this);
                }
            });
        }
    }
}