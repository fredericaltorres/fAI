using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace fAI
{
    public class EmbeddingSongRecord : EmbeddingCommonRecord
    {
        public string Album { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }

        public new static List<EmbeddingSongRecord> FromJsonFile(string fileName)
        {
            var json = File.ReadAllText(fileName);
            return JsonUtils.FromJSON<List<EmbeddingSongRecord>>(json);
        }

        public static List<EmbeddingSongRecord> LoadEmbeddingSongRecord(string jsonFilename)
        {
            var r = new List<EmbeddingSongRecord>();
            if (File.Exists(jsonFilename))
                r.AddRange(EmbeddingSongRecord.FromJsonFile(jsonFilename).Select(rr => rr as EmbeddingSongRecord));

            return r;
        }

        public static void SaveEmbeddingSongRecord(List<EmbeddingSongRecord> embeddingSongRecord, string jsonFilename)
        {
            if (File.Exists(jsonFilename))
                File.Delete(jsonFilename);
            EmbeddingCommonRecord.ToJsonFile(embeddingSongRecord.Select(r => r as EmbeddingCommonRecord).ToList(), jsonFilename);
        }
    }
}
