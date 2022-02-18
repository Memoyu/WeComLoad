using System.Windows.Controls;
using WeComLoad.Open.Services;
using static WeComLoad.Shared.Model.WeComSuiteAppAuthDetail;

namespace WeComLoad.Open.ViewModels
{
    public class CustomAppViewModel : BaseNavigationViewModel
    {
        private CustAppSetting _custAppSettings;

        private Suite currTpl;

        private readonly IWeComOpenSvc _weComOpenSvc;

        private ObservableCollection<Suite> customAppTpls;
        public ObservableCollection<Suite> CustomAppTpls
        {
            get { return customAppTpls; }
            set { customAppTpls = value; RaisePropertyChanged(); }
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

        private string[] _buckets = { "loweb-dev-scrmh5", "loweb-test-scrmh5", "loweb-prod-scrmh5" };
        private int _currOffset = 0;
        private int size = 10;
        private ListBoxItem _env;

        public DelegateCommand RefreshCustomTplsCommand { get; private set; }

        public DelegateCommand RefreshAuditAppListCommand { get; private set; }

        public DelegateCommand<CorpApp> AuthCustomAppCommand { get; private set; }

        public DelegateCommand<Suite> SelectedCustomAppTplCommand { get; private set; }

        public DelegateCommand<ListBoxItem> SelectedEnvCommand { get; private set; }

        public DelegateCommand InputCorpIdCommand { get; private set; }

        public DelegateCommand AuthOnlineAppCommand { get; private set; }

        public CustomAppViewModel(
            CustAppSetting custAppSetting,
            IContainerProvider containerProvider,
            IWeComOpenSvc weComOpenSvc) : base(containerProvider)
        {
            _custAppSettings = custAppSetting;
            _weComOpenSvc = weComOpenSvc;
            RefreshCustomTplsCommand = new DelegateCommand(GetCustAppTplAsync);
            RefreshAuditAppListCommand = new DelegateCommand(RefreshAuditAppListHandler);
            AuthCustomAppCommand = new DelegateCommand<CorpApp>(AuthCustomAppHandler);
            SelectedCustomAppTplCommand = new DelegateCommand<Suite>(SelectedCustAppTplHandler);
            SelectedEnvCommand = new DelegateCommand<ListBoxItem>(SelectedEnvHandler);
            InputCorpIdCommand = new DelegateCommand(InputCorpIdHandler);
            AuthOnlineAppCommand = new DelegateCommand(AuthAndOnlineAppHandler);

            // 获取模板列表
            GetCustAppTplAsync();
        }

        private async void GetCustAppTplAsync()
        {
            var data = await _weComOpenSvc.GetCustomAppTplsAsync();
            if (data?.suite_list == null)
            {
                CustomAppTpls = new ObservableCollection<Suite>();
                return;
            }
            CustomAppTpls = new ObservableCollection<Suite>(data.suite_list.suite);
        }

        private async void SelectedCustAppTplHandler(Suite tpl)
        {
            if (tpl == null) return;
            currTpl = tpl;
            CustomAppAuths = new ObservableCollection<CorpApp>();
            await GetCustAppAuthsAsync(tpl?.suiteid.ToString(), _currOffset);
        }

        private async Task GetCustAppAuthsAsync(string suiteid, int currOffset)
        {
            if (string.IsNullOrWhiteSpace(suiteid)) return;
            var authApps = await _weComOpenSvc.GetCustomAppAuthsAsync(suiteid, currOffset, size);
            var apps = new List<CorpApp>();

            if (authApps?.corpapp_list == null)
            {
                apps = new List<CorpApp>();
            }
            else
            {
                apps = authApps.corpapp_list.corpapp;
            }

            CustomAppAuths.AddRange(apps);

            //if (!authApps.has_next_page)
            //    initLoading = true;
            //else
            //    initLoading = false;
        }

        private void RefreshAuditAppListHandler()
        {
            SelectedCustAppTplHandler(currTpl);
        }

        private async void AuthAndOnlineAppHandler()
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

            try
            {
                var authRes = await _weComOpenSvc.AuthCustAppAndOnlineAsync(authAppReq, authConfig.VerifyBucket);
                if (!authRes)
                {
                    EventAggregator.PubMainSnackbar(new MainSnackbarEventModel
                    {
                        Msg = $"开发上线应用失败"
                    });
                    return;
                }

                EventAggregator.PubMainSnackbar(new MainSnackbarEventModel
                {
                    Msg = $"开发上线应用成功"
                });

                // 刷新企业应用列表
                RefreshAuditAppListHandler();
                // 关闭浮窗
                DialogIsOpen = false;
            }
            catch (Exception ex)
            {
                EventAggregator.PubMainSnackbar(new MainSnackbarEventModel
                {
                    Msg = $"开发上线应用异常，异常信息：{ex.Message}"
                });
                return;
            }
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
                    authConfig.CallbackUrl = _custAppSettings.Callback.Dev;
                    authConfig.CallbackUrlComplete = string.Format(authConfig.CallbackUrl, authConfig.CorpId);
                    authConfig.WhiteIp = _custAppSettings.WhiteIp.Dev;
                    authConfig.Domain = _custAppSettings.Domain.Dev;
                    authConfig.HomePage = _custAppSettings.HomePage.Dev;
                    authConfig.VerifyBucket = _buckets[0];
                    break;
                case "测试":
                    authConfig.CallbackUrl = _custAppSettings.Callback.Test;
                    authConfig.CallbackUrlComplete = string.Format(authConfig.CallbackUrl, authConfig.CorpId);
                    authConfig.WhiteIp = _custAppSettings.WhiteIp.Test;
                    authConfig.Domain = _custAppSettings.Domain.Test;
                    authConfig.HomePage = _custAppSettings.HomePage.Test;
                    authConfig.VerifyBucket = _buckets[1];
                    break;
                case "正式":
                    authConfig.CallbackUrl = _custAppSettings.Callback.Prod;
                    authConfig.CallbackUrlComplete = string.Format(authConfig.CallbackUrl, authConfig.CorpId);
                    authConfig.WhiteIp = _custAppSettings.WhiteIp.Prod;
                    authConfig.Domain = _custAppSettings.Domain.Prod;
                    authConfig.HomePage = _custAppSettings.HomePage.Prod;
                    authConfig.VerifyBucket = _buckets[2];
                    break;
            }
            _env = env;
        }

