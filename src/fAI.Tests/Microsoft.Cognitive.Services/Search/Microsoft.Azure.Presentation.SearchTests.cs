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
    public class MicrosoftAzureSearchPresentationTests
    {
        const string serviceName = "fai-search";

        [Fact()]
        public void Create_Upload()
        {
            var azureSearch = new AzureSearch(serviceName);
            azureSearch.DeleteIndexIfExists(PresentationAI.indexName);
            azureSearch.CreateIndex(PresentationAI.GetIndexMetaData());
            //var cities = CityAI.LoadTestData();
            //azureSearch.UploadDocuments(indexName, cities);
            //Console.WriteLine("Waiting for indexing...\n");
            //System.Threading.Thread.Sleep(2000);
        }

        
    }
}  
