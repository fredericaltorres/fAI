using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using DynamicSugar;
using fAI;
using Xunit;
using static fAI.AudioUtil;
using static fAI.MicrosoftCognitiveServices;
using fAI.Microsoft.Search;
using Azure.Search.Documents.Models;
using Azure.Search.Documents;

namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class MicrosoftAzureSearchPresentationTests
    {
        const string serviceName = "fai-search";
        const string csvFile = @"C:\a\PresentationAI.csv";

        [Fact()]
        public void LoadFromCsv()
        {
            var presentations = PresentationAI.LoadFromCsv(csvFile);
            Assert.True(presentations.Count > 0);
        }

        [Fact()]
        public void Create_Upload()
        {
            var azureSearch = new AzureSearch(serviceName);
            azureSearch.DeleteIndexIfExists(PresentationAI.indexName);
            azureSearch.CreateIndex(PresentationAI.GetIndexMetaData());

            var presentations = PresentationAI.LoadFromCsv(csvFile).Take(32).ToList();
            var presentationDocuments = PresentationAI.PrepareForIndexing(presentations);
            azureSearch.IndexDocuments(presentationDocuments, PresentationAI.indexName);

            //var cities = CityAI.LoadTestData();
            //azureSearch.UploadDocuments(indexName, cities);
            //Console.WriteLine("Waiting for indexing...\n");
            //System.Threading.Thread.Sleep(2000);
        }

        [Fact()]
        public void Search()
        {
            var sb = new System.Text.StringBuilder();
            var azureSearch = new AzureSearch(serviceName);

            var selectColumns = new List<string>() { "id", "title", "description", "category" };
            var documents = azureSearch.VectorSearch(PresentationAI.indexName, "Tell me about cats?", "titleVector", selectColumns);
            foreach(var result in documents)
            {
                sb.Append(result.Document["id"]).Append(", ");
                sb.Append(result.Document["title"]).AppendLine();
            }
            File.WriteAllText(@"C:\a\VectorSearchResults.txt", sb.ToString());
        }
    }
}  
