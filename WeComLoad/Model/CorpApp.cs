using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeComLoad.Model
{
    public class CorpApp
    {
        public List<default_app> default_app { get; set; }

        public List<custom_app> custom_app { get; set; }

        public List<openapi_app> openapi_app { get; set; }

    }

    public class default_app
    {
        public int business_id { get; set; }

        public int create_time { get; set; }

        public int open_state { get; set; }

        public string app_name { get; set; }

        public string app_logo { get; set; }

        public string app_developer { get; set; }

        public string app_info { get; set; }

        public string editor_name { get; set; }

        public object[] user_list { get; set; }

        public bool ever_opened { get; set; }

        public int display_order { get; set; }

    }
    public class custom_app
    {

    }

    public class openapi_app
    {
        public string corp_id { get; set; }

        public string app_id { get; set; }

        public int app_open_id { get; set; }

        public string name { get; set; }

        public string imgid { get; set; }

        public App_Perm app_perm { get; set; }

        public int create_time { get; set; }

        public int update_time { get; set; }

        public string last_mod_time { get; set; }

        public int app_open { get; set; }

        public int arch_rw_flag { get; set; }

        public int order { get; set; }

        public bool is_complete { get; set; }

        public string visible_flag { get; set; }

        public string aes_app_id { get; set; }

        public bool callback_open { get; set; }

        public int control_open_state { get; set; }

        public bool b_chat_shortcut { get; set; }

        public int paidappinfoid { get; set; }

        public int custom_info_open { get; set; }

        public class App_Perm
        {
            public object[] vids { get; set; }

            public string[] partyids { get; set; }

            public object[] tags { get; set; }

            public bool superadmin_visible { get; set; }

            public int app_perm_flag { get; set; }
        }

    }


}
