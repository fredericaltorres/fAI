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

namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class MicrosoftAzureSearchCityAITests
    {
        const string serviceName = "fai-search";
        const string indexName = "fred-city-test-index";

        [Fact()]
        public void Create_Upload()
        {
            var azureSearch = new AzureSearch(serviceName);
            azureSearch.DeleteIndexIfExists(indexName);
            azureSearch.CreateIndex(indexName, typeof(CityAI), new List<string>()); //DS.List("City")
            var cities = CityAI.LoadTestData();
            azureSearch.UploadDocuments(indexName, cities);
            Console.WriteLine("Waiting for indexing...\n");
            System.Threading.Thread.Sleep(2000);
        }

        [Fact()]
        public void Search_CityInTheUSA()
        {
            var azureSearch = new AzureSearch(serviceName);
            var whereClause = "Country eq 'USA'";
            var orderBy = "City desc";
            var results = azureSearch.SearchDocuments<CityAI>(indexName, whereClause, orderBy);
            Assert.Equal(16, results.Count);
            results.ForEach(r => Assert.Equal("USA", r.Country));
        }
        [Fact()]
        public void Search_CityInTheBelgium()
        {
            var azureSearch = new AzureSearch(serviceName);
            var whereClause = "Country eq 'Belgium'";
            var orderBy = "City desc";
            var results = azureSearch.SearchDocuments<CityAI>(indexName, whereClause, orderBy);
            Assert.Equal(1, results.Count);
            results.ForEach(r => Assert.Equal("Belgium", r.Country));
            results.ForEach(r => Assert.Equal("Brussels", r.City));
        }
    }
}  
