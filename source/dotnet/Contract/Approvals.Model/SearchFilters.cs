using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.CFS.Approvals.Model
{
    /// <summary>
    /// The Filters object
    /// </summary>
    public class Filters
    {
        [JsonPropertyName("searchFilters")]
        public SearchFilters SearchFilters { get; set; }

        [JsonPropertyName("sort")]
        public SortOptions Sort { get; set; }

        [JsonPropertyName("limit")]
        public int? Limit { get; set; }

    }

    /// <summary>
    /// The SearchFilters object
    /// </summary>
    public class SearchFilters
    {
        [JsonPropertyName("operator")]
        public string Operator { get; set; }

        [JsonPropertyName("conditions")]
        public List<Condition> Conditions { get; set; }
    }

    /// <summary>
    /// The Condition object
    /// </summary>
    public class Condition
    {
        [JsonPropertyName("field")]
        public string Field { get; set; }

        [JsonPropertyName("operator")]
        public string Operator { get; set; } // =, !=, >, <, >=, <=, ~

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    /// <summary>
    /// The SortOptions object
    /// </summary>
    public class SortOptions
    {
        [JsonPropertyName("field")]
        public string Field { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } 

        [JsonPropertyName("direction")]
        public string Direction { get; set; } // "asc" or "desc"
    }


}
