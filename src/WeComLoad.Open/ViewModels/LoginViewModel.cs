namespace WeComLoad.Open.ViewModels;

public class LoginViewModel : BindableBase
{
    private readonly IWeComOpen _weComOpen;
    private readonly IEventAggregator _eventAggregator;

    private BitmapFrame source;
    public BitmapFrame Source
    {
        get { return source; }
        set { source = value; RaisePropertyChanged(); }
    }


    public LoginViewModel(IWeComOpen weComOpen, IContainerProvider containerProvider)
    {
        _weComOpen = weComOpen;
        _eventAggregator = containerProvider.Resolve<IEventAggregator>();
        GetLoginQrCode();
    }


    private async void GetLoginQrCode()
    {
        var key = await GetLoginAndShowQrCodeAsync();
        var isLogin = false;
        int count = 1;
        var delay = 2000;
        while (!isLogin)
        {
            var state = await GetLoginStatusAsync(key);
            if (state.Code == 4 || state.Code == 5)
            {
                key = await GetLoginAndShowQrCodeAsync();
                continue;
            }
            else if (state.Code == 6)
            {
                isLogin = true;
            }
            await Task.Delay(delay);
            count++;
        }

        _eventAggregator.GetEvent<LoginEvent>().Publish(new LoginEventModel { IsOpen = false });
    }


    #region 登录操作

    private async Task<string> GetLoginAndShowQrCodeAsync()
    {
        var (url, key) = await _weComOpen.GetLoginQrCodeUrlAsync();
        byte[] btyarray = GetImageFromResponse(url);
        MemoryStream ms = new MemoryStream(btyarray);
        Source = BitmapFrame.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.Default);
        return key;
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
            var status = await _weComOpen.GetQrCodeScanStatusAsync(qrCodeKey);
            if (status == null) return (1, "登录失败");
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

                    var res = await _weComOpen.LoginAsync(qrCodeKey, status.AuthCode);
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
            Trace.WriteLine($"获取企微后台登录二维码扫描状态异常 异常：{ex.Message}");
            return (5, "登录异常");
        }
    }

    #endregion

    #region Private

    private byte[] GetImageFromResponse(string url, string cookie = null)
    {
    redo:
        try
        {
            System.Net.WebRequest request = System.Net.WebRequest.Create(url);
            if (!string.IsNullOrWhiteSpace(cookie))
            {
                request.Headers[System.Net.HttpRequestHeader.Cookie] = cookie;
            }

            System.Net.WebResponse response = request.GetResponse();

            using (Stream stream = response.GetResponseStream())
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Byte[] buffer = new Byte[1024];
                    int current = 0;
                    do
                    {
                        ms.Write(buffer, 0, current);
                    } while ((current = stream.Read(buffer, 0, buffer.Length)) != 0);

                    return ms.ToArray();
                }
            }
        }
        catch (System.Net.WebException ex)
        {
            if (ex.Message == "基础连接已经关闭: 发送时发生错误。")
            {
                goto redo;
            }
            else
            {
                throw;
            }
        }


    }

    #endregion
}
