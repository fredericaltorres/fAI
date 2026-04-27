using LiteDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using static DynamicSugar.DS;
using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;

namespace fAI
{
    public class AIMetaData
    {
        public Dictionary<string, List<string>> MetaData { get; set; }
    }

    public class AIMemoryCrossReferenceTableManager
    {
        public AIMemoryManager _aiMemoryManager { get; }

        public AIMemoryCrossReferenceTableManager(AIMemoryManager aiManager)
        {
            _aiMemoryManager = aiManager;
        }

        public void Sync ()
        {
            _aiMemoryManager.DeleteAllCrossReferenceTable();

            var peopleCRT = new AIMemoryCrossReferenceTable("people").Build(_aiMemoryManager);
            _aiMemoryManager.AddCrossReferenceTable(peopleCRT);

            var locationsCRT = new AIMemoryCrossReferenceTable("locations").Build(_aiMemoryManager);
            _aiMemoryManager.AddCrossReferenceTable(locationsCRT);     

            var topicsCRT = new AIMemoryCrossReferenceTable("topics").Build(_aiMemoryManager);
            _aiMemoryManager.AddCrossReferenceTable(topicsCRT);
        }

        public AIMemoryCrossReferenceTable Get(string name)
        {
            return _aiMemoryManager.GetCrossReferenceTable(name);
        }
    } 

    public class AIMemoryCrossReferenceTable 
    {
        public AIMemoryCrossReferenceTable(string name)
        {
            this.Id = ObjectId.NewObjectId();
            this.Name = name;
        }

        public LiteDB.ObjectId Id { get; set; }

        public string Name { get; set; }

        public Dictionary<string, List<string>> Entries { get; set; } = new Dictionary<string, List<string>>();

        public void AddReference(string key, string mid)
        {
            var kk = key.ToLowerInvariant();
            if (this.Entries.ContainsKey(kk))
                this.Entries[kk].Add(mid);
            else
                this.Entries[kk] = new List<string>() { mid };
        }

        public AIMemoryCrossReferenceTable Build(AIMemoryManager aiManager)
        {
            foreach (var ai in aiManager.GetAll())
                if (ai.AIMetaData.MetaData.ContainsKey(this.Name) && ai.AIMetaData.MetaData[this.Name].Count > 0)
                    foreach (var v in ai.AIMetaData.MetaData[this.Name])
                        this.AddReference(v, ai.MID);
            return this;
        }

        [BsonIgnore]
        public List<string> SortedKeys => this.Entries.Keys.OrderBy(k => k).ToList();
    }

    /// <summary>
    /// https://github.com/litedb-org/LiteDB.Studio
    /// https://www.litedb.org/docs/getting-started/
    /// C:\DVT\LiteDB.Studio\LiteDB.Studio\bin\Debug\LiteDB.Studio.exe
    /// </summary>
    public class AIMemory: IBm25Document
    {
        [JsonIgnore]
        public LiteDB.ObjectId Id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PublishedDocumentInfoType Type { get; set; }

        public string PublishedUrl { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string LocalFile { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public AIMetaData AIMetaData { get; set; }

        [JsonIgnore]
        public List<float> Embeddings { get; set; }
        //[JsonIgnore]
        //public byte[] __embeddingsBuffer { get; set; }

        [BsonIgnore]
        public int TextLength => this.Text?.Length ?? 0;

        [BsonIgnore]
        public double Score { get; set; }
        [BsonIgnore]
        public string MID => (this.Id == null ? "undefined" : Id.ToString());

        [BsonIgnore]
        public string BM25ID { get => MID; set => throw new NotImplementedException(); }

        public void Init()
        {
            this.Id = ObjectId.NewObjectId();
            this.CreateDate = DateTime.UtcNow;
        }

        public AIMemory Clone()
        {
            return new AIMemory
            {
                Type = this.Type,
                PublishedUrl = this.PublishedUrl,
                Title = this.Title,
                Text = this.Text,
                Score = this.Score,
                LocalFile = this.LocalFile,
                CreateDate = this.CreateDate,
                ModifiedDate = this.ModifiedDate,
                AIMetaData = this.AIMetaData, // Note: This is a shallow copy. Consider deep copying if necessary.
                Embeddings = this.Embeddings != null ? new List<float>(this.Embeddings) : null
            };
        }

        public bool LocalFileExists() => !string.IsNullOrEmpty(LocalFile) && File.Exists(LocalFile);
        public bool IsMarkDownFile() => !string.IsNullOrEmpty(LocalFile) && LocalFile.EndsWith(".md", StringComparison.OrdinalIgnoreCase);
        public bool IsTextFile() => !string.IsNullOrEmpty(LocalFile) && LocalFile.EndsWith(".txt", StringComparison.OrdinalIgnoreCase);
        public bool IsHtmlFile() => !string.IsNullOrEmpty(LocalFile) && LocalFile.EndsWith(".html", StringComparison.OrdinalIgnoreCase);
        
        internal AIMemory PrepareForSaving()
            
        {
            this.ModifiedDate = DateTime.UtcNow;
            //if (Embeddings != null && Embeddings.Count > 0)
            //{
            //    __embeddingsBuffer = __ZipEmbeddings();
            //    Embeddings = null; // Clear the original list to save space
            //}
            return this;
        }

        internal AIMemory PrepareAfterLoading()
        {
            //if (__embeddingsBuffer != null && __embeddingsBuffer.Length > 0)
            //{
            //    __UnzipEmbeddings(__embeddingsBuffer);
            //    __embeddingsBuffer = null; // Clear the buffer after loading
            //}
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
}
