using Microsoft.JSInterop;
using System.Threading;

namespace WeComLoad.Admin.Blazor.Pages.User.Login;

public partial class Index : IAsyncDisposable
{
    private string qrCode { get; set; } = "https://gw.alipayobjects.com/zos/antfincdn/XAosXuNZyF/BiazfanxmamNRoxxVxka.png";

    private string loginHint { get; set; } = "请扫码登陆";

    private bool modalVisible = false;

    private List<LoginCorp> corps = new List<LoginCorp>();

    private string tlKey = string.Empty;

    private string selectedCorpId = string.Empty;

    private string qrCodeKey = string.Empty;

    private QuickLoginCorpInfo corpInfo { get; set; }

    private bool canRefresh { get; set; } = false;

    private CancellationTokenSource _cts = null;

    [Inject]
    public IJSRuntime JS { get; set; }

    [Inject]
    public IWeComAdmin WeComAdmin { get; set; }

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

    private async Task ModalHandleOk(MouseEventArgs e)
    {
       var flag = await WeComAdmin.WxLoginAsync(tlKey, selectedCorpId);
        if (flag)
        {
            modalVisible = false;
            NavigationManager.NavigateTo("/");
        }
    }

    private void ModalHandleCancel(MouseEventArgs e)
    {
        modalVisible = false;
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
                    if (status.AuthSource.Equals("SOURCE_FROM_WEWORK"))
                    {
                        var res = await WeComAdmin.LoginAsync(qrCodeKey, status.AuthCode);
                        if (!res)
                        {
                            statusCode = 5;
                            statusMsg = "登录失败";
                            break;
                        }

                        statusCode = 6;
                        statusMsg = $"登录成功";

                        break;
                    }
                    else if (status.AuthSource.Equals("SOURCE_FROM_WX"))
                    {
                        var data = await WeComAdmin.GetWxLoginCorpsAsync(qrCodeKey, status.AuthCode);
                        corps = data.Corps;
                        tlKey = data.TlKey;
                        modalVisible = true;
                        statusCode = 7;
                        statusMsg = $"微信扫码成功";
                        break;
                    }

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

