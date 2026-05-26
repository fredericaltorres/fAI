using DynamicSugar;
using fAI.OpenAIModel.ImageResponseGpt;
using fAI.Util.Strings;
using fAI.VectorDB;
using LiteDB;
//using Mistral.SDK.DTOs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Xml.Linq;
using static DynamicSugar.Tokenizer;
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
            private void TraceEntries(List<RRFObject> entries, string text)
            {
                var x = 0;
                Trace(text);
                entries.ForEach((k) => { 
                    HttpBase.Trace($" [{x++}] - {k.Id} - RRFScore: {k.RRFScore:0.000} - {k.Title} - ({k.LocalFile})", this); 
                });
                Trace("");
            }

            private void Trace(string text)
            {
                HttpBase.Trace(text, this);
            }

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

                TraceEntries(entries2, "Ranked:");

                if (applyGapOutlierDetection && entries2.Count > 1)
                {
                    var scores = entries2.Select(e => e.RRFScore);
                    var gaps = scores.Zip(scores.Skip(1), (a, b) => a - b).ToList();
                    if (gaps.Count > 0)
                    {
                        float meanGap = gaps.Average();
                        float stdDev = AIMemoryManager.StandardDeviation(gaps);
                        float cutoffThreshold = meanGap + 1.0f * stdDev; // Flag gap as significant if it exceeds mean + 1*stdDev
                        int cutIndex = Array.FindIndex(gaps.ToArray(), g => g >= cutoffThreshold);
                        if (cutIndex < 0)
                            cutIndex = 0;
                        entries2 = entries2.Take(cutIndex + 1).ToList();
                    }

                    TraceEntries(entries2, "applyGapOutlierDetection: true");
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
        ImageFile,
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
            return SimilaritySearchEngine.ToVector(text, openAiKey);
        }
       
        public (bool, GenericAICompletions.GenericAIUsage, LiteDB.ObjectId) AddUpdate(AIMemory d, string localFile, string openAiKey = null, string llmApiKey = null, bool clearEmbeddings = false)
        {
            LiteDB.ObjectId id = new LiteDB.ObjectId();

            if (d.Type == PublishedDocumentInfoType.ImageFile) // Is image
            {
                d.MediaBase64 = Convert.ToBase64String(File.ReadAllBytes(localFile));
            }

            var u = new GenericAICompletions.GenericAIUsage("","","");
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
                    existingAIMemory.MediaBase64 = d.MediaBase64;

                    if (clearEmbeddings)
                    {
                        existingAIMemory.Embeddings.Clear();
                    }

                    var (rr, uu) = ComputeEmbeddingsAndMetaData(existingAIMemory, embeddingsOpenAIApiKey: openAiKey, llmApiKey: llmApiKey);

                    d.Embeddings = existingAIMemory.Embeddings;
                    d.AIMetaData = existingAIMemory.AIMetaData;

                    u = uu;
                    Update(existingAIMemory);
                    id = existingAIMemory.Id;
                }
                else
                {
                    var (uu, newId) = Add(d, openAiKey);
                    u = uu;
                    id = newId;
                }
            }
            catch (Exception ex)
            {
                r = false;
            }
            return (r, u, id);
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

        public (GenericAICompletions.GenericAIUsage, LiteDB.ObjectId) Add(AIMemory d, string openAiKey = null, string llmApiKey = null)
        {
            LiteDB.ObjectId id = new LiteDB.ObjectId();
            if (d.Type == PublishedDocumentInfoType.ImageFile)
            {
                d.MediaBase64 = Convert.ToBase64String(File.ReadAllBytes(d.LocalFile));
            }

            var (r,u) = ComputeEmbeddingsAndMetaData(d, embeddingsOpenAIApiKey:openAiKey, llmApiKey: llmApiKey);

            d.Init();

            using (var db = new LiteDatabase(this.FileName))
            {
                var col = db.GetCollection<AIMemory>(nameof(AIMemory));
                var newId = col.Insert(d.PrepareForSaving());
                id = newId;
            }

            return (u, id);
        }

        public bool __simulate_embedding_computation__ = false;
        public bool __simulate_metadata_computation__ = false;

        //Model Input               Price(per 1M)    Output Price(per 1M)   Context Window
        //Gemini 2.0 Flash	        $0.10	        $0.40	                1 Million     DEPRECATED JUNE 2026
        //Gemini 2.5 Flash	        $0.30	        $2.50	                1 Million
        //Gemini 3.1 Flash-Lite	    $0.25	        $1.50	                1 Million       
        //Gemini 3 Flash Preview	$0.50	        $3.00	                1 Million
        //Gemini 3 Flash	        $0.50	        $3.00	1 Million

        public const string DEFAULT_MODEL_FOR_META_DATA_EXTRACTION = "gemini-3.1-flash-lite";// "gemini-3.1-flash-Lite"

        public (bool, GenericAICompletions.GenericAIUsage) ComputeEmbeddingsAndMetaData(AIMemory d, 
            string embeddingsOpenAIApiKey = null, 
            string llmApiKey = null,
            string model = DEFAULT_MODEL_FOR_META_DATA_EXTRACTION
            )
        {
            Trace($"[{nameof(ComputeEmbeddingsAndMetaData)}]embeddingsOpenAIApiKey: {embeddingsOpenAIApiKey}, llmApiKey: {llmApiKey}, model: {model}");
            var r1 = ComputeEmbeddings(d, embeddingsOpenAIApiKey);
            var r2 = ExtractMetaDataFromText(d, model, llmApiKey);

            return (r1 && r2.Item1, r2.Item2);
        }

        public (bool, GenericAICompletions.GenericAIUsage) ExtractMetaDataFromText(AIMemory d, string model = DEFAULT_MODEL_FOR_META_DATA_EXTRACTION, string llmApiKey = null)
        {
            try
            {
                var client = new GenericAI(ApiKey: llmApiKey);
                if (__simulate_metadata_computation__)
                {
                    d.AIMetaData = new AIMetaData { MetaData = new Dictionary<string, List<string>>() {
                        ["a"] = new List<string> { "b" }
                    } };
                    return (false, new GenericAICompletions.GenericAIUsage("", "", "") { InputTokens=1, OutputTokens=1 });
                }
                else
                {
                    var medataDictionary = client.Completions.ExtractMetaDataFromNotes(d.Text, model: model);
                    d.AIMetaData = medataDictionary;
                }
                return (true, client.Completions.LastUsage);
            }
            catch (Exception ex)
            {
                return (false, null);
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

            float bm25ScoreOrMode = -3, // -1 top 50%, -2 Greater Than Std Deviation, -3 ApplyGapOutlierDetection, Other > than )
            float bm25MinimumScore = 0.3f,

            float semanticMinimumScore = 0.25f,
            
            float rrfMinimumScore = 1f,  // Minimum RRF score to consider as a strong match
            bool  rffApplyGapOutlierDetection = true
            )
        {
            var z = new HybridSearchResult() { Query = query };
            try
            {
                var allAiMemories = this.GetAll();
                AIMemorys bm25Results = null;
                var isBm25HasStrongResult = ExecuteBm25Search(query, allAiMemories, out bm25Results, minimumScoreMode: bm25ScoreOrMode, bm25MinimumScore: bm25MinimumScore);
                if (isBm25HasStrongResult)
                {
                    var ranker = new RRF.RRFRanker();
                    ranker.AddUpdateBm25Score(bm25Results);

                    var sResults = this.SimilaritySearch(embeddingsQuery,
                        minimumScore: semanticMinimumScore,
                        all: allAiMemories); // Similatiry search is executed on the all data set to also return record ignored by BM25 but has high semantic score

                    ranker.AddUpdateSemanticScore(sResults);

                    var RANK_K = 60f;
                    z.Results = new AIMemorys(ranker.Rank(rffApplyGapOutlierDetection, RANK_K).Cast<AIMemory>().ToList());
                    z.RRFRanker = ranker;
                    z.Type = HybridSearchResultType.Hybrid;
                }
                else
                {
                    z.Results = this.SimilaritySearch(embeddingsQuery, minimumScore: semanticMinimumScore);
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
            float minimumScoreMode = -1, // -1 top 50%, -2 Greater Than Std Deviation, Other > than ,
            float bm25MinimumScore = 0.3f
            )
        {
            var aiMemories = new AIMemorys(allAiMemories.ToList());
            var bm25 = new Bm25(aiMemories);
            var scores = bm25.GetScores(query, aiMemories);
            Trace($"BM25 - INDEX REPORT");

            var maxScore = aiMemories.Select(s => s.Score).Max();
            bm25MinimumScore = Math.Min(bm25MinimumScore, maxScore);
            aiMemories = new AIMemorys(aiMemories.Where(d => d.Score >= bm25MinimumScore).OrderByDescending(d => d.Score).ToList());
            var minimumScoreOrModeStr = minimumScoreMode == -1 ? "Top 50%" : minimumScoreMode == -2 ? "Greater Than Std Deviation" : minimumScoreMode.ToString();
            TraceAIMemorys(aiMemories, $"BM25(1): query: {query}");

            if (minimumScoreMode == -1)
            {
                bm25Results = new AIMemorys(bm25.GetStrongScore(aiMemories, percent: 50f /*default*/ ));
            }
            else if (minimumScoreMode == -2) // Return value greater than standard deviation 
            {
                var scores2 = aiMemories.Select(d => d.Score).ToList();
                bm25Results = new AIMemorys(bm25.GetStrongScore(aiMemories, minimumScore: StandardDeviation(scores2)));
            }
            else if (minimumScoreMode == -3) // ApplyGapOutlierDetection
            {
                //Gap Outlier Detection(Most Robust)
                //Treat the gaps themselves as a distribution and find statistical outliers:

                bm25Results = new AIMemorys(aiMemories);

                var scores3 = bm25Results.Select(e => e.Score);
                var gaps = scores3.Zip(scores3.Skip(1), (a, b) => a - b).ToList();
                if (gaps.Count > 0)
                {
                    float meanGap = gaps.Average();
                    float stdDev = AIMemoryManager.StandardDeviation(gaps);
                    float cutoffThreshold = meanGap + 1.0f * stdDev; // Flag gap as significant if it exceeds mean + 1*stdDev
                    int cutIndex = Array.FindIndex(gaps.ToArray(), g => g >= cutoffThreshold);
                    if (cutIndex < 0)
                        cutIndex = 0;
                    bm25Results = new AIMemorys(bm25Results.ToList().Take(cutIndex + 1).ToList());
                }
                TraceAIMemorys(bm25Results, $"BM25(GapOutlierDetection): query:{query}, minimumScoreOrMode:{minimumScoreOrModeStr}");
            }
            else
            {
                bm25Results = new AIMemorys(bm25.GetStrongScore(aiMemories, minimumScore: minimumScoreMode));
                if(bm25Results.Count == 0) // minimumScoreOrMode is too high
                {
                    var retryTopPercent = 20f;
                    Trace($"minimumScoreOrMode: {minimumScoreMode} is too low, retry with top {retryTopPercent}%");
                    bm25Results = new AIMemorys(bm25.GetStrongScore(aiMemories, percent: retryTopPercent /*default*/ ));
                }
            }

            TraceAIMemorys(bm25Results, $"BM25(2): query:{query}, minimumScoreOrMode:{minimumScoreOrModeStr}");

            return bm25Results.Count > 0;
        }

        private void TraceAIMemorys(AIMemorys am, string text) 
        {
            var x = 0;
            Trace(text);
            am.ForEach((m) => {
                HttpBase.Trace($" [{x++}] {m.MID} - {m.Score:0.000} - {m.Title} - ({m.LocalFile})", this);
            });
            Trace("");
        }

        private void Trace(string text, [CallerMemberName] string methodName = "")
        {
            HttpBase.Trace(text, this, methodName: methodName);
        }

        public AIMemorys SimilaritySearch(
            List<float> embeddingsQuery, 
            float minimumScore = 0.2f,
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

