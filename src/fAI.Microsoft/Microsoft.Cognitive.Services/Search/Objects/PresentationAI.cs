using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace fAI.Microsoft.Search
{
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
    }
}
