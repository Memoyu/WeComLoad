using Prism.Services.Dialogs;
using System.Windows.Input;

namespace WeComLoad.Open.Views;

/// <summary>
/// MainView.xaml 的交互逻辑
/// </summary>
public partial class MainView : Window
{
    public MainView(IEventAggregator eventAggregator, IDialogService dialogService)
    {
        InitializeComponent();

        eventAggregator.Subscribe(arg =>
        {
            LoaddingDialog.IsOpen = arg.IsOpen;
            if (LoaddingDialog.IsOpen)
                LoaddingDialog.DialogContent = new LoadingView(arg.Hint);
        });

        eventAggregator.GetEvent<LoginEvent>().Subscribe(arg =>
        {
            dialogService.ShowDialog("LoginView", sender =>
            {
                if (sender.Result != ButtonResult.OK)
                {
                    return;
                }
                else
                {
                    return;
                }
            });
        });

        btnMin.Click += (s, e) =>
        {
            WindowState = WindowState.Minimized;
        };

        btnMax.Click += (s, e) =>
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        };

        btnClose.Click += (s, e) =>
        {
            Close();
        };

        ColorZone.MouseMove += (s, e) =>
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        };

        ColorZone.MouseDoubleClick += (s, e) =>
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        };

        menuBar.SelectionChanged += (s, e) =>
        {
            MenuToggleButton.IsChecked = false;
        };

    }
}
