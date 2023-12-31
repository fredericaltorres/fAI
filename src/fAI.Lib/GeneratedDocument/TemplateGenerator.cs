﻿using System;
using System.IO;
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
                    client.Audio.Speech.Create(text, OpenAISpeech.Voices.onyx, mp3FileName: mp3FileName);
                    //var mp3Info = AudioUtil.GetMp3Info(mp3FileName);
                }
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
