namespace WeComLoad.Open.Common
{
    public class CustAppSetting
    {
        public Domain Domain { get; set; } = new Domain();

        public Callback Callback { get; set; } = new Callback();

        public HomePage HomePage { get; set; } = new HomePage();

        public WhiteIp WhiteIp { get; set; } = new WhiteIp();
    }
}

public class Domain : BaseEnv<string>
{
}

public class Callback : BaseEnv<string>
{
}

public class HomePage : BaseEnv<string>
{
}

public class WhiteIp : BaseEnv<string>
{
}
