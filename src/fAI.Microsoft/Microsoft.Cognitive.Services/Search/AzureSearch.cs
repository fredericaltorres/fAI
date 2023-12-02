using Azure.Search.Documents.Indexes;
using Azure;
using System;
using System.Collections.Generic;
using System.Text;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents;
using Newtonsoft.Json;
using Azure.Search.Documents.Models;
using DynamicSugar;
using System.Linq;
using System.Collections;

namespace fAI.Microsoft.Search
{
    // Announcing Vector Search in Azure Cognitive Search Public Preview (https://techcommunity.microsoft.com/t5/ai-azure-ai-services-blog/announcing-vector-search-in-azure-cognitive-search-public/ba-p/3872868)
    // All About Vectors, Search, and Function Calling in Azure OpenAI - Labor Day Special (https://www.youtube.com/watch?v=XyFeL5C2Hb0)
    // Vector search in Azure AI Search (https://learn.microsoft.com/en-us/azure/search/vector-search-overview)
    // Use ChatGPT On Your Own Large Data (https://www.youtube.com/watch?v=RcdqdWEYw2A)
    // Use Open AI (ChatGPT) On Your Own Large Data -Part 1 (https://www.youtube.com/watch?v=eNKu307k59g)
    // Open AI Embeddings in Azure Vector Database of Cognitive Search (https://www.youtube.com/watch?v=Re4fLSKi43A)
    // https://github.com/Azure-Samples/azure-search-dotnet-samples
    // See example azure-search-dotnet-samples\quickstart\v11\AzureSearchQuickstart-v11\Hotel.cs
    public class AzureSearch
    {
        // https://portal.azure.com/#@fredericaltorreslive.onmicrosoft.com/resource/subscriptions/57646804-986c-47e8-af66-a3abec32e52a/resourceGroups/fAI/providers/Microsoft.Search/searchServices/fai-search/indexes
        string _serviceName ;
        string apiKey = Environment.GetEnvironmentVariable("MICROSOFT_AZURE_SEARCH_KEY"); // Freds key

        Uri serviceEndpoint;
        AzureKeyCredential credential;
        SearchIndexClient adminClient;

        public AzureSearch(string serviceName)
        {
            _serviceName = serviceName;
            serviceEndpoint = new Uri($"https://{serviceName}.search.windows.net/");
            credential = new AzureKeyCredential(apiKey);
            adminClient = new SearchIndexClient(serviceEndpoint, credential);
        }
        public void DeleteIndexIfExists(string indexName)
        {
            adminClient.GetIndexNames();
            adminClient.DeleteIndex(indexName);
        }

        public void IndexDocuments(List<SearchDocument> documents, string indexName)
        {
            var searchClient = adminClient.GetSearchClient(indexName);
            searchClient.IndexDocumentsAsync(IndexDocumentsBatch.Upload(documents)).GetAwaiter().GetResult();
        }

        public 
            //SearchResults<SearchDocument>
            Pageable<SearchResult<SearchDocument>>
            VectorSearch(string indexName, string query, string vectorPropertyName, List<string> columns, int k = 3)
        {
            var searchClient = adminClient.GetSearchClient(indexName);
            var queryEmbeddings = OpenAI.GenerateEmbeddings(query);
            var searchOptions = new SearchOptions
            {
                VectorSearch = new VectorSearchOptions()
                {
                    Queries = { new VectorizedQuery(queryEmbeddings.ToArray()) { KNearestNeighborsCount = k, Fields = { vectorPropertyName } } }
                },
                Size = k,
                //Select = { "id", "title", "description", "category" },
            };

            if(columns.Count > 0 )
            {
                foreach(var c in columns)
                    searchOptions.Select.Add(c);
            }

            
            SearchResults<SearchDocument> response = searchClient.SearchAsync<SearchDocument>(null, searchOptions).GetAwaiter().GetResult();
            var response2 = response.GetResults();
            return response2;
        }

        public void CreateIndex(SearchIndex searchIndex) 
        {
            adminClient.CreateOrUpdateIndex(searchIndex);
        }
        
        public void CreateIndex(string indexName, Type rootObjectType, List<string> searchSuggester)
        {
            try
            {
                FieldBuilder fieldBuilder = new FieldBuilder();
                var searchFields = fieldBuilder.Build(rootObjectType);

                var definition = new SearchIndex(indexName, searchFields);

                if (searchSuggester != null && searchSuggester.Count > 0)
                {
                    var suggester = new SearchSuggester("sg", searchSuggester.ToArray());
                    definition.Suggesters.Add(suggester);
                }

                adminClient.CreateOrUpdateIndex(definition);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public void UploadDocuments(string indexName, List<CityAI> documents)
        {
            SearchClient ingesterClient = adminClient.GetSearchClient(indexName);
            var a = new List<IndexDocumentsAction<CityAI>>();

            foreach(var doc in documents)
                a.Add(IndexDocumentsAction.Upload(doc));
                
            IndexDocumentsBatch<CityAI> batch = IndexDocumentsBatch.Create(a.ToArray());

            try
            {
                IndexDocumentsResult result = ingesterClient.IndexDocuments(batch);
            }
            catch (Exception)
            {
            }
        }

        /*
{
  "search": "*",
  "filter": "Country eq 'USA'"
}
{
  "SearchFields" : [ "Tags" ],
  "search": "pool"

}
        */

        public List<T> SearchDocuments<T>(string indexName, string filterWhereClause, string orderBy) where T : new()
        {
            var srchclient = new SearchClient(serviceEndpoint, indexName, credential);
            var options = new SearchOptions()
            {
                Filter = filterWhereClause,
                OrderBy = { orderBy }
            };

            //options.Select.Add("ID");
            //options.Select.Add("City");
            //options.Select.Add("Country");
            var props = ReflectionHelper.GetDictionary(new T()).Keys.ToList();
            foreach(var p in props)
                options.Select.Add(p);

            var searchText = "*";
            SearchResults<T> response = srchclient.Search<T>(searchText, options);
            return GetDocuments(response);
        }

        private static List<T> GetDocuments<T>(SearchResults<T> searchResults)
        {
            var r = new List<T>(); 
            foreach (SearchResult<T> result in searchResults.GetResults())
                r.Add(result.Document);

            return r;
        }
    }
}
