using NAudio.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
namespace fAI
{

    // ─────────────────────────────────────────────────────────────────────────────
    //  BM25 — Okapi BM25 relevance scoring
    //  Target: .NET Standard 2.0
    //
    //  Usage:
    //      var corpus = new List<string> { "the cat sat on the mat", "the dog sat on the log" };
    //      var bm25   = new Bm25(corpus);
    //      double[]   scores = bm25.GetScores("cat sat");
    //      int[]      ranked = bm25.GetTopN("cat sat", n: 5);
    // ─────────────────────────────────────────────────────────────────────────────

    public interface IBm25Document
    {
        string BM25ID { get; set; }    
        string Text { get; set; }
        double Score { get; set; }
        string Title { get; set; }
        string LocalFile { get; set; }
    }

    public class Bm25Document : IBm25Document
    {
        public string BM25ID { get; set; }
        public string Text { get; set; }
        public double Score { get; set; }
        public string Title { get; set; }
        public string LocalFile { get; set; }
    }



    /// <summary>
    /// Okapi BM25 relevance scoring over a fixed document corpus.
    /// </summary>
    public sealed class Bm25
    {
        // ── Tuning parameters ────────────────────────────────────────────────────
        // k1  controls term-frequency saturation  (1.2–2.0 is typical)
        // b   controls document-length normalisation (0 = off, 1 = full)
        private readonly double _k1;
        private readonly double _b;

        // ── Corpus statistics ────────────────────────────────────────────────────
        private readonly int _docCount;                              // N
        private readonly double _avgDocLength;                       // avgdl
        private readonly int[] _docLengths;                          // |d| per document
        private readonly Dictionary<string, int[]> _docFrequency;  // term → doc-freq list
        private readonly Dictionary<string, double> _idfCache;      // term → IDF score

        /// <summary>
        /// Number of documents in the corpus.
        /// </summary>
        public int DocumentCount => _docCount;

        // ── Construction ─────────────────────────────────────────────────────────

        public const double SCORE_NO_MATCH = 0.0;
        public const double SCORE_WEAK_MATCH = 3.0;
        public const double SCORE_STRONG_MATCH = 8.0;
        public const double SCORE_VERY_STRONG_MATCH = 15.0;

        /// <summary>
        /// Builds BM25 index from a list of raw-text documents.
        /// </summary>
        /// <param name="documents">Corpus documents (plain text).</param>
        /// <param name="k1">Term-frequency saturation parameter (default 1.5).</param>
        /// <param name="b">Length-normalisation parameter (default 0.75).</param>
        public Bm25(IList<IBm25Document> documents, double k1 = 1.5, double b = 0.75)
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
            _avgDocLength = _docLengths.Average();

            // build inverted term-frequency index
            // _docFrequency[term][docIndex] = frequency of term in that document
            _docFrequency = BuildIndex(tokenised);

            // pre-compute IDF for every known term
            _idfCache = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in _docFrequency)
                _idfCache[kv.Key] = ComputeIdf(kv.Value);
        }

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a BM25 score for every document in the corpus against the query.
        /// Higher is more relevant. Documents are indexed in corpus order.
        /// </summary>
        public double[] GetScores(string query, IList<IBm25Document> documents)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var scores = new double[_docCount];
            var queryTerms = Tokenize(query);

            foreach (var term in queryTerms)
            {
                if (!_docFrequency.TryGetValue(term, out var tf))
                    continue;   // term not in corpus → contributes 0

                double idf = _idfCache[term];

                for (int d = 0; d < _docCount; d++)
                {
                    double termFreq = tf[d];
                    double docLength = _docLengths[d];

                    // BM25 term-weight formula
                    double numerator = termFreq * (_k1 + 1.0);
                    double denominator = termFreq + _k1 * (1.0 - _b + _b * docLength / _avgDocLength);

                    scores[d] += idf * (numerator / denominator);
                }
            }

            for(int i = 0; i < _docCount; i++)
            {
                documents[i].Score = scores[i];
            }
            return scores.OrderByDescending(s => s).ToArray();
        }

        /// <summary>
        /// Returns the indices of the top-<paramref name="n"/> documents,
        /// sorted from most to least relevant.
        /// </summary>
        public int[] GetTopN(string query, int n, IList<IBm25Document> documents)
        {
            if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n), "n must be positive.");

            double[] scores = GetScores(query, documents);

            return Enumerable
                .Range(0, _docCount)
                .OrderByDescending(i => scores[i])
                .Take(Math.Min(n, _docCount))
                .ToArray();
        }

        /// <summary>
        /// Returns the BM25 score of a single document (by index) for the query.
        /// </summary>
        public double GetScore(string query, int documentIndex, IList<IBm25Document> documents)
        {
            if ((uint)documentIndex >= (uint)_docCount)
                throw new ArgumentOutOfRangeException(nameof(documentIndex));

            return GetScores(query, documents)[documentIndex];
        }

        public double MaxScore(IList<IBm25Document> documents)
        {
            return documents.Max(d => d.Score);
        }

        public IList<IBm25Document> WithinXPercentOfMaxScore(IList<IBm25Document> documents, int percent)
        {
            var maxScore = MaxScore(documents);
            var threshold = maxScore - (maxScore * percent / 100);
            return documents.Where(d => d.Score >= threshold).ToList();
        }

        public IList<IBm25Document> MinimumScore(IList<IBm25Document> documents, double miniumScore)
        {
            return documents.Where(d => d.Score >= miniumScore).ToList();
        }

        public IList<IBm25Document> GetStrongScore(IList<IBm25Document> documents)
        {
            var s = MinimumScore(documents, Bm25.SCORE_STRONG_MATCH).ToList(); // Get only the results that have a score of at least 3.0
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
        private double ComputeIdf(int[] docFreqs)
        {
            int df = docFreqs.Count(f => f > 0);   // number of docs containing term
            return Math.Log((_docCount - df + 0.5) / (df + 0.5) + 1.0);
        }
    }

}