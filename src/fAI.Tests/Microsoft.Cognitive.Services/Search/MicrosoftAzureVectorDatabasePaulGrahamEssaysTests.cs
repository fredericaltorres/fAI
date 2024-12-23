﻿using System.IO;
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
    // https://portal.azure.com/#@fredericaltorreslive.onmicrosoft.com/resource/subscriptions/57646804-986c-47e8-af66-a3abec32e52a/resourceGroups/fAI/providers/Microsoft.Search/searchServices/fai-search/indexes

    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class MicrosoftAzureVectorDatabasePaulGrahamEssaysTests
    {
        const string serviceName = "fai-search";
        const string csvFile = @"C:\DVT\fAI\src\fAI.Tests\Microsoft.Cognitive.Services\data\Paul-Graham-Essays\Paul-Graham-Essays.csv";
        const string jsonFile = @"C:\DVT\fAI\src\fAI.Tests\Microsoft.Cognitive.Services\data\Paul-Graham-Essays\Paul-Graham-Essays.json";
        // view-source:https://paulgraham.com/articles.html

        // [Fact()]
        public void LoadFromCsvAndGenerateJsonFile()
        {
            var essays = EssaiAI.LoadFromCsv(csvFile);
            Assert.True(essays.Count > 0);
            essays.ForEach(e => e.LoadTextFromHtmlPageAndComputeEmbeding());
            EssaiAI.ToJsonFile(essays, jsonFile);
        }

      

    }
}  
