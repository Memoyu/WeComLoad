using MaterialDesignThemes.Wpf;
using Prism.Services.Dialogs;

namespace WeComLoad.Open.ViewModels;

public class LoginViewModel : BaseNavigationViewModel
{
    private readonly IWeComOpen _weComOpen;

    private string _qrCodeKey = string.Empty;

    public DelegateCommand RefreshQrCodeCommand { get; private set; }

    public DelegateCommand ExitCommand { get; private set; }

    private SnackbarMessageQueue snackbarMessageQueue;

    public SnackbarMessageQueue SnackbarMessage
    {
        get { return snackbarMessageQueue; }
        set { snackbarMessageQueue = value; RaisePropertyChanged(); }
    }

    private string loginHint = "请扫码登录";
    public string LoginHint
    {
        get { return loginHint; }
        set { loginHint = value; RaisePropertyChanged(); }
    }


    private BitmapFrame source;
    public BitmapFrame Source
    {
        get { return source; }
        set { source = value; RaisePropertyChanged(); }
    }


    public LoginViewModel(IWeComOpen weComOpen, IContainerProvider containerProvider) :
        base(containerProvider)
    {
        SnackbarMessage = new SnackbarMessageQueue();
        _weComOpen = weComOpen;
        RefreshQrCodeCommand = new DelegateCommand(RefreshQrCodeHandler);
        ExitCommand = new DelegateCommand(ExitLoginHandler);
        GoToLogin();
    }



    private void ExitLoginHandler()
    {
        EventAggregator.PubMainDialog(new MainDialogEventModel
        {
            IsOpen = false
        });
    }

    private void RefreshQrCodeHandler()
    {
        GetLoginQrCode();
    }

    public void SnackBar(string msg) => SnackbarMessage.Enqueue(msg);

    private async void GetLoginQrCode()
    {
        _qrCodeKey = await GetLoginAndShowQrCodeAsync();
    }

    private async void GoToLogin()
    {
        _qrCodeKey = await GetLoginAndShowQrCodeAsync();
        var isLogin = false;
        int count = 1;
        var delay = 2000;
        while (!isLogin)
        {
            if (string.IsNullOrWhiteSpace(_qrCodeKey)) continue;
            var state = await GetLoginStatusAsync(_qrCodeKey);
            if (state.Code == 4 || state.Code == 5)
            {
                GetLoginQrCode();
                continue;
            }
            else if (state.Code == 6)
            {
                isLogin = true;
            }
            LoginHint = state.Msg;
            await Task.Delay(delay);
            count++;
        }

        EventAggregator.PubMainDialog(new MainDialogEventModel
        {
            IsOpen = false
        });

        EventAggregator.PubMainSnackbar(new MainSnackbarEventModel
        {
            Msg = "登录成功"
        });
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


    #region IDialogService

    public string Title => "Login";

    public event Action<IDialogResult> RequestClose;

    public bool CanCloseDialog()
    {
        return true;
    }

    public void OnDialogClosed()
    { }

    public void OnDialogOpened(IDialogParameters parameters)
    { }

    #endregion
}
