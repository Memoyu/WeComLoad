namespace WeComLoad.Admin.Blazor.Pages;

public partial class Welcome
{
    public static List<DemoModel> demos = new List<DemoModel>
        {
            new DemoModel{ Id = "get-depts",Title = "获取企业部门列表", Type = "Components.GetDepts" },
            new DemoModel{ Id = "get-apps",Title = "获取企业应用列表",  Type = "Components.GetApps" },
            new DemoModel{ Id = "get-app-detail" ,Title = "获取企业应用详情", Type = "Components.GeAppDetail"},
            new DemoModel{ Id = "send-app-secret",Title = "发送指定Agent Secret查看", Type = "Components.SendAppSecret" },
            new DemoModel{ Id = "add-open-api-app",Title = "创建企业自建应用",  Type = "Components.AddOpenApiApp" },
        };

    private static ConcurrentCache<string, RenderFragment> _showCaseCache;

    protected override async Task OnInitializedAsync()
    {
    }

    public RenderFragment GetShowCase(string type)
    {
        _showCaseCache ??= new ConcurrentCache<string, RenderFragment>();
        return _showCaseCache.GetOrAdd(type, t =>
        {
            var showCase = Type.GetType($"{Assembly.GetExecutingAssembly().GetName().Name}.{type}") ?? typeof(Template);

            void ShowCase(RenderTreeBuilder builder)
            {
                builder.OpenComponent(0, showCase);
                builder.CloseComponent();
            }

            return ShowCase;
        });
    }

}
