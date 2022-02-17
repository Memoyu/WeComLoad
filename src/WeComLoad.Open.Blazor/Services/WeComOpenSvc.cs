namespace WeComLoad.Open.Blazor.Services;

public class WeComOpenSvc : IWeComOpenSvc
{
    private readonly IWeComOpen _weComOpen;
    private readonly NavigationManager _navigationManager;
    private readonly MessageService _messageService;

    public WeComOpenSvc(IWeComOpen weComOpen, NavigationManager navigationManager, MessageService messageService)
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

    public Task<WeComSuiteAppAuthDetail> GetCustomAppAuthDetailAsync(string suitId)
    {
        throw new NotImplementedException();
    }

    public Task<WeComAuthAppResult> AuthCorpAppAsync(AuthCorpAppRequest req)
    {
        throw new NotImplementedException();
    }

    public Task<SubmitAuditCorpAppResult> SubmitAuditCorpAppAsync(SubmitAuditCorpAppRequest req)
    {
        throw new NotImplementedException();
    }

    public Task<OnlineCorpAppResult> OnlineCorpAppAsync(OnlineCorpAppRequest req)
    {
        throw new NotImplementedException();
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
