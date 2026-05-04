using NAudio.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
namespace fAI
{

    public class WordCounter
    {
        private Dictionary<string, int> _wordCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public void AddWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                throw new ArgumentException("Word cannot be null or empty.", nameof(word));

            if (_wordCounts.ContainsKey(word))
                _wordCounts[word]++;
            else
                _wordCounts[word] = 1;
        }

        public int GetCount(string word) => _wordCounts.TryGetValue(word, out int count) ? count : 0;

        public IReadOnlyDictionary<string, int> GetAllWords() => _wordCounts;

        public string GetSummary()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Word Counts:");
            sb.AppendLine(new string('-', 25));
            foreach (var entry in _wordCounts)
                sb.AppendLine($"  {entry.Key,-15} : {entry.Value}");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Okapi BM25 relevance scoring over a fixed document corpus.
    /// </summary>
    public sealed class Bm25
    {
        // ── Tuning parameters ────────────────────────────────────────────────────
        // k1  controls term-frequency saturation  (1.2–2.0 is typical)
        // b   controls document-length normalisation (0 = off, 1 = full)
        private readonly float _k1;
        private readonly float _b;

        // ── Corpus statistics ────────────────────────────────────────────────────
        private readonly int _docCount;                              // N
        private readonly float _avgDocLength;                       // avgdl
        private readonly int[] _docLengths;                          // |d| per document
        private readonly Dictionary<string, int[]> _docFrequency;  // term → doc-freq list
        private readonly Dictionary<string, float> _idfCache;      // term → IDF score

