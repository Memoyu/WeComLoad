namespace WeComLoad.Open.Common;

public class LoadingEvent : PubSubEvent<LoadingEventModel>
{
}

public class LoadingEventModel
{
    public bool IsOpen { get; set; }

    public string Hint { get; set; }
}