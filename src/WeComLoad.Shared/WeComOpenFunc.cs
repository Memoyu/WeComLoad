using static WeComLoad.Shared.Model.WeComSuiteAppAuth;

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
        var model = _weCombReq.GetResponseT<WeComBase<WeComQrCodeKey>>(response);
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
        /**********************************服务商版本的已经改版，登录只需要调用login即可获取到所有所需的cookie，实现登录**************************************
         * if (!_weCombReq.IsResponseRedi(response)) return false;
        var rediUrlData = _weCombReq.GetResponseStr(response);
        if (string.IsNullOrWhiteSpace(rediUrlData)) return false;
        url = _weCombReq.GetRedirectUrl(rediUrlData);

        // 手动重定向到url下，获取第一部分Cookie
        response = await _weCombReq.HttpWebRequestGetAsync(url, true, false);*/
        return _weCombReq.IsResponseRedi(response);
    }

    public async Task<(string Name, byte[] File)> GetDomainVerifyFileAsync(string corpAppId, string suiteId)
    {
        // 获取可新域名校验文件名，domain_belong_to=0（代开发服务商）；domain_belong_to=1（企业客户）
        var url = _weCombReq.GetQueryUrl("wwopen/developer/app/getDomainOwnershipVerifyInfo", new Dictionary<string, string>
            {
                { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" },  { "random", _weCombReq.GetRandom() }, { "domain_belong_to", "0" }, { "corpappid", corpAppId }, { "suiteid", suiteId }
            });
        var response = await _weCombReq.HttpWebRequestGetAsync(url);
        if (!_weCombReq.IsResponseSucc(response)) return (null, null);
        var model = JsonConvert.DeserializeObject<WeComDomainVerifyFileName>(_weCombReq.GetResponseStr(response));

        // 地址写死： https://open.work.weixin.qq.com/wwopen/developer/app/getDomainOwnershipVerifyInfo?action=download&corpappid=5629500845550908&suiteid=1009718&domain_belong_to=0
        var file = await _weCombReq.HttpWebRequestDownloadsync($"wwopen/developer/app/getDomainOwnershipVerifyInfo?action=download&corpappid={corpAppId}&suiteid={suiteId}&domain_belong_to=0");
        return (model.FileName, file);
    }

    public async Task<string> GetCustomAppTplsAsync()
    {
        var url = _weCombReq.GetQueryUrl("wwopen/developer/customApp/tpl/list", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "random", _weCombReq.GetRandom() }
                });
        var response = await _weCombReq.HttpWebRequestGetAsync(url);
        return GetResponseStr(response);
    }

    public async Task<string> GetCustomAppAuthsAsync(string suitId, int offset = 0, int limit = 10)
    {
        var url = _weCombReq.GetQueryUrl("wwopen/developer/customApp/tpl/app/list", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "suiteid", suitId }, { "scene", "1" }, { "offset", offset.ToString() }, { "limit", limit.ToString() }, { "random", _weCombReq.GetRandom() }
                });
        var response = await _weCombReq.HttpWebRequestGetAsync(url);      
        return GetResponseStr(response);
    }

    public async Task<string> GetCustomAppAuthDetailAsync(string suitId)
    {
        var url = _weCombReq.GetQueryUrl("wwopen/developer/customApp/tpl/detail", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "random", _weCombReq.GetRandom() }, { "suiteid", suitId }, { "oper_table", "eSuiteOpTableNormal" }
                });
        var response = await _weCombReq.HttpWebRequestGetAsync(url);
        return GetResponseStr(response);
    }

    public async Task<string> AuthCorpAppAsync(AuthCorpAppRequest req)
    {
        var url = _weCombReq.GetQueryUrl("wwopen/developer/customApp/tpl/corpApp", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "random", _weCombReq.GetRandom() }
                });
        var response = await _weCombReq.HttpWebRequestPostJsonAsync(url, JsonConvert.SerializeObject(req));
        return GetResponseStr(response);
    }

    public async Task<string> SubmitAuditCorpAppAsync(SubmitAuditCorpAppRequest req)
    {
        var url = _weCombReq.GetQueryUrl("wwopen/developer/order/add", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "random", _weCombReq.GetRandom() }
                });
        var response = await _weCombReq.HttpWebRequestPostJsonAsync(url, JsonConvert.SerializeObject(req));
        return GetResponseStr(response);
    }

    public async Task<string> OnlineCorpAppAsync(OnlineCorpAppRequest req)
    {
        var url = _weCombReq.GetQueryUrl("wwopen/developer/order/set", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "random", _weCombReq.GetRandom() }
                });
        var response = await _weCombReq.HttpWebRequestPostJsonAsync(url, JsonConvert.SerializeObject(req));
        return GetResponseStr(response);
    }


    private string GetResponseStr(HttpWebResponse response)
    {
        if (!_weCombReq.IsResponseSucc(response)) return string.Empty;
        Stream responseStream = response.GetResponseStream();
        StreamReader sr = new StreamReader(responseStream);
        var responseStr = sr.ReadToEnd();
        response.Close();
        responseStream.Close();
        return responseStr;
    }
}
