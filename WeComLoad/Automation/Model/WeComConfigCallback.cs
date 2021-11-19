using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeComLoad.Automation
{
    public class WeComConfigCallback
    {
        public int statusCode { get; set; }
        public string method { get; set; }
        public Result result { get; set; }

        public class Result
        {
            public int errCode { get; set; }
            public string humanMessage { get; set; }
        }

    }
}
