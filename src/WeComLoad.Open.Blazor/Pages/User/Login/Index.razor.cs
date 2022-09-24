using Microsoft.JSInterop;
using System.Threading;

namespace WeComLoad.Open.Blazor.Pages.User.Login;

public partial class Index : IAsyncDisposable
{
    private string qrCode { get; set; } =
        "https://gw.alipayobjects.com/zos/antfincdn/XAosXuNZyF/BiazfanxmamNRoxxVxka.png";

    private string loginHint { get; set; } = "请扫码登陆";


    private string qrCodeKey = string.Empty;

    private QuickLoginCorpInfo corpInfo { get; set; }

    private bool canRefresh { get; set; } = false;

    private CancellationTokenSource _cts = null;

    private IJSObjectReference _module;

    private string _webKey = string.Empty;

    private bool _isQuickLogin = false;

    public bool IsQuickLogin
    {
        get { return _isQuickLogin; }
        set
        {
            if (!value)
            {
                _webKey = string.Empty;
            }

            _isQuickLogin = value;
        }
    }

    [Inject] public IJSRuntime JS { get; set; }

    [Inject] public IWeComOpen WeComOpen { get; set; }

    [Inject] public MessageService MessageService { get; set; }

    [Inject] public NavigationManager NavigationManager { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/wecom.js");
            await CheckQuickLoginAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Task.CompletedTask;
        _cts?.Cancel();
    }

    private async Task CheckQuickLoginAsync()
    {
        var param = await WeComOpen.GetQuickLoginParameAsync();
        if (param != null)
        {
            var localLoginState = await CheckLoginStateAsync(param);
            if (localLoginState)
            {
                _webKey = param.WebKey;
                var isConfirm = await ConfirmLoginAsync(param.ClientKey);
                if (isConfirm)
                {
                    var developerInfo = await WeComOpen.GetCorpBindDeveloperInfoAsync(param.WebKey);
                    if (developerInfo != null && developerInfo.is_service && developerInfo.is_service_admin)
                    {
                        corpInfo = await WeComOpen.GetQuickLoginCorpInfoAsync(param.WebKey);
                        if (corpInfo != null)
                        {
                            IsQuickLogin = true;
                            StateHasChanged();
                            return;
                        }
                    }
                }
            }
        }

        await GotoLoginAsync();
    }

    private async Task GotoLoginAsync()
    {
        _cts?.Cancel();
        await GetLoginAndShowQrCodeAsync();
        var isLogin = false;
        int count = 1;
        var delay = 2000;
        _cts = new CancellationTokenSource();
        var tk = _cts.Token;
        _ = Task.Factory.StartNew(async tk =>
        {
            CancellationToken ct = (CancellationToken)tk;
            while (!isLogin)
            {
                ct.ThrowIfCancellationRequested();
                Console.WriteLine("循环中" + Thread.CurrentThread.ManagedThreadId);
                if (string.IsNullOrWhiteSpace(qrCodeKey)) continue;
                var state = await GetLoginStatusAsync(qrCodeKey);
                if (state.Code == 4 || state.Code == 5)
                {
                    await GetLoginAndShowQrCodeAsync();
                    _ = MessageService.Error($"登录失败：{state.Msg}");
                    continue;
                }
                else if (state.Code == 6)
                {
                    isLogin = true;
                    canRefresh = true;
                    delay = 0;
                    _ = MessageService.Success("登录成功");
                }

                loginHint = state.Msg;
                await InvokeAsync(() => StateHasChanged());
                await Task.Delay(delay);
                count++;
            }

            NavigationManager.NavigateTo("/");
        }, tk, TaskCreationOptions.LongRunning);
    }

    private async Task<bool> CheckLoginStateAsync(GetQuickLoginParam param)
    {
        var result = await _module.InvokeAsync<string>("checkLoginState", param.HttpPort, param.HttpsPort);
        if (result == null) return false;
        var state = JsonConvert.DeserializeObject<WeComBase<CheckLoginState>>(result);
        if (state == null || state.Data == null) return false;
        return true;
    }

    private async Task<bool> ConfirmLoginAsync(string clientKey)
    {
        return await _module.InvokeAsync<bool>("confirmLogin", clientKey);
    }

    private async Task QuickLoginAsync()
    {
        if (string.IsNullOrWhiteSpace(_webKey))
        {
            _ = MessageService.Error("快速登录异常，稍后继续");
            await ReloadPage();
            return;
        }

        var auth = await WeComOpen.ConfirmQuickLoginAsync(_webKey);
        var data = auth.data;
        if (data == null || string.IsNullOrWhiteSpace(data.auth_code))
        {
            _ = MessageService.Error($"快速登录失败：{auth.msg}");
            await ReloadPage();
            return;
        }

        var success = await WeComOpen.QuickLoginAsync(data.auth_code);
        if (!success)
        {
            _ = MessageService.Error($"快速登录失败，稍后再试");
            await ReloadPage();
            return;
        }

        _ = MessageService.Success("登录成功");
        NavigationManager.NavigateTo("/");
    }

    private async Task ScanQrCodeLoginAsync()
    {
        IsQuickLogin = false;
        await GotoLoginAsync();
    }

    private async ValueTask ReloadPage()
    {
        await _module.InvokeAsync<object>("reload");
    }

    #region 登录操作

    private async Task GetLoginAndShowQrCodeAsync()
    {
        (qrCode, qrCodeKey) = await WeComOpen.GetLoginQrCodeUrlAsync();
        await InvokeAsync(() => StateHasChanged());
    }

    /// <summary>
    /// 1：等待扫码；2：扫码成功；3：确认登录；4：扫码后取消登录；5：登录失败；6：登录成功
    /// </summary>
    /// <param name="qrCodeKey"></param>
    /// <returns></returns>
    private async Task<(int Code, string Msg)> GetLoginStatusAsync(string qrCodeKey)
    {
        try
        {
            // 1：等待扫码；2：扫码成功；3：确认登录；4：扫码后取消登录；5：登录失败；6：登录成功
            var status = await WeComOpen.GetQrCodeScanStatusAsync(qrCodeKey);
            if (status == null) return (5, "二维码过期");
            var statusCode = 1;
            var statusMsg = "等待扫码";
            switch (status.Status)
            {
                case "QRCODE_SCAN_ING":
                    statusMsg = "扫码成功";
                    statusCode = 2;
                    break;
                case "QRCODE_SCAN_SUCC":
                    var res = await WeComOpen.LoginAsync(qrCodeKey, status.AuthCode);
                    if (!res)
                    {
                        statusCode = 5;
                        statusMsg = "登录失败";
                        break;
                    }

                    statusCode = 6;
                    statusMsg = $"登录成功";

                    break;
                case "QRCODE_SCAN_FAIL":
                    statusCode = 4;
                    statusMsg = "取消登录";
                    break;
            }

            return (statusCode, statusMsg);
        }
        catch (Exception ex)
        {
            return (5, "登录异常");
        }
    }

    #endregion
}