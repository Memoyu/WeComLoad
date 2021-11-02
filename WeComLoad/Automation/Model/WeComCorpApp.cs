using Newtonsoft.Json;
using System.Collections.Generic;

namespace WeComLoad.Automation
{
    public class WeComCorpApp
    {
        [JsonProperty("default_app")]
        public List<DefaultApp> DefaultApps { get; set; }

        [JsonProperty("custom_app")]
        public List<CustomApp> CustomApps { get; set; }

        [JsonProperty("openapi_app")]
        public List<WeComOpenapiApp> OpenapiApps { get; set; }

        public class DefaultApp
        {
            [JsonProperty("business_id")]
            public int BusinessId { get; set; }

            [JsonProperty("create_time")]
            public int CreateTime { get; set; }

            [JsonProperty("open_state")]
            public int OpenState { get; set; }

            [JsonProperty("app_name")]
            public string AppName { get; set; }

            [JsonProperty("app_logo")]
            public string AppLogo { get; set; }

            [JsonProperty("app_developer")]
            public string AppDeveloper { get; set; }

            [JsonProperty("app_info")]
            public string AppInfo { get; set; }

            [JsonProperty("editor_name")]
            public string EditorName { get; set; }

            [JsonProperty("user_list")]
            public List<object> UserList { get; set; }

            [JsonProperty("ever_opened")]
            public bool EverOpened { get; set; }

            [JsonProperty("display_order")]
            public long DisplayOrder { get; set; }
        }

        public class CustomApp
        {
        }
    }
}
