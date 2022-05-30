namespace WeComLoad.Admin.Blazor.Services;

public class WeComAdminSvc : IWeComAdminSvc
{
    private readonly IWeComAdmin _weComAdmin;
    private readonly NavigationManager _navigationManager;
    private readonly MessageService _messageService;
    public WeComAdminSvc(
        IWeComAdmin weComOpen,
        NavigationManager navigationManager,
        MessageService messageService)
    {
        _weComAdmin = weComOpen;
        _navigationManager = navigationManager;
        _messageService = messageService;
    }

    public async Task<(string Url, string Key)> GetLoginQrCodeUrlAsync()
    {
        return await _weComAdmin.GetLoginQrCodeUrlAsync();
    }

    public Task<WeComQrCodeScanStatus> GetQrCodeScanStatusAsync(string qrCodeKey)
    {
        return _weComAdmin.GetQrCodeScanStatusAsync(qrCodeKey);
    }

    public Task<bool> LoginAsync(string qrCodeKey, string authCode)
    {
        return _weComAdmin.LoginAsync(qrCodeKey, authCode);
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
