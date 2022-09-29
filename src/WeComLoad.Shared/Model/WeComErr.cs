namespace WeComLoad.Shared.Model;

public class WeComErr
{
    public Result result { get; set; }

    public class Result
    {
        public long errCode { get; set; }
        public string message { get; set; }
        public string humanMessage { get; set; }
        public string etype { get; set; }
    }

}

