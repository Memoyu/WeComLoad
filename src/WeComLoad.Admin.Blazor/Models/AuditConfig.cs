using System.ComponentModel.DataAnnotations;

namespace WeComLoad.Admin.Blazor.Models;

public class AuditConfig
{
    [Required]
    public string CorpId { get; set; }

    public string AppId { get; set; }

    public int EnvType { get; set; } = 3;

    public string CallbackUrl { get; set; }

    [Required]
    public string CallbackUrlComplete { get; set; }

    public string WhiteIp { get; set; }

    [Required]
    public string Domain { get; set; }

    public string HomePage { get; set; }

    public string VerifyBucket { get; set; }
}

