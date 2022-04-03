namespace WeComLoad.Shared.Model;

public class CheckLoginState
{
    public bool can_open_url_in_app { get; set; }

    public string client_version { get; set; }

    public int login_state { get; set; }

    public bool quick_login_enabled { get; set; }
}
