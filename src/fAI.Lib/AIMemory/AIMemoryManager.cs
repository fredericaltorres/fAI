using DynamicSugar;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;

using System.Text;
using System.Xml.Linq;
using static fAI.HumeAISpeech;
using JsonIgnore2Attribute = System.Text.Json.Serialization.JsonIgnoreAttribute;
using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;

namespace fAI
{
    public enum PublishedDocumentInfoType
    {
        Undefined,
        AI_Generated_Note,
        Markdown_File,
    }

    public class AIMemorys : List<AIMemory>
    {
    }
    public class AIMemory
    {
        public LiteDB.ObjectId Id { get; set; }

        public float Score { get; set; }
        public PublishedDocumentInfoType Type { get; set; }
        public string PublishedUrl { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public int TextLength => !string.IsNullOrEmpty(Text) ? Text.Length : 0;
        public string LocalFile { get; set; }
        public List<float> Embeddings { get; set; }
        public DateTime CreateDate { get; set; }

        public void Init()
        {
            this.Id = ObjectId.NewObjectId();
            this.CreateDate = DateTime.UtcNow;
        }

        public bool LocalFileExists() => !string.IsNullOrEmpty(LocalFile) && File.Exists(LocalFile);
        public bool IsMarkDownFile() => !string.IsNullOrEmpty(LocalFile) && LocalFile.EndsWith(".md", StringComparison.OrdinalIgnoreCase);
        public bool IsTextFile() => !string.IsNullOrEmpty(LocalFile) && LocalFile.EndsWith(".txt", StringComparison.OrdinalIgnoreCase);
        public bool IsHtmlFile() => !string.IsNullOrEmpty(LocalFile) && LocalFile.EndsWith(".html", StringComparison.OrdinalIgnoreCase);
    }

    public class AIMemoryManager
    {
        public bool InMemoryOnly { get; set; } = false;
        public string FileName { get; set; }

        const string CollectionName = "AIMemory";

        public AIMemoryManager(string fileName = null, bool inMemoryOnly = false)
        {
            if (fileName != null)
                this.FileName = fileName;
            this.InMemoryOnly = inMemoryOnly;
        }

        public static List<float> ToVector(string text, string openAiKey = null)
        {
            OpenAI client = null;
            client =  string.IsNullOrEmpty(openAiKey) ? new OpenAI() : new OpenAI(apiKey: openAiKey);
            var r = client.Embeddings.Create(text);
            if (r.Success)
            {
                return r.Data[0].Embedding;
            }
            else
                return null;
        }

        public void Add(AIMemory d, string openAiKey = null)
        {
            ComputeEmbeddings(d, openAiKey);

            d.Init();

            using (var db = new LiteDatabase(this.FileName))
            {
                var col = db.GetCollection<AIMemory>(CollectionName);
                col.Insert(d);
            }
        }


        public bool __simulate_embedding_computation__ = false;

        public void ComputeEmbeddings(AIMemory d, string openAiKey = null)
        {
            if (__simulate_embedding_computation__)
            {
                d.Embeddings = new List<float>();
                for (var c= 0; c< 1536; c++)
                {
                    d.Embeddings.Add((float)(c * 0.113416));
                }
            }
            else
            {
                d.Embeddings = ToVector(d.Text, openAiKey);
            }
        }

        public void Update(AIMemory d)
        {
            using (var db = new LiteDatabase(this.FileName))
            {
                var col = db.GetCollection<AIMemory>(CollectionName);
                col.Update(d);
            }
        }

        public void Delete(AIMemory d)
        {
            using (var db = new LiteDatabase(this.FileName))
            {
                var col = db.GetCollection<AIMemory>(CollectionName);
                col.Delete(d.Id);
            }
        }

        public AIMemory GetFromId(LiteDB.ObjectId id)
        {
            using (var db = new LiteDatabase(this.FileName))
            {
                var col = db.GetCollection<AIMemory>(CollectionName);
                var results = col.Query().Where(x => x.Id == id).ToList();
                if (results.Count > 0)
                    return results[0];
                else
                    return null;
            }
        }

        public List<AIMemory> GetFromIds(List<ObjectId> ids)
        {
            var r = new List<AIMemory>();
            using (var db = new LiteDatabase(this.FileName))
            {
                var col = db.GetCollection<AIMemory>(CollectionName);
                var results = col.Query().Where(x => ids.Contains(x.Id)).ToList();
                return results;
            }
        }

        public List<AIMemory> GetAll()
        {
            using (var db = new LiteDatabase(this.FileName))
            {
                var col = db.GetCollection<AIMemory>(CollectionName);
                return col.Query().ToList();
            }
        }

        public string ToJsonFile()
        {
            var json = ToJSON();
            var jsonFileName = Path.Combine(Path.GetTempPath(), Path.GetFileName( this.FileName) + ".json");
            File.WriteAllText(jsonFileName, json);
            return jsonFileName;
        }

        public string ToJSON()
        {
            var all = GetAll();
            return Newtonsoft.Json.JsonConvert.SerializeObject(all, Newtonsoft.Json.Formatting.Indented);
        }
    }
}
