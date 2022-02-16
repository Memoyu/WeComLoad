namespace WeComLoad.Open.Blazor.Pages.User.Login;

public partial class Index
{
    private string qrCode { get; set; } = "https://gw.alipayobjects.com/zos/antfincdn/XAosXuNZyF/BiazfanxmamNRoxxVxka.png";

    private string loginHint { get; set; } = "请扫码登陆";

    private string qrCodeKey = string.Empty;

    [Inject]
    public IWeComOpen WeComOpen { get; set; }

    [Inject]
    public MessageService Message { get; set; }

    [Inject]
    public NavigationManager NavigationManager { get; set; }



    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        // QrCode = "https://gw.alipayobjects.com/zos/antfincdn/XAosXuNZyF/BiazfanxmamNRoxxVxka.png";
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
            await GotoLoginAsync();
    }

    private async Task GotoLoginAsync()
    {
        await GetLoginAndShowQrCodeAsync();
        var isLogin = false;
        int count = 1;
        var delay = 2000;

        await Task.Run(async () =>
        {
            while (!isLogin)
            {
                if (string.IsNullOrWhiteSpace(qrCodeKey)) continue;
                var state = await GetLoginStatusAsync(qrCodeKey);
                if (state.Code == 4 || state.Code == 5)
                {
                    await GetLoginAndShowQrCodeAsync();
                    continue;
                }
                else if (state.Code == 6)
                {
                    isLogin = true;
                }
                loginHint = state.Msg;
                await InvokeAsync(() => StateHasChanged());
                await Task.Delay(delay);
                count++;
            }

            await Message.Success("登录成功");

            NavigationManager.NavigateTo("/");
        });
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
            if (status == null) return (5, "登录失败");
            var statusCode = 1;
            var statusMsg = "等待扫码";
            switch (status.Status)
            {
                case "QRCODE_SCAN_ING":
                    statusMsg = "扫码成功";
                    statusCode = 2;
                    break;
                case "QRCODE_SCAN_SUCC":
                    if (!status.AuthSource.Equals("SOURCE_FROM_WEWORK"))
                    {
                        statusCode = 5;
                        statusMsg = "请使用企业微信扫码";
                        break;
                    }

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

