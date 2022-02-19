using Prism.Services.Dialogs;
using WeComLoad.Open.Common.Utils;
using WeComLoad.Open.Services;
using WeComLoad.Open.Views.Settings;

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

        // 设置子页面
        containerRegistry.RegisterForNavigation<SkinView, SkinViewModel>();
        containerRegistry.RegisterForNavigation<CustomAppSettingView, CustomAppSettingViewModel>();

        // 注册企微接口请求
        containerRegistry.RegisterSingleton<IWeComOpen, WeComOpenFunc>();
        containerRegistry.RegisterScoped<IWeComOpenSvc, WeComOpenSvc>();

        // 注册代开发自建应用开发、上线配置信息（Scoped 为了实时配置信息）
        containerRegistry.RegisterScoped<CustAppSetting>(f =>
        {
            var config = JsonFileHelper.ReadJson<CustAppSetting>(JsonFileHelper.configPath);
            return config;
        });
    }

    protected override void ConfigureViewModelLocator()
    {
        ViewModelLocationProvider.Register<LoginView, LoginViewModel>();
        base.ConfigureViewModelLocator();
    }
}
