using System.Windows.Controls;

namespace WeComLoad.Open.Views.Base;

/// <summary>
/// Loading.xaml 的交互逻辑
/// </summary>
public partial class LoadingView : UserControl
{

    public LoadingView(string hint)
    {
        InitializeComponent();
        tblockHint.Text = hint;
    }
}
