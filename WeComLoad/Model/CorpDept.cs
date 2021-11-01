using System.Collections.Generic;

namespace WeComLoad.Model
{
    public class CorpDept
    {
        public party_list party_list { get; set; }
    }

    public class party_list
    {
        public List<party> list { get; set; }
    }

    public class party
    {
        public string partyid { get; set; }

        public string openapi_partyid { get; set; }

        public string name { get; set; }

        public string parentid { get; set; }

        public int authority { get; set; }

        public bool islocked { get; set; }

        public long display_order { get; set; }

        public string pinyin { get; set; }

        public string py { get; set; }
    }
}
