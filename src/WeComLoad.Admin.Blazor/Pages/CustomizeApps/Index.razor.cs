namespace WeComLoad.Admin.Blazor.Pages.CustomizeApps;

public partial class Index
{
    private string defaultLogo = "https://gw.alipayobjects.com/zos/antfincdn/XAosXuNZyF/BiazfanxmamNRoxxVxka.png";


    private Suite currTpl { get; set; } = new Suite();

    private bool loading { get; set; } = false;

    private bool authappListLoading { get; set; } = false;

    private bool moreLoading { get; set; } = false;

    private bool initLoading { get; set; } = true;

    private int size { get; set; } = 10;

    private int currOffset { get; set; } = 0;

    private int totalAuthApp { get; set; } = 0;

    private bool modalVisible = false;

    private string modalTitle = string.Empty;

    private List<Suite> customAppTpls { get; set; } = new List<Suite>();

    private List<CorpApp> customAppAuths { get; set; } = new List<CorpApp>();

    private AuditConfig authConfig { get; set; } = new AuditConfig();

    private CustAppSetting Settings { get; set; } = new CustAppSetting();

    [Inject]
    private HttpClient HttpClient { get; set; }

    [Inject]
    public IWeComAdminSvc WeComOpenSvc { get; set; }

    [Inject]
    public MessageService MessageService { get; set; }

    [Inject]
    public NavigationManager NavigationManager { get; set; }


    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Settings = await HttpClient.GetFromJsonAsync<CustAppSetting>("resources/custapp.settings.json");
        await GetCustAppTplAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task GetCustAppTplAsync()
    {
        await Task.CompletedTask;
    }

    private async Task LoadMoreAuthAppAsync()
    {
        loading = true;
        if (customAppAuths.Count >= totalAuthApp) return;
        currOffset += size;
        await GetCustAppAuthsAsync(currTpl?.suiteid.ToString(), currOffset);
        loading = false;
    }

    private async Task RefreshCustAppAuthsAsync()
    {
        await SelectCustAppTplHandler(currTpl);
    }

    private async Task SelectCustAppTplHandler(Suite tpl)
    {
        if (tpl == null) return;
        currTpl = tpl;
        currOffset = 0;
        customAppAuths = new List<CorpApp>();
        await GetCustAppAuthsAsync(tpl?.suiteid.ToString(), currOffset);
    }

    private async Task GetCustAppAuthsAsync(string suiteid, int currOffset)
    {
        await Task.CompletedTask;
    }

    private void AuthCustAppAndOnlineAsync(CorpApp app)
    {
    }

    private async Task SubmitAuditAndOnlineAsync(CorpApp app)
    {
        await Task.CompletedTask;
    }

    private async Task OnlineAsync(CorpApp app)
    {
        await Task.CompletedTask;
    }

    private async Task HandleOk(MouseEventArgs e)
    {
        await Task.CompletedTask;
    }

    private void HandleCancel(MouseEventArgs e)
    {
        modalVisible = false;
    }

    private void InputCorpId(string corpId)
    {
        authConfig.CallbackUrlComplete = string.Format(authConfig.CallbackUrl, corpId);
    }
}
