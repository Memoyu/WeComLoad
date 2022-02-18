using WeComLoad.Open.Common.Utils;

namespace WeComLoad.Open.ViewModels;

public class CustomAppSettingViewModel : BaseViewModel
{
    private CustAppSetting custAppSettings;
    public CustAppSetting CustAppSettings
    {
        get { return custAppSettings; }
        set { custAppSettings = value; RaisePropertyChanged(); }
    }

    public DelegateCommand SaveConfigCommand { get; }

    public CustomAppSettingViewModel(CustAppSetting custAppSettings, IContainerProvider containerProvider) : base(containerProvider)
    {
        SaveConfigCommand = new DelegateCommand(SaveConfigHandler);
        CustAppSettings = custAppSettings;
    }

    private void SaveConfigHandler()
    {
        JsonFileHelper.WriteJson(JsonFileHelper.configPath, custAppSettings);
        EventAggregator.PubMainSnackbar(new MainSnackbarEventModel
        {
            Msg = "保存成功"
        });
    }
}
