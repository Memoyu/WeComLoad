namespace WeComLoad.Open.Blazor.Models;

public class AuditConfig
{
    public string CorpId { get; set; }

    public int EnvType { get; set; } = 1;

    public string CallbackUrl { get; set; }

    public string CallbackUrlComplete { get; set; }

    public string WhiteIp { get; set; }

    public string Domain { get; set; }

    public string HomePage { get; set; }

    public string VerifyBucket { get; set; }
}

