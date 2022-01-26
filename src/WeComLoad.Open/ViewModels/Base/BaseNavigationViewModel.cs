namespace WeComLoad.Open.ViewModels.Base;

public class BaseNavigationViewModel : BaseViewModel, INavigationAware
{
    public static readonly IWeComOpen WeComOpen = new WeComOpenFunc();

    public BaseNavigationViewModel(IContainerProvider containerProvider): base(containerProvider)
    {
    }

    public virtual bool IsNavigationTarget(NavigationContext navigationContext)
    {
        return true;
    }

    public virtual void OnNavigatedFrom(NavigationContext navigationContext)
    {

    }

    public virtual void OnNavigatedTo(NavigationContext navigationContext)
    {

    }

    public void Loading(bool isOpen, string hint = "加载中...")
    {
        EventAggregator.PubMainDialog(new MainDialogEventModel
        {
            IsOpen = isOpen,
            DialogType = MainDialogEnum.Loadding,
            Content = hint,
        });
    }
}
