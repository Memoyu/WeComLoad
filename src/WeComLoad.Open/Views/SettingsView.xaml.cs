using System.Windows.Controls;

namespace WeComLoad.Open.Views
{
    /// <summary>
    /// SettingsView.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView(IRegionManager regionManager)
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                menuBar.SelectedIndex = 0;
                var dataContext = DataContext as SettingsViewModel;
                dataContext.NavigateCommand.Execute(dataContext.MenuBars.First());
            };
        }  
    }
}
