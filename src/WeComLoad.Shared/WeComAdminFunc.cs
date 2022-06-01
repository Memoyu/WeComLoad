using HtmlAgilityPack;

namespace WeComLoad.Shared;

public class WeComAdminFunc : IWeComAdmin
{
    private readonly WeComAdminWebReq _weComReq;

    public WeComAdminFunc()
    {
        _weComReq = new WeComAdminWebReq();
    }

    public WeComAdminWebReq GetWeCombReq() => _weComReq;

    public async Task<(string Url, string Key)> GetLoginQrCodeUrlAsync()
    {
        var keyUrl = _weComReq.GetQueryUrl("wework_admin/wwqrlogin/mng/get_key", new Dictionary<string, string>
            {
                { "r", _weComReq.GetRandom() }
            });
        var response = await _weComReq.HttpWebRequestGetAsync(keyUrl);
        if (!_weComReq.IsResponseSucc(response)) return (null, null);
        var model = JsonConvert.DeserializeObject<WeComBase<WeComQrCodeKey>>(_weComReq.GetResponseStr(response));
        if (model == null || string.IsNullOrWhiteSpace(model.Data.QrCodeKey)) throw new Exception("企微二维码Key为空");
        var key = model.Data.QrCodeKey;
        var qrCodeUrl = _weComReq.GetQueryUrl($"{_weComReq.BaseUrl}wwqrlogin/mng/qrcode/{key}", new Dictionary<string, string>
            {
                { "login_type", "login_admin" }
            });
        return (qrCodeUrl, key);
    }

    public async Task<WeComQrCodeScanStatus> GetQrCodeScanStatusAsync(string qrCodeKey)
    {
        if (string.IsNullOrWhiteSpace(qrCodeKey)) throw new ArgumentNullException("企微登录二维码Key为空");
        var url = _weComReq.GetQueryUrl("wework_admin/wwqrlogin/check", new Dictionary<string, string>
            {
                { "qrcode_key", qrCodeKey }, { "status", "" }
            });
        var response = await _weComReq.HttpWebRequestGetAsync(url);
        if (!_weComReq.IsResponseSucc(response)) return null;
        var model = JsonConvert.DeserializeObject<WeComBase<WeComQrCodeScanStatus>>(_weComReq.GetResponseStr(response));
        return model?.Data;
    }

    public async Task<bool> LoginAsync(string qrCodeKey, string authCode)
    {
        if (string.IsNullOrWhiteSpace(qrCodeKey) || string.IsNullOrWhiteSpace(authCode)) throw new ArgumentNullException("企微登录必要参数为空");

        // 开始进行预登录
        var url = _weComReq.GetQueryUrl("wework_admin/loginpage_wx", new Dictionary<string, string>
            {
               { "_r", new Random().Next(100, 999).ToString() }, { "code", authCode }, { "qrcode_key", qrCodeKey }, { "wwqrlogin", "1" }, { "auth_source", "SOURCE_FROM_WEWORK" }
            });
        var response = await _weComReq.HttpWebRequestGetAsync(url, true);
        if (!_weComReq.IsResponseRedi(response)) return false;
        var rediUrlData = _weComReq.GetResponseStr(response);
        if (string.IsNullOrWhiteSpace(rediUrlData)) return false;
        url = _weComReq.GetRedirectUrl(rediUrlData);

        // 手动重定向到url下，获取第一部分Cookie
        response = await _weComReq.HttpWebRequestGetAsync(url, true);
        if (!_weComReq.IsResponseRedi(response)) return false;
        rediUrlData = _weComReq.GetResponseStr(response);
        url = _weComReq.GetRedirectUrl(rediUrlData);

        // 手动重定向到url下，获取第二部分cookie，且为完整的Cookie
        response = await _weComReq.HttpWebRequestGetAsync(url, true);
        if (!_weComReq.IsResponseSucc(response)) return false;

        return true;
    }

