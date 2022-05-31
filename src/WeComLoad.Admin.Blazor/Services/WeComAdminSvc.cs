﻿namespace WeComLoad.Admin.Blazor.Services;

public class WeComAdminSvc : IWeComAdminSvc
{

    private readonly IWeComAdmin _weComAdmin;
    private readonly NavigationManager _navigationManager;
    private readonly MessageService _messageService;

    public WeComAdminSvc(
        IWeComAdmin weComAdmin,
        NavigationManager navigationManager,
        MessageService messageService)
    {
        _weComAdmin = weComAdmin;
        _navigationManager = navigationManager;
        _messageService = messageService;
    }

    #region 登录

    public async Task<(string Url, string Key)> GetLoginQrCodeUrlAsync()
    {
        return await _weComAdmin.GetLoginQrCodeUrlAsync();
    }

    public Task<WeComQrCodeScanStatus> GetQrCodeScanStatusAsync(string qrCodeKey)
    {
        return _weComAdmin.GetQrCodeScanStatusAsync(qrCodeKey);
    }

    public async Task<WeComWxLoginCorps> GetWxLoginCorpsAsync(string qrCodeKey, string authCode)
    {
        return await _weComAdmin.GetWxLoginCorpsAsync(qrCodeKey, authCode);
    }

    public Task<bool> LoginAsync(string qrCodeKey, string authCode)
    {
        return _weComAdmin.LoginAsync(qrCodeKey, authCode);
    }

    public async Task<int> WxLoginAsync(string tlKey, string corpId)
    {
        var login = await _weComAdmin.WxLoginAsync(tlKey, corpId);
        // TODO：进行 微信扫码登陆后参数校验，因为可能需要输入验证码

        var after = await _weComAdmin.WxLoginAfterAsync();
        return after ? 1 : 0;
    }

    #endregion

    public async Task<WeComCorpApp> GetCorpAppAsync()
    {
        var result = await _weComAdmin.GetCorpAppAsync();
        var parse = IsSuccessT<WeComCorpApp>(result);
        return parse.Data;
    }

    public async Task<WeComOpenapiApp> GetCorpOpenAppAsync(string appOpenId)
    {
        var result = await _weComAdmin.GetCorpOpenAppAsync(appOpenId);
        var parse = IsSuccessT<WeComOpenapiApp>(result);
        return parse.Data;
    }

    public async Task<WeComCorpDept> GetCorpDeptAsync()
    {
        var result = await _weComAdmin.GetCorpDeptAsync();
        var parse = IsSuccessT<WeComCorpDept>(result);
        return parse.Data;
    }

    public async Task<bool> SendSecretAsync(string appId)
    {
        var result = await _weComAdmin.SendSecretAsync(appId);
        var parse = IsSuccessT<string>(result);
        return !string.IsNullOrWhiteSpace(parse.Data);
    }

    public async Task<bool> SendExtContactAndUserSecretAsync()
    {
        // 1、获取应用列表
        var data = await GetCorpAppAsync();
        // 获取客户联系、通讯录appid (app_open_id为固定值，2000002：通讯录同步助手，2000003：外部联系人)
        var appids = data.OpenapiApps.Where(m => m.AppOpenId == 2000002 || m.AppOpenId == 2000003).Select(a => a.AppId).ToList();
        foreach (var id in appids)
        {
            return await SendSecretAsync(id);
        }

        return true;
    }

    public async Task<WeComOpenapiApp> AddOpenApiAppAsync(List<string> pids)
    {
        var depts = await GetCorpDeptAsync();
        var dept = depts.Partys.List.Where(p => p.OpenapiPartyid.Equals("1")).FirstOrDefault();
        if (dept == null) throw new Exception("获取根部门异常");
        var visible_pid = dept.Partyid;
        var result = await _weComAdmin.AddOpenApiAppAsync(pids);
        var parse = IsSuccessT<WeComOpenapiApp>(result);
        return parse.Data;
    }

