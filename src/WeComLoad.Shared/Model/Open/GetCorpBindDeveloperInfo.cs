namespace WeComLoad.Shared.Model;

public class GetCorpBindDeveloperInfo
{
    public string corpid { get; set; }

    public string vid { get; set; }

    public bool is_service { get; set; }

    public bool is_service_admin { get; set; }

    public bool is_super_admin { get; set; }
}
