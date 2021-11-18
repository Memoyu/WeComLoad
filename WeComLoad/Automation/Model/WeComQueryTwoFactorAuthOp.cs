using Newtonsoft.Json;

namespace WeComLoad.Automation
{
    public class WeComQueryTwoFactorAuthOp
    {

        [JsonProperty("confirm_status")]
        public int Status { get; set; }
    }
}
