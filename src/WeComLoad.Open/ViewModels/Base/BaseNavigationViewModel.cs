namespace WeComLoad.Open.ViewModels.Base;

public class BaseNavigationViewModel : BindableBase, INavigationAware
{
    private readonly IContainerProvider _containerProvider;
    public readonly IEventAggregator EventAggregator;
    public static readonly IWeComOpen WeComOpen = new WeComOpenFunc();



    public BaseNavigationViewModel(IContainerProvider containerProvider)
    {
        _containerProvider = containerProvider;
        EventAggregator = containerProvider.Resolve<IEventAggregator>();
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
        EventAggregator.Publish(new MainViewDialogEventModel
        {
            IsOpen = isOpen,
            DialogType = MainViewDialogEnum.Loadding,
            Content = hint,
        });
    }
}
