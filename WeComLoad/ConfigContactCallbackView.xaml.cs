using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WeComLoad.Automation;

namespace WeComLoad
{
    /// <summary>
    /// ConfigContactCallbackView.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigContactCallbackView : Window
    {
        private readonly IWeComAdmin _weComAdmin;
        private string _key = string.Empty;

        public ConfigContactCallbackView(IWeComAdmin weComAdmin)
        {
            InitializeComponent();
            _weComAdmin = weComAdmin;
        }

        private async void Button_TwoFactorAuthOp_Click(object sender, RoutedEventArgs e)
        {
            // 获取客户联系、通讯录appid (app_open_id为固定值，2000002：通讯录同步助手，2000003：外部联系人)
            var apps = await _weComAdmin.GetCorpAppAsync(true);
            if (apps == null || apps.Data == null)
            {
                richText_resp.Document = new FlowDocument(new Paragraph(new Run("获取企业app信息失败")));
                return;
            }
            var appid = apps.Data.OpenapiApps.FirstOrDefault(a => a.AppOpenId == 2000002)?.AppId;
            var key = await _weComAdmin.CreateTwoFactorAuthOp(appid);
            if (string.IsNullOrWhiteSpace(key))
            {
                tb_hint.Text = "发起配置失败";
                return;
            }
            _key = key;
            tb_hint.Text = "请在企微客户端中进行确认操作";
        }


        private void Button_SaveConfig_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
