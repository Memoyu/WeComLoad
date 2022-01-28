namespace WeComLoad.Shared.Model;

public class CorpApp
{
    public string app_id { get; set; }
    public string name { get; set; }
    public string logo { get; set; }
    public string description { get; set; }
    public string authcorp_name { get; set; }
    public string homeurl { get; set; }
    public string redirect_domain { get; set; }
    public White_Ip_List white_ip_list { get; set; }
    public string callbackurl { get; set; }
    public string token { get; set; }
    public string aeskey { get; set; }
    public Menu menu { get; set; }
    public object[] template_list { get; set; }
    public bool islock { get; set; }
    public bool is_domain_check_icp { get; set; }
    public bool is_domain_ownership_verified { get; set; }
    public int domain_belong_to { get; set; }
    public bool enter_homeurl_in_wx { get; set; }
    public string miniprogram_appid { get; set; }
    public string miniprogram_enter_path { get; set; }
    public bool is_homeurl_miniprogram { get; set; }
    public AuditOrder auditorder { get; set; }
    public bool b_can_not_edit_showinfo { get; set; }
    public Sdk_Auth sdk_auth { get; set; }
    public int customized_app_status { get; set; }
}

public class White_Ip_List
{
    public object[] ip { get; set; }
}

public class Menu
{
    public object[] button { get; set; }
}

public class Sdk_Auth
{
    public string bundleid { get; set; }
    public string packagename { get; set; }
    public bool b_android { get; set; }
    public bool b_ios { get; set; }
    public string signature_android { get; set; }
    public string redirect_domain2 { get; set; }
    public string aes_app_id { get; set; }
}
