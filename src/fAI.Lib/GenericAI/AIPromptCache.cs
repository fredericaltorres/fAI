using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace fAI
{
    public class AIPromptCacheEntry
    {
        public string Prompt { get; set; }
        public string Response { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class AIPromptCache
    {
        private const string CacheFileName = @"C:\TEMP\AIPromptCache.json";
        private const int CacheExpiryHours = 24;

        private List<AIPromptCacheEntry> _entries;

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        private static AIPromptCache __instance = null;

        public static AIPromptCache Instance
        {
            get
            {
                if (__instance == null)
                    __instance = new AIPromptCache();
                return __instance;
            }
        }

        public AIPromptCache()
        {
            _entries = LoadFromDisk();
            Purge();
        }

        /// <summary>
        /// Adds a new prompt/response pair to the cache and persists it to disk.
        /// If an identical prompt already exists, it will be replaced.
        /// </summary>
        public void Add(string prompt, string response)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("Prompt cannot be null or empty.", nameof(prompt));

            // Remove any existing entry for the same prompt
            _entries.RemoveAll(e => e.Prompt == prompt);

            _entries.Add(new AIPromptCacheEntry
            {
                Prompt = prompt,
                Response = response,
                Timestamp = DateTime.UtcNow
            });

            SaveToDisk();
        }

        /// <summary>
        /// Returns the cached response for the given prompt, or null if not found / expired.
        /// </summary>
        public string Get(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("Prompt cannot be null or empty.", nameof(prompt));

            var entry = _entries.FirstOrDefault(e => e.Prompt == prompt);

            if (entry is null)
                return null;

            if (IsExpired(entry))
            {
                _entries.Remove(entry);
                SaveToDisk();
                return null;
            }

            return entry.Response;
        }

        // -------------------------------------------------------------------------
        // Private helpers
        // -------------------------------------------------------------------------

        private static bool IsExpired(AIPromptCacheEntry entry) =>
            (DateTime.UtcNow - entry.Timestamp).TotalHours >= CacheExpiryHours;

        /// <summary>Removes all entries that have passed the 24-hour window.</summary>
        private void Purge()
        {
            int removed = _entries.RemoveAll(IsExpired);
            if (removed > 0)
                SaveToDisk();
        }

        private List<AIPromptCacheEntry> LoadFromDisk()
        {
            try
            {
                if (!File.Exists(CacheFileName))
                    return new List<AIPromptCacheEntry>();

                string json = File.ReadAllText(CacheFileName);
                return JsonSerializer.Deserialize<List<AIPromptCacheEntry>>(json, _jsonOptions)
                       ?? new List<AIPromptCacheEntry>();
            }
            catch (Exception ex)
            {
                var m = $"[AIPromptCache] Could not load cache: {ex.Message}";
                HttpBase.TraceError(m, this);
                return new List<AIPromptCacheEntry>();
            }
        }

        private void SaveToDisk()
        {
            try
            {
                var d = Path.GetDirectoryName(CacheFileName);
                if(!Directory.Exists(d))
                    Directory.CreateDirectory(d);

                string json = JsonSerializer.Serialize(_entries, _jsonOptions);
                File.WriteAllText(CacheFileName, json);
            }
            catch (Exception ex)
            {
                var m = $"[AIPromptCache] Could not save cache: {ex.Message}";
                HttpBase.TraceError(m, this);
                throw new ApplicationException(m, ex);
            }
        }
    }
}
