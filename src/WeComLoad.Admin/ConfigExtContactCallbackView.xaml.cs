namespace WeComLoad.Admin;

/// <summary>
/// ConfigContactCallbackView.xaml 的交互逻辑
/// </summary>
public partial class ConfigExtContactCallbackView : Window
{
    private readonly IWeComAdmin _weComAdmin;

    public ConfigExtContactCallbackView(IWeComAdmin weComAdmin)
    {
        InitializeComponent();
        _weComAdmin = weComAdmin;
    }

    private async void Button_SaveConfig_Click(object sender, RoutedEventArgs e)
    {
        var url = tb_url.Text;
        var token = tb_token.Text;
        var aeskey = tb_aeskey.Text;

        if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(token) || string.IsNullOrEmpty(aeskey))
        {
            MessageBox.Show("请填入相关配置信息");
            return;
        }

        // 获取客户联系、通讯录appid (app_open_id为固定值，2000002：通讯录同步助手，2000003：外部联系人)
        var apps = await _weComAdmin.GetCorpAppAsync(true);
        if (apps == null || apps.Data == null)
        {
            richText_resp.Document = new FlowDocument(new Paragraph(new Run("获取企业app信息失败")));
            return;
        }
        var appid = apps.Data.OpenapiApps.FirstOrDefault(a => a.AppOpenId == 2000003)?.AppId;

        var start = url.IndexOf("://") + 3;
        var end = url.IndexOf('/', start);
        var host = url.Substring(start, end - start);

        var data = await _weComAdmin.ConfigContactCallbackAsync(new ConfigCallbackRequest
        {
            CallbackUrl = url,
            HostUrl = host,
            Token = token,
            AesKey = aeskey,
            Appid = appid,
        });

        richText_resp.Document = new FlowDocument(new Paragraph(new Run(JsonConvert.SerializeObject(data))));
    }
}
