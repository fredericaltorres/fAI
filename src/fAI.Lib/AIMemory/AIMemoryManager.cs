using DynamicSugar;
using fAI.VectorDB;
using LiteDB;
using Mistral.SDK.DTOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using static fAI.HumeAISpeech;
using JsonIgnore2Attribute = System.Text.Json.Serialization.JsonIgnoreAttribute;

namespace fAI
{
    public enum PublishedDocumentInfoType
    {
        Undefined,
        UserAINote,
        MarkdownFile,
    }

    public class AIMemoryManager
    {
        public bool InMemoryOnly { get; set; } = false;
        public string FileName { get; set; }

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
       
        public bool AddUpdate(AIMemory d, string localFile, string openAiKey = null, string llmApiKey = null, bool clearEmbeddings = false)
        {
            var r = true;
            try
            {
                var allMemories = GetAll();
                var exists = allMemories.Where(e => e.LocalFile == localFile).ToList();
                if (exists.Count > 0)
                {
                    var existingAIMemory = exists[0];
                    existingAIMemory.Text = d.Text;
                    existingAIMemory.Title = d.Title;
                    existingAIMemory.PublishedUrl = d.PublishedUrl;

                    if (clearEmbeddings)
                    {
                        existingAIMemory.Embeddings.Clear();
                    }

                    ComputeEmbeddingsAndMetaData(existingAIMemory, openAiKey, llmApiKey: llmApiKey);
                    Update(existingAIMemory);
                }
                else
                {
                    Add(d, openAiKey);
                }
            }
            catch (Exception ex)
            {
                r = false;
            }
            return r;
        }

        public void Add(AIMemory d, string openAiKey = null)
        {
            ComputeEmbeddingsAndMetaData(d, openAiKey);

            d.Init();

            using (var db = new LiteDatabase(this.FileName))
            {
                var col = db.GetCollection<AIMemory>(nameof(AIMemory));
                var embeddings = d.Embeddings;
                col.Insert(d.PrepareForSaving());
                d.Embeddings = embeddings; // Restore the original list after saving
                d.__embeddingsBuffer = null; // Clear the buffer after saving
            }
        }

        public bool __simulate_embedding_computation__ = false;

        public const string DEFAULT_MODEL_FOR_META_DATA_EXTRACTION = "gemini-2.5-flash";

        public bool ComputeEmbeddingsAndMetaData(AIMemory d, 
            string embeddingsApiKey = null, 
            string llmApiKey = null,
            string model = DEFAULT_MODEL_FOR_META_DATA_EXTRACTION
            )
        {
            var r1 = ComputeEmbeddings(d, embeddingsApiKey);
            var r2 = ExtractMetaDataFromText(d, model, llmApiKey);

            return r1 && r2;
        }

        public bool ExtractMetaDataFromText(AIMemory d, string model = DEFAULT_MODEL_FOR_META_DATA_EXTRACTION, string llmApiKey = null)
        {
            try
            {
                var client = new GenericAI(ApiKey: llmApiKey);
                var medataDictionary = client.Completions.ExtractMetaDataFromNotes(d.Text, model: model);
                d.AIMetaData = medataDictionary;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool ComputeEmbeddings(AIMemory d, string openAiKey = null)
        {
            try
            {
                if (__simulate_embedding_computation__)
                {
                    d.Embeddings = new List<float>();
                    for (var c = 0; c < 1536; c++)
                    {
                        d.Embeddings.Add((float)(c * 0.113416));
                    }
                }
                else
                {
                    if (d.Embeddings == null || d.Embeddings.Count == 0)
                        d.Embeddings = ToVector($"{d.Title}. {d.Text}", openAiKey);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void Update(AIMemory d)
        {
            using (var db = new LiteDatabase(this.FileName))
            {
                var col = db.GetCollection<AIMemory>(nameof(AIMemory));
                var embeddings = d.Embeddings;
                col.Update(d.PrepareForSaving());
                d.Embeddings = embeddings; // Restore the original list after saving
                d.__embeddingsBuffer = null; // Clear the buffer after saving
            }
        }

        public void Delete(string id)
        {
            Delete(new ObjectId(id));
        }

        public void Delete(AIMemory d)
        {
            Delete(d.Id);
        }

        public void Delete(ObjectId id)
        {
            using (var db = new LiteDatabase(this.FileName))
            {
                var col = db.GetCollection<AIMemory>(nameof(AIMemory));
                col.Delete(id);
            }
        }

        public AIMemory GetFromId(LiteDB.ObjectId id)
        {
            using (var db = new LiteDatabase(this.FileName))
            {
                var col = db.GetCollection<AIMemory>(nameof(AIMemory));
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
                var col = db.GetCollection<AIMemory>(nameof(AIMemory));
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

            var rrr = r.OrderByDescending(e => e.Score).ToList();
            r.Clear();
            r.AddRange(rrr);

            return r;
        }

        public AIMemorys SimilaritySearch(List<float> embeddingsQuery, float minimumScore = 0.2f, 
            float scoreToNotApplyRefining = -1f,  // If we found at least 3 items with score higher than this threshold, we will not apply refining to improve performance, we just return the items
            int scoreToNotApplyRefiningTopK = 3
            )
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

            if(scoreToNotApplyRefining != -1)
            {
                var aiMemoryWithHighScore = result.Where(rr => rr.Score >= scoreToNotApplyRefining).ToList();
                if(aiMemoryWithHighScore.Count >= scoreToNotApplyRefiningTopK)
                {
                    var am = new AIMemorys();
                    am.AddRange(aiMemoryWithHighScore.OrderByDescending(e => e.Score).ToList());

                    am.ForEach(m =>
                    {
                        HttpBase.Trace($"Search score {m.Score}, title: {m.Title}", this);
                    });

                    return am;
                }
            }

            var am2 = ReFineResultWithDynamicScore(result);
            am2.ForEach(m =>
            {
                HttpBase.Trace($"Search score {m.Score}, title: {m.Title}", this);
            });
            return am2;
        }

        public List<AIMemory> GetAll()
        {
            using (var db = new LiteDatabase(this.FileName))
            {
                var col = db.GetCollection<AIMemory>(nameof(AIMemory));
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
