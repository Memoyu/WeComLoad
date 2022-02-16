using Microsoft.JSInterop;

var builder = WebApplication.CreateBuilder(args);

var Configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddAntDesign();
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(sp.GetService<NavigationManager>().BaseUri)
});
builder.Services.Configure<ProSettings>(Configuration.GetSection("ProSettings"));

// 注册全局企微操作
builder.Services.AddSingleton<IWeComOpen>(sp =>
{
    var func = new WeComOpenFunc();
    //func.GetWeCombReq().SetUnAuthEvent(() =>
    //{
    //    using (var serviceScope = sp.CreateScope())
    //    {
    //        var navigationManager = serviceScope.ServiceProvider.GetService<NavigationManager>();
    //        navigationManager.NavigateTo("/login");
    //    }
    //});

    func.GetWeCombReq().SetUnAuthEvent(async () =>
    {
        var serviceScope = sp.CreateScope();
        var js = serviceScope.ServiceProvider.GetService<IJSRuntime>();
        await js.InvokeAsync<object>("Blazor._internal.navigationManager.navigateTo", "/login", true);
    });

    return func;
});
builder.Services.AddSingleton<IFileClientPro>(f => FileClientPro.Create());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
