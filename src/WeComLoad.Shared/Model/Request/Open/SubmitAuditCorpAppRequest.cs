namespace WeComLoad.Shared.Model;

public class SubmitAuditCorpAppRequest
{
    public SubmitAuditCorpAppRequest(string corpAppId, string suiteId)
    {
        auditorder = new SubmitAuditCorpAuditorder
        {
            corpappid = corpAppId,
            suiteid = suiteId
        };
    }

    public SubmitAuditCorpAuditorder? auditorder { get; set; }

    public class SubmitAuditCorpAuditorder
    {
        public string corpappid { get; set; }

        public string suiteid { get; set; }
    }
}
