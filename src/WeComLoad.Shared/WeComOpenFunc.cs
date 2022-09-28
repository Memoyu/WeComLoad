using static WeComLoad.Shared.Model.WeComSuiteAppAuth;

namespace WeComLoad.Shared;

public class WeComOpenFunc : IWeComOpen
{
    private readonly WeComAdminWebReq _weComReq;

    public WeComOpenFunc()
    {
        _weComReq = new WeComAdminWebReq("https://open.work.weixin.qq.com/");
    }

    public void ClearReqCookie() => _weComReq.ClearCookie();

    public WeComAdminWebReq GetWeComReq() => _weComReq;

    public async Task<string> GetLoginQrCodeUrlAsync()
    {
        var keyUrl = _weComReq.GetQueryUrl("https://work.weixin.qq.com/wework_admin/wwqrlogin/mng/get_key", new Dictionary<string, string>
            {
                { "login_type", "service_login" },
                { "r", _weComReq.GetRandom() }
            });
        var response = await _weComReq.HttpWebRequestGetAsync(keyUrl, false, false);
        return GetResponseStr(response);
    }

    public async Task<string> GetQrCodeScanStatusAsync(string qrCodeKey)
    {
        if (string.IsNullOrWhiteSpace(qrCodeKey)) throw new ArgumentNullException("企微登录二维码Key为空");
        var url = _weComReq.GetQueryUrl("https://work.weixin.qq.com/wework_admin/wwqrlogin/check", new Dictionary<string, string>
            {
                { "qrcode_key", qrCodeKey }, { "status", "" }
            });
        var response = await _weComReq.HttpWebRequestGetAsync(url, false, false);
        return GetResponseStr(response);
    }

