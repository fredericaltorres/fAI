using System;
using System.Collections.Generic;
using System.Text;

namespace fAI.Microsoft.Search
{
    // https://github.com/Azure-Samples/azure-search-dotnet-samples
    public class AzureSearch
    {
        // https://portal.azure.com/#@fredericaltorreslive.onmicrosoft.com/resource/subscriptions/57646804-986c-47e8-af66-a3abec32e52a/resourceGroups/fAI/providers/Microsoft.Search/searchServices/fai-search/indexes
        string serviceName = "fai-search";
        string apiKey = Environment.GetEnvironmentVariable("MICROSOFT_AZURE_SEARCH_KEY");
        string indexName = "fred-presentation-index";
    }
}
