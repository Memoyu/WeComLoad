namespace WeComLoad.Shared.Model;

public class WeComCheckCustomAppURLDomain
{
    [JsonProperty("service_corpids")]
    public List<string> CerviceCorpids { get; set; } = null!;

    public string Status { get; set; } = null!;
}
