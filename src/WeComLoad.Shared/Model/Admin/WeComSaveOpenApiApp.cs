namespace WeComLoad.Shared.Model;

public class WeComSaveOpenApiApp
{
    [JsonProperty("reject_subadmin_ids")]
    public List<string> RejectSubadminIds { get; set; }

    [JsonProperty("is_domain_ownership_verified")]
    public bool IsDomainOwnershipVerified { get; set; }
}

