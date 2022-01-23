namespace WeComLoad.Open.Common.Extensions;

public static class MainViewDialogExt
{
    /// <summary>
    /// 发布消息
    /// </summary>
    /// <param name="eventAggregator"></param>
    /// <param name="model"></param>
    public static void Publish(this IEventAggregator eventAggregator, MainViewDialogEventModel model)
    {
        eventAggregator.GetEvent<MainViewDialogEvent>().Publish(model);
    }

    /// <summary>
    /// 订阅消息
    /// </summary>
    /// <param name="eventAggregator"></param>
    /// <param name="action"></param>
    public static void Subscribe(this IEventAggregator eventAggregator, Action<MainViewDialogEventModel> action)
    {
        eventAggregator.GetEvent<MainViewDialogEvent>().Subscribe(action);
    }
}

