namespace WeComLoad.Open.Common;

public class MainDialogEvent : PubSubEvent<MainDialogEventModel>
{
}

public class MainDialogEventModel
{
    public bool IsOpen { get; set; }

    public MainDialogEnum DialogType { get; set; }

    public string Content { get; set; }
}

public enum MainDialogEnum
{
    Login = 1,
    Loadding = 2
}