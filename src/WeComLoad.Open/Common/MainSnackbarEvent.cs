namespace WeComLoad.Open.Common;

public class MainSnackbarEvent : PubSubEvent<MainSnackbarEventModel>
{
}

public class MainSnackbarEventModel
{
    public string Msg { get; set; }
}
