using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace fAI.Microsoft.Search
{
    public partial class PresentationAI
    {
        [SimpleField(IsKey = true, IsFilterable = true, IsSortable = true, IsFacetable = true)]
        public string PresentationId { get; set; }

        [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EnLucene, IsFilterable = true)]
        public string Title { get; set; }

        [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EnLucene, IsFilterable = true)]
        public string Description { get; set; }

        [Azure.Search.Documents.Indexes.VectorSearchField()]
        public string Description { get; set; }
    }
}
