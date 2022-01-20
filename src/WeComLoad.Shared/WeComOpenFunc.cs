namespace WeComLoad.Shared;

public class WeComOpenFunc : IWeComOpen
{
    private readonly WeComAdminWebReq _weCombReq;

    public WeComOpenFunc()
    {
        _weCombReq = new WeComAdminWebReq("https://open.work.weixin.qq.com/");
    }

    public WeComAdminWebReq GetWeCombReq() => _weCombReq;

    public async Task<(string Url, string Key)> GetLoginQrCodeUrlAsync()
    {
        var keyUrl = _weCombReq.GetQueryUrl("https://work.weixin.qq.com/wework_admin/wwqrlogin/mng/get_key", new Dictionary<string, string>
            {
                { "login_type", "service_login" },
                { "r", _weCombReq.GetRandom() }
            });
        var response = await _weCombReq.HttpWebRequestGetAsync(keyUrl, false, false);
        if (!_weCombReq.IsResponseSucc(response)) return (null, null);
        var model = JsonConvert.DeserializeObject<WeComBase<WeComQrCodeKey>>(_weCombReq.GetResponseStr(response));
        if (model == null || string.IsNullOrWhiteSpace(model.Data.QrCodeKey)) throw new Exception("企微二维码Key为空");
        var key = model.Data.QrCodeKey;
        var qrCodeUrl = _weCombReq.GetQueryUrl($"https://work.weixin.qq.com/wwqrlogin/mng/qrcode/{key}", new Dictionary<string, string>
            {
                { "login_type", "service_login" }
            });
        return (qrCodeUrl, key);
    }

    public async Task<WeComQrCodeScanStatus> GetQrCodeScanStatusAsync(string qrCodeKey)
    {
        if (string.IsNullOrWhiteSpace(qrCodeKey)) throw new ArgumentNullException("企微登录二维码Key为空");
        var url = _weCombReq.GetQueryUrl("https://work.weixin.qq.com/wework_admin/wwqrlogin/check", new Dictionary<string, string>
            {
                { "qrcode_key", qrCodeKey }, { "status", "" }
            });
        var response = await _weCombReq.HttpWebRequestGetAsync(url, false, false);
        if (!_weCombReq.IsResponseSucc(response)) return null;
        var model = JsonConvert.DeserializeObject<WeComBase<WeComQrCodeScanStatus>>(_weCombReq.GetResponseStr(response));
        return model?.Data;
    }

    public async Task<bool> LoginAsync(string qrCodeKey, string authCode)
    {
        if (string.IsNullOrWhiteSpace(qrCodeKey) || string.IsNullOrWhiteSpace(authCode)) throw new ArgumentNullException("企微登录必要参数为空");

        // 开始进行预登录
        var url = _weCombReq.GetQueryUrl("https://open.work.weixin.qq.com/wwopen/login", new Dictionary<string, string>
            {
               { "code", authCode }, { "qrcode_key", qrCodeKey }, { "wwqrlogin", "1" }, { "auth_source", "SOURCE_FROM_WEWORK" }
            });
        var response = await _weCombReq.HttpWebRequestGetAsync(url, true, false);
        if (!_weCombReq.IsResponseRedi(response)) return false;
        var rediUrlData = _weCombReq.GetResponseStr(response);
        if (string.IsNullOrWhiteSpace(rediUrlData)) return false;
        url = _weCombReq.GetRedirectUrl(rediUrlData);

        // 手动重定向到url下，获取第一部分Cookie
        response = await _weCombReq.HttpWebRequestGetAsync(url, true, false);
        return _weCombReq.IsResponseSucc(response);
    }

    public async Task<WeComBase<WeComSuiteApp>> GetCustomAppsAsync()
    {
        var url = _weCombReq.GetQueryUrl("wwopen/developer/customApp/tpl/list", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "random", _weCombReq.GetRandom() }
                });
        var response = await _weCombReq.HttpWebRequestGetAsync(url);
        if (!_weCombReq.IsResponseSucc(response)) return null;
        var model = JsonConvert.DeserializeObject<WeComBase<WeComSuiteApp>>(_weCombReq.GetResponseStr(response));
        return model;
    }

    public async Task<WeComBase<WeComSuiteAppAuth>> GetCustomAppAuthsAsync(string suitId, int offset = 0, int limit = 10)
    {
        var url = _weCombReq.GetQueryUrl("wwopen/developer/customApp/tpl/app/list", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "suiteid", suitId }, { "scene", "1" }, { "offset", offset.ToString() }, { "limit", limit.ToString() }, { "random", _weCombReq.GetRandom() }
                });
        var response = await _weCombReq.HttpWebRequestGetAsync(url);
        if (!_weCombReq.IsResponseSucc(response)) return null;
        var model = JsonConvert.DeserializeObject<WeComBase<WeComSuiteAppAuth>>(_weCombReq.GetResponseStr(response));
        return model;
    }
}
