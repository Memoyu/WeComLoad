namespace WeComLoad.Open.Blazor.Pages.CustomizeApps;

public partial class Index
{
    private List<SuiteAppItem> customAppTpls { get; set; } = new List<SuiteAppItem> ();

    private List<CorpApp> customAppAuths { get; set; } = new List<CorpApp> ();

    [Inject]
    public IWeComOpen WeComOpen { get; set; }

    [Inject]
    public IFileClientPro FileClientPro { get; set; }

    [Inject]
    public MessageService Message { get; set; }

    [Inject]
    public NavigationManager NavigationManager { get; set; }


    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task GetCustAppTplAsync()
    {
        var apps = await WeComOpen.GetCustomAppsAsync();
        if (apps?.Data?.suite_list == null)
        {
            customAppTpls = new List<SuiteAppItem>();
            return;
        }
        customAppTpls = apps.Data.suite_list.suite;
    }
}
