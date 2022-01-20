namespace WeComLoad.Open.Common.Extensions;

public static class LoadingDialogExt
{
    /// <summary>
    /// 发布消息
    /// </summary>
    /// <param name="eventAggregator"></param>
    /// <param name="model"></param>
    public static void Publish(this IEventAggregator eventAggregator, LoadingEventModel model)
    {
        eventAggregator.GetEvent<LoadingEvent>().Publish(model);
    }

    /// <summary>
    /// 订阅消息
    /// </summary>
    /// <param name="eventAggregator"></param>
    /// <param name="action"></param>
    public static void Subscribe(this IEventAggregator eventAggregator, Action<LoadingEventModel> action)
    {
        eventAggregator.GetEvent<LoadingEvent>().Subscribe(action);
    }
}

