using DynamicSugar;
using fAI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DynamicSugar.DS;
using static fAI.OpenAIImage;

namespace fAIConsole
{
    internal class SoLonelyPrompt
    {
        public string Verse1 { get; set; } = @"
            Someone told me. 
            That when I throw my love away.
            I act as if I just don't care.			
            I look as if I going somewhere.
            But you just can't convince myself.				 
            You couldn't live with no one else.
            And you can only play that part.
            And sit and nurse my broken heart.";

        public string Verse2 { get; set; } = @"
            No body's knocked upon my door.
            For one years or more.
            All made up and nowhere to go.
            this is my one man show .
            Just take a seat they're always free.
            surprise and no mystery.
            In this theater I call my soul.
            She always play the starring role.
        ";


        public string DestinationFolder = @"C:\ZIC\So Lonely.VOICE\So Lonely.VOICE\AI Prompt\images\DALLE";

        public void GenerateImageSequence()
        {
            var nl = Environment.NewLine;
            var paragraphLineCount = 4;
            var verse1Lines = this.Verse1.Replace(@"\r","").Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0).ToList();
            var promptIntro  = $"Render image based only on the following:{nl}";
            var imageIndex = 0;
            var masterPromptFile = $"{this.DestinationFolder}\\MasterPrompt.md";
            if (File.Exists(masterPromptFile))
                File.Delete(masterPromptFile);

            for (var i = 1; i < verse1Lines.Count; i++)
            {
                var prompt = $"{promptIntro}{nl}";
                for(var j = 0; j < paragraphLineCount; j++)
                {
                    var index = i + j;
                    if (index < verse1Lines.Count)
                        prompt += $"{verse1Lines[index]}{nl}";
                }
                var destinationImage0 = $"{this.DestinationFolder}\\{imageIndex.ToString("000")}.0.png";
                var destinationImage1 = $"{this.DestinationFolder}\\{imageIndex.ToString("000")}.1.png";
                File.AppendAllText(masterPromptFile, $"## PROMPT[{imageIndex.ToString("000")}]{nl}{prompt}{nl}{nl}");

                var client = new OpenAI();
                var r = client.Image.Generate(prompt, size: ImageSize._1792x1024);
                var pngFileNames = r.DownloadImages(DS.List(destinationImage0));
                var pngFileNamesAll = pngFileNames;

                r = client.Image.Generate(prompt, size: ImageSize._1792x1024);
                pngFileNames = r.DownloadImages(DS.List(destinationImage1));
                pngFileNamesAll.AddRange(pngFileNames);

                File.AppendAllText(masterPromptFile, $"{string.Join(nl, pngFileNamesAll)}{nl}");
            }
        }
    }
}
