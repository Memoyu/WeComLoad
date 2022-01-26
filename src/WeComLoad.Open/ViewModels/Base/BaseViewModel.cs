namespace WeComLoad.Open.ViewModels.Base;

public class BaseViewModel : BindableBase
{
    public readonly IEventAggregator EventAggregator;
    public readonly IContainerProvider ContainerProvider;

    public BaseViewModel(IContainerProvider containerProvider)
    {
        EventAggregator = containerProvider.Resolve<IEventAggregator>();
        this.ContainerProvider = containerProvider;
    }
}
