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
            public AIMemorys Bm25Results { get; set; }
            public AIMemorys SemanticResults { get; set; }
            public AIMemorys RRFResults { get; set; }
            public bool Succeeded => Exception == null;
            public Exception Exception { get; set; }

            public string GetInformation()
            {
                var sb = new StringBuilder();
                sb.AppendLine($"HybridSearchResult Type:{Type}, Query:{Query}" );

                sb.AppendLine().AppendLine($"{this.Bm25Results.Count} BM25 Results");
                foreach(var z in this.Bm25Results)
                    sb.AppendLine($"BM25 - {z.MID} - {z.Score} [rrf:{z.RRFScore}, bm25:{z.Bm25Score}, sema:{z.SemanticScore}] - {z.Title} - ({z.LocalFile})");

                sb.AppendLine().AppendLine($"{this.SemanticResults.Count} Semantic Results");
                foreach (var z in this.SemanticResults)
                    sb.AppendLine($"SEMA - {z.MID} - {z.Score} [rrf:{z.RRFScore}, bm25:{z.Bm25Score}, sema:{z.SemanticScore}] - {z.Title} - ({z.LocalFile})");

                sb.AppendLine().AppendLine($"{this.RRFResults.Count} Final Results");
                foreach (var z in this.RRFResults)
                    sb.AppendLine($"RRF  - {z.MID} - {z.Score} [rrf:{z.RRFScore}, bm25:{z.Bm25Score}, sema:{z.SemanticScore}] - {z.Title} - ({z.LocalFile})");

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
                IList<IBm25Document> bm25Results = null;
                var isBm25HasStrongResult = ExecuteBm25Search(query, allAiMemories, out bm25Results);
                if (isBm25HasStrongResult)
                {
                    for (var i = 0; i < bm25Results.Count; i++) // Reciprocal Rank Fusion(RRF)
                    {
                        bm25Results[i].Bm25Score = 1 / (reciprocalRankFusionK + bm25Results[i].Score);
                    }

                    //results = bm25.WithinXPercentOfMaxScore(results.ToList(), 10); // Get only the 10 10% of the results that are closest to the max score
                    var bm25ResultIds = bm25Results.Select(rr => rr.BM25ID).ToList();
                    z.Bm25Results = new AIMemorys(bm25Results.Cast<AIMemory>().ToList(), clone: true); // Do it now the Score property is the BM25 one

                    var sResults = this.SimilaritySearch(embeddingsQuery,
                        minimumScore: minimumScore,
                        scoreToNotApplyRefining: scoreToNotApplyRefining,
                        scoreToNotApplyRefiningTopK: scoreToNotApplyRefiningTopK,
                        all: allAiMemories);

                    for (var i = 0; i < bm25Results.Count; i++) // Reciprocal Rank Fusion(RRF)
                    {
                        if (i < sResults.Count)
                        {
                            sResults[i].SemanticScore = 1 / (reciprocalRankFusionK + bm25Results[i].Score);
                            sResults[i].RRFScore = sResults[i].SemanticScore + sResults[i].Bm25Score;
                        }
                    }

                    z.RRFResults = new AIMemorys(sResults.OrderByDescending(ss => ss.RRFScore).ToList());
                    z.SemanticResults = sResults;
                    z.Type = HybridSearchResultType.Hybrid;
                }
                else
                {
                    z.SemanticResults = this.SimilaritySearch(embeddingsQuery,
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

        private static bool ExecuteBm25Search(string query, IEnumerable<AIMemory> allAiMemories, out IList<IBm25Document> bm25Results, double reciprocalRankFusionK = 60)
        {
            var corpus = new List<IBm25Document>() as IList<IBm25Document>;
            foreach (var aiM in allAiMemories)
                corpus.Add(aiM);
            var bm25 = new Bm25(corpus);
            bm25.GetScores(query, corpus);
            corpus = corpus.OrderByDescending(d => d.Score).ToList();
            bm25Results = corpus;
            var tmpR = bm25.GetStrongScore(bm25Results).ToList();
            return tmpR.Count > 0;
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
                if(aiMemoryWithHighScore.Count >= scoreToNotApplyRefiningTopK)
                {
                    var am = new AIMemorys();
                    am.AddRange(aiMemoryWithHighScore.OrderByDescending(e => e.Score).ToList());

                    am.ForEach(m =>
                    {
                        HttpBase.Trace($"Search {m.MID} - {m.Score} - {m.Title}", this);
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
