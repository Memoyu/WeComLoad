namespace WeComLoad.Open.Common;

public class MainViewDialogEvent : PubSubEvent<MainViewDialogEventModel>
{
}

public class MainViewDialogEventModel
{
    public bool IsOpen { get; set; }

    public MainViewDialogEnum DialogType { get; set; }

    public string Content { get; set; }
}

public enum MainViewDialogEnum
{
    Login = 1,
    Loadding = 2
}