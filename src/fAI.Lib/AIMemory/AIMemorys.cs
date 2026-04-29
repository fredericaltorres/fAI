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
    }
}
