using DynamicSugar;
using System.Collections.Generic;
using System.IO;

namespace fAI
{
    public class EmbeddingRecord
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public List<float> Embedding { get; set; }
        public int TextLength => this.Text.Length;

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
        public int Year { get; set; }

        public new static List<EmbeddingSongRecord> FromJsonFile(string fileName)
        {
            var json = File.ReadAllText(fileName);
            return JsonUtils.FromJSON<List<EmbeddingSongRecord>>(json);
        }
    }

    public class EmbeddingSourceCodeRecord : EmbeddingRecord
    {
        public const int MAX_TEXT_LENGTH = 4096;

        public string FileName { get; set; }
        public int ChunkIndex { get; set; } = 1;// When a file is split into chunks
        public string Project { get; set; }
        public string FileNameOnly => Path.GetFileName(FileName);



        public EmbeddingSourceCodeRecord Clone()
        {
            return new EmbeddingSourceCodeRecord
            {
                FileName = this.FileName,
                Text = this.Text,
                Project = this.Project,
                ChunkIndex = this.ChunkIndex
            };
        }

        public new static List<EmbeddingSourceCodeRecord> FromJsonFile(string fileName)
        {
            var json = File.ReadAllText(fileName);
            if(!string.IsNullOrEmpty(json))
                return JsonUtils.FromJSON<List<EmbeddingSourceCodeRecord>>(json);
            return new List<EmbeddingSourceCodeRecord>();
        }

        public static List<EmbeddingSourceCodeRecord> LoadSourceCode(string fileName, int projectLocationIndex)
        {
            var l = new List<EmbeddingSourceCodeRecord>();

            var r1 = new EmbeddingSourceCodeRecord();
            r1.FileName = fileName;
            r1.Project = fileName.Split('\\')[projectLocationIndex];
            r1.Text = File.ReadAllText(r1.FileName);
            var chunkIndex = 1;
            l.Add(r1);

            while (r1.Text.Length > MAX_TEXT_LENGTH)
            {
                var r2 = r1.Clone();
                r2.Text = r1.Text.Substring(MAX_TEXT_LENGTH);
                r2.ChunkIndex = ++chunkIndex;
                l.Add(r2);

                r1.Text = r1.Text.Substring(0, MAX_TEXT_LENGTH);

                r1 = r2;
            }



            return l;


        }

        public static List<EmbeddingSourceCodeRecord> LoadSourceCodeList(string fileName, int projectLocationIndex)
        {
            var r = new List<EmbeddingSourceCodeRecord>();
            var text = File.ReadAllText(fileName);
            var lines = text.SplitByCRLF();
            foreach (var l in lines)
            {
                var e = EmbeddingSourceCodeRecord.LoadSourceCode(l, projectLocationIndex);
                r.AddRange(e);
            }
            return r;
        }
    }
}
