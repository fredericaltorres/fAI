using DynamicSugar;
using System;
using System.Linq;
using Xunit;

namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class GenericAI__STT_UnitTests : OpenAIUnitTestsBase
    {
        
        public GenericAI__STT_UnitTests()
        {
            OpenAI.TraceOn = true;
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenericAITranscription_Create_Mp3_en()
        {
            var client = new GenericAI();
            var mp3FileName = base.GetTestFile("TestFile.01.48Khz.mp3");
            var expected1 = "I am he as you are he as you are me. And we are all together. See how they run like pigs from a gun. See how they fly. I'm crying.";
            var expected2 = "I am he as you are he as you are me. And we are all together. See how they run like pigs from a gun. See how they fly. I Am crying.";

            client.Transcription.GetModels().ForEach(model =>
            {
                try
                {
                    var (text, usage) = client.Transcription.Create(mp3FileName, model: model);
                    Assert.True(
                        WhisperSpeechToTextEngineTests.ReplacePunctuation(expected1) == WhisperSpeechToTextEngineTests.ReplacePunctuation(text) ||
                        WhisperSpeechToTextEngineTests.ReplacePunctuation(expected2) == WhisperSpeechToTextEngineTests.ReplacePunctuation(text));
                }
                catch (Exception ex)
                {
                    HttpBase.Trace($"[ERROR] Model: {model}, Exception: {ex.Message}", this);
                }
            });
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenericAITranscription_Create_wav_en()
        {
            var client = new GenericAI();
            var mp3FileName = base.GetTestFile("Fred Voice Sample - With Compressor - I am Jordan Lee.wav");
            var expected1 = "Hey there, everyone. I am Jordan Lee, and I'm super excited to be here with you today because I've got something to share with you that's going to blow your mind. Introducing the all-new Swift Gadget X, the gadget of your dreams. This little marvel is not just a device; it's your personal assistant, your entertainment hub, and your productivity powerhouse, all rolled into one. Trust me, folks, this isn't just your ordinary gadget. This is a game changer. Imagine having the world at your fingertips with lightning-fast performance, crystal-clear display, and a battery life that seems to go on forever. You won't miss a beat with Swift Gadget X by your side. Now, I know what you might be thinking, Jordan, this is too good to be true. But let me tell you, but let me tell you, we put Swift Gadget X through the ringer. We've tested it in extreme conditions, pushed it to the limits, and it came out on top every single time. We believe in this product so much that we are offering an exclusive deal just for you, our online community.";

            client.Transcription.GetModels().Take(1).ToList().ForEach(model =>
            {
                var (text, usage) = client.Transcription.Create(mp3FileName, model: model);
                Assert.True(WhisperSpeechToTextEngineTests.ReplacePunctuation(expected1) == WhisperSpeechToTextEngineTests.ReplacePunctuation(text));
            });
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenericAITranscription_Create_wav_fr()
        {
            var client = new GenericAI();
            var mp3FileName = base.GetTestFile("Fred Voice Sample - With Compressor - EN FRANCAIS.ORIGINAL.wav");
            var expected1 = "L'automne débute fin septembre. Durant cette saison, les feuilles des arbres tombent et couvrent le sol d'un tapis brun, rouge et jaune. Le temps devient de plus en plus froid, il commence à y avoir de la pluie et du vent. C'est le moment de sortir son manteau et son parapluie. L'automne est aussi la saison des récoltes. On ramasse le maïs, le tournesol, les pommes et le raisin. Vient ensuite l'hiver, la saison la plus froide qui commence fin décembre. Le paysage devient tout blanc à cause de la neige et de la glace. Cette période marque l'arrivée de Noël et des fêtes de fin d'année. Avec le printemps qui commence en mars, le soleil est de retour et le temps se réchauffe. La nature redevient verte, l'herbe et les fleurs poussent à nouveau. Il est agréable de se promener pour observer les papillons, les abeilles et écouter le chant des oiseaux. Enfin, l'été arrive à la fin du mois de juin. C'est la saison la plus chaude pendant laquelle on recommence à mettre ses lunettes de soleil et à aller se promener sur la plage. C'est aussi le moment idéal pour profiter de l'eau et aller nager à la mer ou à la piscine.";

            client.Transcription.GetModels().Take(1).ToList().ForEach(model =>
            {
                var (text, usage) = client.Transcription.Create(mp3FileName, model: model, language: "fr");
                Assert.True(WhisperSpeechToTextEngineTests.ReplacePunctuation(expected1) == WhisperSpeechToTextEngineTests.ReplacePunctuation(text));
            });
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenericAITranscription_Create_wav_en_SpeedStudy()
        {
            var client = new GenericAI();
            var mp3FileName = base.GetTestFile("Fred Voice Sample - With Compressor - I am Jordan Lee.wav");
            var expected1 = "Hey there, everyone. I am Jordan Lee, and I'm super excited to be here with you today because I've got something to share with you that's going to blow your mind. Introducing the all-new Swift Gadget X, the gadget of your dreams. This little marvel is not just a device; it's your personal assistant, your entertainment hub, and your productivity powerhouse, all rolled into one. Trust me, folks, this isn't just your ordinary gadget. This is a game changer. Imagine having the world at your fingertips with lightning-fast performance, crystal-clear display, and a battery life that seems to go on forever. You won't miss a beat with Swift Gadget X by your side. Now, I know what you might be thinking, Jordan, this is too good to be true. But let me tell you, but let me tell you, we put Swift Gadget X through the ringer. We've tested it in extreme conditions, pushed it to the limits, and it came out on top every single time. We believe in this product so much that we are offering an exclusive deal just for you, our online community.";

            client.Transcription.GetModels().ToList().ForEach(model =>
            {
                var (text, usage) = client.Transcription.Create(mp3FileName, model: model);
                DS.List("excited", "super", "jordan").ForEach(word => Assert.Contains(word, text.ToLowerInvariant()));
            });
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenericAI_Credits()
        {
            var client = new GenericAI();
            var credits = client.Utility.GetCredits();
            Assert.NotNull(credits);
            Assert.True(credits.TotalCredits > 0);
            Assert.True(credits.CreditsRemaining > 0);
        }

        //private static string ReplaceInvalidFileNameChars(string fileName)
        //{
        //    var invalidChars = Path.GetInvalidFileNameChars();
        //    foreach (var c in invalidChars)
        //    {
        //        fileName = fileName.Replace(c, '_');
        //    }
        //    return fileName;
        //}


    }
}