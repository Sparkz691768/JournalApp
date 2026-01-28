using JournalApp.Services.Interfaces;

namespace JournalApp.Services;

public class ThemeService : IThemeService
{
    private const string ThemeKey = "app_theme";

    public string GetTheme() => Preferences.Get(ThemeKey, "light");

    public void SetTheme(string theme)
    {
        theme = (theme == "dark") ? "dark" : "light";
        Preferences.Set(ThemeKey, theme);
    }
}
