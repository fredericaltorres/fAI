using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;
using DynamicSugar;
using System.Runtime.CompilerServices;

namespace fAI
{
    public class GeneratedDocumentDetail
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public string LocalImage { get; set; }
        public string Id => Path.GetFileNameWithoutExtension(this.LocalImage);

        public string _summaryPrompt { get; set; }
        public string _imagePrompt { get; set; }
    }


    public class GeneratedDocumentProperties 
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }


    public class GeneratedDocuments : List<GeneratedDocumentDetail>
    {
        public GeneratedDocumentProperties Properties { get; set; } = new GeneratedDocumentProperties();

        public string FileName { get; set; }
        public string ParentFolder => Path.GetDirectoryName(this.FileName);
        public string ParentFolderNameOnly => Path.GetFileName(this.ParentFolder);

        public GeneratedDocumentDetail Add(string title)
        {
            var d = new GeneratedDocumentDetail { Title = title };
            this.Add(d);
            return d;
        }

        public string ToJson()
        {
            return fAI.JsonUtils.ToJSON(this);
        }

        public string GetPropertiesAsJson()
        {
            return fAI.JsonUtils.ToJSON(this.Properties, Newtonsoft.Json.Formatting.None);
        }

        public void Save(string fileName)
        {
            File.WriteAllText(fileName, GetPropertiesAsJson() + Environment.NewLine + ToJson());
        }

        public static GeneratedDocuments Load(string fileName)
        {
            
            var json = File.ReadAllText(fileName);
            var lines = json.SplitByCRLF();

            // Load first line as properties
            var properties = JsonUtils.FromJSON<GeneratedDocumentProperties>(lines[0]);

            // Load the rest as documents for the List<GeneratedDocumentDetail>
            var newJson = string.Join(Environment.NewLine, lines.Skip(1));

            var r =  JsonUtils.FromJSON<GeneratedDocuments>(newJson);
            r.Properties = properties;
            r.FileName = fileName;  
            return r;
        }
    }
}