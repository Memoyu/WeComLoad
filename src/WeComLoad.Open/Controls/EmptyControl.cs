using System.Windows.Controls;

namespace WeComLoad.Open.Controls;

public class EmptyControl : ContentControl
{
    /// <summary>
    /// 内容
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(EmptyControl), new PropertyMetadata(default(string)));

    /// <summary>
    /// 数据条数
    /// </summary>
    public int ItemsCount
    {
        get => (int)GetValue(ItemsCountProperty);
        set => SetValue(ItemsCountProperty, value);
    }

    public static readonly DependencyProperty ItemsCountProperty = DependencyProperty.Register("ItemsCount", typeof(int), typeof(EmptyControl), new PropertyMetadata(default(int)));
}
