using DynamicSugar;
using fAI.Util.Strings;
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
    namespace RRF // Reciprocal Rank Fusion
    {
        public class RRFObject 
        {
            public string Id { get; set; } // Set by User
            public string Title { get; set; } // Set by User
            public string LocalFile { get; set; } // Set by User
            public object obj { get; set; } // Set by User
            public float Bm25Score { get; set; }// Set by User
            public float SemanticScore { get; set; }// Set by User

            //public float RRF_Bm25Score { get; set; } // Computed
            //public float RRF_SemanticScore { get; set; } // Computed
            public float RRFScore { get; set; } = 0; // Computed

            public override string ToString()
            {
                return $"Id: {Id}, Title: {Title}, Bm25Score: {Bm25Score}, SemanticScore: {SemanticScore}, RRFScore: {RRFScore}, LocalFile: ({LocalFile})";
            }
        }

        public class RRFRanker
        {
            public Dictionary<string, RRFObject> EntriesDictionary = new Dictionary<string, RRFObject>();

            public void AddUpdateBm25Score(AIMemorys aIMemories)
            {
                foreach (var aim in aIMemories)
                {
                    var exists = this.EntriesDictionary.ContainsKey(aim.MID);
                    if (!exists)
                        this.EntriesDictionary.Add(aim.MID, new RRFObject { Id = aim.MID, obj = aim, Title = aim.Title, LocalFile = aim.LocalFile });

                    this.EntriesDictionary[aim.MID].Bm25Score = aim.Score;
                }
            }

            public void AddUpdateSemanticScore(AIMemorys aIMemories)
            {
                foreach (var aim in aIMemories)
                {
                    var exists = this.EntriesDictionary.ContainsKey(aim.MID);
                    if (!exists)
                        this.EntriesDictionary.Add(aim.MID, new RRFObject { Id = aim.MID, obj = aim, Title = aim.Title, LocalFile = aim.LocalFile });

                    this.EntriesDictionary[aim.MID].SemanticScore= aim.Score;
                }
            }

            public IEnumerable<object> Rank(float k = 60)
            {
                var entries = EntriesDictionary.Values.ToList();

                var entriesSortedForBm25 = entries.OrderByDescending(e => e.Bm25Score).ToList();    
                for(var rank = 0; rank < entriesSortedForBm25.Count; rank++)
                {
                    var id = entriesSortedForBm25[rank].Id;
                    
                    EntriesDictionary[id].RRFScore += (entriesSortedForBm25[rank].Bm25Score * 1) / (k + rank + 1);
                }

                var entriesSortedForSemantic = entries.OrderByDescending(e => e.SemanticScore).ToList();
                for (var rank = 0; rank < entriesSortedForSemantic.Count; rank++)
                {
                    var id = entriesSortedForSemantic[rank].Id;
                    EntriesDictionary[id].RRFScore += (entriesSortedForSemantic[rank].SemanticScore * 1) / (k + rank + 1);
                }

                foreach(var x in EntriesDictionary) // To make to score easier to read
                    x.Value.RRFScore *= 100;

                var entries2 = this.EntriesDictionary.Values.ToList();
                foreach (var rrfo in entries2)
                    ReflectionHelper.SetProperty(rrfo.obj, "Score", rrfo.RRFScore);

                var entriesOrdered = entries2.OrderByDescending(e => e.RRFScore).Select(ee => ee.obj);
                return entriesOrdered;
            }
        }
    }

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

        public IEnumerable<AIMemoryCrossReferenceTable> GetAllCrossReferenceTable()
        {
            using (var db = new LiteDatabase(this.FileName))
            {
                var col = db.GetCollection<AIMemoryCrossReferenceTable>(nameof(AIMemoryCrossReferenceTable));
                var l = col.Query().ToEnumerable();
                return l.ToList();
            }
        }

        public void AddCrossReferenceTable(AIMemoryCrossReferenceTable d)
        {
            var e = GetCrossReferenceTable(d.Name);
            if(e != null)
                DeleteCrossReferenceTable(d.Id);

            using (var db = new LiteDatabase(this.FileName))
            {
                var col = db.GetCollection<AIMemoryCrossReferenceTable>(nameof(AIMemoryCrossReferenceTable));
                col.Insert(d);
            }
        }

        public AIMemoryCrossReferenceTable GetCrossReferenceTable(string name)
        {
            using (var db = new LiteDatabase(this.FileName))
            {
                var col = db.GetCollection<AIMemoryCrossReferenceTable>(nameof(AIMemoryCrossReferenceTable));
                var results = col.Query().Where(x => x.Name == name).ToList();
                if (results.Count > 0)
                    return results[0];
                else
                    return null;
            }
        }

        public void DeleteAllCrossReferenceTable()
        {
            var all = GetAllCrossReferenceTable();
            using (var db = new LiteDatabase(this.FileName))
            {
                var col = db.GetCollection<AIMemoryCrossReferenceTable>(nameof(AIMemoryCrossReferenceTable));
                foreach (var crt in all)
                    col.Delete(crt.Id);
            }
        }

        public void DeleteCrossReferenceTable(ObjectId id)
        {
            using (var db = new LiteDatabase(this.FileName))
            {
                var col = db.GetCollection<AIMemoryCrossReferenceTable>(nameof(AIMemoryCrossReferenceTable));
                col.Delete(id);
            }
        }

        private string LocalizePath(string p, string markdownRootFolder)
        {
            return p.Replace(markdownRootFolder, @".\").Replace(@"\\", @"\").Replace(@"\", @"/").Replace(@" ", @"%20");
        }

        private string GenerateLocalMarkdownLink(string p, string markdownRootFolder)
        {
            return $"[{Path.GetFileName(p)}]({LocalizePath(p, markdownRootFolder)})";
        }

        public string GenerateReportCrossReferenceTable(AIMemoryCrossReferenceTable d, string markdownRootFolder)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"---");
            sb.AppendLine($"## Cross Reference Table: {d.Name}");
            sb.AppendLine($"- Total Entries: {d.Entries.Count}");
            sb.AppendLine($"---");
            sb.AppendLine();
            foreach (var entry in d.Entries)
            {
                sb.AppendLine($"## {d.Name}:  {StringUtil.CapitalizeWords(entry.Key)}");
                foreach(var v in entry.Value) 
                {
                    var aiMemory = GetFromId(new ObjectId(v));
                    if (aiMemory != null)
                        sb.AppendLine($"- Title: {aiMemory.Title}, {aiMemory.ModifiedDate.ToString("yyyy-MM-dd HH")}, {GenerateLocalMarkdownLink(aiMemory.LocalFile, markdownRootFolder)}");
                }
            }
            return sb.ToString();
        }

        public void Add(AIMemory d, string openAiKey = null, string llmApiKey = null)
        {
            ComputeEmbeddingsAndMetaData(d, embeddingsApiKey:openAiKey, llmApiKey: llmApiKey);

            d.Init();

            using (var db = new LiteDatabase(this.FileName))
            {
                var col = db.GetCollection<AIMemory>(nameof(AIMemory));
                //var embeddings = d.Embeddings;
                col.Insert(d.PrepareForSaving());
                //d.Embeddings = embeddings; // Restore the original list after saving
                //d.__embeddingsBuffer = null; // Clear the buffer after saving
            }
        }

        public bool __simulate_embedding_computation__ = false;
        public bool __simulate_metatdata_computation__ = false;

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
                if (__simulate_metatdata_computation__)
                {
                    d.AIMetaData = new AIMetaData { MetaData = new Dictionary<string, List<string>>() };
                }
                else
                {
                    var client = new GenericAI(ApiKey: llmApiKey);
                    var medataDictionary = client.Completions.ExtractMetaDataFromNotes(d.Text, model: model);
                    d.AIMetaData = medataDictionary;
                }
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
                //var embeddings = d.Embeddings;
                col.Update(d.PrepareForSaving());
                //d.Embeddings = embeddings; // Restore the original list after saving
                //d.__embeddingsBuffer = null; // Clear the buffer after saving
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

        const string outputFileName = @"c:\Brainshark\logs\bm25.log";
        void TraceBm25Score(string r) => File.AppendAllText(outputFileName, r + Environment.NewLine);

        public enum HybridSearchResultType
        {
            Undefined,
            Hybrid,
            BM25Only,
            SemanticOnly,
        }

        public class HybridSearchResult
        {
            public string Query { get; set; }
            public HybridSearchResultType Type { get; set; } = HybridSearchResultType.Undefined;
            public bool Succeeded => Exception == null;
            public Exception Exception { get; set; }
            public RRF.RRFRanker RRFRanker { get; set; }
            public AIMemorys Results { get; set; }

            public string GetInformation()
            {
                var sb = new StringBuilder();
                sb.AppendLine($"HybridSearchResult Type:{Type}, Query:{Query}" );

                if (Type == HybridSearchResultType.Hybrid)
                {
                    sb.AppendLine($"Rank");
                    foreach (var r in RRFRanker.EntriesDictionary)
                        sb.AppendLine(r.Value.ToString());
                    sb.AppendLine();
                }
                
                sb.AppendLine($"AIMemory Results");
                foreach (var rr in Results)
                {
                    sb.AppendLine(rr.ToString());
                }
          
                sb.AppendLine("");
                return sb.ToString();
            }
        }

        public HybridSearchResult HybridSearch(
            string query, 
            List<float> embeddingsQuery, 
            float minimumScore = 0.2f,
            float scoreToNotApplyRefining = -1f,  // If we found at least 3 items with score higher than this threshold, we will not apply refining to improve performance, we just return the items
            int scoreToNotApplyRefiningTopK = 3, 
            double reciprocalRankFusionK = 60)
        {
            var z = new HybridSearchResult() { Query = query };
            try
            {
                var allAiMemories = this.GetAll();
                AIMemorys bm25Results = null;
                var isBm25HasStrongResult = ExecuteBm25Search(query, allAiMemories, out bm25Results);
                if (isBm25HasStrongResult)
                {
                    var ranker = new RRF.RRFRanker();
                    ranker.AddUpdateBm25Score(bm25Results);
                    
                    var subAiMemories = allAiMemories.Where(m => bm25Results.Select(b => b.MID).Contains(m.MID)).ToList(); // Speed up the Semantic Search

                    var sResults = this.SimilaritySearch(embeddingsQuery,
                        minimumScore: minimumScore,
                        scoreToNotApplyRefining: scoreToNotApplyRefining,
                        scoreToNotApplyRefiningTopK: scoreToNotApplyRefiningTopK,
                        all: subAiMemories);

                    ranker.AddUpdateSemanticScore(sResults);
                    z.Results = new AIMemorys(ranker.Rank().Cast<AIMemory>().ToList());
                    z.RRFRanker = ranker;
                    z.Type = HybridSearchResultType.Hybrid;
                }
                else
                {
                    z.Results = this.SimilaritySearch(embeddingsQuery,
                       minimumScore: minimumScore,
                       scoreToNotApplyRefining: scoreToNotApplyRefining,
                       scoreToNotApplyRefiningTopK: scoreToNotApplyRefiningTopK);
                    z.Type = HybridSearchResultType.SemanticOnly;
                }
            }
            catch (Exception ex)
            {
                z.Exception = ex;
            }
            return z;
        }

        private static bool ExecuteBm25Search(string query, IEnumerable<AIMemory> allAiMemories, out AIMemorys bm25Results, double reciprocalRankFusionK = 60)
        {
            var aiMemories = new AIMemorys(allAiMemories.ToList());
            var bm25 = new Bm25(aiMemories);
            var scores = bm25.GetScores(query, aiMemories);
            aiMemories = new AIMemorys(aiMemories.Where(d => d.Score > 0).OrderByDescending(d => d.Score).ToList());
            bm25Results = new AIMemorys(bm25.GetStrongScore(aiMemories));
            //var tmpR = bm25.GetStrongScore(bm25Results).ToList();
            return bm25Results.Count > 0;
        }

        public AIMemorys SimilaritySearch(List<float> embeddingsQuery, float minimumScore = 0.2f, 
            float scoreToNotApplyRefining = -1f,  // If we found at least 3 items with score higher than this threshold, we will not apply refining to improve performance, we just return the items
            int scoreToNotApplyRefiningTopK = 3,
            IEnumerable<AIMemory> all = null
            )
        {
            var result = new AIMemorys();
            if(all == null)
                all = this.GetAll();
            foreach (var e in all)
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
                var am = new AIMemorys();
                am.AddRange(aiMemoryWithHighScore.Take(scoreToNotApplyRefiningTopK).OrderByDescending(e => e.Score).ToList());
                am.ForEach(m => { HttpBase.Trace($"Search {m.MID} - {m.Score} - {m.Title}", this); });
                return am;
            }

            var am2 = ReFineResultWithDynamicScore(result);
            am2.ForEach(m => { HttpBase.Trace($"Search {m.MID} - {m.Score} - {m.Title}", this); });
            return am2;
        }

        public IEnumerable<AIMemory> GetAll()
        {
            using (var db = new LiteDatabase(this.FileName))
            {
                var col = db.GetCollection<AIMemory>(nameof(AIMemory));
                var l = col.Query().ToEnumerable();
                return l.ToList();
                //foreach (var m in l)
                //    m.PrepareAfterLoading();
                //return l;
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

