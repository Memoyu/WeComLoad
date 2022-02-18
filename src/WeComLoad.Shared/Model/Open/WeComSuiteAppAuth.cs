namespace WeComLoad.Shared.Model;

public class WeComSuiteAppAuth
{
    public bool has_next_page { get; set; }

    public CorpappList corpapp_list { get; set; }

    public int total { get; set; }


    public class CorpappList
    {
        public List<CorpApp> corpapp { get; set; }
    }
}
