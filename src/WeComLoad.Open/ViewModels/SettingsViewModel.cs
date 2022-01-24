using MaterialDesignThemes.Wpf;

namespace WeComLoad.Open.ViewModels;

public class SettingsViewModel : BaseNavigationViewModel
{
    private readonly IRegionManager _regionManager;

    private ObservableCollection<MenuBar> menuBars;
    public ObservableCollection<MenuBar> MenuBars
    {
        get { return menuBars; }
        set { menuBars = value; RaisePropertyChanged(); }
    }

    private SnackbarMessageQueue snackbarMessageQueue;

    public SnackbarMessageQueue SnackbarMessage
    {
        get { return snackbarMessageQueue; }
        set { snackbarMessageQueue = value; RaisePropertyChanged(); }
    }

    public DelegateCommand<MenuBar> NavigateCommand { get; private set; }

    public SettingsViewModel(IRegionManager regionManager, IContainerProvider containerProvider): base(containerProvider)
    {
        SnackbarMessage = new SnackbarMessageQueue();
        _regionManager = regionManager;
        NavigateCommand = new DelegateCommand<MenuBar>(Navigate);
        CreateMenuBar();
    }
    void CreateMenuBar()
    {
        MenuBars = new ObservableCollection<MenuBar>();

        MenuBars.Add(new MenuBar
        {
            Icon = "Cog",
            Title = "代开发自建应用配置",
            NameSpace = "CustomAppSettingView",
        });

        MenuBars.Add(new MenuBar
        {
            Icon = "Palette",
            Title = "个性化",
            NameSpace = "SkinView",
        });
    }

    private void Navigate(MenuBar menuBar)
    {
        if (menuBar == null || string.IsNullOrWhiteSpace(menuBar.NameSpace)) return;
        _regionManager.Regions[PrismManager.SettingsViewRegionName].RequestNavigate(menuBar.NameSpace);
    }
}