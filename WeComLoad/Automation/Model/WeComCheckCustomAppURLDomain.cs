using Newtonsoft.Json;
using System.Collections.Generic;

namespace WeComLoad.Automation
{
    public class WeComCheckCustomAppURLDomain
    {
        [JsonProperty("service_corpids")]
        public List<string> CerviceCorpids { get; set; }

        public string Status { get; set; }
    }
}
