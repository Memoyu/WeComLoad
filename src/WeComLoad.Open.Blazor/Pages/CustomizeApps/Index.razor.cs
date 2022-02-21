namespace WeComLoad.Open.Blazor.Pages.CustomizeApps;

public partial class Index
{
    private string defaultLogo = "https://gw.alipayobjects.com/zos/antfincdn/XAosXuNZyF/BiazfanxmamNRoxxVxka.png";

    private string[] buckets = { "loweb-dev-scrmh5", "loweb-test-scrmh5", "loweb-prod-scrmh5" };

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

    private AuditConfig authConfig { get; set; } = new AuditConfig();

    private CustAppSetting Settings { get; set; } = new CustAppSetting();

    [Inject]
    private HttpClient HttpClient { get; set; }

    [Inject]
    public IWeComOpenSvc WeComOpenSvc { get; set; }

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
        var data = await WeComOpenSvc.GetCustomAppTplsAsync();
        if (data?.suite_list == null)
        {
            customAppTpls = new List<Suite>();
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
        if (string.IsNullOrWhiteSpace(suiteid)) return;
        authappListLoading = true;
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

        if (!authApps.has_next_page)
            initLoading = true;
        else
            initLoading = false;

        authappListLoading = false;

        StateHasChanged();
    }

    private void AuthCustAppAndOnlineAsync(CorpApp app)
    {

        authConfig = new AuditConfig
        {
            AppId = app.app_id,
            CallbackUrl = Settings.Callback.Prod,
            CallbackUrlComplete = Settings.Callback.Prod,
            Domain = Settings.Domain.Prod,
            HomePage = Settings.HomePage.Prod,
            WhiteIp = Settings.WhiteIp.Prod,
            VerifyBucket = buckets[2],
        };

        if (currTpl.name.Contains("开发"))
        {
            authConfig.CallbackUrl = Settings.Callback.Dev;
            authConfig.CallbackUrlComplete = Settings.Callback.Dev;
            authConfig.Domain = Settings.Domain.Dev;
            authConfig.HomePage = Settings.HomePage.Dev;
            authConfig.WhiteIp = Settings.WhiteIp.Dev;
            authConfig.EnvType = 1;
            authConfig.VerifyBucket = buckets[0];
        }
        else if (currTpl.name.Contains("测试"))
        {
            authConfig.CallbackUrl = Settings.Callback.Test;
            authConfig.CallbackUrlComplete = Settings.Callback.Test;
            authConfig.Domain = Settings.Domain.Test;
            authConfig.HomePage = Settings.HomePage.Test;
            authConfig.WhiteIp = Settings.WhiteIp.Test;
            authConfig.EnvType = 2;
            authConfig.VerifyBucket = buckets[1];
        }

        modalTitle = app.authcorp_name;
        modalVisible = true;
    }

    private async Task SubmitAuditAndOnlineAsync(CorpApp app)
    {
        try
        {
            // 提交审核
            var resSubmitAudit = await WeComOpenSvc.SubmitAuditCorpAppAsync(new SubmitAuditCorpAppRequest(app.app_id, currTpl.suiteid.ToString()));
            if (resSubmitAudit.Flag)
            {
                _ = MessageService.Error("审核应用失败！");
                return;
            }

            var auditOrderId = resSubmitAudit.Result?.auditorder?.auditorderid;

            // 上线应用
            var resOnline = await WeComOpenSvc.OnlineCorpAppAsync(new OnlineCorpAppRequest(auditOrderId));
            if (resOnline.Flag)
            {
                _ = MessageService.Error("上线应用失败！");
                return;
            }

            _ = MessageService.Success($"审核上线应用成功");
            await RefreshCustAppAuthsAsync();
        }
        catch (Exception ex)
        {
            _ = MessageService.Error($"审核上线应用异常，异常信息：{ex.Message}");
            return;
        }
    }