        private async void AuthCustomAppHandler(CorpApp app)
        {
            if (app.customized_app_status == 0)
            {
                authConfig = new AuditConfig
                {
                    CorpId = string.Empty,
                    CallbackUrl = _custAppSettings.Callback.Dev,
                    CallbackUrlComplete = _custAppSettings.Callback.Dev,
                    Domain = _custAppSettings.Domain.Dev,
                    HomePage = _custAppSettings.HomePage.Dev,
                    WhiteIp = _custAppSettings.WhiteIp.Dev,
                    VerifyBucket = _buckets[0],
                };

                CorpName = app.authcorp_name;

                // 展开浮窗
                DialogIsOpen = true;
            }
            else if (app.customized_app_status == 1)
            {

                if (app.auditorder == null)
                {
                    await SubmitAuditAndOnlineAsync(app);
                }
                else if (app.auditorder.status == 2 || app.auditorder.status == 4)
                {
                    await OnlineAppAsync(app);
                }
            }
        }

        /// <summary>
        /// 提交审核上线自建应用
        /// </summary>
        /// <param name="app">审核应用</param>
        /// <returns></returns>
        private async Task SubmitAuditAndOnlineAsync(CorpApp app)
        {
            try
            {
                // 提交审核
                var resSubmitAudit = await _weComOpenSvc.SubmitAuditCorpAppAsync(new SubmitAuditCorpAppRequest(app.app_id, currTpl.suiteid.ToString()));
                if (resSubmitAudit.Flag)
                {
                    EventAggregator.PubMainSnackbar(new MainSnackbarEventModel
                    {
                        Msg = "审核应用失败！"
                    });
                    return;
                }

                var auditOrderId = resSubmitAudit.Result?.auditorder?.auditorderid;

                // 上线应用
                var resOnline = await _weComOpenSvc.OnlineCorpAppAsync(new OnlineCorpAppRequest(auditOrderId));
                if (resOnline.Flag)
                {
                    EventAggregator.PubMainSnackbar(new MainSnackbarEventModel
                    {
                        Msg = "上线应用失败！"
                    });
                    return;
                }

                RefreshAuditAppListHandler();
                EventAggregator.PubMainSnackbar(new MainSnackbarEventModel
                {
                    Msg = "审核上线应用成功！"
                });
            }
            catch (Exception ex)
            {
                EventAggregator.PubMainSnackbar(new MainSnackbarEventModel
                {
                    Msg = $"审核上线应用异常，异常信息：{ex.Message}"
                });
                return;
            }
        }


        /// <summary>
        /// 上线自建应用
        /// </summary>
        /// <param name="app">审核应用</param>
        /// <returns></returns>
        private async Task OnlineAppAsync(CorpApp app)
        {
            try
            {
                // 上线应用
                var resOnline = await _weComOpenSvc.OnlineCorpAppAsync(new OnlineCorpAppRequest(app.auditorder.auditorderid));
                if (resOnline.Flag)
                {
                    EventAggregator.PubMainSnackbar(new MainSnackbarEventModel
                    {
                        Msg = "上线应用失败！"
                    });
                    return;
                }

                RefreshAuditAppListHandler();
                EventAggregator.PubMainSnackbar(new MainSnackbarEventModel
                {
                    Msg = "上线应用成功！"
                });
            }
            catch (Exception ex)
            {
                EventAggregator.PubMainSnackbar(new MainSnackbarEventModel
                {
                    Msg = $"上线应用异常，异常信息：{ex.Message}"
                });
                return;
            }
        }
    }
}