    public async Task<(bool flag, string msg)> AddChatMenuAsync(List<AddChatMenuRequest> menus, WeComOpenapiApp agent)
    {
        var flag = true;
        var msg = string.Empty;
        foreach (var item in menus)
        {
            var result = await _weComAdmin.AddChatMenuAsync(item, agent);
            var parse = IsSuccessT<object>(result);
            if (parse.Data == null)
            {
                msg += $"{item.MenuName} 添加失败";
                flag = false;
            }
        }
        return (flag, msg);
    }

    public async Task<bool> SaveOpenApiAppAsync(List<(string Key, string Value)> req)
    {
        var result = await _weComAdmin.SaveOpenApiAppAsync(req);
        var parse = IsSuccessT<object>(result);
        return parse.Data != null;
    }

    public async Task<WeComCreateTwoFactorAuthOp> CreateTwoFactorAuthOpAsync(string appId)
    {
        var result = await _weComAdmin.CreateTwoFactorAuthOpAsync(appId);
        var parse = IsSuccessT<WeComCreateTwoFactorAuthOp>(result);
        return parse.Data;
    }

    public async Task<WeComQueryTwoFactorAuthOp> QueryTwoFactorAuthOpAsync(string key)
    {
        var result = await _weComAdmin.QueryTwoFactorAuthOpAsync(key);
        var parse = IsSuccessT<WeComQueryTwoFactorAuthOp>(result);
        return parse.Data;
    }

    public async Task<WeComConfigCallback> ConfigContactCallbackAsync(ConfigCallbackRequest req)
    {
        var result = await _weComAdmin.ConfigContactCallbackAsync(req);
        var parse = IsSuccessT<WeComConfigCallback>(result);
        return parse.Data;
    }

    public async Task<WeComConfigCallback> ConfigExtContactCallbackAsync(ConfigCallbackRequest req)
    {
        var result = await _weComAdmin.ConfigExtContactCallbackAsync(req);
        var parse = IsSuccessT<WeComConfigCallback>(result);
        return parse.Data;
    }

    public async Task<WeComGetApiAccessibleApps> GetApiAccessibleAppsAsync(string businessId)
    {
        var result = await _weComAdmin.GetApiAccessibleAppsAsync(businessId);
        var parse = IsSuccessT<WeComGetApiAccessibleApps>(result);
        return parse.Data;
    }

    public async Task<WeComSaveOpenApiApp> SetApiAccessibleAppsAsync(string businessId, List<string> accessibleApps)
    {
        var currentAcc = await GetApiAccessibleAppsAsync(businessId);
        if (currentAcc == null) return null;
        var accs = currentAcc.Auths?.AppIds ?? new List<string>();
        accessibleApps.AddRange(accs);
        accessibleApps = accessibleApps.Distinct().ToList();

        var result = await _weComAdmin.SetApiAccessibleAppsAsync(businessId, accessibleApps);
        var parse = IsSuccessT<WeComSaveOpenApiApp>(result);
        return parse.Data;
    }
    public async Task<(string Name, byte[] File)> GetDomainVerifyFileAsync()
    {
        return await _weComAdmin.GetDomainVerifyFileAsync();
    }

    public async Task<bool> CheckCustomAppURLAsync(string appid, string domian)
    {
        var result = await _weComAdmin.CheckCustomAppURLAsync(appid, domian);
        var parse = IsSuccessT<WeComCheckCustomAppURLDomain>(result);
        return parse.Data.Status.Equals("PASS");
    }

    public async Task<bool> CheckXcxDomainStatusAsync(string domian)
    {
        var result = await _weComAdmin.CheckXcxDomainStatusAsync(domian);
        var parse = IsSuccessT<WeComCheckXcxDomain>(result);
        var domain = parse.Data.Result.FirstOrDefault();
        if (domain == null) return false;
        return domain.Status;
    }

    public async Task<bool> SetCustomizedAppPrivilege(string appId)
    {
        var result = await _weComAdmin.SetCustomizedAppPrivilege(appId);
        var parse = IsSuccessT<object>(result);
        return parse.Data != null;
    }

    private (bool flag, T Data) IsSuccessT<T>(string result) where T : class
    {
        if (!IsSuccess(result)) return (false, default(T));
        var res = JsonConvert.DeserializeObject<WeComBase<T>>(result);
        return (true, res?.Data);
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