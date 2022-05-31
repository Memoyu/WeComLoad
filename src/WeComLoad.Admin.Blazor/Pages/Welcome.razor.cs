namespace WeComLoad.Admin.Blazor.Pages;

public partial class Welcome
{
    private string depts = string.Empty;

    [Inject]
    public IWeComAdminSvc WeComAdminSvc { get; set; }

    private async Task GetCorpDeptsAsync()
    {
        var model = await WeComAdminSvc.GetCorpDeptAsync();
        depts = JsonConvert.SerializeObject(model);
    }
}
