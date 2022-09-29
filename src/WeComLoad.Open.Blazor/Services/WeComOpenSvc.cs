using System.Text.RegularExpressions;
using System.Web;
using WeComLoad.Shared;
using WeComLoad.Shared.Model.Admin;

namespace WeComLoad.Open.Blazor.Services;

public class WeComOpenSvc : IWeComOpenSvc
{
    private readonly IWeComOpen _weComOpen;
    private readonly NavigationManager _navigationManager;
    private readonly MessageService _messageService;
    public WeComOpenSvc(
        IWeComOpen weComOpen,
        NavigationManager navigationManager,
        MessageService messageService)
    {
        _weComOpen = weComOpen;
        _navigationManager = navigationManager;
        _messageService = messageService;
    }

    public void InitWeComOpen() => _weComOpen.ClearReqCookie();

    public async Task<(string Url, string Key)> GetLoginQrCodeUrlAsync()
    {

        var result = await _weComOpen.GetLoginQrCodeUrlAsync();
        var parse = IsSuccessT<WeComQrCodeKey>(result);
        if (parse.Data == null || string.IsNullOrWhiteSpace(parse.Data.QrCodeKey)) throw new Exception("企微二维码Key为空");
        var key = parse.Data.QrCodeKey;
        var qrCodeUrl = $"https://work.weixin.qq.com/wework_admin/wwqrlogin/mng/qrcode?qrcode_key={key}&login_type=service_login";

        return (qrCodeUrl, key);
    }

    public async Task<WeComQrCodeScanStatus> GetQrCodeScanStatusAsync(string qrCodeKey)
    {
        var result = await _weComOpen.GetQrCodeScanStatusAsync(qrCodeKey);
        var parse = IsSuccessT<WeComQrCodeScanStatus>(result);
        return parse.Data;
    }

    public async Task<(int flag, string msg, CaptchParam param)> LoginAsync(string qrCodeKey, string authCode, string authSource)
    {
        CaptchParam param = new CaptchParam();
        var res = await _weComOpen.LoginAsync(qrCodeKey, authCode, authSource);
        if (res.flag == -1) return (res.flag, res.msg, param);
        if (res.flag == 0) // 需要输入验证码
        {
            var urlSplit = res.url.Split("?");
            var tlKey = HttpUtility.ParseQueryString(urlSplit[1])["tl_key"];
            var result = await _weComOpen.LoginCaptchaAsync(res.url);
            if (string.IsNullOrWhiteSpace(result)) return (-1, "跳转验证码页面失败", param);
            string pattern = "(?<=mobile\\\":\\\").*?(?=\\\",)";
            var match = Regex.Matches(result, pattern);
            param.TlKey = tlKey;
            param.Mobile = match.FirstOrDefault()?.Value;
            return (0, string.Empty, param);
        }
        param.AuthCode = authCode;
        param.AuthSource = authSource;
        return (1, string.Empty, param);
    }

    public async Task<bool> LoginAfterAsync(string authCode, string authSource)
    {
        return await _weComOpen.LoginAfterAsync(authCode, authSource);
    }

    public async Task<(bool flag, string msg)> LoginSendCaptchaAsync(string tlKey)
    {
        var res = await _weComOpen.LoginSendCaptchaAsync(tlKey);
        Console.WriteLine("验证码发送：" + res);
        var send = JsonConvert.DeserializeObject<WeComErr>(res);
        if (send is not null && send.result?.errCode != null)
        {
            return (false, send.result?.humanMessage);
        }
        return (true, string.Empty);
    }

    public async Task<(bool flag, string msg)> LoginConfirmCaptchaAsync(CaptchParam param, string captcha)
    {
        var res = await _weComOpen.LoginConfirmCaptchaAsync(param.TlKey, captcha);
        var confirm = JsonConvert.DeserializeObject<WeComErr>(res);
        if (confirm is not null && confirm.result?.errCode != null)
        {
            return (false , confirm.result?.message);
        }

        await _weComOpen.LoginAfterAsync(param.AuthCode, param.AuthSource);
        return (true,  string.Empty);
    }

    public async Task<(string Name, byte[] File)> GetDomainVerifyFileAsync(string corpAppId, string suiteId)
    {
        return await _weComOpen.GetDomainVerifyFileAsync(corpAppId, suiteId);
    }

    public async Task<WeComSuiteApp> GetCustomAppTplsAsync()
    {
        var result = await _weComOpen.GetCustomAppTplsAsync();
        var parse = IsSuccessT<WeComSuiteApp>(result);
        return parse.Data;
    }

    public async Task<WeComSuiteAppAuth> GetCustomAppAuthsAsync(string suitId, int offset = 0, int limit = 10)
    {
        var result = await _weComOpen.GetCustomAppAuthsAsync(suitId, offset, limit);
        var parse = IsSuccessT<WeComSuiteAppAuth>(result);
        return parse.Data;
    }

    public async Task<bool> AuthCustAppAndOnlineAsync(AuthCorpAppRequest req, string verifyBucket)
    {
        // 开发应用
        var restAuth = await AuthCorpAppAsync(req);
        if (restAuth.Flag)
        {
            _ = _messageService.Error("授权开发自建应用异常！");
            return false;
        }

        var appId = restAuth.Result?.corpapp?.app_id;
        var suiteId = req.suiteid;

        // 下载可信域名校验文件
        var resFile = await UploadDomainVerify(suiteId, appId, verifyBucket);
        if (!resFile)
        {
            _ = _messageService.Error("上传应用可信域名校验文件异常！");
            return false;
        }

        // 提交审核
        var resSubmitAudit = await SubmitAuditCorpAppAsync(new SubmitAuditCorpAppRequest(appId, suiteId));
        if (resSubmitAudit.Flag)
        {
            _ = _messageService.Error("审核应用失败！");
            return false;
        }

        var auditOrderId = resSubmitAudit.Result?.auditorder?.auditorderid;

        // 上线应用
        var resOnline = await OnlineCorpAppAsync(new OnlineCorpAppRequest(auditOrderId));
        if (resOnline.Flag)
        {
            _ = _messageService.Error("上线应用异常！");
            return false;
        }



        return true;
    }

