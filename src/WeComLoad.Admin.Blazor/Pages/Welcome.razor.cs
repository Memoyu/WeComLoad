namespace WeComLoad.Admin.Blazor.Pages;

public partial class Welcome
{
    private string depts = string.Empty;

    [Inject]
    public IWeComAdmin WeComAdmin { get; set; }

    private async Task GetCorpDeptsAsync()
    {
        var model = await WeComAdmin.GetCorpDeptAsync();
        depts = JsonConvert.SerializeObject(model);
    }
}
