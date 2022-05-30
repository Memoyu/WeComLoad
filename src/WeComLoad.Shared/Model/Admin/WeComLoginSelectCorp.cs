namespace WeComLoad.Shared.Model;

public class WeComWxLoginCorps
{
    public string TlKey { get; set; }

    public List<LoginCorp> Corps { get; set; }
}

public class LoginCorp
{
    public string CorpName { get; set; }

    public string CorpId { get; set; }
}
