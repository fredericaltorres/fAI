using DynamicSugar;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace fAI
{
    public class EmbeddingCommonRecord
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public List<float> Embedding { get; set; }
        public int TextLength => this.Text.Length;
        public int TextLengthKb => this.Text.Length / 1024;

        [Newtonsoft.Json.JsonIgnore]
        public double Score { get; set; }

        public static void ToJsonFile(List<EmbeddingCommonRecord> embeddingRecords, string fileName)
        {
            var json = JsonUtils.ToJSON(embeddingRecords);
            if (File.Exists(fileName))
                File.Delete(fileName);
            File.WriteAllText(fileName, json);
        }
        public static List<EmbeddingCommonRecord> FromJsonFile(string fileName)
        {
            var json = File.ReadAllText(fileName);
            return JsonUtils.FromJSON<List<EmbeddingCommonRecord>>(json);
        }
    }
}
