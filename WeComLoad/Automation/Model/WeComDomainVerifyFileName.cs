using Newtonsoft.Json;

namespace WeComLoad.Automation
{

    public class WeComDomainVerifyFileName
    {
        [JsonProperty("file_name")]
        public string FileName { get; set; }
    }
}
