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

        public DelegateCommand RefreshCustomAppListCommand { get; private set; }

        public DelegateCommand<SuiteAppItem> SelectedCustomAppCommand { get; private set; }

        public CustomAppViewModel(IContainerProvider containerProvider, IWeComOpen weComOpen) : base(containerProvider)
        {

            EventAggregator.GetEvent<LoginEvent>().Publish(new LoginEventModel { IsOpen = true });
            customApps = new ObservableCollection<SuiteAppItem>();
            _weComOpen = weComOpen;
            RefreshCustomAppListCommand = new DelegateCommand(GetCustomAppListHandler);
            SelectedCustomAppCommand = new DelegateCommand<SuiteAppItem>(SelectedCustomAppHandler);
        }

        private async void SelectedCustomAppHandler(SuiteAppItem app)
        {
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
            var apps = await _weComOpen.GetCustomAppsAsync();
            if (apps?.Data?.suite_list == null)
            {
                CustomApps = new ObservableCollection<SuiteAppItem>();
                return;
            }
            CustomApps = new ObservableCollection<SuiteAppItem>(apps.Data.suite_list.suite);
        }
    }
}
