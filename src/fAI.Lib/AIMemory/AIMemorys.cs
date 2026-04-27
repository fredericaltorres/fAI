using System.Collections.Generic;

namespace fAI
{
    public class AIMemorys : List<AIMemory>
    {
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
    }
}
