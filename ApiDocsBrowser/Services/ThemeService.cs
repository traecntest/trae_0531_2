using MudBlazor;

namespace ApiDocsBrowser.Services;

public class ThemeService
{
    public event Action? OnThemeChanged;
    
    public bool IsDarkMode { get; private set; }
    
    public string PrimaryColor { get; private set; } = "#2563eb";
    public string SecondaryColor { get; private set; } = "#8b5cf6";
    
    public MudTheme CurrentTheme { get; private set; } = CreateLightTheme("#2563eb", "#8b5cf6");

    public void ToggleDarkMode(bool isDark)
    {
        IsDarkMode = isDark;
        CurrentTheme = isDark 
            ? CreateDarkTheme(PrimaryColor, SecondaryColor) 
            : CreateLightTheme(PrimaryColor, SecondaryColor);
        OnThemeChanged?.Invoke();
    }

    public void SetBrandColors(string primary, string secondary)
    {
        PrimaryColor = primary;
        SecondaryColor = secondary;
        CurrentTheme = IsDarkMode 
            ? CreateDarkTheme(primary, secondary) 
            : CreateLightTheme(primary, secondary);
        OnThemeChanged?.Invoke();
    }

    private static MudTheme CreateLightTheme(string primary, string secondary)
    {
        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = primary,
                Secondary = secondary,
                Tertiary = "#10b981",
                Surface = "#ffffff",
                Background = "#f8fafc",
                AppbarBackground = primary,
                AppbarText = "#ffffff",
                DrawerBackground = "#ffffff",
                DrawerText = "#1e293b",
                TextPrimary = "#0f172a",
                TextSecondary = "#64748b",
                Info = "#0ea5e9",
                Success = "#10b981",
                Warning = "#f59e0b",
                Error = "#ef4444"
            }
        };
    }

    private static MudTheme CreateDarkTheme(string primary, string secondary)
    {
        return new MudTheme
        {
            PaletteDark = new PaletteDark
            {
                Primary = primary,
                Secondary = secondary,
                Tertiary = "#10b981",
                Surface = "#1e293b",
                Background = "#0f172a",
                AppbarBackground = "#1e293b",
                AppbarText = "#ffffff",
                DrawerBackground = "#1e293b",
                DrawerText = "#e2e8f0",
                TextPrimary = "#f1f5f9",
                TextSecondary = "#94a3b8",
                Info = "#38bdf8",
                Success = "#34d399",
                Warning = "#fbbf24",
                Error = "#f87171"
            }
        };
    }

    public static List<(string Name, string Primary, string Secondary)> GetPresetThemes()
    {
        return new List<(string, string, string)>
        {
            ("海洋蓝", "#2563eb", "#8b5cf6"),
            ("森林绿", "#059669", "#0891b2"),
            ("日落橙", "#ea580c", "#dc2626"),
            ("玫瑰粉", "#db2777", "#9333ea"),
            ("午夜黑", "#374151", "#6366f1"),
            ("皇家紫", "#7c3aed", "#ec4899")
        };
    }
}
