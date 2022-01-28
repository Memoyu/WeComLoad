namespace WeComLoad.Shared.Model;

public class AuditOrder
{
    public string auditorderid { get; set; }
    public int createtime { get; set; }
    public string ww_corpid { get; set; }
    public string vid { get; set; }
    public int suiteid { get; set; }
    public int status { get; set; }
    public int audittime { get; set; }
    public string suitename { get; set; }
    public string corpname { get; set; }
    public string logo { get; set; }
    public string app_name { get; set; }
    public string corpappid { get; set; }
    public int audit_order_type { get; set; }
}
