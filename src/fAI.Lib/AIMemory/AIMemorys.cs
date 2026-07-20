using fAI.RRF;
using fAI.Util.Strings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace fAI
{
    public class AIMemorys : List<AIMemory>
    {
        public AIMemorys(AIMemorys aiMemories, bool clone = false) : this((IList<AIMemory>)aiMemories, clone)
        {

        }
        public AIMemorys (IList<AIMemory> aiMemories, bool clone = false) : base()
        {
            if (clone)
            {
                foreach (var aiMemory in aiMemories)
                    this.Add(aiMemory.Clone());
            }
            else
            {
                this.AddRange(aiMemories);
            }
        }
        public AIMemorys FilterForRequiredKeyWords(string query, string dataSourceInfo)
        {
            var requiredKeywords = StringUtil.ExtractBackTilda(query);
            if (requiredKeywords.Count > 0)
            {
                HttpBase.Trace($"Filtering {dataSourceInfo} for required keywords: {String.Join(", ", requiredKeywords)}", this);
                var filtered = this.Where(m => StringUtil.ContainsAllKeywords(requiredKeywords, m.Text)).ToList();
                return new AIMemorys(filtered);
            }
            return this;
        }

        public AIMemorys() : base()
        {
        }

        public string GetTopPercentInfo(int percent)
        {
            var maxScore = this.Max(m => m.Score);
            var threshold = maxScore - (maxScore * percent / 100.0);
            var count = new AIMemorys(this.Where(m => m.Score >= threshold).ToList()).Count;

            return $"Top {percent}%, maxScore: {maxScore:0.000}, threshold: {threshold:0.000}, TotalCount: {this.Count}, NewCount: {count} ";
        }

        public AIMemorys GetTopPercent(int percent)
        {
            var maxScore = this.Max(m => m.Score);
            var threshold = maxScore - (maxScore * percent / 100.0);
            return new AIMemorys(this.Where(m => m.Score >= threshold).ToList());
        }

        internal AIMemorys LoadFromFiles(List<string> localFiles)
        {
            foreach (var file in localFiles)
            {
                var aiMemory = AIMemory.LoadFromFile(file);
                if (aiMemory != null)
                    this.Add(aiMemory);
            }
            return this;
        }

        public void TraceEntries( string text)
        {
            var x = 0;
            HttpBase.Trace(text, this);
            this.ForEach((k) => {
                HttpBase.Trace($" [{x++}] - {k.Id} - Score: {k.Score:0.000} - {k.Title} - ({k.LocalFile})", this);
            });
            HttpBase.Trace("", this);
        }
    }
}
