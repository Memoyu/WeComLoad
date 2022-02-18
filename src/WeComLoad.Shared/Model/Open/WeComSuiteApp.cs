using static WeComLoad.Shared.Model.WeComSuiteAppAuthDetail;

namespace WeComLoad.Shared.Model;

public class WeComSuiteApp
{
    public int suite_cnt_before_customized_filter { get; set; }

    public SuiteApp suite_list { get; set; }

    public class SuiteApp
    {
        public List<Suite> suite { get; set; }
    }
}