        public List<string> GetIndexReport()
        {
            var r = new List<string>();
            foreach (var kv in _docFrequency)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"Term: {kv.Key}");
                sb.AppendLine($" IDF: {_idfCache[kv.Key]}");
                sb.AppendLine($" DocFs: {string.Join(",", kv.Value)}");
                r.Add(sb.ToString());
            }
            return r;
        }

        /// <summary>
        /// Number of documents in the corpus.
        /// </summary>
        public int DocumentCount => _docCount;

        // ── Construction ─────────────────────────────────────────────────────────

        /// <summary>
        /// Builds BM25 index from a list of raw-text documents.
        /// </summary>
        /// <param name="documents">Corpus documents (plain text).</param>
        /// <param name="k1">Term-frequency saturation parameter (default 1.5).</param>
        /// <param name="b">Length-normalisation parameter (default 0.75).</param>
        public Bm25(AIMemorys documents, float k1 = 1.5f, float b = 0.75f)
        {
            if (documents == null) throw new ArgumentNullException(nameof(documents));
            if (documents.Count == 0) throw new ArgumentException("Corpus must not be empty.", nameof(documents));

            _k1 = k1;
            _b = b;
            _docCount = documents.Count;

            // tokenise every document once
            var tokenised = documents.Select(d => d.Text)
                .Select(Tokenize)
                .ToArray();

            // document lengths
            _docLengths = tokenised.Select(t => t.Length).ToArray();
            _avgDocLength = (float)_docLengths.Average();

            // build inverted term-frequency index
            // _docFrequency[term][docIndex] = frequency of term in that document
            _docFrequency = BuildIndex(tokenised);

            // pre-compute IDF for every known term
            _idfCache = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in _docFrequency)
                _idfCache[kv.Key] = ComputeIdf(kv.Value);
        }

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a BM25 score for every document in the corpus against the query.
        /// Higher is more relevant. Documents are indexed in corpus order.
        /// </summary>
        public float[] GetScores(string query, AIMemorys documents)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var scores = new float[_docCount];
            var queryTerms = Tokenize(query);

            foreach (var term in queryTerms)
            {
                if (!_docFrequency.TryGetValue(term, out var tf))
                    continue;   // term not in corpus → contributes 0

                float idf = _idfCache[term];

                for (int d = 0; d < _docCount; d++)
                {
                    float termFreq = tf[d];
                    float docLength = _docLengths[d];

                    // BM25 term-weight formula
                    float numerator = termFreq * (_k1 + 1.0f);
                    float denominator = termFreq + _k1 * (1.0f - _b + _b * docLength / _avgDocLength);

                    scores[d] += idf * (numerator / denominator);
                }
            }

            for(int i = 0; i < _docCount; i++)
            {
                documents[i].Score = scores[i] / 10f; /* Divide by 10 so the number are in the nsame range than the OpenAI Semantic search*/
            }
            return scores.OrderByDescending(s => s).ToArray();
        }

        /// <summary>
        /// Returns the indices of the top-<paramref name="n"/> documents,
        /// sorted from most to least relevant.
        /// </summary>
        public int[] GetTopN(string query, int n, AIMemorys documents)
        {
            if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n), "n must be positive.");

            float[] scores = GetScores(query, documents);

            return Enumerable
                .Range(0, _docCount)
                .OrderByDescending(i => scores[i])
                .Take(Math.Min(n, _docCount))
                .ToArray();
        }

        /// <summary>
        /// Returns the BM25 score of a single document (by index) for the query.
        /// </summary>
        public float GetScore(string query, int documentIndex, AIMemorys documents)
        {
            if ((uint)documentIndex >= (uint)_docCount)
                throw new ArgumentOutOfRangeException(nameof(documentIndex));

            return GetScores(query, documents)[documentIndex];
        }

        public float MaxScore(AIMemorys documents)
        {
            if(documents.Count > 0)
                return documents.Max(d => d.Score);
            else 
                return 0f;
        }

        public IList<AIMemory> WithinXPercentOfMaxScore(AIMemorys documents, int percent)
        {
            var maxScore = MaxScore(documents);
            var threshold = maxScore - (maxScore * percent / 100);
            return documents.Where(d => d.Score >= threshold).ToList();
        }

        public AIMemorys MinimumScore(AIMemorys documents, float miniumScore)
        {
            return new AIMemorys(documents.Where(d => d.Score >= miniumScore).ToList());
        }

        public IList<AIMemory> GetStrongScore(AIMemorys documents, float percent = 50f, float minimumScore = -1f)
        {
            var maxScore = MaxScore(documents);
            var threshold = maxScore - (maxScore * percent / 100f);

            if(minimumScore != -1f)
                threshold = minimumScore;

            var s = MinimumScore(documents, threshold).ToList(); // Get only the results that have a score of at least 3.0
            return s.OrderByDescending(d => d.Score).ToList(); // Order the results by score, highest first
        }

        // ── Internal helpers ─────────────────────────────────────────────────────

        /// <summary>
        /// Lowercases and splits on non-word characters.
        /// Extend this method to add stemming / stop-word removal as needed.
        /// </summary>
        private static string[] Tokenize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Array.Empty<string>();

            return text
                .ToLowerInvariant()
                .Split(new[] { ' ', '\t', '\r', '\n', '.', ',', '!', '?', ';', ':', '-', '_', '(', ')', '|', '"', '\'', '+', '/', '*', '[', ']', '{', '}', '<', '>', '=', '@', '#', '$', '%', '^', '&','`'  },
                       StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Builds a term → per-document frequency array index.
        /// </summary>
        private static Dictionary<string, int[]> BuildIndex(string[][] tokenised)
        {
            int n = tokenised.Length;
            var index = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);

            for (int d = 0; d < n; d++)
            {
                foreach (var term in tokenised[d])
                {
                    if (!index.TryGetValue(term, out var freqs))
                    {
                        freqs = new int[n];
                        index[term] = freqs;
                    }
                    freqs[d]++;
                }
            }

            

            return index;
        }

        /// <summary>
        /// Robertson–Spärck Jones IDF (with smoothing to avoid division by zero):
        ///   IDF = ln( (N - df + 0.5) / (df + 0.5) + 1 )
        /// </summary>
        private float ComputeIdf(int[] docFreqs)
        {
            int df = docFreqs.Count(f => f > 0);   // number of docs containing term
            return (float)Math.Log((_docCount - df + 0.5f) / (df + 0.5f) + 1.0f);
        }
    }
}