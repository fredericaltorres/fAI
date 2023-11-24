using System.IO;
using System.Collections.Generic;

namespace fAI
{
    public class GeneratedDocument
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public string LocalImage { get; set; }
    }

    public class GeneratedDocuments : List<GeneratedDocument>
    {
        public GeneratedDocument Add(string title)
        {
            var d = new GeneratedDocument { Title = title };
            this.Add(d);
            return d;
        }

        public string ToJson()
        {
            return fAI.JsonUtils.ToJSON(this);
        }

        public void Save(string fileName)
        {
            File.WriteAllText(fileName, ToJson());
        }

        public static GeneratedDocuments Load(string fileName)
        {
            return JsonUtils.FromJSON<GeneratedDocuments>(File.ReadAllText(fileName));
        }
    }
}