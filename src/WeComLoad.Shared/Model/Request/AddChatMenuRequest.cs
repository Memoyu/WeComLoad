namespace WeComLoad.Shared.Model;

public class AddChatMenuRequest
{
    public WeComOpenapiApp Agent { get; set; }

    public string MenuName { get; set; }

    public string MenuUrl { get; set; }
}
