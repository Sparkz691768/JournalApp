using JournalApp.Models;

namespace JournalApp.Services.Interfaces;

public interface IExportService
{
    Task<string> ExportPdfAsync(List<JournalEntry> entries, DateOnly from, DateOnly to);
}
