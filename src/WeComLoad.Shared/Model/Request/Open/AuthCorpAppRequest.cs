namespace WeComLoad.Shared.Model;

public class AuthCorpAppRequest
{
    public string suiteid { get; set; }
    public CorpappReq corpapp { get; set; }

    public class CorpappReq
    {
        public string logo { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string redirect_domain { get; set; }
        public int domain_belong_to { get; set; }
        public Jssdkdomain_List jssdkdomain_list { get; set; } = new Jssdkdomain_List();
        public White_Ip_List white_ip_list { get; set; } = new White_Ip_List();
        public string callbackurl { get; set; }
        public string token { get; set; }
        public string aeskey { get; set; }
        public bool enter_homeurl_in_wx { get; set; }
        public bool is_homeurl_miniprogram { get; set; }
        public string miniprogram_enter_path { get; set; }
        public Miniprograminfo miniprogramInfo { get; set; } = new Miniprograminfo();
        public string page_type { get; set; }
        public int suiteid { get; set; }
        public string name_pinyin { get; set; }
        public int createtime { get; set; }
        public string industry { get; set; }
        public string in_domain { get; set; }
        public string out_domain { get; set; }
        public int updatetime { get; set; }
        public string service_tag { get; set; }
        public string service_tag_pinyin { get; set; }
        public string kitid { get; set; }
        public string homepage { get; set; }
        public string suite_type { get; set; }
        public string apply_tag_name { get; set; }
        public string apply_tag_reason { get; set; }
        public string auth_type { get; set; }
        public string modstatus { get; set; }
        public string publish_status { get; set; }
        public string setting_url { get; set; }
        public string qrcode_url { get; set; }
        public string ww_corpid { get; set; }
        public bool islock { get; set; }
        public int service_tag_type { get; set; }
        public int industry_level1_id { get; set; }
        public Extended_Info extended_info { get; set; } = new Extended_Info();
        public string kitsecret { get; set; }
        public object[] thirdapp_dotlist { get; set; }
        public string app_id { get; set; }
    }

    public class Jssdkdomain_List
    {
        public object[] domains { get; set; }
    }

    public class White_Ip_List
    {
        public string[] ip { get; set; }
    }

    public class Miniprograminfo
    {
    }

    public class Extended_Info
    {
        public Industry_Domain_List industry_domain_list { get; set; }
    }

    public class Industry_Domain_List
    {
        public object[] domain { get; set; }
    }

}
