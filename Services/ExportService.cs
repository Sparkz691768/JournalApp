using JournalApp.Models;
using JournalApp.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QColors = QuestPDF.Helpers.Colors;

namespace JournalApp.Services;

public class ExportService : IExportService
{
    public async Task<string> ExportPdfAsync(List<JournalEntry> entries, DateOnly from, DateOnly to)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var fileName = $"JournalExport_{from:yyyyMMdd}_{to:yyyyMMdd}.pdf";
        var dir = FileSystem.AppDataDirectory;
        Directory.CreateDirectory(dir);

        var path = Path.Combine(dir, fileName);

        await Task.Run(() =>
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(24);

                    page.Header()
                        .Text($"Journal Export ({from:yyyy-MM-dd} to {to:yyyy-MM-dd})")
                        .SemiBold()
                        .FontSize(16);

                    page.Content().Column(col =>
                    {
                        foreach (var e in entries.OrderBy(x => x.EntryDateKey))
                        {
                            col.Item()
                               .PaddingBottom(10)
                               .BorderBottom(1)
                               .BorderColor(QColors.Grey.Lighten2)
                               .Column(c =>
                               {
                                   c.Item().Text($"{e.EntryDateKey} | {e.Title ?? ""}").SemiBold();

                                   c.Item().Text($"Primary Mood: {e.PrimaryMood}").FontSize(10);

                                   if (e.SecondaryMood1 != null || e.SecondaryMood2 != null)
                                   {
                                       c.Item()
                                        .Text($"Secondary: {e.SecondaryMood1?.ToString() ?? "-"}, {e.SecondaryMood2?.ToString() ?? "-"}")
                                        .FontSize(10);
                                   }

                                   if (!string.IsNullOrWhiteSpace(e.Category))
                                       c.Item().Text($"Category: {e.Category}").FontSize(10);

                                   c.Item().Text(e.ContentMarkdown ?? "").FontSize(11);

                                   c.Item()
                                    .Text($"Created: {e.CreatedAt:g} | Updated: {e.UpdatedAt:g}")
                                    .FontSize(9)
                                    .FontColor(QColors.Grey.Darken1);
                               });
                        }
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text($"Generated: {DateTime.Now:g}")
                        .FontSize(9);
                });
            }).GeneratePdf(path);
        });

        return path;
    }
}
