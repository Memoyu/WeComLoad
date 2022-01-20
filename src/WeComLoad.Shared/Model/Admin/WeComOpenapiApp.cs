namespace WeComLoad.Shared.Model;

public class WeComOpenapiApp
{
    [JsonProperty("corp_id")]
    public string CorpId { get; set; }

    [JsonProperty("app_id")]
    public string AppId { get; set; }

    [JsonProperty("app_open_id")]
    public int AppOpenId { get; set; }

    public string Name { get; set; }

    public string Imgid { get; set; }

    [JsonProperty("app_perm")]
    public AppPermObj AppPerm { get; set; }

    [JsonProperty("create_time")]
    public int CreateTime { get; set; }

    [JsonProperty("update_time")]
    public int UpdateTime { get; set; }

    [JsonProperty("last_mod_time")]
    public string LastModTime { get; set; }

    [JsonProperty("app_open")]
    public int AppOpen { get; set; }

    [JsonProperty("arch_rw_flag")]
    public int ArchRwFlag { get; set; }

    public int Order { get; set; }

    [JsonProperty("is_complete")]
    public bool IsComplete { get; set; }

    [JsonProperty("visible_flag")]
    public string VisibleFlag { get; set; }

    [JsonProperty("aes_app_id")]
    public string AesAppId { get; set; }

    [JsonProperty("callback_open")]
    public bool CallbackOpen { get; set; }

    [JsonProperty("control_open_state")]
    public int ControlOpenState { get; set; }

    [JsonProperty("b_chat_shortcut")]
    public bool BChatShortcut { get; set; }

    public int Paidappinfoid { get; set; }

    [JsonProperty("custom_info_open")]
    public int CustomInfoOpen { get; set; }

    public class AppPermObj
    {
        public List<object> Vids { get; set; }

        public List<string> Partyids { get; set; }

        public List<object> Tags { get; set; }

        [JsonProperty("superadmin_visible")]
        public bool SuperadminVisible { get; set; }

        [JsonProperty("app_perm_flag")]
        public int AppPermFlag { get; set; }
    }
}

