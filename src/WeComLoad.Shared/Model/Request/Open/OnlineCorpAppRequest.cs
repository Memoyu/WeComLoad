namespace WeComLoad.Shared.Model;

public class OnlineCorpAppRequest
{
    /// <summary>
    /// 构造
    /// </summary>
    /// <param name="auditOrderId">审核Id</param>
    /// <param name="status">状态默认是5</param>
    public OnlineCorpAppRequest(string auditOrderId, int status = 5)
    {
        auditorder = new OnlineCorpAuditorder
        {
            auditorderid = auditOrderId,
            status = status
        };
    }

    public OnlineCorpAuditorder? auditorder { get; set; }

    public class OnlineCorpAuditorder
    {
        public string auditorderid { get; set; }

        public int status { get; set; }
    }
}
