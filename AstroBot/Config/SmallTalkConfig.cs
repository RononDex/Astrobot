// Generated with https://json2csharp.com/
// Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace AstroBot.Config
{
    public class SmallTalkRespons
    {
        private List<Regex> _matchRegexList;

        [JsonPropertyName("matches")]
        public List<string> Matches { get; } = new List<string>();

        [JsonPropertyName("isMentioned")]
        public bool IsMentioned { get; set; }

        [JsonPropertyName("responses")]
        public List<string> Responses { get; } = new List<string>();

        [JsonIgnore]
        public IReadOnlyList<Regex> MatchRegexList
        {
            get
            {
                return _matchRegexList ??= Matches.Select(m => new Regex(m, RegexOptions.IgnoreCase)).ToList();
            }
        }
    }

    public class Root
    {
        [JsonPropertyName("smallTalkResponses")]
        public List<SmallTalkRespons> SmallTalkResponses { get; } = new List<SmallTalkRespons>();
    }
}
