using Newtonsoft.Json;

namespace WeComLoad.Automation
{
    public class WeComQrCodeScanStatus
    {
        public string Status { get; set; }

        [JsonProperty("auth_source")]
        public string AuthSource { get; set; }

        [JsonProperty("auth_code")]
        public string AuthCode { get; set; }

        [JsonProperty("corp_id")]
        public int CorpId { get; set; }

        [JsonProperty("code_type")]
        public int CodeType { get; set; }

        public string Clientip { get; set; }

        [JsonProperty("confirm_clientip")]
        public string ConfirmClientip { get; set; }
    }
}
