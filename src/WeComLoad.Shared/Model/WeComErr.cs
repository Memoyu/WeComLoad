namespace WeComLoad.Shared.Model;

public class WeComErr
{
    public Result result { get; set; }

    public class Result
    {
        public int errCode { get; set; }
        public string message { get; set; }
        public string etype { get; set; }
    }

}

