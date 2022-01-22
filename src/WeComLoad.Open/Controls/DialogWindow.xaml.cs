using Prism.Services.Dialogs;

namespace WeComLoad.Open.Controls;

/// <summary>
/// DialogWindow.xaml 的交互逻辑
/// </summary>
public partial class DialogWindow : Window, IDialogWindows
{
    public IDialogResult Result { get; set; }

    public Window Window { get; }

    public DialogWindow()
    {
        InitializeComponent();
    }
}
