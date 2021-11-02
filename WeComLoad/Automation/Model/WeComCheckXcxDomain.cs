using System.Collections.Generic;

namespace WeComLoad.Automation
{
    public class WeComCheckXcxDomain
    {
        public List<CheckSdkDomainResult> Result { get; set; }

        public bool CheckXcxDomain { get; set; }

        public class CheckSdkDomainResult
        {
            public string Name { get; set; }

            public bool Status { get; set; }
        }
    }
}
