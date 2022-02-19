using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WeComLoad.Open.Common;

public class AuditConfig : INotifyPropertyChanged
{
    private string corpId ;

    private string appId ;

    private string callbackUrl ;

    private string callbackUrlComplete ;

    private string whiteIp ;

    private string domain ;

    private string homePage ;

    private string verifyBucket ;


    public string CorpId
    {
        get { return corpId; }
        set { corpId = value; OnPropertyChanged(); }
    }

    public string AppId
    {
        get { return appId; }
        set { appId = value; OnPropertyChanged(); }
    }

    public string CallbackUrl
    {
        get { return callbackUrl; }
        set { callbackUrl = value; OnPropertyChanged(); }
    }

    public string CallbackUrlComplete
    {
        get { return callbackUrlComplete; }
        set { callbackUrlComplete = value; OnPropertyChanged(); }
    }

    public string WhiteIp
    {
        get { return whiteIp; }
        set { whiteIp = value; OnPropertyChanged(); }
    }

    public string Domain
    {
        get { return domain; }
        set { domain = value; OnPropertyChanged(); }
    }

    public string HomePage
    {
        get { return homePage; }
        set { homePage = value; OnPropertyChanged(); }
    }

    public string VerifyBucket
    {
        get { return verifyBucket; }
        set { verifyBucket = value; OnPropertyChanged(); }
    }


    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// 实现通知更新
    /// </summary>
    /// <param name="propertyName"></param>
    public void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

