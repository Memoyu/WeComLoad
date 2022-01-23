using Prism.Services.Dialogs;
using System.Windows.Navigation;

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
        containerRegistry.Register<IDialogService, DialogServices>();
        containerRegistry.Register<IDialogWindows, Controls.DialogWindow>();

        // 主页面
        containerRegistry.RegisterForNavigation<IndexView, IndexViewModel>();
        containerRegistry.RegisterForNavigation<CustomAppView, CustomAppViewModel>();
        containerRegistry.RegisterForNavigation<SettingsView, SettingsViewModel>();

        containerRegistry.RegisterSingleton<IWeComOpen>(f =>
        {
            var func = new WeComOpenFunc();
            var eventAggregator = Container.Resolve<IEventAggregator>();
            func.GetWeCombReq().SetUnAuthEvent(() =>
            {
                eventAggregator.GetEvent<MainViewDialogEvent>().Publish(new MainViewDialogEventModel { IsOpen = true , DialogType = MainViewDialogEnum.Login });
            });
            return func;
        });
    }

    protected override void ConfigureViewModelLocator()
    {
        ViewModelLocationProvider.Register<LoginView, LoginViewModel>();
        base.ConfigureViewModelLocator();
    }
}
