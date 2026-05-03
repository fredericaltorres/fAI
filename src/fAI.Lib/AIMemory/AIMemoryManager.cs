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
                return $"Id: {Id}, Scores [Bm25: {Bm25Score:0.000}, Semantic: {SemanticScore:0.000}, RRF: {RRFScore:0.000}], Title: {Title}, LocalFile: ({LocalFile})";
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

            public IEnumerable<object> Rank(bool applyGapOutlierDetection = false, float k = 60)
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

                entries2 = entries2.OrderByDescending(s => s.RRFScore).ToList();

                if (applyGapOutlierDetection)
                {
                    var scores = entries2.Select(e => e.RRFScore);
                    var gaps = scores.Zip(scores.Skip(1), (a, b) => a - b).ToList();
                    // gaps = [0.1, 0.1, 1.5, 0.5]

                    double meanGap = gaps.Average();               // 0.55
                    double stdDev = AIMemoryManager.StandardDeviation(gaps);                 // ~0.6

                    // Flag gap as significant if it exceeds mean + 1*stdDev
                    double cutoffThreshold = meanGap + 1.0 * stdDev;
                    int cutIndex = Array.FindIndex(gaps.ToArray(), g => g > cutoffThreshold);
                    entries2 = entries2.Take(cutIndex + 1).ToList();
                }

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

        public (float maxScore, float minimumScore) GetReFineResultWithDynamicScores(AIMemorys docInfo)
        {
            var maxScore = docInfo.ToArray().Select(rr => (float)rr.Score).DefaultIfEmpty(0).Max();
            var minimumScore = SimilaritySearchEngine.GetOpenAIEmbeddingDynamicScore(maxScore);
            return (maxScore, minimumScore);
        }

        public AIMemorys ReFineResultWithDynamicScore(AIMemorys docInfo)
        {
            var (maxScore, minimumScore) = GetReFineResultWithDynamicScores(docInfo);

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

            public string GetInformation(string query, float rrfMinimumScore = 2f)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"HybridSearchResult Type:{Type}, rrfMinimumScore: {rrfMinimumScore}, Query:{query}" );

                if (Type == HybridSearchResultType.Hybrid)
                {
                    sb.AppendLine($"Rank");
                    foreach (var r in RRFRanker.EntriesDictionary)
                        sb.AppendLine(r.Value.ToString());
                    sb.AppendLine();
                }
                
                sb.AppendLine($"Final Hybrid Results");
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
            float semanticMinimumScore = 0.2f,
            float semanticScoreToNotApplyRefining = -1f,  // If we found at least 3 items with score higher than this threshold, we will not apply refining to improve performance, we just return the items
            int semanticScoreToNotApplyRefiningTopK = 3, 
            //double reciprocalRankFusionK = 60,
            float bm25MinimumScore = -1, // -1 top 50%, -2 Greater Than Std Deviation, Other > than )
            float rrfMinimumScore = 2f  // Minimum RRF score to consider as a strong match
            )
        {
            var z = new HybridSearchResult() { Query = query };
            try
            {
                var allAiMemories = this.GetAll();
                AIMemorys bm25Results = null;
                var isBm25HasStrongResult = ExecuteBm25Search(query, allAiMemories, out bm25Results, minimumScoreOrMode: bm25MinimumScore);
                if (isBm25HasStrongResult)
                {
                    var ranker = new RRF.RRFRanker();
                    ranker.AddUpdateBm25Score(bm25Results);

                    var sResults = this.SimilaritySearch(embeddingsQuery,
                        minimumScore: semanticMinimumScore,
                        scoreToNotApplyRefining: semanticScoreToNotApplyRefining,
                        scoreToNotApplyRefiningTopK: semanticScoreToNotApplyRefiningTopK,
                        all: allAiMemories); // Similatiry search is executed on the all data set to also return record ignored by BM25 but has high semantic score

                    ranker.AddUpdateSemanticScore(sResults);
                    z.Results = new AIMemorys(ranker.Rank().Cast<AIMemory>().ToList());
                    z.RRFRanker = ranker;
                    z.Type = HybridSearchResultType.Hybrid;
                }
                else
                {
                    z.Results = this.SimilaritySearch(embeddingsQuery,
                       minimumScore: semanticMinimumScore,
                       scoreToNotApplyRefining: semanticScoreToNotApplyRefining,
                       scoreToNotApplyRefiningTopK: semanticScoreToNotApplyRefiningTopK);
                    z.Type = HybridSearchResultType.SemanticOnly;
                }
                z.Results = new AIMemorys(z.Results.Where(r => r.Score >= rrfMinimumScore).ToList());

            }
            catch (Exception ex)
            {
                z.Exception = ex;
            }
            return z;
        }


        public static float StandardDeviation(List<float> values)
        {
            if (values == null || values.Count == 0)
                throw new ArgumentException("List must not be null or empty.");

            float mean = values.Average();
            float sumOfSquaredDiffs = values.Sum(v => (v - mean) * (v - mean));

            return (float)Math.Sqrt(sumOfSquaredDiffs / values.Count);
        }

        public static float StandardDeviation(List<double> values)
        {
            if (values == null || values.Count == 0)
                throw new ArgumentException("List must not be null or empty.");

            double mean = values.Average();
            double sumOfSquaredDiffs = values.Sum(v => (v - mean) * (v - mean));

            return (float)Math.Sqrt(sumOfSquaredDiffs / values.Count);
        }

        private bool ExecuteBm25Search(string query, IEnumerable<AIMemory> allAiMemories, out AIMemorys bm25Results, 
            float minimumScoreOrMode = -1 // -1 top 50%, -2 Greater Than Std Deviation, Other > than 
            )
        {
            var aiMemories = new AIMemorys(allAiMemories.ToList());
            var bm25 = new Bm25(aiMemories);
            var scores = bm25.GetScores(query, aiMemories);
            var bm25MiniScore = 0.5f;
            aiMemories = new AIMemorys(aiMemories.Where(d => d.Score > bm25MiniScore).OrderByDescending(d => d.Score).ToList());
            var minimumScoreOrModeStr = minimumScoreOrMode == -1 ? "Top 50%" : minimumScoreOrMode == -2 ? "Greater Than Std Deviation" : minimumScoreOrMode.ToString();

            TraceAIMemorys(aiMemories, $"BM25(1): query: {query}");

            if (minimumScoreOrMode == -1)
            {
                bm25Results = new AIMemorys(bm25.GetStrongScore(aiMemories, percent: 50f /*default*/ ));
            }
            else if (minimumScoreOrMode == -2) // Return value greater than standard deviation 
            {
                var scores2 = aiMemories.Select(d => d.Score).ToList();
                bm25Results = new AIMemorys(bm25.GetStrongScore(aiMemories, minimumScore: StandardDeviation(scores2)));
            }
            else
            {
                bm25Results = new AIMemorys(bm25.GetStrongScore(aiMemories, minimumScore: minimumScoreOrMode));
                if(bm25Results.Count == 0) // minimumScoreOrMode is too high
                {
                    var retryTopPercent = 20f;
                    Trace($"minimumScoreOrMode: {minimumScoreOrMode} is too low, retry with top {retryTopPercent}%");
                    bm25Results = new AIMemorys(bm25.GetStrongScore(aiMemories, percent: retryTopPercent /*default*/ ));
                }
            }

            TraceAIMemorys(bm25Results, $"BM25(2): query:{query}, minimumScoreOrMode:{minimumScoreOrModeStr}");

            return bm25Results.Count > 0;
        }

        private void TraceAIMemorys(AIMemorys am, string text) 
        {
            var x = 0;
            HttpBase.Trace(text, this);
            am.ForEach((m) => { HttpBase.Trace($" [{x++}] {m.MID} - {m.Score:0.000} - {m.Title} - ({m.LocalFile})", this); });
            HttpBase.Trace("", this);
        }

        private void Trace(string text)
        {
            HttpBase.Trace(text, this);
        }

        public AIMemorys SimilaritySearch(

            List<float> embeddingsQuery, float minimumScore = 0.2f, 

            // mode 0: default

            // mode 1
            float scoreToNotApplyRefining = -1f,  // If we found at least 3 items with score higher than this threshold, we will not apply refining to improve performance, we just return the items
            int scoreToNotApplyRefiningTopK = 3,

            // mode 2
            float topBestScorePercent = -1f, // If the best score is higher than this threshold, we will consider it as a strong match and we will not apply refining to improve performance, we just return the items
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

            var x = 0;

            // Mode 3, return the top X% best score
            if (topBestScorePercent != -1f)
            {
                var bestScore = result.Select(r => (float)r.Score).DefaultIfEmpty(0).Max();
                var threshHold = bestScore - (bestScore * topBestScorePercent / 100f);
                var aiMemoryWithHighScore = result.Where(rr => rr.Score >= threshHold).ToList();
                var am = new AIMemorys(aiMemoryWithHighScore.OrderByDescending(e => e.Score).ToList());

                TraceAIMemorys(am, $"Semantic topBestScorePercent: {topBestScorePercent}");
                return am;
            }

            // Mode 2, Return all score greater than the scoreToNotApplyRefining, Top K if there are too many
            if (scoreToNotApplyRefining != -1)
            {
                var aiMemoryWithHighScore = result.Where(rr => rr.Score >= scoreToNotApplyRefining).OrderByDescending(e => e.Score).ToList();
                var am = new AIMemorys();
                am.AddRange(aiMemoryWithHighScore.Take(scoreToNotApplyRefiningTopK).OrderByDescending(e => e.Score).ToList());
                TraceAIMemorys(am, $"Semantic scoreToNotApplyRefining: {scoreToNotApplyRefining}, scoreToNotApplyRefiningTopK: {scoreToNotApplyRefiningTopK}");
                return am;
            }

            // Mode 1, Default, return % of the best score based on the default established for open ai embedding model
            var (maxScore, minimumScore2) = GetReFineResultWithDynamicScores(result);
            var am2 = ReFineResultWithDynamicScore(result);

            TraceAIMemorys(am2, $"Semantic ReFineResultWithDynamicScore maxScore: {maxScore}, minimumScore: {minimumScore2}");

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

