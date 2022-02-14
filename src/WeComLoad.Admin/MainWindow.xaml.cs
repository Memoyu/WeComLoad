using WeComLoad.Admin.Dialogs;

namespace WeComLoad.Admin;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly IWeComAdmin _weComAdmin;
    public MainWindow()
    {
        InitializeComponent();
        _weComAdmin = new WeComAdminFunc();
    }

    /// <summary>
    /// 获取登录二维码并在扫码确认后登录
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_GetQrCode_Click(object sender, RoutedEventArgs e)
    {
        var key = await GetLoginAndShowQrCodeAsync();
        var isLogin = false;
        int count = 1;
        var delay = 2000;
        while (!isLogin)
        {
            var state = await GetLoginStatusAsync(key);
            richText_login_status.Document = new FlowDocument(new Paragraph(new Run($"{state.Msg}\r\n\r\n当前刷新次数：{count}")));
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
        richText_login_cookie.Document = new FlowDocument(new Paragraph(new Run(_weComAdmin.GetWeCombReq().CookieString)));
    }

    /// <summary>
    /// 获取企业部门列表
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_GetCorpDepts_Click(object sender, RoutedEventArgs e)
    {
        var model = await _weComAdmin.GetCorpDeptAsync();
        richText_resp.Document = new FlowDocument(new Paragraph(new Run(JsonConvert.SerializeObject(model))));
    }

    /// <summary>
    /// 获取应用列表
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_GetAgents_Click(object sender, RoutedEventArgs e)
    {
        var model = await _weComAdmin.GetCorpAppAsync();
        richText_resp.Document = new FlowDocument(new Paragraph(new Run(JsonConvert.SerializeObject(model))));
    }

    /// <summary>
    /// 获取应用详情
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_GetAgentInfo_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_weComAdmin.GetWeCombReq().CookieString))
        {
            MessageBox.Show("请先扫码登录");
            return;
        }
        var view = new GetOpenAppInfoView(_weComAdmin);
        view.ShowDialog();
    }

    /// <summary>
    /// 查看客户联系、通讯录Secret
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_GetExtContactAndUserSecret_Click(object sender, RoutedEventArgs e)
    {
        // 1、获取应用列表
        var model = await _weComAdmin.SendExtContactAndUserSecretAsync();
        richText_resp.Document = new FlowDocument(new Paragraph(new Run("完成Secret发送，请在企微客户端查看")));
    }

    /// <summary>
    /// 创建应用
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_CreateAgent_Click(object sender, RoutedEventArgs e)
    {
        var name = "自动化创建应用";
        var description = "自动化创建应用-描述";
        var logoimage = "http://p.qlogo.cn/bizmail/Qiabx6eW3f7yic7dk0QOpdUW3X0ic2QONc4f95JoGcMUoIS2Pl4fqgZlQ/0";
        // 1、创建应用
        var agent = await _weComAdmin.AddOpenApiAppAsync(new AddOpenApiAppRequest
        {
            Name = name,
            Desc = description,
            LogoImage = logoimage
        });
        richText_resp.Document = new FlowDocument(new Paragraph(new Run($"创建成功 Agent:{JsonConvert.SerializeObject(agent)}")));
    }

    /// <summary>
    /// 配置企业通讯录回调
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_ConfigContactCallback_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_weComAdmin.GetWeCombReq().CookieString))
        {
            MessageBox.Show("请先扫码登录");
            return;
        }
        var view = new ConfigContactCallbackView(_weComAdmin);
        view.ShowDialog();
    }

    /// <summary>
    /// 配置企业客户联系回调
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_ConfigExtContactCallback_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_weComAdmin.GetWeCombReq().CookieString))
        {
            MessageBox.Show("请先扫码登录");
            return;
        }
        var view = new ConfigExtContactCallbackView(_weComAdmin);
        view.ShowDialog();
    }


    #region 登录操作

    private async Task<string> GetLoginAndShowQrCodeAsync()
    {
        var (url, key) = await _weComAdmin.GetLoginQrCodeUrlAsync();
        byte[] btyarray = GetImageFromResponse(url);
        MemoryStream ms = new MemoryStream(btyarray);
        imgage_qrcode.Source = BitmapFrame.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.Default);
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
            var status = await _weComAdmin.GetQrCodeScanStatusAsync(qrCodeKey);
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

                    var corpId = await _weComAdmin.LoginAsync(qrCodeKey, status.AuthCode);
                    if (string.IsNullOrWhiteSpace(corpId))
                    {
                        statusCode = 5;
                        statusMsg = "登录失败";
                        break;
                    }

                    statusCode = 6;
                    statusMsg = $"登录成功, CorpId：{corpId}";

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

    /// <summary>
    /// 遍历CookieContainer
    /// </summary>
    /// <param name="cc"></param>
    /// <returns></returns>

    #endregion
}
