using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeComLoad.Automation
{
    public class WeComAdminFunc : IWeComAdmin
    {
        private readonly WeComAdminWebReq _weCombReq;
        private WeComBase<WeComCorpApp> _weComCorpApps = null;
        private WeComBase<WeComCorpDept> _weComCorpDepts = null;

        public WeComAdminFunc()
        {
            _weCombReq = new WeComAdminWebReq();
        }

        public WeComAdminWebReq GetWeCombReq() => _weCombReq;

        public async Task<(string Url, string Key)> GetLoginQrCodeUrlAsync()
        {
            var keyUrl = _weCombReq.GetQueryUrl("wework_admin/wwqrlogin/mng/get_key", new Dictionary<string, string>
            {
                { "r", _weCombReq.GetRandom() }
            });
            var response = await _weCombReq.HttpWebRequestGetAsync(keyUrl);
            if (!_weCombReq.IsResponseSucc(response)) return (null, null);
            var model = JsonConvert.DeserializeObject<WeComBase<WeComQrCodeKey>>(_weCombReq.GetResponseStr(response));
            if (model == null || string.IsNullOrWhiteSpace(model.Data.QrCodeKey)) throw new Exception("企微二维码Key为空");
            var key = model.Data.QrCodeKey;
            var qrCodeUrl = _weCombReq.GetQueryUrl($"{_weCombReq.BaseUrl}wwqrlogin/mng/qrcode/{key}", new Dictionary<string, string>
            {
                { "login_type", "login_admin" }
            });
            return (qrCodeUrl, key);
        }

        public async Task<WeComQrCodeScanStatus> GetQrCodeScanStatusAsync(string qrCodeKey)
        {
            if (string.IsNullOrWhiteSpace(qrCodeKey)) throw new ArgumentNullException("企微登录二维码Key为空");
            var url = _weCombReq.GetQueryUrl("wework_admin/wwqrlogin/check", new Dictionary<string, string>
            {
                { "qrcode_key", qrCodeKey }, { "status", "" }
            });
            var response = await _weCombReq.HttpWebRequestGetAsync(url);
            if (!_weCombReq.IsResponseSucc(response)) return null;
            var model = JsonConvert.DeserializeObject<WeComBase<WeComQrCodeScanStatus>>(_weCombReq.GetResponseStr(response));
            return model?.Data;
        }

        public async Task<bool> LoginAsync(string qrCodeKey, string authCode)
        {
            if (string.IsNullOrWhiteSpace(qrCodeKey) || string.IsNullOrWhiteSpace(authCode)) throw new ArgumentNullException("企微登录必要参数为空");

            // 开始进行预登录
            var url = _weCombReq.GetQueryUrl("wework_admin/loginpage_wx", new Dictionary<string, string>
            {
               { "_r", new Random().Next(100, 999).ToString() }, { "code", authCode }, { "qrcode_key", qrCodeKey }, { "wwqrlogin", "1" }, { "auth_source", "SOURCE_FROM_WEWORK" }
            });
            var response = await _weCombReq.HttpWebRequestGetAsync(url, true);
            if (!_weCombReq.IsResponseRedi(response)) return false;
            var rediUrlData = _weCombReq.GetResponseStr(response);
            if (string.IsNullOrWhiteSpace(rediUrlData)) return false;
            url = _weCombReq.GetRedirectUrl(rediUrlData);

            // 手动重定向到url下，获取第一部分Cookie
            response = await _weCombReq.HttpWebRequestGetAsync(url, true);
            if (!_weCombReq.IsResponseRedi(response)) return false;
            rediUrlData = _weCombReq.GetResponseStr(response);
            url = _weCombReq.GetRedirectUrl(rediUrlData);

            // 手动重定向到url下，获取第二部分cookie，且为完整的Cookie
            response = await _weCombReq.HttpWebRequestGetAsync(url, true);
            return _weCombReq.IsResponseSucc(response);
        }

        public async Task<WeComBase<WeComCorpApp>> GetCorpAppAsync(bool isReLoad = false)
        {
            if (_weComCorpApps == null || isReLoad)
            {
                var dic = new List<(string, string)>()
                {
                    ("_d2st", _weCombReq.GetD2st()), ("app_type", "0")
                };
                var url = _weCombReq.GetQueryUrl("wework_admin/getCorpApplication", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "random", _weCombReq.GetRandom() }
                });
                var response = await _weCombReq.HttpWebRequestPostAsync(url, dic);
                if (!_weCombReq.IsResponseSucc(response)) return null;
                var model = JsonConvert.DeserializeObject<WeComBase<WeComCorpApp>>(_weCombReq.GetResponseStr(response));
                _weComCorpApps = model;
                return model;
            }

            return _weComCorpApps;
        }

        public async Task<WeComBase<WeComCorpDept>> GetCorpDeptAsync(bool isReLoad = false)
        {
            if (_weComCorpDepts == null || isReLoad)
            {
                var dic = new List<(string, string)>
                {
                    ("_d2st", _weCombReq.GetD2st())
                };
                var url = _weCombReq.GetQueryUrl("wework_admin/contacts/party/cache", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "random", _weCombReq.GetRandom() }
                });
                var response = await _weCombReq.HttpWebRequestPostAsync(url, dic);
                if (!_weCombReq.IsResponseSucc(response)) return null;
                var model = JsonConvert.DeserializeObject<WeComBase<WeComCorpDept>>(_weCombReq.GetResponseStr(response));
                _weComCorpDepts = model;
                return model;
            }

            return _weComCorpDepts;
        }

        public async Task<bool> SendExtContactAndUserSecretAsync()
        {
            // 1、获取应用列表
            var model = await GetCorpAppAsync();

            // 获取客户联系、通讯录appid (app_open_id为固定值，2000002：通讯录同步助手，2000003：外部联系人)
            var appids = model.Data.OpenapiApps.Where(m => m.AppOpenId == 2000002 || m.AppOpenId == 2000003).Select(a => a.AppId).ToList();
            foreach (var id in appids)
            {
                var dic = new List<(string, string)>
                {
                  ("appid", id), ("business_type", "1"), ("app_type", "1"), ("_d2st", _weCombReq.GetD2st())
                };
                var url = _weCombReq.GetQueryUrl("wework_admin/two_factor_auth_operation/create", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "random", _weCombReq.GetRandom() }
                });
                var response = await _weCombReq.HttpWebRequestPostAsync(url, dic);
            }

            return true;
        }

        public async Task<WeComOpenapiApp> AddOpenApiAppAsync(AddOpenApiAppRequest req)
        {
            var appModel = await GetCorpAppAsync(true);
            var agent = appModel.Data.OpenapiApps.FirstOrDefault(a => a.Name.Equals(req.Name));
            if (agent != null) return agent;

            var deptModel = await GetCorpDeptAsync();
            var dept = deptModel.Data.Partys.List.Where(p => p.OpenapiPartyid.Equals("1")).FirstOrDefault();
            if (dept == null) throw new Exception("获取根部门异常");

            var visible_pid = dept.Partyid;
            var dic = new List<(string, string)>
            {
                ("name", req.Name),
                ("description", req.Desc),
                ("english_name", ""),
                ("english_description", ""),
                ("app_open", "true"),
                ("logoimage", req.LogoImage),
                ("visible_pid[]", visible_pid),
                ("_d2st", _weCombReq.GetD2st())
            };
            var url = _weCombReq.GetQueryUrl("wework_admin/apps/addOpenApiApp", new Dictionary<string, string>
            {
                { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "random", _weCombReq.GetRandom() }
            });
            var response = await _weCombReq.HttpWebRequestPostAsync(url, dic);
            if (!_weCombReq.IsResponseSucc(response)) return null;
            var model = JsonConvert.DeserializeObject<WeComBase<WeComOpenapiApp>>(_weCombReq.GetResponseStr(response));
            return model.Data;
        }

        public async Task<bool> AddChatMenuAsync(AddChatMenuRequest req)
        {
            var dic = new List<(string, string)>
            {
                ("banner_list[0][corp_app][app_id]", req.Agent.AppId),
                ("banner_list[0][corpAppModel][app_open]", "1"),
                ("banner_list[0][corpAppModel][isThirdApp]", "0"),
                ("banner_list[0][corpAppModel][isMiniApp]", "0"),
                ("banner_list[0][corpAppModel][isBaseApp]", "false"),
                ("banner_list[0][corpAppModel][id]", req.Agent.AppId),
                ("banner_list[0][corpAppModel][logoimage]", req.Agent.Imgid),
                ("banner_list[0][corpAppModel][name]", req.Agent.Name),
                ("banner_list[0][corpAppModel][sm_imgid]", req.Agent.Imgid),
                ("banner_list[0][item_type]", "1"),
                ("banner_list[0][name]", req.MenuName),
                ("banner_list[0][item_name]", req.MenuName),
                ("banner_list[0][item_info]", req.MenuUrl),
                ("_d2st", _weCombReq.GetD2st())
            };
            var url = _weCombReq.GetQueryUrl("wework_admin/customer/addChatMenu", new Dictionary<string, string>
            {
                { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "random", _weCombReq.GetRandom() }
            });
            var response = await _weCombReq.HttpWebRequestPostAsync(url, dic);
            if (!_weCombReq.IsResponseSucc(response)) return false;
            return true;
        }

        public async Task<WeComSaveOpenApiApp> SaveOpenApiAppAsync(SaveOpenApiAppRequest req)
        {
            // app_id=5629502294442019&redirect_domain=devscrmh5.lianou.tech&is_check_domain_ownership=true&miniprogram_domains_operate=true&_d2st=a3652781
            var dic = new List<(string, string)>
            {
                ("app_id", req.AppId),
                ("redirect_domain", req.Domain),
                ("is_check_domain_ownership", "true"),
                ("miniprogram_domains_operate", "true"),
                ("_d2st", _weCombReq.GetD2st())
            };

            // wework_admin/apps/saveOpenApiApp?lang=zh_CN&f=json&ajax=1&timeZoneInfo%5Bzone_offset%5D=-8&random=0.6716191463354881
            var url = _weCombReq.GetQueryUrl("wework_admin/apps/saveOpenApiApp", new Dictionary<string, string>
            {
                { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "random", _weCombReq.GetRandom() }
            });
            var response = await _weCombReq.HttpWebRequestPostAsync(url, dic);
            if (!_weCombReq.IsResponseSucc(response)) return null;
            var model = JsonConvert.DeserializeObject<WeComBase<WeComSaveOpenApiApp>>(_weCombReq.GetResponseStr(response));
            return model.Data;
        }

        public async Task<List<string>> GetApiAccessibleApps(string businessId)
        {
            var url = _weCombReq.GetQueryUrl("wework_admin/apps/getApiAccessibleApps", new Dictionary<string, string>
                {
                    { "lang", "zh_CN" }, { "f", "json" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" },
                    { "random", _weCombReq.GetRandom() }, { "businessId", businessId }, { "_d2st", _weCombReq.GetD2st() }
                });
            var response = await _weCombReq.HttpWebRequestGetAsync(url);
            if (!_weCombReq.IsResponseSucc(response)) return null;
            var model = JsonConvert.DeserializeObject<WeComBase<WeComGetApiAccessibleApps>>(_weCombReq.GetResponseStr(response));
            return model.Data.Auths.AppIds;
        }

        public async Task<string> CreateTwoFactorAuthOpAsync(string appId)
        {
            var dic = new List<(string, string)>
            {
                ("appid", appId),
                ("business_type", "3"),
                ("app_type", "1"),
                ("_d2st", _weCombReq.GetD2st())
            };
            var url = _weCombReq.GetQueryUrl("wework_admin/two_factor_auth_operation/create", new Dictionary<string, string>
            {
                { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "random", _weCombReq.GetRandom() }
            });
            var response = await _weCombReq.HttpWebRequestPostAsync(url, dic);
            if (!_weCombReq.IsResponseSucc(response)) return null;
            var model = JsonConvert.DeserializeObject<WeComBase<WeComCreateTwoFactorAuthOp>>(_weCombReq.GetResponseStr(response));
            return model?.Data?.Key;
        }

        public async Task<int> QueryTwoFactorAuthOpAsync(string key)
        {
            var url = _weCombReq.GetQueryUrl("wework_admin/two_factor_auth_operation/query", new Dictionary<string, string>
            {
                { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "key",key },
                { "random", _weCombReq.GetRandom() }
            });
            var response = await _weCombReq.HttpWebRequestGetAsync(url);
            if (!_weCombReq.IsResponseSucc(response)) return 0;
            var model = JsonConvert.DeserializeObject<WeComBase<WeComQueryTwoFactorAuthOp>>(_weCombReq.GetResponseStr(response));
            return model?.Data?.Status ?? 0;
        }

        public async Task<WeComConfigCallback> ConfigContactCallbackAsync(ConfigCallbackRequest req)
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
               ("_d2st", _weCombReq.GetD2st())
            };
            var url = _weCombReq.GetQueryUrl("wework_admin/apps/saveOpenApiApp", new Dictionary<string, string>
            {
                { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "random", _weCombReq.GetRandom() }
            });
            var response = await _weCombReq.HttpWebRequestPostAsync(url, dic);
            if (!_weCombReq.IsResponseSucc(response)) return null;
            var model = JsonConvert.DeserializeObject<WeComConfigCallback>(_weCombReq.GetResponseStr(response));
            return model;
        }

        public async Task<WeComConfigCallback> ConfigExtContactCallbackAsync(ConfigCallbackRequest req)
        {
            var dic = new List<(string, string)>
            {
               ("callback_url", req.CallbackUrl),
               ("url_token", req.Token),
               ("callback_aeskey", req.AesKey),
               ("app_id", req.Appid),
               ("callback_host", req.HostUrl),
               ("_d2st", _weCombReq.GetD2st())
            };
            var url = _weCombReq.GetQueryUrl("wework_admin/apps/saveOpenApiApp", new Dictionary<string, string>
            {
                { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "random", _weCombReq.GetRandom() }
            });
            var response = await _weCombReq.HttpWebRequestPostAsync(url, dic);
            if (!_weCombReq.IsResponseSucc(response)) return null;
            var model = JsonConvert.DeserializeObject<WeComConfigCallback>(_weCombReq.GetResponseStr(response));
            return model;
        }

        public async Task<bool> SetApiAccessibleAppsAsync(SetApiAccessibleAppsRequest req)
        {
            // 客户联系Id
            var businessId = "2000003";
            var accessibleApps = (await GetApiAccessibleApps(businessId)) ?? new List<string>();
            req.AccessibleApps.AddRange(accessibleApps);
            var dic = new List<(string, string)>
            {
                ("businessId", businessId),
                ("_d2st", _weCombReq.GetD2st())
            };
            req.AccessibleApps = req.AccessibleApps.Distinct().ToList();
            foreach (var item in req.AccessibleApps)
            {
                dic.Add(("auth_list[appid_list][]", item));
            }

            // wework_admin/apps/saveOpenApiApp?lang=zh_CN&f=json&ajax=1&timeZoneInfo%5Bzone_offset%5D=-8&random=0.6716191463354881
            var url = _weCombReq.GetQueryUrl("wework_admin/apps/setApiAccessibleApps", new Dictionary<string, string>
            {
                { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "random", _weCombReq.GetRandom() }
            });
            var response = await _weCombReq.HttpWebRequestPostAsync(url, dic);
            if (!_weCombReq.IsResponseSucc(response)) return false;
            var model = JsonConvert.DeserializeObject<WeComBase<WeComSaveOpenApiApp>>(_weCombReq.GetResponseStr(response));
            return true;
        }

        public async Task<(string Name, byte[] File)> GetDomainVerifyFileAsync()
        {
            // 获取可新域名校验文件名
            var url = _weCombReq.GetQueryUrl("wework_admin/apps/getDomainOwnershipVerifyInfo", new Dictionary<string, string>
            {
                { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "random", _weCombReq.GetRandom() }, { "_d2st", _weCombReq.GetD2st() }
            });
            var response = await _weCombReq.HttpWebRequestGetAsync(url);
            if (!_weCombReq.IsResponseSucc(response)) return (null, null);
            var model = JsonConvert.DeserializeObject<WeComDomainVerifyFileName>(_weCombReq.GetResponseStr(response));

            // 地址写死： https://work.weixin.qq.com/wework_admin/apps/getDomainOwnershipVerifyInfo?action=download
            var file = await _weCombReq.HttpWebRequestDownloadsync("wework_admin/apps/getDomainOwnershipVerifyInfo?action=download");
            return (model.FileName, file);
        }

        public async Task<bool> CheckCustomAppURLAsync(string appid, string domian)
        {
            // wework_admin/apps/checkCustomAppURL?
            // lang=zh_CN&f=json&ajax=1&timeZoneInfo%5Bzone_offset%5D=-8&random=0.8351865053727201&url=devscrmh5.lianou.tech&appid=5629502294442019&type=redirect_domain&_d2st=a3652781
            var url = _weCombReq.GetQueryUrl("wework_admin/apps/checkCustomAppURL", new Dictionary<string, string>
            {
                { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "random", _weCombReq.GetRandom() },
                { "url", domian }, { "appid", appid }, { "type", "redirect_domain" }, { "_d2st", _weCombReq.GetD2st() }
            });
            var response = await _weCombReq.HttpWebRequestGetAsync(url);
            if (!_weCombReq.IsResponseSucc(response)) return false;
            var model = JsonConvert.DeserializeObject<WeComBase<WeComCheckCustomAppURLDomain>>(_weCombReq.GetResponseStr(response));
            var result = model.Data.Status;
            return result.Equals("PASS");
        }

        public async Task<bool> CheckXcxDomainStatusAsync(string domian)
        {
            var dic = new List<(string, string)>
            {
                ("xcxDomains[]", domian), ("_d2st", _weCombReq.GetD2st())
            };

            // https://work.weixin.qq.com/wework_admin/apps/checkXcxDomains?lang=zh_CN&f=json&ajax=1&timeZoneInfo%5Bzone_offset%5D=-8&random=0.9542216877814127
            var url = _weCombReq.GetQueryUrl("wework_admin/apps/checkXcxDomains", new Dictionary<string, string>
            {
                { "lang", "zh_CN" }, { "f", "json" }, { "ajax", "1" }, { "timeZoneInfo%5Bzone_offset%5D", "-8" }, { "random", _weCombReq.GetRandom() }
            });
            var response = await _weCombReq.HttpWebRequestPostAsync(url, dic);
            if (!_weCombReq.IsResponseSucc(response)) return false;
            var model = JsonConvert.DeserializeObject<WeComBase<WeComCheckXcxDomain>>(_weCombReq.GetResponseStr(response));
            var result = model.Data.Result.FirstOrDefault();
            if (result == null) return false;
            return result.Status;
        }
    }
}
