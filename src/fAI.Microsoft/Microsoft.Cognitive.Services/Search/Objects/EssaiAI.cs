using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Deepgram.Models;
using System.Threading;
using HtmlAgilityPack;
using static DynamicSugar.DS;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.IO;

namespace fAI.Microsoft.Search
{
    /*
     
{
  "search": "id,title,description",
  "filter": "Country eq 'USA'",
    "queryLanguage": "en-US"
}

{
  "search": "id,title,description",
  "queryType": "semantic",
  "semanticConfiguration": "my-semantic-config",
  "captions": "extractive",
  "answers": "extractive|count-3",
  "queryLanguage": "en-US"
}
     */
    
    public class EmbedingsData
    {
        public string Id { get; set; }
        public List<string> Texts { get; set; } = new List<string>();
        
        public List<List<float>> Embedings { get; set; } = new List<List<float>>();

        public void ToJsonFile(string parentFileName, string subFile)
        {
            string fileName = MakeFileName(parentFileName, subFile);
            System.IO.File.WriteAllText(fileName,
                               Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented));
        }

        public static string MakeFileName(string parentFileName, string subFile)
        {
            var p = Path.GetDirectoryName(parentFileName);
            var f = Path.GetFileNameWithoutExtension(parentFileName);
            var e = Path.GetExtension(parentFileName);
            var fileName = Path.Combine(p, $"{f}.{subFile}.{e}");
            return fileName;
        }
    }

    public partial class EssaiAI
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public bool DataReady { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public List<EmbedingsData> EmbedingsData { get; set; } = new List<EmbedingsData>();

        private const string ModelName = "text-embedding-ada-002";
        private const int ModelDimensions = 1536;
        private const string SemanticSearchConfigName = "my-semantic-config";
        public const string indexName = "fred-presentation-test-index";

        public static SearchIndex GetIndexMetaData()
        {
            string vectorSearchProfile      = "my-vector-profile";
            string vectorSearchHnswConfig   = "my-hnsw-vector-config";

            SearchIndex searchIndex = new SearchIndex(indexName)
            {
                VectorSearch = new VectorSearch()
                {
                    Profiles   = { new VectorSearchProfile(vectorSearchProfile, vectorSearchHnswConfig) },
                    Algorithms = { new HnswAlgorithmConfiguration(vectorSearchHnswConfig) }
                },
                SemanticSearch = new SemanticSearch()
                {
                    Configurations = {
                       new SemanticConfiguration(SemanticSearchConfigName, new SemanticPrioritizedFields()
                       {
                           TitleField = new SemanticField("title"),
                           ContentFields = { new SemanticField("description") },
                           KeywordsFields = { new SemanticField("category") }
                       })
                    },
                },
                Fields = {
                new SimpleField("id", SearchFieldDataType.String) { IsKey = true, IsFilterable = true, IsSortable = true, IsFacetable = true },
                new SearchableField("title") { IsFilterable = true, IsSortable = true },
                new SearchableField("description") { IsFilterable = true },
                new SearchField("titleVector", SearchFieldDataType.Collection(SearchFieldDataType.Single))
                {
                    IsSearchable = true,
                    VectorSearchDimensions = ModelDimensions,
                    VectorSearchProfileName = vectorSearchProfile
                },
                new SearchField("descriptionVector", SearchFieldDataType.Collection(SearchFieldDataType.Single))
                {
                    IsSearchable = true,
                    VectorSearchDimensions = ModelDimensions,
                    VectorSearchProfileName = vectorSearchProfile
                },
                new SearchableField("category") { IsFilterable = true, IsSortable = true, IsFacetable = true }
            }
            };
            return searchIndex;
        }


        public static List<EssaiAI> FromJsonFile(string fileName) => 
            Newtonsoft.Json.JsonConvert.DeserializeObject<List<EssaiAI>>(System.IO.File.ReadAllText(fileName));

        public static void ToJsonFile(List<EssaiAI> essays, string fileName)
        {
            foreach(var e in essays)
            {
                foreach(var ed in e.EmbedingsData)
                {
                    ed.ToJsonFile(fileName, $"{e.Id}");
                }
            }

            File.WriteAllText(fileName,
                Newtonsoft.Json.JsonConvert.SerializeObject(essays, Newtonsoft.Json.Formatting.Indented));
        }

        public bool LoadTextFromHtmlPageAndComputeEmbeding()
        {
            try
            {
                var mc = new ModernWebClient(60);
                var r = mc.GET(this.Url);
                if (r.Success)
                {
                    var html = r.Text;
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(html);
                    var htmlBody = htmlDoc.DocumentNode.SelectSingleNode("//body");
                    var text = htmlBody.InnerText.Replace(".  ", ". ");
                    return this.ComputeEmbeding(text);
                }
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }
            return false;
        }

        public bool ComputeEmbeding(string text)
        {
            try
            {
                var client = new OpenAI();
                var texts = client.Embeddings.BreakDownLongTextBasedOnDot(text);

                var e = new EmbedingsData()
                {
                    Id = this.Id,
                };
                this.EmbedingsData.Add(e);

                foreach (var t in texts)
                {
                    var re = client.Embeddings.Create(t);
                    if (re.Success)
                    {
                        this.DataReady = true;
                        e.Texts.Add(t);
                        e.Embedings.Add(re.Data[0].Embedding);
                    }
                    else
                    {
                        Debugger.Break();
                        this.DataReady = false;
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }
            return false;
        }

        public static List<EssaiAI> LoadFromCsv(string fileName)
        {
            var csvLoader = new CSVEssaiAILoader();
            var r = csvLoader.Load(fileName);
            foreach(var p in r)
            {
                var title = p.Title.ToLowerInvariant();
                p.Url = $"https://paulgraham.com/{p.Url}";
                p.Id = Guid.NewGuid().ToString();
            }
            return r;
        }

        public static List<SearchDocument> PrepareForIndexing(List<EssaiAI> presentations)
        {
            var r = new List<SearchDocument>();
            foreach(var p in presentations)
                r.Add(PrepareForIndexing(p));
            return r;
        }

        public static SearchDocument PrepareForIndexing(EssaiAI presentation)
        {
            float[] titleEmbeddings = (OpenAI.GenerateEmbeddings(presentation.Title)).ToArray();
            return new SearchDocument()
            {
                ["id"] = presentation.Id,
                ["title"] = presentation.Title,
                ["url"] = presentation.Url,
                ["titleVector"] = titleEmbeddings
            };
        }
    }
}


