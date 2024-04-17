using System.Collections.Generic;
using System.IO;

namespace fAI
{
    public class EmbeddingRecord
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public List<float> Embedding { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public double Score { get; set; }

        public static void ToJsonFile(List<EmbeddingRecord> embeddingRecords, string fileName)
        {
            var json = JsonUtils.ToJSON(embeddingRecords);
            File.WriteAllText(fileName, json);
        }
        public static List<EmbeddingRecord> FromJsonFile(string fileName)
        {
            var json = File.ReadAllText(fileName);
            return JsonUtils.FromJSON<List<EmbeddingRecord>>(json);
        }
    }

    public class EmbeddingSongRecord : EmbeddingRecord
    {
        public string Album { get; set; }
        public string Title { get; set; }
    }
}
