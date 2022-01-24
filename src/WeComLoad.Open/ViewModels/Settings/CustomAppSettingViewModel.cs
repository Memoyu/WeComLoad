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
            DevHomePage = GetRangStr(homePages, 0),
            TestHomePage = GetRangStr(homePages, 1),
            ProdHomePage = GetRangStr(homePages, 2),
            DevIp = GetRangStr(ips, 0),
            TestIp = GetRangStr(ips, 1),
            ProdIp = GetRangStr(ips, 2),
            DevDomain = GetRangStr(domains, 0),
            TestDomain = GetRangStr(domains, 1),
            ProdDomain = GetRangStr(domains, 2),
            DevCallback = GetRangStr(callbacks, 0),
            TestCallback = GetRangStr(callbacks, 1),
            ProdCallback = GetRangStr(callbacks, 2),
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
    }

    public string GetRangStr(string[] inputs, int index)
    {
        try
        {
            return inputs[index];
        }
        catch (Exception ex)
        {
            return string.Empty;
        }
    }
}
