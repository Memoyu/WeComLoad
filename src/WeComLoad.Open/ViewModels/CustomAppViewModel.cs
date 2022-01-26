using Microsoft.Extensions.Configuration;
using System.Windows.Controls;
using WeComLoad.Open.Common.Dto;
using static WeComLoad.Shared.Model.AuthCorpAppRequest;
using static WeComLoad.Shared.Model.WeComSuiteApp;
using static WeComLoad.Shared.Model.WeComSuiteAppAuth;

namespace WeComLoad.Open.ViewModels
{
    public class CustomAppViewModel : BaseNavigationViewModel
    {
        private AppSettings _appSettings;

        private readonly IWeComOpen _weComOpen;

        private ObservableCollection<SuiteAppItem> customApps;
        public ObservableCollection<SuiteAppItem> CustomApps
        {
            get { return customApps; }
            set { customApps = value; RaisePropertyChanged(); }
        }


        private ObservableCollection<Corpapp> customAppAuths;
        public ObservableCollection<Corpapp> CustomAppAuths
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

        private string corpId = string.Empty;
        public string CorpId
        {
            get { return corpId; }
            set { corpId = value; RaisePropertyChanged(); }
        }

        private string callbackUrl = string.Empty;
        public string CallbackUrl
        {
            get { return callbackUrl; }
            set { callbackUrl = value; RaisePropertyChanged(); }
        }

        private string ip = string.Empty;
        public string Ip
        {
            get { return ip; }
            set { ip = value; RaisePropertyChanged(); }
        }


        private string domain = string.Empty;
        public string Domain
        {
            get { return domain; }
            set { domain = value; RaisePropertyChanged(); }
        }

        private string homePage = string.Empty;
        public string HomePage
        {
            get { return homePage; }
            set { homePage = value; RaisePropertyChanged(); }
        }

        private string _currentSuiteId;
        private ListBoxItem _env;
        private CustomAppConfigDto _configDto;

        public DelegateCommand RefreshCustomAppListCommand { get; private set; }

        public DelegateCommand<Corpapp> AuditCustomAppCommand { get; private set; }

        public DelegateCommand<SuiteAppItem> SelectedCustomAppCommand { get; private set; }

        public DelegateCommand<ListBoxItem> SelectedEnvCommand { get; private set; }

        public DelegateCommand InputCorpIdCommand { get; private set; }

        public DelegateCommand AuditPublishAppCommand { get; private set; }

        public CustomAppViewModel(AppSettings appSettings, IContainerProvider containerProvider, IWeComOpen weComOpen) : base(containerProvider)
        {
            _appSettings = appSettings;
            customApps = new ObservableCollection<SuiteAppItem>();
            _weComOpen = weComOpen;
            RefreshCustomAppListCommand = new DelegateCommand(GetCustomAppListHandler);
            AuditCustomAppCommand = new DelegateCommand<Corpapp>(AuditCustomAppHandler);
            SelectedCustomAppCommand = new DelegateCommand<SuiteAppItem>(SelectedCustomAppHandler);
            SelectedEnvCommand = new DelegateCommand<ListBoxItem>(SelectedEnvHandler);
            InputCorpIdCommand = new DelegateCommand(InputCorpIdHandler);
            AuditPublishAppCommand = new DelegateCommand(AuditPublishAppHandler);
        }

        private void AuditPublishAppHandler()
        {
            authAppSet.corpapp.callbackurl = CallbackUrl;
            authAppSet.corpapp.redirect_domain = Domain;
            if (!string.IsNullOrWhiteSpace(ip))
                authAppSet.corpapp.white_ip_list.ip = new string[1] { Ip };
            authAppSet.corpapp.homepage = HomePage;
            authAppSet.corpapp.enter_homeurl_in_wx = true;
            authAppSet.corpapp.page_type = "CREATE";
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
                    CallbackUrl = string.Format(_configDto.DevCallback, CorpId);
                    Ip = _configDto.DevIp;
                    Domain = _configDto.DevDomain;
                    HomePage = _configDto.DevHomePage;
                    break;
                case "测试":
                    CallbackUrl = string.Format(_configDto.TestCallback, CorpId);
                    Ip = _configDto.TestIp;
                    Domain = _configDto.TestDomain;
                    HomePage = _configDto.TestHomePage;
                    break;
                case "正式":
                    CallbackUrl = string.Format(_configDto.ProdCallback, CorpId);
                    Ip = _configDto.ProdIp;
                    Domain = _configDto.ProdDomain;
                    HomePage = _configDto.ProdHomePage;
                    break;
            }
            _env = env;
        }

        private async void SelectedCustomAppHandler(SuiteAppItem app)
        {
            if (app == null) return;
            _currentSuiteId = app.suiteid.ToString();
            var authApps = await _weComOpen.GetCustomAppAuthsAsync(_currentSuiteId, 0, 20);
            if (authApps?.Data?.corpapp_list == null)
            {
                CustomAppAuths = new ObservableCollection<Corpapp>();
                return;
            }
            CustomAppAuths = new ObservableCollection<Corpapp>(authApps.Data.corpapp_list.corpapp.Where(a => a.customized_app_status == 0));
        }

        private async void GetCustomAppListHandler()
        {
            CustomAppAuths?.Clear();
            var apps = await _weComOpen.GetCustomAppsAsync();
            if (apps?.Data?.suite_list == null)
            {
                CustomApps = new ObservableCollection<SuiteAppItem>();
                return;
            }
            CustomApps = new ObservableCollection<SuiteAppItem>(apps.Data.suite_list.suite);
        }

        private async void AuditCustomAppHandler(Corpapp app)
        {
            // 获取配置
            var homePages = _appSettings.AuditApp.HomePage.Split(';');
            var ips = _appSettings.AuditApp.Ip.Split(';');
            var callbacks = _appSettings.AuditApp.Callback.Split(';');
            var domains = _appSettings.AuditApp.Domain.Split(';');
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
            authAppSet.corpapp = new CorpappReq().MapFrom(detail.Data.suite);
            authAppSet.corpapp.app_id = app.app_id;
        }
    }
}
