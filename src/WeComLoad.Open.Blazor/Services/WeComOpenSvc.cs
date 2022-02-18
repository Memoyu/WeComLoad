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


    public async Task<(string Url, string Key)> GetLoginQrCodeUrlAsync()
    {
        return await _weComOpen.GetLoginQrCodeUrlAsync();
    }

    public Task<WeComQrCodeScanStatus> GetQrCodeScanStatusAsync(string qrCodeKey)
    {
        return _weComOpen.GetQrCodeScanStatusAsync(qrCodeKey);
    }

    public Task<bool> LoginAsync(string qrCodeKey, string authCode)
    {
        return _weComOpen.LoginAsync(qrCodeKey, authCode);
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
            await _messageService.Error("授权开发自建应用异常！");
            return false;
        }

        var appId = restAuth.Result?.corpapp?.app_id;
        var suiteId = req.suiteid;

        // 下载可信域名校验文件
        var resFile = await UploadDomainVerify(suiteId, appId, verifyBucket);
        if (!resFile)
        {
            await _messageService.Error("上传应用可信域名校验文件异常！");
            return false;
        }

        // 提交审核
        var resSubmitAudit = await SubmitAuditCorpAppAsync(new SubmitAuditCorpAppRequest(appId, suiteId));
        if (resSubmitAudit.Flag)
        {
            await _messageService.Error("审核应用失败！");
            return false;
        }

        var auditOrderId = resSubmitAudit.Result?.auditorder?.auditorderid;

        // 上线应用
        var resOnline = await OnlineCorpAppAsync(new OnlineCorpAppRequest(auditOrderId));
        if (resOnline.Flag)
        {
            await _messageService.Error("上线应用异常！");
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
            int upFileCount = 0;

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

        UploadBegin:
            /* 上传可信域名校验文件到域名的根目录下操作
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
        catch (Exception ex)
        {
            return false;
        }
    }

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

    private bool NeedLogin(int? errCode)
    {
        if (errCode == null) return false;
        // "{\"result\":{\"errCode\":-3,\"message\":\"outsession\",\"etype\":\"otherLogin\"}}"
        if (errCode == -3)
            return true;
        return false;
    }
}
