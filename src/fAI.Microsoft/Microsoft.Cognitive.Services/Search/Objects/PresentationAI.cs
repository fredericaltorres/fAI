using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

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
    public partial class PresentationAI
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }

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

        public static List<PresentationAI> LoadFromCsv(string fileName)
        {
            var csvLoader = new CSVMLoader();
            var r = csvLoader.Load<PresentationAI, PresentationAICsvMapping>(fileName, new PresentationAICsvMapping());
            foreach(var p in r)
            {
                if (p.Description == "NULL")
                    p.Description = null;

                var title = p.Title.ToLowerInvariant();
                if (title.Contains(@"\sub_") && (title.Contains(@".mp4") || title.Contains(@".webm") || title.Contains(@".pdf")))
                {
                    p.Title = p.Description;
                }
            }
            return r;
        }

        public static List<SearchDocument> PrepareForIndexing(List<PresentationAI> presentations)
        {
            var r = new List<SearchDocument>();
            foreach(var p in presentations)
                r.Add(PrepareForIndexing(p));
            return r;
        }

        public static SearchDocument PrepareForIndexing(PresentationAI presentation)
        {
            float[] titleEmbeddings = (OpenAI.GenerateEmbeddings(presentation.Title)).ToArray();
            return new SearchDocument()
            {
                ["id"] = presentation.Id,
                ["title"] = presentation.Title,
                ["description"] = presentation.Description,
                ["category"] = presentation.Category,
                ["titleVector"] = titleEmbeddings
            };
        }
    }
}


