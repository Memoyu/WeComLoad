using MaterialDesignColors;
using MaterialDesignThemes.Wpf;

namespace WeComLoad.Open.ViewModels;

public class SkinViewModel : BindableBase
{
    public IEnumerable<ISwatch> Swatches { get; } = SwatchHelper.Swatches;

    private bool _isDarkTheme = true;
    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        set
        {
            if (SetProperty(ref _isDarkTheme, value))
            {
                ModifyTheme(theme => theme.SetBaseTheme(value ? Theme.Dark : Theme.Light));
            }
        }
    }

    public SkinViewModel()
    {
    }

    private static void ModifyTheme(Action<ITheme> modificationAction)
    {
        var paletteHelper = new PaletteHelper();
        ITheme theme = paletteHelper.GetTheme();
        modificationAction?.Invoke(theme);
        paletteHelper.SetTheme(theme);
    }
}
