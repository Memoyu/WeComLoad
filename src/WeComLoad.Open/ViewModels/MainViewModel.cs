namespace WeComLoad.Open.ViewModels;

public class MainViewModel : BindableBase
{
    private readonly IRegionManager _regionManager;
    private IRegionNavigationJournal _regionNavigationJournal;

    private ObservableCollection<MenuBar> menuBars;
    public ObservableCollection<MenuBar> MenuBars
    {
        get { return menuBars; }
        set { menuBars = value; RaisePropertyChanged(); }
    }

    private string title = "WeCom Service Tool";
    public string Title
    {
        get { return title; }
        set { title = value; RaisePropertyChanged(); }
    }


    public DelegateCommand<MenuBar> NavigateCommand { get; private set; }

    public DelegateCommand GoBackCommand { get; private set; }
    public DelegateCommand GoForwardCommand { get; private set; }

    public MainViewModel(IRegionManager regionManager, IRegionNavigationJournal regionNavigationJournal)
    {
        _regionManager = regionManager;
        _regionNavigationJournal = regionNavigationJournal;
        CreateMenuBar();
        NavigateCommand = new DelegateCommand<MenuBar>(Navigate);
        GoBackCommand = new DelegateCommand(GoBack);
        GoForwardCommand = new DelegateCommand(GoForward);
    }

    void CreateMenuBar()
    {
        MenuBars = new ObservableCollection<MenuBar>();
        MenuBars.Add(new MenuBar
        {
            Icon = "Home",
            Title = "首页",
            NameSpace = "IndexView",
        });
        MenuBars.Add(new MenuBar
        {
            Icon = "Home",
            Title = "代开发自建应用管理",
            NameSpace = "CustomAppView",
        });
        MenuBars.Add(new MenuBar
        {
            Icon = "Cog",
            Title = "设置",
            NameSpace = "SettingsView",
        });
    }

    private void Navigate(MenuBar menuBar)
    {
        if (menuBar == null || string.IsNullOrWhiteSpace(menuBar.NameSpace)) return;
        _regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate(menuBar.NameSpace, back =>
        {
                // 添加到导航日志中
                _regionNavigationJournal = back.Context.NavigationService.Journal;
        });
        Title = menuBar.Title;
    }

    private void GoBack()
    {
        if (_regionNavigationJournal != null && _regionNavigationJournal.CanGoBack)
            _regionNavigationJournal.GoBack();
    }

    private void GoForward()
    {
        if (_regionNavigationJournal != null && _regionNavigationJournal.CanGoForward)
            _regionNavigationJournal.GoForward();
    }
}