    public async Task<WeComWxLoginCorps> GetWxLoginCorpsAsync(string qrCodeKey, string authCode)
    {
        var dto = new WeComWxLoginCorps();
        if (string.IsNullOrWhiteSpace(qrCodeKey) || string.IsNullOrWhiteSpace(authCode)) throw new ArgumentNullException("企微登录必要参数为空");

        // 开始进行预登录
        var url = _weComReq.GetQueryUrl("wework_admin/loginpage_wx", new Dictionary<string, string>
            {
               { "_r", new Random().Next(100, 999).ToString() }, { "code", authCode }, { "qrcode_key", qrCodeKey }, { "wwqrlogin", "1" }, { "auth_source", "SOURCE_FROM_WEWORK" }
            });
        var response = await _weComReq.HttpWebRequestGetAsync(url, true);
        if (!_weComReq.IsResponseRedi(response)) return dto;
        var rediUrlData = _weComReq.GetResponseStr(response);
        if (string.IsNullOrWhiteSpace(rediUrlData)) return dto;
        url = _weComReq.GetRedirectUrl(rediUrlData);

        // 手动重定向到选择企业页面
        response = await _weComReq.HttpWebRequestGetAsync(url, true);
        if (!_weComReq.IsResponseSucc(response)) return dto;
        var html = _weComReq.GetResponseStr(response);

        // 解析html页面企业数据
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);
        var list = htmlDoc.DocumentNode.SelectNodes("//li[@class='login_selectBiz_item']/a").Select(e =>
        {
            var corp = new LoginCorp
            {
                CorpId = e.GetAttributeValue("data-id", ""),
                CorpName = e.GetAttributeValue("data-corpname", "")
            };
            return corp;
        }).ToList();
        dto.Corps = list;
        dto.TlKey = GetQueryString("tl_key", url);
        return dto;
    }

    public async Task<string> WxLoginAsync(string tlKey, string corpId)
    {
        if (string.IsNullOrWhiteSpace(tlKey) || string.IsNullOrWhiteSpace(corpId)) throw new ArgumentNullException("企微登录必要参数为空");

        var dic = new List<(string, string)>()
                {
                    ("tl_key", tlKey), ("corp_id", corpId)
                };
        var url = _weComReq.GetQueryUrl("wework_admin/login/choose_corp/login", new Dictionary<string, string> { });
        var response = await _weComReq.HttpWebRequestPostAsync(url, dic, true);
        if (!_weComReq.IsResponseSucc(response)) return string.Empty;
        return _weComReq.GetResponseStr(response);
    }

    public async Task<bool> WxLoginAfterAsync()
    {
        var response = await _weComReq.HttpWebRequestGetAsync("wework_admin/frame", true);
        return _weComReq.IsResponseSucc(response);
    }

    public async Task<string> GetCorpAppAsync()
    {

        var dic = new List<(string, string)>()
                {
                    ("_d2st", _weComReq.GetD2st()), ("app_type", "0")
                };
        var url = _weComReq.GetQueryUrl("wework_admin/getCorpApplication", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "random", _weComReq.GetRandom() }
                });
        var response = await _weComReq.HttpWebRequestPostAsync(url, dic);
        return GetResponseStr(response);
    }

    public async Task<string> GetCorpOpenAppAsync(string appOpenId)
    {

        var url = _weComReq.GetQueryUrl("wework_admin/apps/getOpenApiApp", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "random", _weComReq.GetRandom() },
                    { "app_open_id", appOpenId }, { "notperm", "true" }, { "_d2st", _weComReq.GetD2st() }
                });
        var response = await _weComReq.HttpWebRequestGetAsync(url);
        return GetResponseStr(response);
    }

    public async Task<string> GetCorpDeptAsync()
    {
        var dic = new List<(string, string)>
                {
                    ("_d2st", _weComReq.GetD2st())
                };
        var url = _weComReq.GetQueryUrl("wework_admin/contacts/party/cache", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "random", _weComReq.GetRandom() }
                });
        var response = await _weComReq.HttpWebRequestPostAsync(url, dic);
        return GetResponseStr(response);
    }

    public async Task<string> SendSecretAsync(string appId)
    {
        var dic = new List<(string, string)>
                {
                  ("appid", appId), ("business_type", "1"), ("app_type", "1"), ("_d2st", _weComReq.GetD2st())
                };
        var url = _weComReq.GetQueryUrl("wework_admin/two_factor_auth_operation/create", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "random", _weComReq.GetRandom() }
                });
        var response = await _weComReq.HttpWebRequestPostAsync(url, dic);
        return GetResponseStr(response);
    }

    public async Task<string> AddOpenApiAppAsync(AddOpenApiAppRequest req)
    {
        var dic = new List<(string, string)>
            {
                ("name", req.Name),
                ("description", req.Desc),
                ("english_name", ""),
                ("english_description", ""),
                ("app_open", "true"),
                ("logoimage", req.LogoImage),
                ("_d2st", _weComReq.GetD2st())
            };
        foreach (var pid in req.VisiblePIds)
        {
            dic.Add(("visible_pid[]", pid));
        }
        var url = _weComReq.GetQueryUrl("wework_admin/apps/addOpenApiApp", new Dictionary<string, string>
            {
                { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "random", _weComReq.GetRandom() }
            });
        var response = await _weComReq.HttpWebRequestPostAsync(url, dic);
        return GetResponseStr(response);
    }

    public async Task<string> AddChatMenuAsync(AddChatMenuRequest menu, WeComOpenapiApp agent)
    {

        var dic = new List<(string, string)>
                {
                    ("banner_list[0][corp_app][app_id]", agent.AppId),
                    ("banner_list[0][corpAppModel][app_open]", "1"),
                    ("banner_list[0][corpAppModel][isThirdApp]", "0"),
                    ("banner_list[0][corpAppModel][isMiniApp]", "0"),
                    ("banner_list[0][corpAppModel][isBaseApp]", "false"),
                    ("banner_list[0][corpAppModel][id]", agent.AppId),
                    ("banner_list[0][corpAppModel][logoimage]", agent.Imgid),
                    ("banner_list[0][corpAppModel][name]", agent.Name),
                    ("banner_list[0][corpAppModel][sm_imgid]", agent.Imgid),
                    ("banner_list[0][item_type]", "1"),
                    ("banner_list[0][name]", menu.MenuName),
                    ("banner_list[0][item_name]", menu.MenuName),
                    ("banner_list[0][item_info]", menu.MenuUrl),
                    ("_d2st", _weComReq.GetD2st())
                };
        var url = _weComReq.GetQueryUrl("wework_admin/customer/addChatMenu", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "random", _weComReq.GetRandom() }
                });
        var response = await _weComReq.HttpWebRequestPostAsync(url, dic);
        return GetResponseStr(response);
    }

    public async Task<string> SaveOpenApiAppAsync(List<(string Key, string Value)> req)
    {
        req.Add(new("_d2st", _weComReq.GetD2st()));

        // wework_admin/apps/saveOpenApiApp?lang=zh_CN&f=json&ajax=1&timeZoneInfo%5Bzone_offset%5D=-8&random=0.6716191463354881
        var url = _weComReq.GetQueryUrl("wework_admin/apps/saveOpenApiApp", new Dictionary<string, string>
            {
                { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "random", _weComReq.GetRandom() }
            });
        var response = await _weComReq.HttpWebRequestPostAsync(url, req);
        return GetResponseStr(response);
    }

    public async Task<string> CreateTwoFactorAuthOpAsync(string appId)
    {
        var dic = new List<(string, string)>
            {
                ("appid", appId),
                ("business_type", "3"),
                ("app_type", "1"),
                ("_d2st", _weComReq.GetD2st())
            };
        var url = _weComReq.GetQueryUrl("wework_admin/two_factor_auth_operation/create", new Dictionary<string, string>
            {
                { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "random", _weComReq.GetRandom() }
            });
        var response = await _weComReq.HttpWebRequestPostAsync(url, dic);
        return GetResponseStr(response);
    }

    public async Task<string> QueryTwoFactorAuthOpAsync(string key)
    {
        var url = _weComReq.GetQueryUrl("wework_admin/two_factor_auth_operation/query", new Dictionary<string, string>
            {
                { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "key",key },
                { "random", _weComReq.GetRandom() }
            });
        var response = await _weComReq.HttpWebRequestGetAsync(url);
        return GetResponseStr(response);
    }

    public async Task<string> ConfigContactCallbackAsync(ConfigCallbackRequest req)
    {
        var dic = new List<(string, string)>
            {
               ("callback_url", req.CallbackUrl),
               ("url_token", req.Token),
               ("callback_aeskey", req.AesKey),
               ("confirm_check_key", req.CheckKey),
               ("app_id", req.Appid),
               ("callback_host", req.HostUrl),
               ("report_loc_flag", "0"),
               ("is_report_enter", "false"),
               ("report_approval_event", "false"),
               ("_d2st", _weComReq.GetD2st())
            };
        var url = _weComReq.GetQueryUrl("wework_admin/apps/saveOpenApiApp", new Dictionary<string, string>
            {
                { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "random", _weComReq.GetRandom() }
            });
        var response = await _weComReq.HttpWebRequestPostAsync(url, dic);
        return GetResponseStr(response);
    }

    public async Task<string> ConfigExtContactCallbackAsync(ConfigCallbackRequest req)
    {
        var dic = new List<(string, string)>
            {
               ("callback_url", req.CallbackUrl),
               ("url_token", req.Token),
               ("callback_aeskey", req.AesKey),
               ("app_id", req.Appid),
               ("callback_host", req.HostUrl),
               ("_d2st", _weComReq.GetD2st())
            };
        var url = _weComReq.GetQueryUrl("wework_admin/apps/saveOpenApiApp", new Dictionary<string, string>
            {
                { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "random", _weComReq.GetRandom() }
            });
        var response = await _weComReq.HttpWebRequestPostAsync(url, dic);
        return GetResponseStr(response);
    }

    public async Task<string> GetApiAccessibleAppsAsync(string businessId)
    {
        var url = _weComReq.GetQueryUrl("wework_admin/apps/getApiAccessibleApps", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" },
                    { "random", _weComReq.GetRandom() }, { "businessId", businessId }, { "_d2st", _weComReq.GetD2st() }
                });
        var response = await _weComReq.HttpWebRequestGetAsync(url);
        return GetResponseStr(response);
        //if (!_weComReq.IsResponseSucc(response)) return null;
        //var model = JsonConvert.DeserializeObject<WeComBase<WeComGetApiAccessibleApps>>(_weComReq.GetResponseStr(response));
    }


    public async Task<string> SetApiAccessibleAppsAsync(string businessId, List<string> accessibleApps)
    {
        var dic = new List<(string, string)>
            {
                ("businessId", businessId),
                ("_d2st", _weComReq.GetD2st())
            };
        foreach (var item in accessibleApps)
        {   
            dic.Add(("auth_list[appid_list][]", item));
        }

        // wework_admin/apps/saveOpenApiApp?lang=zh_CN&f=json&ajax=1&timeZoneInfo%5Bzone_offset%5D=-8&random=0.6716191463354881
        var url = _weComReq.GetQueryUrl("wework_admin/apps/setApiAccessibleApps", new Dictionary<string, string>
            {
                { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "random", _weComReq.GetRandom() }
            });
        var response = await _weComReq.HttpWebRequestPostAsync(url, dic);
        return GetResponseStr(response);
    }

    public async Task<(string Name, byte[] File)> GetDomainVerifyFileAsync()
    {
        // 获取可新域名校验文件名
        var url = _weComReq.GetQueryUrl("wework_admin/apps/getDomainOwnershipVerifyInfo", new Dictionary<string, string>
            {
                { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "random", _weComReq.GetRandom() }, { "_d2st", _weComReq.GetD2st() }
            });
        var response = await _weComReq.HttpWebRequestGetAsync(url);
        if (!_weComReq.IsResponseSucc(response)) return (null, null);
        var model = JsonConvert.DeserializeObject<WeComDomainVerifyFileName>(_weComReq.GetResponseStr(response));

        // 地址写死： https://work.weixin.qq.com/wework_admin/apps/getDomainOwnershipVerifyInfo?action=download
        var file = await _weComReq.HttpWebRequestDownloadsync("wework_admin/apps/getDomainOwnershipVerifyInfo?action=download");
        return (model.FileName, file);
    }

    public async Task<string> CheckCustomAppURLAsync(string appid, string domian)
    {
        // wework_admin/apps/checkCustomAppURL?
        // lang=zh_CN&f=json&ajax=1&timeZoneInfo%5Bzone_offset%5D=-8&random=0.8351865053727201&url=devscrmh5.lianou.tech&appid=5629502294442019&type=redirect_domain&_d2st=a3652781
        var url = _weComReq.GetQueryUrl("wework_admin/apps/checkCustomAppURL", new Dictionary<string, string>
            {
                { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "random", _weComReq.GetRandom() },
                { "url", domian }, { "appid", appid }, { "type", "redirect_domain" }, { "_d2st", _weComReq.GetD2st() }
            });
        var response = await _weComReq.HttpWebRequestGetAsync(url);
        return GetResponseStr(response);
    }

    public async Task<string> CheckXcxDomainStatusAsync(string domian)
    {
        var dic = new List<(string, string)>
            {
                ("xcxDomains[]", domian), ("_d2st", _weComReq.GetD2st())
            };

        // https://work.weixin.qq.com/wework_admin/apps/checkXcxDomains?lang=zh_CN&f=json&ajax=1&timeZoneInfo%5Bzone_offset%5D=-8&random=0.9542216877814127
        var url = _weComReq.GetQueryUrl("wework_admin/apps/checkXcxDomains", new Dictionary<string, string>
            {
                { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "random", _weComReq.GetRandom() }
            });
        var response = await _weComReq.HttpWebRequestPostAsync(url, dic);
        return GetResponseStr(response);
    }

    public async Task<string> SetCustomizedAppPrivilege(string appId)
    {
        var dic = new List<(string, string)>
                {
                    ("app_id", appId),
                    ("app_privilege[0][val]", "19"),
                    ("app_privilege[0][b_check]", "true"),
                    ("app_privilege[0][sub_app_privilege][0][val]", "1009"),
                    ("app_privilege[0][sub_app_privilege][0][b_check]", "true"),
                    ("app_privilege[0][sub_app_privilege][0][sub_app_privilege][0][val]", "10006"),
                    ("app_privilege[0][sub_app_privilege][0][sub_app_privilege][0][b_check]", "true"),
                    ("app_privilege[0][sub_app_privilege][0][sub_app_privilege][1][val]", "10001"),
                    ("app_privilege[0][sub_app_privilege][0][sub_app_privilege][1][b_check]", "true"),
                    ("app_privilege[0][sub_app_privilege][0][sub_app_privilege][2][val]", "10010"),
                    ("app_privilege[0][sub_app_privilege][0][sub_app_privilege][2][b_check]", "true"),
                    ("app_privilege[0][sub_app_privilege][0][sub_app_privilege][3][val]", "10004"),
                    ("app_privilege[0][sub_app_privilege][0][sub_app_privilege][3][b_check]", "true"),
                    ("app_privilege[0][sub_app_privilege][0][sub_app_privilege][4][val]", "10011"),
                    ("app_privilege[0][sub_app_privilege][0][sub_app_privilege][4][b_check]", "true"),
                    ("app_privilege[0][sub_app_privilege][0][sub_app_privilege][5][val]", "10009"),
                    ("app_privilege[0][sub_app_privilege][0][sub_app_privilege][5][b_check]", "true"),
                    ("app_privilege[0][sub_app_privilege][0][sub_app_privilege][6][val]", "10015"),
                    ("app_privilege[0][sub_app_privilege][0][sub_app_privilege][6][b_check]", "true"),
                    ("app_privilege[0][sub_app_privilege][0][sub_app_privilege][7][val]", "10020"),
                    ("app_privilege[0][sub_app_privilege][0][sub_app_privilege][7][b_check]", "true"),
                    ("app_privilege[0][sub_app_privilege][0][sub_app_privilege][8][val]", "10021"),
                    ("app_privilege[0][sub_app_privilege][0][sub_app_privilege][8][b_check]", "true"),
                    ("app_privilege[0][sub_app_privilege][0][sub_app_privilege][9][val]", "10022"),
                    ("app_privilege[0][sub_app_privilege][0][sub_app_privilege][9][b_check]", "true"),
                    ("app_privilege[1][val]", "1"),
                    ("app_privilege[1][b_check]", "true"),
                    ("app_privilege[1][sub_app_privilege][0][val]", "1100"),
                    ("app_privilege[1][sub_app_privilege][0][b_check]", "true"),
                    ("app_privilege[1][sub_app_privilege][0][sub_app_privilege][0][val]", "110005"),
                    ("app_privilege[1][sub_app_privilege][0][sub_app_privilege][0][b_check]", "true"),
                    ("app_privilege[1][sub_app_privilege][1][val]", "1104"),
                    ("app_privilege[1][sub_app_privilege][1][b_check]", "true"),
                    ("app_privilege[1][sub_app_privilege][1][sub_app_privilege][0][val]", "110405"),
                    ("app_privilege[1][sub_app_privilege][1][sub_app_privilege][0][b_check]", "true"),
                    ("app_privilege[1][sub_app_privilege][1][sub_app_privilege][1][val]", "110406"),
                    ("app_privilege[1][sub_app_privilege][1][sub_app_privilege][1][b_check]", "true"),
                    ("app_privilege[1][sub_app_privilege][2][val]", "1101"),
                    ("app_privilege[1][sub_app_privilege][2][b_check]", "true"),
                    ("app_privilege[1][sub_app_privilege][2][sub_app_privilege][0][val]", "110100"),
                    ("app_privilege[1][sub_app_privilege][2][sub_app_privilege][0][b_check]", "true"),
                    ("app_privilege[1][sub_app_privilege][2][sub_app_privilege][1][val]", "110101"),
                    ("app_privilege[1][sub_app_privilege][2][sub_app_privilege][1][b_check]", "true"),
                    ("app_privilege[1][sub_app_privilege][2][sub_app_privilege][2][val]", "110102"),
                    ("app_privilege[1][sub_app_privilege][2][sub_app_privilege][2][b_check]", "true"),
                    ("app_privilege[1][sub_app_privilege][2][sub_app_privilege][3][val]", "110103"),
                    ("app_privilege[1][sub_app_privilege][2][sub_app_privilege][3][b_check]", "true"),
                    ("app_privilege[1][sub_app_privilege][2][sub_app_privilege][4][val]", "110105"),
                    ("app_privilege[1][sub_app_privilege][2][sub_app_privilege][4][b_check]", "true"),
                    ("app_privilege[1][sub_app_privilege][2][sub_app_privilege][5][val]", "110106"),
                    ("app_privilege[1][sub_app_privilege][2][sub_app_privilege][5][b_check]", "true"),
                    ("app_privilege[1][sub_app_privilege][2][sub_app_privilege][6][val]", "110107"),
                    ("app_privilege[1][sub_app_privilege][2][sub_app_privilege][6][b_check]", "true"),
                    ("app_privilege[1][sub_app_privilege][2][sub_app_privilege][7][val]", "110108"),
                    ("app_privilege[1][sub_app_privilege][2][sub_app_privilege][7][b_check]", "true"),
                    ("app_privilege[1][sub_app_privilege][2][sub_app_privilege][8][val]", "110109"),
                    ("app_privilege[1][sub_app_privilege][2][sub_app_privilege][8][b_check]", "true"),
                    ("app_privilege[1][sub_app_privilege][3][val]", "1102"),
                    ("app_privilege[1][sub_app_privilege][3][b_check]", "true"),
                    ("app_privilege[1][sub_app_privilege][3][sub_app_privilege][0][val]", "110200"),
                    ("app_privilege[1][sub_app_privilege][3][sub_app_privilege][0][b_check]", "true"),
                    ("app_privilege[1][sub_app_privilege][3][sub_app_privilege][1][val]", "110202"),
                    ("app_privilege[1][sub_app_privilege][3][sub_app_privilege][1][b_check]", "true"),
                    ("app_privilege[1][sub_app_privilege][3][sub_app_privilege][2][val]", "110201"),
                    ("app_privilege[1][sub_app_privilege][3][sub_app_privilege][2][b_check]", "true"),
                    ("app_privilege[1][sub_app_privilege][4][val]", "1103"),
                    ("app_privilege[1][sub_app_privilege][4][b_check]", "true"),
                    ("app_privilege[1][sub_app_privilege][4][sub_app_privilege][0][val]", "110300"),
                    ("app_privilege[1][sub_app_privilege][4][sub_app_privilege][0][b_check]", "true"),
                    ("app_privilege[1][sub_app_privilege][4][sub_app_privilege][1][val]", "110301"),
                    ("app_privilege[1][sub_app_privilege][4][sub_app_privilege][1][b_check]", "true"),
                    ("app_privilege[2][val]", "9"),
                    ("app_privilege[2][b_check]", "false"),
                    ("app_privilege[2][sub_app_privilege][0][val]", "1900"),
                    ("app_privilege[2][sub_app_privilege][0][b_check]", "false"),
                    ("app_privilege[2][sub_app_privilege][0][sub_app_privilege][0][val]", "190000"),
                    ("app_privilege[2][sub_app_privilege][0][sub_app_privilege][0][b_check]", "false"),
                    ("app_privilege[3][val]", "2"),
                    ("app_privilege[3][b_check]", "false"),
                    ("app_privilege[3][sub_app_privilege][0][val]", "1200"),
                    ("app_privilege[3][sub_app_privilege][0][b_check]", "false"),
                    ("app_privilege[3][sub_app_privilege][0][sub_app_privilege][0][val]", "120000"),
                    ("app_privilege[3][sub_app_privilege][0][sub_app_privilege][0][b_check]", "false"),
                    ("app_privilege[4][val]", "3"),
                    ("app_privilege[4][b_check]", "false"),
                    ("app_privilege[4][sub_app_privilege][0][val]", "1300"),
                    ("app_privilege[4][sub_app_privilege][0][b_check]", "false"),
                    ("app_privilege[4][sub_app_privilege][0][sub_app_privilege][0][val]", "130000"),
                    ("app_privilege[4][sub_app_privilege][0][sub_app_privilege][0][b_check]", "false"),
                    ("app_privilege[5][val]", "10"),
                    ("app_privilege[5][b_check]", "false"),
                    ("app_privilege[5][sub_app_privilege][0][val]", "2000"),
                    ("app_privilege[5][sub_app_privilege][0][b_check]", "false"),
                    ("app_privilege[5][sub_app_privilege][0][sub_app_privilege][0][val]", "200000"),
                    ("app_privilege[5][sub_app_privilege][0][sub_app_privilege][0][b_check]", "false"),
                    ("app_privilege[6][val]", "11"),
                    ("app_privilege[6][b_check]", "false"),
                    ("app_privilege[6][sub_app_privilege][0][val]", "2100"),
                    ("app_privilege[6][sub_app_privilege][0][b_check]", "false"),
                    ("app_privilege[6][sub_app_privilege][0][sub_app_privilege][0][val]", "210000"),
                    ("app_privilege[6][sub_app_privilege][0][sub_app_privilege][0][b_check]", "false"),
                    ("app_privilege[6][sub_app_privilege][1][val]", "2101"),
                    ("app_privilege[6][sub_app_privilege][1][b_check]", "false"),
                    ("app_privilege[6][sub_app_privilege][1][sub_app_privilege][0][val]", "210001"),
                    ("app_privilege[6][sub_app_privilege][1][sub_app_privilege][0][b_check]", "false"),
                    ("_d2st", _weComReq.GetD2st())
                };
        var url = _weComReq.GetQueryUrl("wework_admin/apps/custom/perm/setCustomizedAppPrivilege", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "random", _weComReq.GetRandom() }
                });
        var response = await _weComReq.HttpWebRequestPostAsync(url, dic);
        return GetResponseStr(response);
    }

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

    private string GetQueryString(string name, string url)
    {
        Regex re = new Regex(@"(^|&)?(\w+)=([^&]+)(&|$)?", RegexOptions.Compiled);
        MatchCollection mc = re.Matches(url);
        foreach (Match m in mc)
        {
            if (m.Result("$2").Equals(name))
            {
                return m.Result("$3");
            }
        }
        return "";
    }
}
