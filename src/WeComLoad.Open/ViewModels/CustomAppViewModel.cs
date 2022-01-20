using static WeComLoad.Shared.Model.WeComSuiteApp;

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

        public DelegateCommand RefreshCustomAppListCommand { get; private set; }

        public CustomAppViewModel(IContainerProvider containerProvider, IWeComOpen weComOpen) : base(containerProvider)
        {

            EventAggregator.GetEvent<LoginEvent>().Publish(new LoginEventModel { IsOpen = true });
            customApps = new ObservableCollection<SuiteAppItem>();
            _weComOpen = weComOpen;
            RefreshCustomAppListCommand = new DelegateCommand(GetCustomAppList);
        }

        private async void GetCustomAppList()
        {
            var apps = await _weComOpen.GetCustomAppsAsync();
            if (apps == null) return;
            CustomApps = new ObservableCollection<SuiteAppItem>(apps.Data.suite_list.suite);
        }
    }
}
