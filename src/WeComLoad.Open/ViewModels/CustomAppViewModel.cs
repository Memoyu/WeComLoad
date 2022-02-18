using LianOu.FileLib;
using System.Windows.Controls;
using WeComLoad.Open.Common.Dto;
using static WeComLoad.Shared.Model.AuthCorpAppRequest;
using static WeComLoad.Shared.Model.WeComSuiteAppAuthDetail;

namespace WeComLoad.Open.ViewModels
{
    public class CustomAppViewModel : BaseNavigationViewModel
    {
        private CustAppSetting _custAppSettings;

        private Suite currTpl;

        private readonly IWeComOpen _weComOpen;

        private ObservableCollection<Suite> customApps;
        public ObservableCollection<Suite> CustomApps
        {
            get { return customApps; }
            set { customApps = value; RaisePropertyChanged(); }
        }


        private ObservableCollection<CorpApp> customAppAuths;
        public ObservableCollection<CorpApp> CustomAppAuths
        {
            get { return customAppAuths; }
            set { customAppAuths = value; RaisePropertyChanged(); }
        }

        private bool isOpen;
        public bool DialogIsOpen
        {
            get { return isOpen; }
            set { isOpen = value; RaisePropertyChanged(); }
        }

        private AuthCorpAppRequest authAppSet;
        public AuthCorpAppRequest AuthAppSet
        {
            get { return authAppSet; }
            set { authAppSet = value; RaisePropertyChanged(); }
        }

        private string corpName = string.Empty;
        public string CorpName
        {
            get { return corpName; }
            set { corpName = value; RaisePropertyChanged(); }
        }

        private string corpId = string.Empty;
        public string CorpId
        {
            get { return corpId; }
            set { corpId = value; RaisePropertyChanged(); }
        }

        private AuditConfig authConfig;
        public AuditConfig AuthConfig
        {
            get { return authConfig; }
            set { authConfig = value; RaisePropertyChanged(); }
        }

        private string _currentSuiteId;
        private string[] buckets = { "loweb-dev-scrmh5", "loweb-test-scrmh5", "loweb-prod-scrmh5" };
        private ListBoxItem _env;
        private CustomAppConfigDto _configDto;
        private readonly IFileClientPro _fileClientPro;

        public DelegateCommand RefreshCustomAppListCommand { get; private set; }

        public DelegateCommand RefreshAuditAppListCommand { get; private set; }

        public DelegateCommand<CorpApp> AuditCustomAppCommand { get; private set; }

        public DelegateCommand<Suite> SelectedCustomAppCommand { get; private set; }

        public DelegateCommand<ListBoxItem> SelectedEnvCommand { get; private set; }

        public DelegateCommand InputCorpIdCommand { get; private set; }

        public DelegateCommand AuditPublishAppCommand { get; private set; }

        public CustomAppViewModel(
            CustAppSetting custAppSetting,
            IContainerProvider containerProvider,
            IWeComOpen weComOpen,
            IFileClientPro fileClientPro) : base(containerProvider)
        {
            _custAppSettings = custAppSetting;
            customApps = new ObservableCollection<Suite>();
            _weComOpen = weComOpen;
            _fileClientPro = fileClientPro;
            RefreshCustomAppListCommand = new DelegateCommand(GetCustomAppListHandler);
            RefreshAuditAppListCommand = new DelegateCommand(RefreshAuditAppListHandler);
            AuditCustomAppCommand = new DelegateCommand<CorpApp>(AuditCustomAppHandler);
            SelectedCustomAppCommand = new DelegateCommand<Suite>(SelectedCustAppTplHandler);
            SelectedEnvCommand = new DelegateCommand<ListBoxItem>(SelectedEnvHandler);
            InputCorpIdCommand = new DelegateCommand(InputCorpIdHandler);
            AuditPublishAppCommand = new DelegateCommand(AuditPublishAppHandler);
        }

