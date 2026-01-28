using JournalApp.Data;
using JournalApp.Services;
using JournalApp.Services.Interfaces;

namespace JournalApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        // DI
        builder.Services.AddSingleton<AppDb>();
        builder.Services.AddSingleton<IJournalService, JournalService>();
        builder.Services.AddSingleton<ITagService, TagService>();
        builder.Services.AddSingleton<IAnalyticsService, AnalyticsService>();
        builder.Services.AddSingleton<ISecurityService, SecurityService>();
        builder.Services.AddSingleton<IExportService, ExportService>();

        return builder.Build();
    }
}
