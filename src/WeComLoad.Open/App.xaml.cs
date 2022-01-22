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
                eventAggregator.GetEvent<LoginEvent>().Publish(new LoginEventModel { IsOpen = true });
            });
            return func;
        });

        containerRegistry.RegisterDialog<LoginView, LoginViewModel>();
    }

    protected override void ConfigureViewModelLocator()
    {
        base.ConfigureViewModelLocator();
    }
}
