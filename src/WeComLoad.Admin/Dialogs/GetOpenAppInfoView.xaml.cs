namespace WeComLoad.Admin.Dialogs
{
    /// <summary>
    /// GetOpenAppInfoView.xaml 的交互逻辑
    /// </summary>
    public partial class GetOpenAppInfoView : Window
    {
        private readonly IWeComAdmin _weComAdmin;

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
            richText_resp.Document = new FlowDocument(new Paragraph(new Run(JsonConvert.SerializeObject(model))));
        }
    }
}
