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
using Newtonsoft.Json;

namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class MicrosoftAzureVectorDatabasePaulGrahamEssaysTests
    {
        const string serviceName = "fai-search";
        const string csvFile = @"C:\DVT\fAI\src\fAI.Tests\Microsoft.Cognitive.Services\Paul-Graham-Essays.csv";
        // view-source:https://paulgraham.com/articles.html

        [Fact()]
        public void LoadFromCsvAndGenerateJsonFile()
        {
            var essays = EssaiAI.LoadFromCsv(csvFile);
            Assert.True(essays.Count > 0);
            essays.ForEach(e => e.LoadTextFromHtmlPage());
            EssaiAI.ToJsonFile(essays, @"C:\DVT\fAI\src\fAI.Tests\Microsoft.Cognitive.Services\Paul-Graham-Essays.json");
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
            var selectColumns = DS.List("id", "title", "description", "category");
            var  presentationFounds = azureSearch.VectorSearch(PresentationAI.indexName, "Tell me about cats?", "titleVector", selectColumns);

            File.AppendAllText(@"C:\a\VectorSearchResults.txt", JsonConvert.SerializeObject(presentationFounds, Formatting.Indented));
        }
    }
}  
