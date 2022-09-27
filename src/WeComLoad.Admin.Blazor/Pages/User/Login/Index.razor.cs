using Microsoft.JSInterop;
using System.Threading;
using System.Timers;

namespace WeComLoad.Admin.Blazor.Pages.User.Login;

public partial class Index : IAsyncDisposable
{
    private string qrCode { get; set; } = "https://gw.alipayobjects.com/zos/antfincdn/XAosXuNZyF/BiazfanxmamNRoxxVxka.png";

    private string loginHint { get; set; } = "请扫码登陆";

    private bool selectModalVisible = false;

    private bool captchaModalVisible = false;

    private List<LoginCorp> corps = new List<LoginCorp>();

    private string tlKey = string.Empty;

    /// <summary>
    /// 验证码
    /// </summary>
    private string captcha = string.Empty;

    private string mobile = string.Empty;

    private string selectedCorpId = string.Empty;

    private string qrCodeKey = string.Empty;

    private QuickLoginCorpInfo corpInfo { get; set; }

    private bool canRefresh { get; set; } = false;

    private bool canReSendCaptcha { get; set; } = false;

    private int second { get; set; } = 60;

    private System.Timers.Timer t = new System.Timers.Timer(1000);//实例化Timer类，设置间隔时间为10000毫秒；

    private CancellationTokenSource _cts = null;

    [Inject]
    public IJSRuntime JS { get; set; }

    [Inject]
    public IWeComAdminSvc WeComAdmin { get; set; }

    [Inject]
    public MessageService MessageService { get; set; }

    [Inject]
    public NavigationManager NavigationManager { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        // QrCode = "https://gw.alipayobjects.com/zos/antfincdn/XAosXuNZyF/BiazfanxmamNRoxxVxka.png";
        await GotoLoginAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {

    }

    public async ValueTask DisposeAsync()
    {
        await Task.CompletedTask;
        _cts?.Cancel();
    }

    private async Task GotoLoginAsync()
    {
        _cts?.Cancel();
        await GetLoginAndShowQrCodeAsync();
        var isConfirmLogin = false;
        var isWxLogin = false;
        int count = 1;
        var delay = 2000;
        _cts = new CancellationTokenSource();
        var tk = _cts.Token;
        _ = Task.Factory.StartNew(async tk =>
        {
            CancellationToken ct = (CancellationToken)tk;
            while (!isConfirmLogin)
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
                    isConfirmLogin = true;
                    canRefresh = true;
                    delay = 0;
                    _ = MessageService.Success("登录成功");
                }
                else if (state.Code == 7)
                {
                    isConfirmLogin = true;
                    isWxLogin = true;
                    captchaModalVisible = true;
                }
                loginHint = state.Msg;
                await InvokeAsync(() => StateHasChanged());
                await Task.Delay(delay);
                count++;
            }

            if (!isWxLogin)
            {
                NavigationManager.NavigateTo("/");
            }

        }, tk, TaskCreationOptions.LongRunning);
    }

    private void SelectedCorp(string corpId)
    {
        selectedCorpId = corpId;
        StateHasChanged();
    }

    private async Task HandleSelectModalOk(MouseEventArgs e)
    {
        // 如果微信未绑定企业微信，则直接关闭弹窗，刷新二维码
        if (corps == null || corps.Count <= 0)
        {
            // 刷新二维码
            await GetLoginAndShowQrCodeAsync();
            selectModalVisible = false;
            return;
        }

        var flag = await WeComAdmin.WxLoginAsync(tlKey, selectedCorpId);
        if (flag == 1)
        {
            selectModalVisible = false;
            NavigationManager.NavigateTo("/");
        }
        else if (flag == 2)
        {
            selectModalVisible = false;
            captchaModalVisible = true;
            CreateCaptchaTimer();
            StateHasChanged();
        }
        else // 登录失败
        {
            // 刷新二维码
            await GetLoginAndShowQrCodeAsync();
            selectModalVisible = false;
        }
    }

    private async Task HandleSelectModalCancel(MouseEventArgs e)
    {
        // 刷新二维码
        await GetLoginAndShowQrCodeAsync();
        selectModalVisible = false;
    }

    private async Task RefreshCaptchaAsync()
    {
        await WeComAdmin.LoginSendCaptchaAsync(tlKey);
        second = 60;
        canReSendCaptcha = false;
        CreateCaptchaTimer();
        StateHasChanged();
    }

    private async Task HandleCaptchaModalOk(MouseEventArgs e)
    {
        if (captcha.Length != 6)
        {
            _ = MessageService.Error($"验证码位数有误");
            return;
        }

        await WeComAdmin.LoginConfirmCaptchaAsync(tlKey, captcha);
    }

    private async Task HandleCaptchaModalCancel(MouseEventArgs e)
    {
        // 刷新二维码
        await GetLoginAndShowQrCodeAsync();
        captchaModalVisible = false;
    }

    #region 登录操作

    private async Task GetLoginAndShowQrCodeAsync()
    {
        (qrCode, qrCodeKey) = await WeComAdmin.GetLoginQrCodeUrlAsync();
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
            var status = await WeComAdmin.GetQrCodeScanStatusAsync(qrCodeKey);
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
                    var res = await WeComAdmin.LoginAsync(qrCodeKey, status.AuthCode, status.AuthSource);
                    if (res.flag == -1)
                    {
                        statusCode = 5;
                        statusMsg = "登录失败";
                        break;
                    }
                    else if (res.flag == 0) // 需要输入验证码
                    {
                        statusCode = 7;
                        mobile = res.mobile;
                        statusMsg = "需要验证码登录";
                    }


                    statusCode = 6;
                    statusMsg = $"登录成功";

                    break;

                // 原本需要判断是企微扫码还是微信扫码，然后进行不同的处理
                // 现在改版后只需要统一处理即可
                //if (status.AuthSource.Equals("SOURCE_FROM_WEWORK"))
                //{
                //}
                //else if (status.AuthSource.Equals("SOURCE_FROM_WX"))
                //{
                //    var data = await WeComAdmin.GetWxLoginCorpsAsync(qrCodeKey, status.AuthCode);
                //    corps = data.Corps;
                //    tlKey = data.TlKey;
                //    selectModalVisible = true;
                //    statusCode = 7;
                //    statusMsg = $"微信扫码成功";
                //    break;
                //}
                // break;
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

    #region Private

    private void CreateCaptchaTimer()
    {
        t.Elapsed += new ElapsedEventHandler(TimeExecute);//到达时间的时候执行事件；
        t.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；
        t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
        t.Start(); //启动定时器
    }

    private void TimeExecute(object source, ElapsedEventArgs e)
    {
        if (second <= 0)
        {
            t.Stop();
            canReSendCaptcha = true;
        }

        _ = InvokeAsync(() =>
        {
            second -= 1;
            StateHasChanged();
        });
    }

    #endregion
}

