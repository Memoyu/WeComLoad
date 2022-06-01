namespace WeComLoad.Admin.Blazor.Components.Base;

public partial class SearchForm : AntDomComponentBase
{
    private readonly string _prefixCls = "ant-form";

    [Parameter]
    public RenderFragment ChildContent { get; set; }

    [Parameter]
    public EventCallback<MouseEventArgs> OnSearch { get; set; }

    [Parameter]
    public EventCallback<MouseEventArgs> OnRest { get; set; }


    private async Task HandleOnSearch(MouseEventArgs args)
    {
        if (OnSearch.HasDelegate)
        {
            await OnSearch.InvokeAsync(args);
        }
    }
}
