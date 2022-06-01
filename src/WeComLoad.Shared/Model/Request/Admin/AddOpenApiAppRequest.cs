namespace WeComLoad.Shared.Model;

public class AddOpenApiAppRequest
{
    public string Name { get; set; }

    public string Desc { get; set; }

    public string LogoImage { get; set; }

    public List<string> VisiblePIds { get; set; }

}
