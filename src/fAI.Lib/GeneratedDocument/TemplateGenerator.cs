using System;
using System.IO;
using System.Text;
using DynamicSugar;

namespace fAI
{
    public abstract class TemplateGenerator
    {
        public abstract string GenerateHeader();
        public abstract string GenerateFooter();

        public abstract string GenerateTemplate(GeneratedDocumentDetail gdd);

        public string Template(string template, object poco)
        {
            var d = ReflectionHelper.GetDictionary(poco);
            foreach(var k in d.Keys)
                template = template.Replace($"{{{{{k}}}}}", d[k].ToString());
            return template;
        }

        public string GenerateTemplate(GeneratedDocuments generatedDocuments)
        {
            var sb = new System.Text.StringBuilder();
            var htmlHeader = this.Template(this.GenerateHeader(), new { generatedDocuments.Properties.Title, generatedDocuments.Properties.Description, generatedDocuments.Properties.CreatedDate });
            sb.AppendLine(htmlHeader + Environment.NewLine);
            foreach (var gdd in generatedDocuments)
                sb.AppendLine(this.GenerateTemplate(gdd));

            sb.AppendLine(this.GenerateFooter() + Environment.NewLine);

            return sb.ToString();
        }

        public void GenerateNarration(GeneratedDocuments generatedDocuments, OpenAISpeech.Voices voice)
        {
            var client = new OpenAI();
            foreach (var gdd in generatedDocuments)
            {
                var text = gdd.Summary;
                var mp3FileName = Path.Combine(@".\audio", Path.GetFileNameWithoutExtension(gdd.LocalImage) + ".mp3");
                if (!File.Exists(mp3FileName))
                {
                    client.Audio.Speech.Create(text, "onyx", mp3FileName: mp3FileName);
                    //var mp3Info = AudioUtil.GetMp3Info(mp3FileName);
                }
            }
        }

        public void GenerateVideo(GeneratedDocuments generatedDocuments, OpenAISpeech.Voices? voice = null)
        {
            //if (voice.HasValue)
            //    this.GenerateNarration(generatedDocuments, voice.Value);

            foreach (var gdd in generatedDocuments)
            {
                var image = Path.Combine(generatedDocuments.ParentFolder, "images", Path.GetFileNameWithoutExtension(gdd.LocalImage) + ".png");
                var mp3File = Path.Combine(generatedDocuments.ParentFolder, "audio", Path.GetFileNameWithoutExtension(gdd.LocalImage) + ".mp3");
                var videoFolder = Path.Combine(generatedDocuments.ParentFolder, "video");
                new TestFileHelper().CreateDirectory(videoFolder);
                var outputMp4File = Path.Combine(videoFolder, Path.GetFileNameWithoutExtension(gdd.LocalImage) + ".mp4");
                var command = $@"--image ""{image}"" --audio ""{mp3File}"" --outputVideo ""{outputMp4File}""";
                Console.WriteLine(command);
                Logger.Trace(command, this);

                var fmsComversionConsoleexe = @"C:\DVT\finux\fms\fmsComversionConsole\fmsComversionConsole\bin\x64\Debug\fmsComversionConsole.exe";
                var sb = new StringBuilder();
                sb.AppendLine($@" generateVideoBasedOnImageAndMp3 --imageFileName ""{image}""  --audioFileName ""{mp3File}"" --outputVideoFileName ""{outputMp4File}"" ");

                var exitCode = 0;
                var r = ExecuteProgramUtilty.ExecProgram(fmsComversionConsoleexe, sb.ToString(), ref exitCode);
                var ok = r && exitCode == 0;
                if(ok)
                    Logger.Trace($"Video generated for {gdd.Id}, ({outputMp4File})", this);
                else
                    Logger.TraceError($"Error generating video for {gdd.Id}", this);
            }
        }

        public void GenerateFile(GeneratedDocuments generatedDocuments, string htmlStaticOutputFile, OpenAISpeech.Voices? voice = null)
        {
            if (voice.HasValue)
                this.GenerateNarration(generatedDocuments, voice.Value);

            var html = this.GenerateTemplate(generatedDocuments);
            File.WriteAllText(htmlStaticOutputFile, html);
        }

        public string GetStaticFile(string fileName)
        {
            return File.ReadAllText($@"GeneratedDocument\{fileName}");
        }
    }
}
