using Newtonsoft.Json;
using System.Collections.Generic;

namespace WeComLoad.Automation
{

    public class WeComGetApiAccessibleApps
    {
        [JsonProperty("auth_list")]
        public AuthApp Auths { get; set; }

        public class AuthApp
        {
            [JsonProperty("appid_list")]
            public List<string> AppIds { get; set; }
        }
    }
}
