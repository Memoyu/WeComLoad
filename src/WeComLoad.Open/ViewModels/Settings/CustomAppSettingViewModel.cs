using WeComLoad.Open.Common.Dto;
using WeComLoad.Open.Common.Utils;

namespace WeComLoad.Open.ViewModels;

public class CustomAppSettingViewModel : BaseViewModel
{
    private AppSettings _appSettings;

    private CustomAppConfigDto config;

    public CustomAppConfigDto Config
    {
        get { return config; }
        set { config = value; RaisePropertyChanged(); }
    }

    public DelegateCommand SaveConfigCommand { get; }


    public CustomAppSettingViewModel(AppSettings appSettings, IContainerProvider containerProvider) : base(containerProvider)
    {
        var homePages = appSettings.AuditApp.HomePage.Split(';');
        var ips = appSettings.AuditApp.Ip.Split(';');
        var callbacks = appSettings.AuditApp.Callback.Split(';');
        var domains = appSettings.AuditApp.Domain.Split(';');
        Config = new CustomAppConfigDto
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

        SaveConfigCommand = new DelegateCommand(SaveConfigHandler);
        _appSettings = appSettings;
    }

    private void SaveConfigHandler()
    {
        _appSettings.AuditApp.HomePage = $"{config.DevHomePage};{config.TestHomePage};{config.ProdHomePage}";
        _appSettings.AuditApp.Domain = $"{config.DevDomain};{config.TestDomain};{config.ProdDomain}";
        _appSettings.AuditApp.Ip = $"{config.DevIp};{config.TestIp};{config.ProdIp}";
        _appSettings.AuditApp.Callback = $"{config.DevCallback};{config.TestCallback};{config.ProdCallback}";

        JsonFileHelper.WriteJson(JsonFileHelper.configPath, _appSettings);
        EventAggregator.PubMainSnackbar(new MainSnackbarEventModel
        {
            Msg = "保存成功"
        });
    }
}
