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


        eventAggregator.SubMainDialog(arg =>
        {
            MainViewDialog.IsOpen = arg.IsOpen;
            if (MainViewDialog.IsOpen)
            {
                switch (arg.DialogType)
                {
                    case MainDialogEnum.Login:
                        MainViewDialog.DialogContent = new LoginView();
                        break;
                    case MainDialogEnum.Loadding:
                        MainViewDialog.DialogContent = new LoadingView(arg.Content);
                        break;
                    default:
                        break;
                }
            }
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

       // eventAggregator.PubMainDialog(new MainDialogEventModel { IsOpen = true, DialogType = MainDialogEnum.Login });
    }

    protected override void OnContentRendered(EventArgs e)
    {
        menuBar.SelectedIndex = 0;
        var dataContext = DataContext as MainViewModel;
        dataContext.NavigateCommand.Execute(dataContext.MenuBars.First());
        base.OnContentRendered(e);
    }
}
