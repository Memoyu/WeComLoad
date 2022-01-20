namespace WeComLoad.Open.ViewModels
{
    public class IndexViewModel : BindableBase
    {
        private ObservableCollection<TaskStatBar> taskStatBars;
        public ObservableCollection<TaskStatBar> TaskStatBars
        {
            get { return taskStatBars; }
            set { taskStatBars = value; RaisePropertyChanged(); }
        }

        public IndexViewModel()
        {
            TaskStatBars = new ObservableCollection<TaskStatBar>();
            CreatTaskStatBar();
            CreateTestData();
        }

        private void CreatTaskStatBar()
        {
            taskStatBars.Add(new TaskStatBar
            {
                Icon = "ClockFast",
                Title = "汇总",
                Content = "9",
                Color = "#FF0CA0FF",
                Target = "",
            });
            taskStatBars.Add(new TaskStatBar
            {
                Icon = "ClockChechOutline",
                Title = "已完成",
                Content = "9",
                Color = "#FF1ECA3A",
                Target = "",
            });
            taskStatBars.Add(new TaskStatBar
            {
                Icon = "ChartLineVariant",
                Title = "完成比例",
                Content = "100%",
                Color = "#FF02C6DC",
                Target = "",
            });
            taskStatBars.Add(new TaskStatBar
            {
                Icon = "PlaylistStar",
                Title = "备忘录",
                Content = "19",
                Color = "#FFFFA000",
                Target = "",
            });
        }

        private void CreateTestData()
        {
            
        }

    }
}
