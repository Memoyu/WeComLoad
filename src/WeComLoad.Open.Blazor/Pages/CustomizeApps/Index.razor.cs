using Microsoft.AspNetCore.Components.Web;

namespace WeComLoad.Open.Blazor.Pages.CustomizeApps;

public partial class Index
{
    private string defaultLogo = "https://gw.alipayobjects.com/zos/antfincdn/XAosXuNZyF/BiazfanxmamNRoxxVxka.png";

    private string[] buckets = { "loweb-dev-scrmh5", "loweb-test-scrmh5", "loweb-prod-scrmh5" };

    private SuiteAppItem currTpl { get; set; } = new SuiteAppItem();

    private bool loading { get; set; } = false;

    private bool initLoading { get; set; } = true;

    private int size { get; set; } = 3;

    private int currOffset { get; set; } = 0;

    private int totalAuthApp { get; set; } = 0;

    private bool modalVisible = false;

    private string modalTitle = string.Empty;

    private List<SuiteAppItem> customAppTpls { get; set; } = new List<SuiteAppItem>();
    //{
    //    new SuiteAppItem { name="Test", description="ddddddd", logo ="https://gw.alipayobjects.com/zos/antfincdn/XAosXuNZyF/BiazfanxmamNRoxxVxka.png" },
    //    new SuiteAppItem { name="Tegagst", description="ddddagagargagagddd", logo ="https://gw.alipayobjects.com/zos/antfincdn/XAosXuNZyF/BiazfanxmamNRoxxVxka.png" },
    //    new SuiteAppItem { name="Teagagagast", description="ddddddfrgsrhgsrgd", logo ="https://gw.alipayobjects.com/zos/antfincdn/XAosXuNZyF/BiazfanxmamNRoxxVxka.png" },
    //    new SuiteAppItem { name="Test", description="ddddddd", logo ="https://gw.alipayobjects.com/zos/antfincdn/XAosXuNZyF/BiazfanxmamNRoxxVxka.png" },
    //    new SuiteAppItem { name="Tegagst", description="ddddagagargagagddd", logo ="https://gw.alipayobjects.com/zos/antfincdn/XAosXuNZyF/BiazfanxmamNRoxxVxka.png" },
    //    new SuiteAppItem { name="Teagagagast", description="ddddddfrgsrhgsrgd", logo ="https://gw.alipayobjects.com/zos/antfincdn/XAosXuNZyF/BiazfanxmamNRoxxVxka.png" },
    //    new SuiteAppItem { name="Test", description="ddddddd", logo ="https://gw.alipayobjects.com/zos/antfincdn/XAosXuNZyF/BiazfanxmamNRoxxVxka.png" },
    //    new SuiteAppItem { name="Tegagst", description="ddddagagargagagddd", logo ="https://gw.alipayobjects.com/zos/antfincdn/XAosXuNZyF/BiazfanxmamNRoxxVxka.png" },
    //    new SuiteAppItem { name="Teagagagast", description="ddddddfrgsrhgsrgd", logo ="https://gw.alipayobjects.com/zos/antfincdn/XAosXuNZyF/BiazfanxmamNRoxxVxka.png" }
    //};

    private List<CorpApp> customAppAuths { get; set; } = new List<CorpApp>();

    private AuditConfig auditConfig { get; set; } = new AuditConfig();

    private CustAppSetting Settings { get; set; } = new CustAppSetting();

    [Inject]
    private HttpClient HttpClient { get; set; }

    [Inject]
    public IWeComOpenSvc WeComOpenSvc { get; set; }

    [Inject]
    public IFileClientPro FileClientPro { get; set; }

    [Inject]
    public MessageService Message { get; set; }

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
        var data = await WeComOpenSvc.GetCustomAppTplsAsync();
        if (data?.suite_list == null)
        {
            customAppTpls = new List<SuiteAppItem>();
            return;
        }
        customAppTpls = data.suite_list.suite;
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

    private async Task SelectCustAppTplHandler(SuiteAppItem tpl)
    {
        if (tpl == null) return;
        currTpl = tpl;
        currOffset = 0;
        customAppAuths = new List<CorpApp>();
        await GetCustAppAuthsAsync(tpl?.suiteid.ToString(), currOffset);
    }

    private async Task GetCustAppAuthsAsync(string suiteid, int currOffset)
    {
        if (string.IsNullOrWhiteSpace(suiteid)) return;
        var authApps = await WeComOpenSvc.GetCustomAppAuthsAsync(suiteid, currOffset, size);
        var apps = new List<CorpApp>();

        if (authApps?.corpapp_list == null)
        {
            apps = new List<CorpApp>();
        }
        else
        {
            apps = authApps.corpapp_list.corpapp;
        }

        totalAuthApp = authApps.total;

        customAppAuths.AddRange(apps);

        if (customAppAuths.Count <= 0 || customAppAuths.Count >= totalAuthApp)
            initLoading = true;
        else
            initLoading = false;

        StateHasChanged();
    }

    private async Task AuditCustAppAndOnlineAsync(CorpApp app)
    {
        auditConfig = new AuditConfig
        {
            CallbackUrl = Settings.Callback.Dev,
            CallbackUrlComplete = Settings.Callback.Dev,
            Domain = Settings.Domain.Dev,
            HomePage = Settings.HomePage.Dev,
            WhiteIp = Settings.WhiteIp.Dev,
            VerifyBucket = buckets[0],
        };
        modalTitle = app.authcorp_name;
        modalVisible = true;
    }

    private void HandleOk(MouseEventArgs e)
    {
        Console.WriteLine(e);
        modalVisible = false;
    }

    private void HandleCancel(MouseEventArgs e)
    {
        Console.WriteLine(e);
        modalVisible = false;
    }

    private void InputCorpId(string corpId)
    {
        auditConfig.CallbackUrlComplete = string.Format(auditConfig.CallbackUrl, corpId);
    }

    private void SelectEnv(int type)
    {
        switch (type)
        {
            case 1:
                auditConfig.CallbackUrl = Settings.Callback.Dev;
                auditConfig.CallbackUrlComplete = string.Format(auditConfig.CallbackUrl, auditConfig.CorpId);
                auditConfig.WhiteIp = Settings.WhiteIp.Dev;
                auditConfig.Domain = Settings.Domain.Dev;
                auditConfig.HomePage = Settings.HomePage.Dev;
                auditConfig.VerifyBucket = buckets[0];
                break;
            case 2:
                auditConfig.CallbackUrl = Settings.Callback.Test;
                auditConfig.CallbackUrlComplete = string.Format(auditConfig.CallbackUrl, auditConfig.CorpId);
                auditConfig.WhiteIp = Settings.WhiteIp.Test;
                auditConfig.Domain = Settings.Domain.Test;
                auditConfig.HomePage = Settings.HomePage.Test;
                auditConfig.VerifyBucket = buckets[1];
                break;
            case 3:
                auditConfig.CallbackUrl = Settings.Callback.Prod;
                auditConfig.CallbackUrlComplete = string.Format(auditConfig.CallbackUrl, auditConfig.CorpId);
                auditConfig.WhiteIp = Settings.WhiteIp.Prod;
                auditConfig.Domain = Settings.Domain.Prod;
                auditConfig.HomePage = Settings.HomePage.Prod;
                auditConfig.VerifyBucket = buckets[2];
                break;
        }
    }
}
