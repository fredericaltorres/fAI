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
    public class MicrosoftAzureSearchTests
    {
        const string serviceName = "fai-search";
        const string indexName = "fred-presentation-test-index";

        const string citiesJson = @"[
    {""ID"": 5549, ""City"": ""New York"", ""Country"": ""USA""},
    {""ID"": 3767, ""City"": ""London"", ""Country"": ""UK""},
    {""ID"": 2932, ""City"": ""Paris"", ""Country"": ""France""},
    {""ID"": 9064, ""City"": ""Tokyo"", ""Country"": ""Japan""},
    {""ID"": 9514, ""City"": ""Sydney"", ""Country"": ""Australia""},
    {""ID"": 6589, ""City"": ""Cape Town"", ""Country"": ""South Africa""},
    {""ID"": 2008, ""City"": ""Rio de Janeiro"", ""Country"": ""Brazil""},
    {""ID"": 5285, ""City"": ""Moscow"", ""Country"": ""Russia""},
    {""ID"": 3694, ""City"": ""Mumbai"", ""Country"": ""India""},
    {""ID"": 3563, ""City"": ""Beijing"", ""Country"": ""China""},
    {""ID"": 3664, ""City"": ""Los Angeles"", ""Country"": ""USA""},
    {""ID"": 8670, ""City"": ""Berlin"", ""Country"": ""Germany""},
    {""ID"": 9175, ""City"": ""Rome"", ""Country"": ""Italy""},
    {""ID"": 6447, ""City"": ""Madrid"", ""Country"": ""Spain""},
    {""ID"": 7705, ""City"": ""Toronto"", ""Country"": ""Canada""},
    {""ID"": 7938, ""City"": ""Chicago"", ""Country"": ""USA""},
    {""ID"": 3964, ""City"": ""Dubai"", ""Country"": ""UAE""},
    {""ID"": 9487, ""City"": ""Singapore"", ""Country"": ""Singapore""},
    {""ID"": 3232, ""City"": ""Hong Kong"", ""Country"": ""China""},
    {""ID"": 6936, ""City"": ""Amsterdam"", ""Country"": ""Netherlands""},
    {""ID"": 2289, ""City"": ""San Francisco"", ""Country"": ""USA""},
    {""ID"": 9275, ""City"": ""Barcelona"", ""Country"": ""Spain""},
    {""ID"": 4015, ""City"": ""Bangkok"", ""Country"": ""Thailand""},
    {""ID"": 5277, ""City"": ""Istanbul"", ""Country"": ""Turkey""},
    {""ID"": 6241, ""City"": ""Melbourne"", ""Country"": ""Australia""},
    {""ID"": 6532, ""City"": ""Delhi"", ""Country"": ""India""},
    {""ID"": 2741, ""City"": ""Shanghai"", ""Country"": ""China""},
    {""ID"": 3707, ""City"": ""Sao Paulo"", ""Country"": ""Brazil""},
    {""ID"": 7653, ""City"": ""Seoul"", ""Country"": ""South Korea""},
    {""ID"": 5789, ""City"": ""Mexico City"", ""Country"": ""Mexico""},
    {""ID"": 4320, ""City"": ""Las Vegas"", ""Country"": ""USA""},
    {""ID"": 2927, ""City"": ""Miami"", ""Country"": ""USA""},
    {""ID"": 9027, ""City"": ""Buenos Aires"", ""Country"": ""Argentina""},
    {""ID"": 5219, ""City"": ""Cairo"", ""Country"": ""Egypt""},
    {""ID"": 6368, ""City"": ""Lisbon"", ""Country"": ""Portugal""},
    {""ID"": 8474, ""City"": ""Budapest"", ""Country"": ""Hungary""},
    {""ID"": 4116, ""City"": ""Vienna"", ""Country"": ""Austria""},
    {""ID"": 2870, ""City"": ""Prague"", ""Country"": ""Czech Republic""},
    {""ID"": 7048, ""City"": ""Brussels"", ""Country"": ""Belgium""},
    {""ID"": 8994, ""City"": ""Stockholm"", ""Country"": ""Sweden""},
    {""ID"": 9025, ""City"": ""Helsinki"", ""Country"": ""Finland""},
    {""ID"": 8545, ""City"": ""Oslo"", ""Country"": ""Norway""},
    {""ID"": 3867, ""City"": ""Copenhagen"", ""Country"": ""Denmark""},
    {""ID"": 3586, ""City"": ""Warsaw"", ""Country"": ""Poland""},
    {""ID"": 7893, ""City"": ""Dublin"", ""Country"": ""Ireland""},
    {""ID"": 9078, ""City"": ""Athens"", ""Country"": ""Greece""},
    {""ID"": 8150, ""City"": ""Zurich"", ""Country"": ""Switzerland""},
    {""ID"": 2413, ""City"": ""Geneva"", ""Country"": ""Switzerland""},
    {""ID"": 8771, ""City"": ""Venice"", ""Country"": ""Italy""},
    {""ID"": 7215, ""City"": ""Florence"", ""Country"": ""Italy""},
    {""ID"": 6570, ""City"": ""Naples"", ""Country"": ""Italy""},
    {""ID"": 4348, ""City"": ""Milan"", ""Country"": ""Italy""},
    {""ID"": 3218, ""City"": ""Bologna"", ""Country"": ""Italy""},
    {""ID"": 8182, ""City"": ""Pisa"", ""Country"": ""Italy""},
    {""ID"": 2681, ""City"": ""Turin"", ""Country"": ""Italy""},
    {""ID"": 8456, ""City"": ""Verona"", ""Country"": ""Italy""},
    {""ID"": 8932, ""City"": ""Palermo"", ""Country"": ""Italy""},
    {""ID"": 2305, ""City"": ""Catania"", ""Country"": ""Italy""},
    {""ID"": 5798, ""City"": ""Genoa"", ""Country"": ""Italy""},
    {""ID"": 2338, ""City"": ""Bari"", ""Country"": ""Italy""},
    {""ID"": 3949, ""City"": ""Krakow"", ""Country"": ""Poland""},
    {""ID"": 4575, ""City"": ""Gdansk"", ""Country"": ""Poland""},
    {""ID"": 8765, ""City"": ""Wroclaw"", ""Country"": ""Poland""},
    {""ID"": 3895, ""City"": ""Poznan"", ""Country"": ""Poland""},
    {""ID"": 4287, ""City"": ""Lodz"", ""Country"": ""Poland""},
    {""ID"": 4895, ""City"": ""Katowice"", ""Country"": ""Poland""},
    {""ID"": 3222, ""City"": ""Szczecin"", ""Country"": ""Poland""},
    {""ID"": 2239, ""City"": ""Bydgoszcz"", ""Country"": ""Poland""},
    {""ID"": 6480, ""City"": ""Lublin"", ""Country"": ""Poland""},
    {""ID"": 2958, ""City"": ""Bialystok"", ""Country"": ""Poland""},
    {""ID"": 8450, ""City"": ""Edinburgh"", ""Country"": ""UK""},
    {""ID"": 9346, ""City"": ""Glasgow"", ""Country"": ""UK""},
    {""ID"": 6369, ""City"": ""Manchester"", ""Country"": ""UK""},
    {""ID"": 2663, ""City"": ""Birmingham"", ""Country"": ""UK""},
    {""ID"": 4991, ""City"": ""Liverpool"", ""Country"": ""UK""},
    {""ID"": 5712, ""City"": ""Leeds"", ""Country"": ""UK""},
    {""ID"": 7267, ""City"": ""Sheffield"", ""Country"": ""UK""},
    {""ID"": 9694, ""City"": ""Bristol"", ""Country"": ""UK""},
    {""ID"": 5294, ""City"": ""Newcastle"", ""Country"": ""UK""},
    {""ID"": 6374, ""City"": ""Belfast"", ""Country"": ""UK""},
    {""ID"": 2520, ""City"": ""Vancouver"", ""Country"": ""Canada""},
    {""ID"": 3950, ""City"": ""Montreal"", ""Country"": ""Canada""},
    {""ID"": 7514, ""City"": ""Calgary"", ""Country"": ""Canada""},
    {""ID"": 5194, ""City"": ""Ottawa"", ""Country"": ""Canada""},
    {""ID"": 3874, ""City"": ""Edmonton"", ""Country"": ""Canada""},
    {""ID"": 6279, ""City"": ""Winnipeg"", ""Country"": ""Canada""},
    {""ID"": 6945, ""City"": ""Quebec City"", ""Country"": ""Canada""},
    {""ID"": 3779, ""City"": ""Hamilton"", ""Country"": ""Canada""},
    {""ID"": 3134, ""City"": ""Kitchener"", ""Country"": ""Canada""},
    {""ID"": 8215, ""City"": ""London (Canada)"", ""Country"": ""Canada""},
    {""ID"": 6339, ""City"": ""Los Angeles"", ""Country"": ""USA""},
    {""ID"": 6582, ""City"": ""San Diego"", ""Country"": ""USA""},
    {""ID"": 9682, ""City"": ""San Jose"", ""Country"": ""USA""},
    {""ID"": 7518, ""City"": ""San Antonio"", ""Country"": ""USA""},
    {""ID"": 5886, ""City"": ""Phoenix"", ""Country"": ""USA""},
    {""ID"": 4370, ""City"": ""Philadelphia"", ""Country"": ""USA""},
    {""ID"": 5862, ""City"": ""Houston"", ""Country"": ""USA""},
    {""ID"": 5943, ""City"": ""Dallas"", ""Country"": ""USA""},
    {""ID"": 9892, ""City"": ""Austin"", ""Country"": ""USA""},
    {""ID"": 7508, ""City"": ""Jacksonville"", ""Country"": ""USA""}
]
";

        [Fact()]
        public void Create_Upload()
        {
            var azureSearch = new AzureSearch(serviceName);
            azureSearch.DeleteIndexIfExists(indexName);
            azureSearch.CreateIndex(indexName, typeof(CityAI), DS.List("City"));
            var cities = CityAI.FromJson(citiesJson);
            azureSearch.UploadDocuments(indexName, cities);
            Console.WriteLine("Waiting for indexing...\n");
            System.Threading.Thread.Sleep(2000);
        }

        [Fact()]
        public void Search()
        {
            var azureSearch = new AzureSearch(serviceName);
            var result = azureSearch.SearchDocuments(indexName, "USA");
            Assert.Equal(7, result.Count);
        }
    }
}