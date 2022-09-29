using System.Net;
using System.Text;
using Microsoft.JSInterop;
using System.Threading;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using WeComLoad.Shared;
using System.Timers;

namespace WeComLoad.Open.Blazor.Pages.User.Login;

public partial class Index : IAsyncDisposable
{
    private string qrCode { get; set; } =
        "https://gw.alipayobjects.com/zos/antfincdn/XAosXuNZyF/BiazfanxmamNRoxxVxka.png";

    private string loginHint { get; set; } = "请扫码登陆";


    private string qrCodeKey = string.Empty;

    private QuickLoginCorpInfo corpInfo { get; set; }

    private bool canRefresh { get; set; } = false;

    private bool captchaModalVisible = false;

    /// <summary>
    /// 验证码
    /// </summary>
    private string captcha = string.Empty;

    private bool canReSendCaptcha { get; set; } = false;


    private CaptchParam captchParam;


    private int second { get; set; } = 60;

    private System.Timers.Timer t = new System.Timers.Timer(1000);//实例化Timer类，设置间隔时间为10000毫秒；

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

    [Inject] public IWeComOpenSvc WeComOpenSvc { get; set; }

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
        /* try
         {
             HttpWebRequest request = null;
             HttpWebResponse response = null;
             request = (HttpWebRequest)WebRequest.Create("https://localhost.work.weixin.qq.com:50010/checkLoginState");
             request.Method = "POST";
             request.Referer = "https://open.work.weixin.qq.com";
             byte[] postdatabyte =
                 Encoding.UTF8.GetBytes("{\"scene\":1,\"redirect_uri\":\"https://open.work.weixin.qq.com\"}");
             Stream stream = await request.GetRequestStreamAsync();
             stream.Write(postdatabyte, 0, postdatabyte.Length);
             stream.Close();

             request.ContentType = "application/json";
             request.ContentLength = postdatabyte.Length;


             response = (HttpWebResponse)await request.GetResponseAsync();
             Stream responseStream = response.GetResponseStream();
             StreamReader sr = new StreamReader(responseStream);
             var responseStr = sr.ReadToEnd();
             response.Close();
             responseStream.Close();
             Console.WriteLine(responseStr);
         }
         catch (WebException ex)
         {
             var resp = (HttpWebResponse)ex.Response;
             Stream responseStream = resp.GetResponseStream();
             StreamReader sr = new StreamReader(responseStream);
             var responseStr = sr.ReadToEnd();
             resp.Close();
             responseStream.Close();
             Console.WriteLine(responseStr);
         }*/

        var param = await WeComOpenSvc.GetQuickLoginParameAsync();
        if (param != null)
        {
            var localLoginState = await CheckLoginStateAsync(param);
            if (localLoginState)
            {
                _webKey = param.WebKey;
                var isConfirm = await ConfirmLoginAsync(param.ClientKey);
                if (isConfirm)
                {
                    var developerInfo = await WeComOpenSvc.GetCorpBindDeveloperInfoAsync(param.WebKey);
                    if (developerInfo != null && developerInfo.is_service && developerInfo.is_service_admin)
                    {
                        corpInfo = await WeComOpenSvc.GetQuickLoginCorpInfoAsync(param.WebKey);
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
        var isConfirmLogin = false;
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
                    isLogin = true;
                    canRefresh = true;
                    delay = 0;
                    _ = MessageService.Success("登录成功");
                }
                else if (state.Code == 7)
                {
                    isConfirmLogin = true;
                    captchaModalVisible = true;
                    CreateCaptchaTimer();
                }

                loginHint = state.Msg;
                await InvokeAsync(() => StateHasChanged());
                await Task.Delay(delay);
                count++;
            }

            if (isLogin)
            {
                NavigationManager.NavigateTo("/");
            }

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

        var auth = await WeComOpenSvc.ConfirmQuickLoginAsync(_webKey);
        if (auth == null || string.IsNullOrWhiteSpace(auth.auth_code))
        {
            _ = MessageService.Error($"快速登录失败：{auth.msg}");
            await ReloadPage();
            return;
        }

        var success = await WeComOpenSvc.LoginAfterAsync(auth.auth_code, "SOURCE_FROM_PC_APP");
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

    private async Task HandleCaptchaModalOk(MouseEventArgs e)
    {
        if (captcha.Length != 6)
        {
            _ = MessageService.Error($"验证码位数有误");
            return;
        }

        var res = await WeComOpenSvc.LoginConfirmCaptchaAsync(captchParam, captcha);
        if (!res.flag)
        {
            _ = MessageService.Error($"确认验证码失败 Err:{res.msg}");
            return;
        }

        _ = MessageService.Success("登录成功");
        NavigationManager.NavigateTo("/");
    }

    private async Task HandleCaptchaModalCancel(MouseEventArgs e)
    {
        // 刷新二维码
        await GotoLoginAsync();
        captchaModalVisible = false;
    }

    private async Task RefreshCaptchaAsync()
    {
        var res = await WeComOpenSvc.LoginSendCaptchaAsync(captchParam.TlKey);
        if (res.flag)
        {
            second = 60;
            canReSendCaptcha = false;
            CreateCaptchaTimer();
            StateHasChanged();
        }
        else
        {
            _ = MessageService.Error($"刷新验证码失败 Err:{res.msg}");
        }
    }

    private async ValueTask ReloadPage()
    {
        await _module.InvokeAsync<object>("reload");
    }

    #region 登录操作

    private async Task GetLoginAndShowQrCodeAsync()
    {
        (qrCode, qrCodeKey) = await WeComOpenSvc.GetLoginQrCodeUrlAsync();
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
            var status = await WeComOpenSvc.GetQrCodeScanStatusAsync(qrCodeKey);
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
                    var res = await WeComOpenSvc.LoginAsync(qrCodeKey, status.AuthCode, status.AuthSource);
                    if (res.flag == -1)
                    {
                        statusCode = 5;
                        statusMsg = $"登录失败：{res.msg}";
                        break;
                    }
                    else if (res.flag == 0)
                    {
                        statusCode = 7;
                        statusMsg = "需要验证码校验";
                        captchParam = res.param;
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
            Console.WriteLine($"{ex.Message}, {ex.StackTrace}");
            return (5, "登录异常");
        }
    }


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