using JournalApp.Models;

namespace JournalApp.Services.Interfaces;

public interface IJournalService
{
    Task<JournalEntry?> GetByDateAsync(DateOnly date);
    Task<JournalEntry?> GetByIdAsync(int id);

    Task<(List<JournalEntry> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        string? query = null,
        DateOnly? from = null, DateOnly? to = null,
        MoodName? primaryMood = null,
        List<string>? tagNames = null);

    Task<JournalEntry> UpsertAsync(
        DateOnly date,
        string title,
        string markdown,
        MoodName primary,
        MoodName? secondary1,
        MoodName? secondary2,
        string category,
        List<string> tags);

    Task DeleteByDateAsync(DateOnly date);
    Task<List<string>> GetAllTagNamesAsync();

    Task<List<JournalEntry>> GetRangeAsync(DateOnly from, DateOnly to);
}
