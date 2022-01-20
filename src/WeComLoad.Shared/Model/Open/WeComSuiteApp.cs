namespace WeComLoad.Shared.Model;

public class WeComSuiteApp
{
    public int suite_cnt_before_customized_filter { get; set; }

    public SuiteApp suite_list { get; set; }

    public class SuiteApp
    {
        public List<SuiteAppItem> suite { get; set; }
    }

    public class SuiteAppItem
    {
        public int suiteid { get; set; }
        public string name { get; set; }
        public string name_pinyin { get; set; }
        public int createtime { get; set; }
        public string logo { get; set; }
        public string description { get; set; }
        public string industry { get; set; }
        public string in_domain { get; set; }
        public string out_domain { get; set; }
        public string callbackurl { get; set; }
        public string token { get; set; }
        public string aeskey { get; set; }
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
        public Extended_Info extended_info { get; set; }
        public string kitsecret { get; set; }
        public object[] thirdapp_dotlist { get; set; }

        public class Extended_Info
        {
            public Industry_Domain_List industry_domain_list { get; set; }
        }

        public class Industry_Domain_List
        {
            public object[] domain { get; set; }
        }

    }
}
