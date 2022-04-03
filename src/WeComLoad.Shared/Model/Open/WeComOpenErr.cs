namespace WeComLoad.Shared.Model;

public class WeComOpenErr
{
    public int statusCode { get; set; }

    public string method { get; set; }

    public Result result { get; set; }
}


public class Result
{
    public int errCode { get; set; }

    public string humanMessage { get; set; }
}


