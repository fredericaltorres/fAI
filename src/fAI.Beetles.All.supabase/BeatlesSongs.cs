using fAI;
using Microsoft.Extensions.AI;
using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SupabaseThoughts
{
    // https://www.youtube.com/watch?v=uviVTDtYeeE
    // https://themurph.hashnode.dev/supabase-csharp
    [Table("beatles_songs")]
    public class BeatlesSongs : BaseModel
    {
        public BeatlesSongs() 
        {
        }

        [PrimaryKey("id", shouldInsert:true)]
        public string Id { get; set; }

        [Column("text")]
        public string Text { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("album")]
        public string Album { get; set; }

        [Column("year")]
        public int Year { get; set; }

        [Column("embedding")]
        public string Embedding { get; set; }
        
        public BeatlesSongs(string id, string title, string album, int year, string text, List<float> embedding)
        {
            this.Id = id;
            this.Year = year;
            this.Album = album;
            this.Title = title;
            this.Text = text;

            if (embedding == null)
            {
                var client = new OpenAI();
                var r = client.Embeddings.Create(text, model: "text-embedding-3-small");
                if (r.Success)
                    embedding = r.Data[0].Embedding;
            }

            this.Embedding = JsonConvert.SerializeObject(embedding);
        }
    }
}
