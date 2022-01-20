namespace WeComLoad.Open.Common;

public class LoginEvent : PubSubEvent<LoginEventModel>
{
}

public class LoginEventModel
{
    public bool IsOpen { get; set; }
}
