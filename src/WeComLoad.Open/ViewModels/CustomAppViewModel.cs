using Microsoft.Extensions.Configuration;
using static WeComLoad.Shared.Model.WeComSuiteApp;
using static WeComLoad.Shared.Model.WeComSuiteAppAuth;

namespace WeComLoad.Open.ViewModels
{
    public class CustomAppViewModel : BaseNavigationViewModel
    {
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

        /// <summary>
        /// 浮窗是否显示
        /// </summary>
        public bool DialogIsOpen
        {
            get { return isOpen; }
            set { isOpen = value; RaisePropertyChanged(); }
        }

        public DelegateCommand RefreshCustomAppListCommand { get; private set; }

        public DelegateCommand<Corpapp> AuditCustomAppCommand { get; private set; }

        public DelegateCommand<SuiteAppItem> SelectedCustomAppCommand { get; private set; }

        public CustomAppViewModel(IContainerProvider containerProvider, IWeComOpen weComOpen) : base(containerProvider)
        {
            customApps = new ObservableCollection<SuiteAppItem>();
            _weComOpen = weComOpen;
            RefreshCustomAppListCommand = new DelegateCommand(GetCustomAppListHandler);
            AuditCustomAppCommand = new DelegateCommand<Corpapp>(AuditCustomAppHandler);
            SelectedCustomAppCommand = new DelegateCommand<SuiteAppItem>(SelectedCustomAppHandler);
        }

        private async void SelectedCustomAppHandler(SuiteAppItem app)
        {
            if (app == null) return;
            var authApps = await _weComOpen.GetCustomAppAuthsAsync(app.suiteid.ToString(), 0, 20);
            if (authApps?.Data?.corpapp_list == null)
            {
                CustomAppAuths = new ObservableCollection<Corpapp>();
                return;
            }
            CustomAppAuths = new ObservableCollection<Corpapp>(authApps.Data.corpapp_list.corpapp);
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
            DialogIsOpen = true;
        }
    }
}
