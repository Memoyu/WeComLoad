namespace WeComLoad.Open.ViewModels.Base;

public class BaseViewModel : BindableBase
{
    protected IEventAggregator aggregator;
    private readonly IContainerProvider containerProvider;

    public BaseViewModel(IContainerProvider containerProvider)
    {
        aggregator = containerProvider.Resolve<IEventAggregator>();
        this.containerProvider = containerProvider;
    }
}
