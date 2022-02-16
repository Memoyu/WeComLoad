namespace WeComLoad.Open.Blazor.Pages.Settings.CustApp
{
    public partial class Index
    {
        private CustAppSetting Settings { get; set; } = new CustAppSetting();

        [Inject]
        private HttpClient HttpClient { get; set; }

        [Inject]
        public MessageService Message { get; set; }

        [Inject]
        private IWebHostEnvironment HostingEnv { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            Settings = await HttpClient.GetFromJsonAsync<CustAppSetting>("resources/custapp.settings.json");
        }

        private async Task HandleSubmit()
        {
            string path = Path.Combine("resources", "custapp.settings.json");
            JsonFileHelper.WriteJson(Path.Combine(HostingEnv.WebRootPath, path), Settings);
            await Message.Success("保存成功");
        }
    }
}