    public Task<WeComSuiteAppAuthDetail> GetCustomAppAuthDetailAsync(string suitId)
    {
        throw new NotImplementedException();
    }

    public async Task<(bool Flag, WeComAuthAppResult Result)> AuthCorpAppAsync(AuthCorpAppRequest req)
    {
        var result = await _weComOpen.AuthCorpAppAsync(req);
        var parse = IsSuccessT<WeComAuthAppResult>(result);
        return (string.IsNullOrWhiteSpace(parse.Data?.corpapp?.app_id), parse.Data);
    }

    public async Task<(bool Flag, SubmitAuditCorpAppResult Result)> SubmitAuditCorpAppAsync(SubmitAuditCorpAppRequest req)
    {
        var result = await _weComOpen.SubmitAuditCorpAppAsync(req);
        var parse = IsSuccessT<SubmitAuditCorpAppResult>(result);
        return (string.IsNullOrWhiteSpace(parse.Data?.auditorder?.auditorderid), parse.Data);
    }

    public async Task<(bool Flag, OnlineCorpAppResult Result)> OnlineCorpAppAsync(OnlineCorpAppRequest req)
    {
        var result = await _weComOpen.OnlineCorpAppAsync(req);
        var parse = IsSuccessT<OnlineCorpAppResult>(result);
        return (string.IsNullOrWhiteSpace(parse.Data?.auditorder?.auditorderid), parse.Data);
    }

    private async Task<bool> UploadDomainVerify(string suiteId, string appId, string verifyBucket)
    {
        try
        {
            int downFileCount = 0;
        // int upFileCount = 0;

        DownloadBegin:
            var (fileName, file) = await _weComOpen.GetDomainVerifyFileAsync(suiteId, appId);
            if (file.Length <= 0)
            {
                if (downFileCount < 3)
                {
                    downFileCount++;
                    await Task.Delay(1000);
                    goto DownloadBegin;
                }
                else
                {
                    return false;
                }
            }

            /*UploadBegin:
                // 上传可信域名校验文件到域名的根目录下操作
                var (uploadFlag, uploadMsg) = await _fileClientPro.UploadToRootPathAsync(file, verifyBucket, fileName, OSSType.Aliyun);
                if (!uploadFlag)
                {
                    if (upFileCount < 3)
                    {
                        upFileCount++;
                        await Task.Delay(1000);
                        goto UploadBegin;
                    }
                    else
                    {
                        return false;
                    }
                }*/

            return true;
        }
        catch
        {
            return false;
        }
    }


    #region 快速登陆

    public async Task<GetQuickLoginParam> GetQuickLoginParameAsync()
    {
        var result = await _weComOpen.GetQuickLoginParameAsync();
        var parse = IsSuccessT<GetQuickLoginParam>(result);
        return parse.Data;
    }

    public async Task<GetCorpBindDeveloperInfo> GetCorpBindDeveloperInfoAsync(string webKey)
    {
        var result = await _weComOpen.GetCorpBindDeveloperInfoAsync(webKey);
        var parse = IsSuccessT<GetCorpBindDeveloperInfo>(result);
        return parse.Data;
    }


    public async Task<QuickLoginCorpInfo> GetQuickLoginCorpInfoAsync(string webKey)
    {
        var result = await _weComOpen.GetQuickLoginCorpInfoAsync(webKey);
        var parse = IsSuccessT<QuickLoginCorpInfo>(result);
        return parse.Data;
    }

    public async Task<ConfirmQuickLoginAuthInfo> ConfirmQuickLoginAsync(string webKey)
    {
        var result = await _weComOpen.ConfirmQuickLoginAsync(webKey);
        var data = JsonConvert.DeserializeObject<WeComBase<ConfirmQuickLoginAuthInfo>>(result)?.Data;
        if (data == null)
        {
            data = new ConfirmQuickLoginAuthInfo();
            var err = JsonConvert.DeserializeObject<WeComOpenErr>(result);
            data.msg = err?.result?.humanMessage;
        }

        return data;
    }

    #endregion

    private bool IsSuccess(string result)
    {
        if (string.IsNullOrWhiteSpace(result))
        {
            _messageService.Error("获取数据失败，请稍后重试！");
            return false;
        }

        var err = JsonConvert.DeserializeObject<WeComErr>(result);
        if (err is not null && NeedLogin(err?.result?.errCode))
        {
            GoToLogin();
            return false;
        }
        return true;
    }

    private (bool flag, T Data) IsSuccessT<T>(string result) where T : class
    {
        if (!IsSuccess(result)) return (false, default(T));
        var res = JsonConvert.DeserializeObject<WeComBase<T>>(result);
        return (true, res?.Data);
    }

    private void GoToLogin()
    {
        _navigationManager.NavigateTo("/login");
    }

    private bool NeedLogin(long? errCode)
    {
        if (errCode == null) return false;
        // "{\"result\":{\"errCode\":-3,\"message\":\"outsession\",\"etype\":\"otherLogin\"}}"
        if (errCode == -3)
            return true;
        return false;
    }
}