        private async void AuditPublishAppHandler()
        {
            var hint = string.Empty;
            if (string.IsNullOrWhiteSpace(CorpId))
                hint = "请输入企业ID";

            if (string.IsNullOrWhiteSpace(authConfig.Domain))
                hint = "请输入可信域名";

            if (string.IsNullOrWhiteSpace(authConfig.CallbackUrlComplete))
                hint = "请输入回调地址";

            if (!string.IsNullOrWhiteSpace(hint))
            {
                EventAggregator.PubMainSnackbar(new MainSnackbarEventModel
                {
                    Msg = hint
                });
                return;
            }


            var authAppReq = new AuthCorpAppRequest();
            authAppReq.suiteid = currTpl.suiteid.ToString();
            authAppReq.corpapp = new AuthCorpAppRequest.CorpappReq().MapFrom(currTpl);
            authAppReq.corpapp.app_id = authConfig.AppId;
            authAppReq.corpapp.callbackurl = authConfig.CallbackUrlComplete;
            authAppReq.corpapp.redirect_domain = authConfig.Domain;
            if (!string.IsNullOrWhiteSpace(authConfig.WhiteIp))
                authAppReq.corpapp.white_ip_list.ip = new string[1] { authConfig.WhiteIp };
            authAppReq.corpapp.homepage = authConfig.HomePage;
            authAppReq.corpapp.enter_homeurl_in_wx = true;
            authAppReq.corpapp.page_type = "CREATE";

            // 审核应用
            var resAudit = await _weComOpen.AuthCorpAppAsync(authAppSet);
            var appId = resAudit?.corpapp?.app_id;
            var suiteId = authAppSet.suiteid;
            if (string.IsNullOrWhiteSpace(appId))
            {
                EventAggregator.PubMainSnackbar(new MainSnackbarEventModel
                {
                    Msg = "审核自建应用异常！"
                });
                return;
            }

            // 下载可信域名校验文件
            var resFile = await UploadDomainVerify(suiteId, appId);
            if (!resFile)
            {
                EventAggregator.PubMainSnackbar(new MainSnackbarEventModel
                {
                    Msg = "上传应用可信域名校验文件异常！"
                });
                return;
            }

            var msg = "开发并上线成功";

            // 提交审核
            var auditOrderId = await SubmitAuditAppAsync(appId, suiteId);
            if (string.IsNullOrWhiteSpace(auditOrderId))
            {
                msg = "审核应用失败";
            }
            else
            {
                // 上线应用
                if (!await OnlineAppAsync(auditOrderId))
                    msg = "上线应用失败";
            }

            // 刷新企业应用列表
            RefreshAuditAppListHandler();

            // 关闭浮窗
            DialogIsOpen = false;

            EventAggregator.PubMainSnackbar(new MainSnackbarEventModel
            {
                Msg = msg
            });

        }

        private void InputCorpIdHandler()
        {
            SelectedEnvHandler(_env);
        }

        private void SelectedEnvHandler(ListBoxItem env)
        {
            if (env is null)
                env = new ListBoxItem { Content = "开发" };

            switch (env.Content)
            {
                case "开发":
                    authConfig.CallbackUrl = Settings.Callback.Dev;
                    authConfig.CallbackUrlComplete = string.Format(authConfig.CallbackUrl, authConfig.CorpId);
                    authConfig.WhiteIp = Settings.WhiteIp.Dev;
                    authConfig.Domain = Settings.Domain.Dev;
                    authConfig.HomePage = Settings.HomePage.Dev;
                    authConfig.VerifyBucket = buckets[0];
                    break;
                case "测试":
                    authConfig.CallbackUrl = Settings.Callback.Test;
                    authConfig.CallbackUrlComplete = string.Format(authConfig.CallbackUrl, authConfig.CorpId);
                    authConfig.WhiteIp = Settings.WhiteIp.Test;
                    authConfig.Domain = Settings.Domain.Test;
                    authConfig.HomePage = Settings.HomePage.Test;
                    authConfig.VerifyBucket = buckets[1];
                    break;
                case "正式":
                    authConfig.CallbackUrl = Settings.Callback.Prod;
                    authConfig.CallbackUrlComplete = string.Format(authConfig.CallbackUrl, authConfig.CorpId);
                    authConfig.WhiteIp = Settings.WhiteIp.Prod;
                    authConfig.Domain = Settings.Domain.Prod;
                    authConfig.HomePage = Settings.HomePage.Prod;
                    authConfig.VerifyBucket = buckets[2];
                    break;
            }
            _env = env;
        }

        private async void SelectedCustAppTplHandler(Suite tpl)
        {
            if (tpl == null) return;
            currTpl = tpl;
            _currentSuiteId = tpl.suiteid.ToString();
            var authApps = await _weComOpen.GetCustomAppAuthsAsync(_currentSuiteId, 0, 20);
            if (authApps?.Data?.corpapp_list == null)
            {
                CustomAppAuths = new ObservableCollection<CorpApp>();
                return;
            }
            CustomAppAuths = new ObservableCollection<CorpApp>(authApps.Data.corpapp_list.corpapp.Where(a => a.customized_app_status != 2 || (a.auditorder != null && a.auditorder.status != 5)));
        }

        private async void GetCustomAppListHandler()
        {
            CustomAppAuths?.Clear();
            var apps = await _weComOpen.GetCustomAppTplsAsync();
            if (apps?.Data?.suite_list == null)
            {
                CustomApps = new ObservableCollection<Suite>();
                return;
            }
            CustomApps = new ObservableCollection<Suite>(apps.Data.suite_list.suite);
        }

        private void RefreshAuditAppListHandler()
        {
            SelectedCustomAppHandler(new SuiteAppItem { suiteid = int.Parse(_currentSuiteId) });
        }

