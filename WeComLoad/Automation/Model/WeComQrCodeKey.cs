using Newtonsoft.Json;

namespace WeComLoad.Automation
{
    public class WeComQrCodeKey
    {
        [JsonProperty("qrcode_key")]
        public string QrCodeKey { get; set; }
    }
}
