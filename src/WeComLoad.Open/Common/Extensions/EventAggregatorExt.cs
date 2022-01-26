namespace WeComLoad.Open.Common.Extensions;

public static class EventAggregatorExt
{
    /// <summary>
    /// 发布消息主窗口Dialog
    /// </summary>
    /// <param name="eventAggregator"></param>
    /// <param name="model"></param>
    public static void PubMainDialog(this IEventAggregator eventAggregator, MainDialogEventModel model)
    {
        eventAggregator.GetEvent<MainDialogEvent>().Publish(model);
    }

    /// <summary>
    /// 订阅消息主窗口Dialog
    /// </summary>
    /// <param name="eventAggregator"></param>
    /// <param name="action"></param>
    public static void SubMainDialog(this IEventAggregator eventAggregator, Action<MainDialogEventModel> action)
    {
        eventAggregator.GetEvent<MainDialogEvent>().Subscribe(action);
    }

    /// <summary>
    /// 发布消息主窗口Snackbar
    /// </summary>
    /// <param name="eventAggregator"></param>
    /// <param name="model"></param>
    public static void PubMainSnackbar(this IEventAggregator eventAggregator, MainSnackbarEventModel model)
    {
        eventAggregator.GetEvent<MainSnackbarEvent>().Publish(model);
    }

    /// <summary>
    /// 订阅消息主窗口Snackbar
    /// </summary>
    /// <param name="eventAggregator"></param>
    /// <param name="action"></param>
    public static void SubMainSnackbar(this IEventAggregator eventAggregator, Action<MainSnackbarEventModel> action)
    {
        eventAggregator.GetEvent<MainSnackbarEvent>().Subscribe(action);
    }
}