        private async void AuditCustomAppHandler(CorpApp app)
        {
            if (app.customized_app_status == 0)
            {
                // 获取配置
                var homePages = _custAppSettings.AuditApp.HomePage.Split(';');
                var ips = _custAppSettings.AuditApp.Ip.Split(';');
                var callbacks = _custAppSettings.AuditApp.Callback.Split(';');
                var domains = _custAppSettings.AuditApp.Domain.Split(';');
                _configDto = new CustomAppConfigDto
                {
                    DevHomePage = homePages.GetRangStr(0),
                    TestHomePage = homePages.GetRangStr(1),
                    ProdHomePage = homePages.GetRangStr(2),
                    DevIp = ips.GetRangStr(0),
                    TestIp = ips.GetRangStr(1),
                    ProdIp = ips.GetRangStr(2),
                    DevDomain = domains.GetRangStr(0),
                    TestDomain = domains.GetRangStr(1),
                    ProdDomain = domains.GetRangStr(2),
                    DevCallback = callbacks.GetRangStr(0),
                    TestCallback = callbacks.GetRangStr(1),
                    ProdCallback = callbacks.GetRangStr(2),
                };

                // 展开浮窗
                DialogIsOpen = true;
                SelectedEnvHandler(_env);
                var detail = await _weComOpen.GetCustomAppAuthDetailAsync(_currentSuiteId);
                if (detail?.Data?.suite == null)
                {
                    EventAggregator.PubMainSnackbar(new MainSnackbarEventModel
                    {
                        Msg = "获取模板详情失败"
                    });
                    DialogIsOpen = false;
                    return;
                }
                authAppSet = new AuthCorpAppRequest();
                authAppSet.suiteid = _currentSuiteId;
                authAppSet.corpapp = new CorpappReq().MapFrom(detail.Data.suite);
                authAppSet.corpapp.app_id = app.app_id;
                CorpName = app.authcorp_name;
                CorpId = string.Empty;
            }
            else if (app.customized_app_status == 1)
            {

                if (app.auditorder == null)
                {
                    var msg = "审核并上线成功";

                    // 提交审核
                    var auditOrderId = await SubmitAuditAppAsync(app.app_id, _currentSuiteId);
                    if (string.IsNullOrWhiteSpace(auditOrderId))
                    {
                        msg = "审核应用失败";
                    }
                    else
                    {
                        // 上线应用
                        if (!await OnlineAppAsync(auditOrderId))
                            msg = "上线应用失败";
                    }

                    // 刷新企业应用列表
                    RefreshAuditAppListHandler();

                    EventAggregator.PubMainSnackbar(new MainSnackbarEventModel
                    {
                        Msg = msg
                    });
                }
                else if (app.auditorder.status == 2 || app.auditorder.status == 4)
                {
                    var msg = "上线应用成功";

                    // 上线应用
                    if (!await OnlineAppAsync(app.auditorder.auditorderid))
                        msg = "上线应用失败";

                    // 刷新企业应用列表
                    RefreshAuditAppListHandler();

                    EventAggregator.PubMainSnackbar(new MainSnackbarEventModel
                    {
                        Msg = msg
                    });
                }

            }
        }

        /// <summary>
        /// 提交审核自建应用
        /// </summary>
        /// <param name="corpAppId">企业AppId</param>
        /// <param name="suiteId">自建应用Id</param>
        /// <returns></returns>
        private async Task<string> SubmitAuditAppAsync(string corpAppId, string suiteId)
        {
            var resSubmit = await _weComOpen.SubmitAuditCorpAppAsync(new SubmitAuditCorpAppRequest(corpAppId, suiteId));
            if (string.IsNullOrWhiteSpace(resSubmit?.auditorder?.auditorderid))
            {
                EventAggregator.PubMainSnackbar(new MainSnackbarEventModel
                {
                    Msg = "提交审核自建应用异常！"
                });
                return string.Empty;
            }
            return resSubmit.auditorder.auditorderid;
        }


        /// <summary>
        /// 上线自建应用
        /// </summary>
        /// <param name="auditorderId">审核Id</param>
        /// <returns></returns>
        private async Task<bool> OnlineAppAsync(string auditorderId)
        {
            var resOnline = await _weComOpen.OnlineCorpAppAsync(new OnlineCorpAppRequest(auditorderId));
            if (string.IsNullOrWhiteSpace(resOnline?.auditorder?.auditorderid) || resOnline.auditorder.status != 5)
            {
                EventAggregator.PubMainSnackbar(new MainSnackbarEventModel
                {
                    Msg = "上线自建应用异常！"
                });
                return false;
            }
            return true;
        }

        private async Task<bool> UploadDomainVerify(string suiteId, string appId)
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
                var (uploadFlag, uploadMsg) = await _fileClientPro.UploadToRootPathAsync(file, _verifyBucket, fileName, OSSType.Aliyun);
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
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