    private async Task OnlineAsync(CorpApp app)
    {
        try
        {
            // 上线应用
            var resOnline = await WeComOpenSvc.OnlineCorpAppAsync(new OnlineCorpAppRequest(app.auditorder.auditorderid));
            if (resOnline.Flag)
            {
                _ = MessageService.Error("上线应用失败！");
                return;
            }

            _ = MessageService.Success($"上线应用成功");
            await RefreshCustAppAuthsAsync();
        }
        catch (Exception ex)
        {
            _ = MessageService.Error($"上线应用异常，异常信息：{ex.Message}");
            return;
        }
    }

    private async Task HandleOk(MouseEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(authConfig.CorpId))
        {
            _ = MessageService.Warning("请输入授权企业Id");
            return;
        }

        if (string.IsNullOrWhiteSpace(authConfig.Domain))
        {
            _ = MessageService.Warning("请输入可信域名");
            return;
        }

        if (string.IsNullOrWhiteSpace(authConfig.CallbackUrlComplete))
        {
            _ = MessageService.Warning("请输入回调地址");
            return;
        }


        var authAppReq = new AuthCorpAppRequest();
        authAppReq.suiteid = currTpl.suiteid.ToString();
        authAppReq.corpapp = new AuthCorpAppRequest.CorpappReq().MapFrom(currTpl);
        authAppReq.corpapp.app_id = authConfig.AppId;
        authAppReq.corpapp.callbackurl = authConfig.CallbackUrlComplete;
        authAppReq.corpapp.redirect_domain = authConfig.Domain;
        if (!string.IsNullOrWhiteSpace(authConfig.WhiteIp))
            authAppReq.corpapp.white_ip_list.ip = new string[1] { authConfig.WhiteIp };
        authAppReq.corpapp.homepage = authConfig.HomePage;
        authAppReq.corpapp.enter_homeurl_in_wx = true;
        authAppReq.corpapp.page_type = "CREATE";

        try
        {
            loading = true;
            var authRes = await WeComOpenSvc.AuthCustAppAndOnlineAsync(authAppReq, authConfig.VerifyBucket);
            if (!authRes)
            {
                _ = MessageService.Error($"开发上线应用失败");
                loading = false;
                return;
            }
           _ = MessageService.Success($"开发上线应用成功");
            modalVisible = false;
            loading = false;
            await RefreshCustAppAuthsAsync();
        }
        catch (Exception ex)
        {
            _ = MessageService.Error($"开发上线应用异常，异常信息：{ex.Message}");
            return;
        }
    }

    private void HandleCancel(MouseEventArgs e)
    {
        modalVisible = false;
    }

    private void InputCorpId(string corpId)
    {
        authConfig.CallbackUrlComplete = string.Format(authConfig.CallbackUrl, corpId);
    }

    private void SelectEnv(int type)
    {
        switch (type)
        {
            case 1:
                authConfig.CallbackUrl = Settings.Callback.Dev;
                authConfig.CallbackUrlComplete = string.Format(authConfig.CallbackUrl, authConfig.CorpId);
                authConfig.WhiteIp = Settings.WhiteIp.Dev;
                authConfig.Domain = Settings.Domain.Dev;
                authConfig.HomePage = Settings.HomePage.Dev;
                authConfig.VerifyBucket = buckets[0];
                break;
            case 2:
                authConfig.CallbackUrl = Settings.Callback.Test;
                authConfig.CallbackUrlComplete = string.Format(authConfig.CallbackUrl, authConfig.CorpId);
                authConfig.WhiteIp = Settings.WhiteIp.Test;
                authConfig.Domain = Settings.Domain.Test;
                authConfig.HomePage = Settings.HomePage.Test;
                authConfig.VerifyBucket = buckets[1];
                break;
            case 3:
                authConfig.CallbackUrl = Settings.Callback.Prod;
                authConfig.CallbackUrlComplete = string.Format(authConfig.CallbackUrl, authConfig.CorpId);
                authConfig.WhiteIp = Settings.WhiteIp.Prod;
                authConfig.Domain = Settings.Domain.Prod;
                authConfig.HomePage = Settings.HomePage.Prod;
                authConfig.VerifyBucket = buckets[2];
                break;
        }
    }
}
