using Prism.Services.Dialogs;

namespace WeComLoad.Open;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : PrismApplication
{
    protected override Window CreateShell()
    {
        return Container.Resolve<MainView>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // 主页面
        containerRegistry.RegisterForNavigation<IndexView, IndexViewModel>();
        containerRegistry.RegisterForNavigation<CustomAppView, CustomAppViewModel>();
        containerRegistry.RegisterForNavigation<SettingsView, SettingsViewModel>();

        containerRegistry.RegisterSingleton<IWeComOpen, WeComOpenFunc>();
    }

    protected override void ConfigureViewModelLocator()
    {

        ViewModelLocationProvider.Register(typeof(LoginView).ToString(), typeof(LoginViewModel));
        base.ConfigureViewModelLocator();
    }
}
