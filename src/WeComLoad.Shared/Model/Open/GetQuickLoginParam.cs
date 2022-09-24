namespace WeComLoad.Shared.Model;

public class GetQuickLoginParam
{
    [JsonProperty("web_key")]
    public string WebKey { get; set; }

    [JsonProperty("client_key")]
    public string ClientKey { get; set; }
    
    [JsonProperty("pc_http_port")]
    public List<string> HttpPort { get; set; }
    
    [JsonProperty("pc_https_port")]
    public List<string> HttpsPort { get; set; }
    
    [JsonProperty("allow_pc_auth_login")]
    public bool AllowAuthLogin { get; set; }
}