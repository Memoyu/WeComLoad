namespace WeComLoad.Admin.Blazor.Components.Base;
public partial class SearchFormItem
{
    private readonly string _prefixCls = "ant-form-item";

    [Parameter]
    public string Label { get; set; }


    [Parameter]
    public RenderFragment ChildContent { get; set; }
}
