using DynamicSugar;
using fAI.VectorDB;
using LiteDB;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
    /// <summary>
    /// https://github.com/litedb-org/LiteDB.Studio
    /// https://www.litedb.org/docs/getting-started/
    /// C:\DVT\LiteDB.Studio\LiteDB.Studio\bin\Debug\LiteDB.Studio.exe
    /// </summary>
    public class AIMemory
    {
        [JsonIgnore]
        public LiteDB.ObjectId Id { get; set; }

        
        public PublishedDocumentInfoType Type { get; set; }
        public string PublishedUrl { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string LocalFile { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        [JsonIgnore]
        public List<float> Embeddings { get; set; }
        [JsonIgnore]
        public byte[] __embeddingsBuffer { get; set; }

        [BsonIgnore]
        public float Score { get; set; }
        [BsonIgnore]
        public string MID => Id.ToString();

        public void Init()
        {
            this.Id = ObjectId.NewObjectId();
            this.CreateDate = DateTime.UtcNow;
        }

        public bool LocalFileExists() => !string.IsNullOrEmpty(LocalFile) && File.Exists(LocalFile);
        public bool IsMarkDownFile() => !string.IsNullOrEmpty(LocalFile) && LocalFile.EndsWith(".md", StringComparison.OrdinalIgnoreCase);
        public bool IsTextFile() => !string.IsNullOrEmpty(LocalFile) && LocalFile.EndsWith(".txt", StringComparison.OrdinalIgnoreCase);
        public bool IsHtmlFile() => !string.IsNullOrEmpty(LocalFile) && LocalFile.EndsWith(".html", StringComparison.OrdinalIgnoreCase);
        
        internal AIMemory PrepareForSaving()
            
        {
            this.ModifiedDate = DateTime.UtcNow;
            if (Embeddings != null && Embeddings.Count > 0)
            {
                __embeddingsBuffer = __ZipEmbeddings();
                Embeddings = null; // Clear the original list to save space
            }
            return this;
        }

        internal AIMemory PrepareAfterLoading()
        {
            if (__embeddingsBuffer != null && __embeddingsBuffer.Length > 0)
            {
                __UnzipEmbeddings(__embeddingsBuffer);
                __embeddingsBuffer = null; // Clear the buffer after loading
            }
            return this;
        }

        /// <summary>
        /// Compresses the Embeddings list into a GZip-compressed byte array.
        /// </summary>
        private byte[] __ZipEmbeddings()
        {
            if (Embeddings == null || Embeddings.Count == 0)
                return new byte[0];

            // Convert List<float> → raw bytes (4 bytes per float)
            byte[] rawBytes = new byte[Embeddings.Count * sizeof(float)];
            Buffer.BlockCopy(Embeddings.ToArray(), 0, rawBytes, 0, rawBytes.Length);

            // GZip compress the raw bytes
            using (var outputStream = new MemoryStream())
            {
                using (var gzip = new GZipStream(outputStream, CompressionLevel.Optimal))
                {
                    gzip.Write(rawBytes, 0, rawBytes.Length);
                }

                return outputStream.ToArray();
            }
        }

        /// <summary>
        /// Decompresses a GZip-compressed byte array back into the Embeddings list.
        /// </summary>
        private void __UnzipEmbeddings(byte[] compressedData)
        {
            if (compressedData == null || compressedData.Length == 0)
            {
                Embeddings = new List<float>();
                return;
            }

            // GZip decompress back to raw bytes
            using (var inputStream = new MemoryStream(compressedData))
            using (var outputStream = new MemoryStream())
            {
                using (var gzip = new GZipStream(inputStream, CompressionMode.Decompress))
                {
                    gzip.CopyTo(outputStream);
                }

                byte[] rawBytes = outputStream.ToArray();

                // Convert raw bytes → float[]  (4 bytes per float)
                float[] floats = new float[rawBytes.Length / sizeof(float)];
                Buffer.BlockCopy(rawBytes, 0, floats, 0, rawBytes.Length);

                Embeddings = new List<float>(floats);
            }
        }
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
                var embeddings = d.Embeddings;
                col.Insert(d.PrepareForSaving());
                d.Embeddings = embeddings; // Restore the original list after saving
                d.__embeddingsBuffer = null; // Clear the buffer after saving
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
                if(d.Embeddings == null || d.Embeddings.Count == 0)
                    d.Embeddings = ToVector(d.Text, openAiKey);
            }
        }

        public void Update(AIMemory d)
        {
            using (var db = new LiteDatabase(this.FileName))
            {
                var col = db.GetCollection<AIMemory>(CollectionName);
                var embeddings = d.Embeddings;
                col.Update(d.PrepareForSaving());
                d.Embeddings = embeddings; // Restore the original list after saving
                d.__embeddingsBuffer = null; // Clear the buffer after saving
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
                    return results[0].PrepareAfterLoading();
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
                var results = col.Query().Where(x => ids.Contains(x.Id)).Select(e => e.PrepareAfterLoading()).ToList();
                return results;
            }
        }

        private AIMemorys ReFineResultWithDynamicScore(AIMemorys docInfo)
        {
            var maxScore = docInfo.ToArray().Select(rr => (float)rr.Score).DefaultIfEmpty(0).Max();
            var minimumScore = SimilaritySearchEngine.GetOpenAIEmbeddingDynamicScore(maxScore);
            var items = docInfo.Where(e => e.Score >= minimumScore).ToList();
            var r = new AIMemorys();
            r.AddRange(items);
            return r;
        }

        public AIMemorys SimilaritySearch(List<float> embeddingsQuery, float minimumScore = 0.2f)
        {
            var result = new AIMemorys();
            foreach (var e in this.GetAll())
            {
                if (e.Embeddings != null && e.Embeddings.Count > 0)
                {
                    var score = SimilaritySearchEngine.CosineSimilarity(e.Embeddings, embeddingsQuery);
                    if (score >= minimumScore)
                    {
                        e.Score = (float)score;
                        result.Add(e);
                    }
                }
            }
            return ReFineResultWithDynamicScore(result);
        }

        public List<AIMemory> GetAll()
        {
            using (var db = new LiteDatabase(this.FileName))
            {
                var col = db.GetCollection<AIMemory>(CollectionName);
                var l = col.Query().ToList();
                foreach (var m in l)
                    m.PrepareAfterLoading();
                return l;
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

        public void AddFile(string id, string fileName)
        {
            using (var db = new LiteDatabase(this.FileName))
            {
                var storage = db.GetStorage<string>();
                storage.Upload(id, fileName);
            }
        }

        public string GetFile(string id, string fileName)
        {
            using (var db = new LiteDatabase(this.FileName))
            {
                var storage = db.GetStorage<string>();
                storage.Download(id, fileName, true);
                return fileName;
            }
        }

        public void UpdateFile(string id, string fileName)
        {
            using (var db = new LiteDatabase(this.FileName))
            {
                var storage = db.GetStorage<string>();
                storage.Delete(id);
                storage.Upload(id, fileName);
            }
        }

        public void DeleteFle(string id)
        {
            using (var db = new LiteDatabase(this.FileName))
            {
                var storage = db.GetStorage<string>();
                storage.Delete(id);
            }
        }
    }
}
