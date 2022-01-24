using Microsoft.Extensions.Configuration;

namespace WeComLoad.Open.Common
{
    public class AppSettings
    {
        public AuditApp AuditApp { get; set; }
    }


    public class AuditApp
    {
        public string Domain { get; set; }

        public string Callback { get; set; }

        public string HomePage { get; set; }

        public string Ip { get; set; }
    }
}
