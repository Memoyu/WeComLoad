using System.Collections.Generic;

namespace WeComLoad.Admin.Dialogs
{
    /// <summary>
    /// GetOpenAppInfoView.xaml 的交互逻辑
    /// </summary>
    public partial class GetOpenAppInfoView : Window
    {
        private readonly IWeComAdmin _weComAdmin;
        private WeComOpenapiApp _appInfo;

        public GetOpenAppInfoView(IWeComAdmin weComAdmin)
        {
            InitializeComponent();
            _weComAdmin = weComAdmin;
        }

        /// <summary>
        /// 获取应用详情
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_GetAgentInfo_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tb_appOpenId.Text))
            {
                MessageBox.Show("请填入AppOpenId");
                return;
            }

            var model = await _weComAdmin.GetCorpOpenAppAsync(tb_appOpenId.Text);
            _appInfo = model?.Data;
            richText_resp.Document = new FlowDocument(new Paragraph(new Run(JsonConvert.SerializeObject(model))));

            InitialAgent();
        }

        private void InitialAgent()
        {
            var auth = _appInfo.CustAppCorpAuthInfo;
            if (auth != null && auth.Status == 1)
                SetCustomizedAppPrivilege.IsEnabled = true;
            else
                SetCustomizedAppPrivilege.IsEnabled = false;
        }

        private async void Button_ConfigAgentVisible_Click(object sender, RoutedEventArgs e)
        {
            if (_appInfo == null)
            {
                MessageBox.Show("请获取应用详情");
                return;
            }
            var deptModel = await _weComAdmin.GetCorpDeptAsync();
            var dept = deptModel.Data.Partys.List.Where(p => p.OpenapiPartyid.Equals("1")).FirstOrDefault();
            if (dept == null) throw new Exception("获取根部门异常");
            var visible_pid = dept.Partyid;

            var res = await _weComAdmin.SaveOpenApiAppAsync(new List<(string Key, string Value)>
                    {
                        ("app_id", _appInfo.AppId),
                        ("name", _appInfo.Name),
                        ("description", _appInfo.Description),
                        ("english_name", ""),
                        ("english_description", ""),
                        ("app_open", "1"),
                        ("logoimage", _appInfo.Imgid),
                        ("visible_pid[]", visible_pid),
                    });

            richText_resp.Document = new FlowDocument(new Paragraph(new Run(JsonConvert.SerializeObject(res))));
        }
        private async void Button_ConfigAgentMenu_Click(object sender, RoutedEventArgs e)
        {
            var name = tb_name.Text;
            var address = tb_address.Text;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(address))
            {
                MessageBox.Show("请输入侧边栏信息");
                return;
            }

            var menus = new List<AddChatMenuRequest> { new AddChatMenuRequest
            {
             MenuName = name,
             MenuUrl = address,
            } };

            var flag = await _weComAdmin.AddChatMenuAsync(menus, _appInfo);
            var msg = $"添加 {_appInfo.Name} 侧边栏信息成功";
            if (!flag)
                msg = $"添加 {_appInfo.Name} 侧边栏信息失败";
            richText_resp.Document = new FlowDocument(new Paragraph(new Run(msg)));
        }

        private async void Button_ConfigAgentPrivilege_Click(object sender, RoutedEventArgs e)
        {
            var flag = await _weComAdmin.SetCustomizedAppPrivilege(_appInfo.AppId);
            var msg = $"设置 {_appInfo.Name} 授权信息成功";
            if (!flag)
                msg = $"设置 {_appInfo.Name} 授权信息成功失败";
            richText_resp.Document = new FlowDocument(new Paragraph(new Run(msg)));
        }
    }
}
