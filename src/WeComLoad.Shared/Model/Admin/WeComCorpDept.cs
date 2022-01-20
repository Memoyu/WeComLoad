namespace WeComLoad.Shared.Model;

public class WeComCorpDept
{
    [JsonProperty("party_list")]
    public Party Partys { get; set; }

    public class Party
    {
        public List<PartyDto> List { get; set; }
    }

    public class PartyDto
    {
        public string Partyid { get; set; }

        [JsonProperty("openapi_partyid")]
        public string OpenapiPartyid { get; set; }

        public string Name { get; set; }

        public string Parentid { get; set; }

        public int Authority { get; set; }

        public bool Islocked { get; set; }

        [JsonProperty("display_order")]
        public long DisplayOrder { get; set; }

        public string Pinyin { get; set; }

        public string Py { get; set; }
    }
}