    public async Task<(int flag, string msg, string url)> LoginAsync(string qrCodeKey, string authCode, string authSource)
    {
        Console.WriteLine(_weComReq.CookieString);

        if (string.IsNullOrWhiteSpace(qrCodeKey) || string.IsNullOrWhiteSpace(authCode)) throw new ArgumentNullException("企微登录必要参数为空");

        // 开始进行预登录
        var url = _weComReq.GetQueryUrl("wwopen/login", new Dictionary<string, string>
            {
               { "code", authCode }, { "qrcode_key", qrCodeKey }, { "wwqrlogin", "1" }, { "auth_source", authSource }
            });
        var response = await _weComReq.HttpWebRequestGetAsync(url, true);
        if (!_weComReq.IsResponseRedi(response)) return (-1, "跳转登录失败", string.Empty);
        Console.WriteLine($"cookie：{_weComReq.CookieString}，" +
            $"status:{response.StatusCode}, " +
            $"header keys:{JsonConvert.SerializeObject(response.Headers.AllKeys)}, " +
            $"header values:{JsonConvert.SerializeObject(response.Headers.AllKeys.Select(k => response.Headers.GetValues(k)).ToList())}");
        // 说明需要输入验证码
        url = response.Headers.GetValues("Location")?.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(url))
        {
            return (0, "需要验证码校验", url);
        }
        //Console.WriteLine($"cookie：{_weComReq.CookieString}，" + 
        //    $"status:{response.StatusCode}, " +
        //    $"header keys:{JsonConvert.SerializeObject(response.Headers.AllKeys)}, " +
        //    $"header values:{JsonConvert.SerializeObject(response.Headers.AllKeys.Select(k => response.Headers.GetValues(k)).ToList())}");
        return (1, string.Empty, string.Empty);
    }

    public async Task<bool> LoginAfterAsync(string authCode, string authSource)
    {
        if (string.IsNullOrWhiteSpace(authCode)) throw new ArgumentNullException("企微登录必要参数为空");

        // 开始进行预登录 ?code=s9ON_xe61CJfOJDyexe5knz421Vrks8qF0Q-jSoXZQ8&auth_source=SOURCE_FROM_PC_APP
        var url = _weComReq.GetQueryUrl("wwopen/login", new Dictionary<string, string>
            {
               { "code", authCode }, { "auth_source", authSource }
            });
        var response = await _weComReq.HttpWebRequestGetAsync(url, true);
        return _weComReq.IsResponseRedi(response);
    }

    public async Task<string> LoginCaptchaAsync(string url)
    {
        // https://work.weixin.qq.com/wework_admin/mobile_confirm/captcha_page?
        // tl_key=b5182f15d0f1dd44e4e2865dbfc384a0
        // &redirect_url=https%3A%2F%2Fwork.weixin.qq.com%2Fwework_admin%2Flogin%2Fchoose_corp%3Ftl_key%3Db5182f15d0f1dd44e4e2865dbfc384a0
        // &from=spamcheck
        // get
        /*var url = _weComReq.GetQueryUrl("wework_admin/mobile_confirm/captcha_page", new Dictionary<string, string>
                {
                    { "tl_key", tlKey },
                    { "redirect_url", $"https%3A%2F%2Fwork.weixin.qq.com%2Fwework_admin%2Flogin%2Fchoose_corp%3Ftl_key%{tlKey}" },
                    { "from", "spamcheck" }
                });*/
        var response = await _weComReq.HttpWebRequestGetAsync(url);
        return GetResponseStr(response);
    }

    public async Task<string> LoginSendCaptchaAsync(string tlKey)
    {
        // https://work.weixin.qq.com/wework_admin/mobile_confirm/send_captcha?
        // lang=zh_CN
        // &ajax=1
        // &f=json&
        // random=945334
        // post :{tl_key: "b5182f15d0f1dd44e4e2865dbfc384a0"}

        var dic = new List<(string, string)>()
                {
                    ("tl_key", tlKey)
                };
        var url = _weComReq.GetQueryUrl("https://work.weixin.qq.com/wework_admin/mobile_confirm/send_captcha", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "random", _weComReq.GetRandom() }
                });
        var response = await _weComReq.HttpWebRequestPostAsync(url, dic, true, false);
        return GetResponseStr(response);
    }

    public async Task<string> LoginConfirmCaptchaAsync(string tlKey, string captcha)
    {
        // https://work.weixin.qq.com/wework_admin/mobile_confirm/confirm_captcha?lang=zh_CN&ajax=1&f=json&random=188841
        // post :{captcha: "222222", tl_key: "b5182f15d0f1dd44e4e2865dbfc384a0"}
        var content = new
        {
            tl_key = tlKey,
            captcha = captcha
        };
        var url = _weComReq.GetQueryUrl("https://work.weixin.qq.com/wework_admin/mobile_confirm/confirm_captcha", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "random", _weComReq.GetRandom() }
                });
        var response = await _weComReq.HttpWebRequestPostJsonAsync(url, JsonConvert.SerializeObject(content), false);
        return GetResponseStr(response);
    }

    public async Task<(string Name, byte[] File)> GetDomainVerifyFileAsync(string corpAppId, string suiteId)
    {
        // 获取可新域名校验文件名，domain_belong_to=0（代开发服务商）；domain_belong_to=1（企业客户）
        var url = _weComReq.GetQueryUrl("wwopen/developer/app/getDomainOwnershipVerifyInfo", new Dictionary<string, string>
            {
                { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" },  { "random", _weComReq.GetRandom() }, { "domain_belong_to", "0" }, { "corpappid", corpAppId }, { "suiteid", suiteId }
            });
        var response = await _weComReq.HttpWebRequestGetAsync(url);
        if (!_weComReq.IsResponseSucc(response)) return (null, null);
        var model = JsonConvert.DeserializeObject<WeComDomainVerifyFileName>(GetResponseStr(response));

        // 地址写死： https://open.work.weixin.qq.com/wwopen/developer/app/getDomainOwnershipVerifyInfo?action=download&corpappid=5629500845550908&suiteid=1009718&domain_belong_to=0
        var file = await _weComReq.HttpWebRequestDownloadsync($"wwopen/developer/app/getDomainOwnershipVerifyInfo?action=download&corpappid={corpAppId}&suiteid={suiteId}&domain_belong_to=0");
        return (model.FileName, file);
    }

    public async Task<string> GetCustomAppTplsAsync()
    {
        var url = _weComReq.GetQueryUrl("wwopen/developer/customApp/tpl/list", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "random", _weComReq.GetRandom() }
                });
        var response = await _weComReq.HttpWebRequestGetAsync(url);
        return GetResponseStr(response);
    }

    public async Task<string> GetCustomAppAuthsAsync(string suitId, int offset = 0, int limit = 10)
    {
        var url = _weComReq.GetQueryUrl("wwopen/developer/customApp/tpl/app/list", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "suiteid", suitId }, { "scene", "1" }, { "offset", offset.ToString() }, { "limit", limit.ToString() }, { "random", _weComReq.GetRandom() }
                });
        var response = await _weComReq.HttpWebRequestGetAsync(url);
        return GetResponseStr(response);
    }

    public async Task<string> GetCustomAppAuthDetailAsync(string suitId)
    {
        var url = _weComReq.GetQueryUrl("wwopen/developer/customApp/tpl/detail", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "random", _weComReq.GetRandom() }, { "suiteid", suitId }, { "oper_table", "eSuiteOpTableNormal" }
                });
        var response = await _weComReq.HttpWebRequestGetAsync(url);
        return GetResponseStr(response);
    }

    public async Task<string> AuthCorpAppAsync(AuthCorpAppRequest req)
    {
        var url = _weComReq.GetQueryUrl("wwopen/developer/customApp/tpl/corpApp", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "random", _weComReq.GetRandom() }
                });
        var response = await _weComReq.HttpWebRequestPostJsonAsync(url, JsonConvert.SerializeObject(req));
        return GetResponseStr(response);
    }

    public async Task<string> SubmitAuditCorpAppAsync(SubmitAuditCorpAppRequest req)
    {
        var url = _weComReq.GetQueryUrl("wwopen/developer/order/add", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "random", _weComReq.GetRandom() }
                });
        var response = await _weComReq.HttpWebRequestPostJsonAsync(url, JsonConvert.SerializeObject(req));
        return GetResponseStr(response);
    }

    public async Task<string> OnlineCorpAppAsync(OnlineCorpAppRequest req)
    {
        var url = _weComReq.GetQueryUrl("wwopen/developer/order/set", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "random", _weComReq.GetRandom() }
                });
        var response = await _weComReq.HttpWebRequestPostJsonAsync(url, JsonConvert.SerializeObject(req));
        return GetResponseStr(response);
    }

    #region 快速登录

    public async Task<string> GetQuickLoginParameAsync()
    {
        // 参数： code_type=13&redirect_uri=https://open.work.weixin.qq.com&version=0.2.0
        var url = _weComReq.GetQueryUrl("wwopen/wwLogin/wwQuickLogin", new Dictionary<string, string>
                {
                    { "code_type", "13" }, { "redirect_uri", "https://open.work.weixin.qq.com" }, { "version", "0.6.2" }
                });
        var param = new GetQuickLoginParam();
        var response = await _weComReq.HttpWebRequestGetAsync(url);
        var html = GetResponseStr(response);
        // 通过正则获取登录配置参数
        string pattern = "(?<=window.settings =).*?(?=;</script>)";
        var paramJson = Regex.Matches(html, pattern).FirstOrDefault()?.Value;
        return paramJson;
    }

    public async Task<string> GetCorpBindDeveloperInfoAsync(string webKey)
    {
        // 参数： lang=zh_CN&ajax=1&f=json&random=269405
        var url = _weComReq.GetQueryUrl("wwopen/monoApi/wwQuickLogin/login/getCorpBindDeveloperInfo", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "random", _weComReq.GetRandom() }
                });
        var param = new
        {
            qrcode_key = webKey
        };
        var response = await _weComReq.HttpWebRequestPostJsonAsync(url, JsonConvert.SerializeObject(param));
        return GetResponseStr(response); ;
    }

    public async Task<string> ConfirmQuickLoginAsync(string webKey)
    {
        // 参数： lang=zh_CN&ajax=1&f=json&random=334758
        var url = _weComReq.GetQueryUrl("wwopen/monoApi/wwQuickLogin/login/confirmQuickLoginByKey", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "random", _weComReq.GetRandom() }
                });
        var param = new
        {
            qrcode_key = webKey
        };
        var response = await _weComReq.HttpWebRequestPostJsonAsync(url, JsonConvert.SerializeObject(param));
        return GetResponseStr(response);
    }

    public async Task<string> GetQuickLoginCorpInfoAsync(string webKey)
    {
        // 参数： lang=zh_CN&ajax=1&f=json&random=269405
        var url = _weComReq.GetQueryUrl("wwopen/monoApi/wwQuickLogin/login/getWebKeyStatus", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "random", _weComReq.GetRandom() }
                });
        var param = new
        {
            webKey = webKey
        };
        var response = await _weComReq.HttpWebRequestPostJsonAsync(url, JsonConvert.SerializeObject(param));
        return GetResponseStr(response);
    }

    #endregion

    private string GetResponseStr(HttpWebResponse response)
    {
        if (!_weComReq.IsResponseSucc(response)) return string.Empty;
        Stream responseStream = response.GetResponseStream();
        StreamReader sr = new StreamReader(responseStream);
        var responseStr = sr.ReadToEnd();
        response.Close();
        responseStream.Close();
        return responseStr;
    }
}
