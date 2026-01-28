using JournalApp.Data;
using JournalApp.Models;
using JournalApp.Services.Interfaces;

namespace JournalApp.Services;

public class JournalService : IJournalService
{
    private readonly AppDb _db;
    public JournalService(AppDb db) => _db = db;

    private static string Key(DateOnly d) => d.ToString("yyyy-MM-dd");

    private static int CountWords(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return 0;
        return s.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    public async Task<JournalEntry?> GetByDateAsync(DateOnly date)
    {
        var conn = await _db.GetAsync();
        var key = Key(date);
        return await conn.Table<JournalEntry>().FirstOrDefaultAsync(x => x.EntryDateKey == key);
    }

    public async Task<JournalEntry?> GetByIdAsync(int id)
    {
        var conn = await _db.GetAsync();
        return await conn.Table<JournalEntry>().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<string>> GetAllTagNamesAsync()
    {
        var conn = await _db.GetAsync();
        return (await conn.Table<Tag>().ToListAsync())
            .Select(t => t.Name)
            .OrderBy(x => x)
            .ToList();
    }

    public async Task<JournalEntry> UpsertAsync(
        DateOnly date,
        string title,
        string markdown,
        MoodName primary,
        MoodName? secondary1,
        MoodName? secondary2,
        string category,
        List<string> tags)
    {
        var conn = await _db.GetAsync();
        var key = Key(date);
        var now = DateTime.Now;

        var existing = await conn.Table<JournalEntry>().FirstOrDefaultAsync(x => x.EntryDateKey == key);

        JournalEntry entry;
        if (existing == null)
        {
            entry = new JournalEntry
            {
                EntryDateKey = key,
                CreatedAt = now,
                UpdatedAt = now
            };
            await conn.InsertAsync(entry);
            existing = entry;
        }

        entry = existing;

        entry.Title = title?.Trim() ?? "";
        entry.ContentMarkdown = markdown ?? "";
        entry.PrimaryMood = primary;
        entry.SecondaryMood1 = secondary1;
        entry.SecondaryMood2 = secondary2;
        entry.Category = category?.Trim() ?? "";
        entry.UpdatedAt = now;
        entry.WordCount = CountWords(entry.ContentMarkdown);

        await conn.UpdateAsync(entry);

        // Tags
        tags = tags
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var existingTags = await conn.Table<Tag>().ToListAsync();
        foreach (var t in tags)
        {
            if (!existingTags.Any(x => x.Name.Equals(t, StringComparison.OrdinalIgnoreCase)))
                await conn.InsertAsync(new Tag { Name = t });
        }

        var allTags = await conn.Table<Tag>().ToListAsync();

        // remove old links
        var oldLinks = await conn.Table<EntryTag>().Where(x => x.EntryId == entry.Id).ToListAsync();
        foreach (var link in oldLinks) await conn.DeleteAsync(link);

        // add new links
        foreach (var t in tags)
        {
            var tagId = allTags.First(x => x.Name.Equals(t, StringComparison.OrdinalIgnoreCase)).Id;
            await conn.InsertAsync(new EntryTag { EntryId = entry.Id, TagId = tagId });
        }

        return entry;
    }

    public async Task DeleteByDateAsync(DateOnly date)
    {
        var conn = await _db.GetAsync();
        var key = Key(date);

        var entry = await conn.Table<JournalEntry>().FirstOrDefaultAsync(x => x.EntryDateKey == key);
        if (entry == null) return;

        var links = await conn.Table<EntryTag>().Where(x => x.EntryId == entry.Id).ToListAsync();
        foreach (var link in links) await conn.DeleteAsync(link);

        await conn.DeleteAsync(entry);
    }

    public async Task<(List<JournalEntry> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        string? query = null,
        DateOnly? from = null, DateOnly? to = null,
        MoodName? primaryMood = null,
        List<string>? tagNames = null)
    {
        var conn = await _db.GetAsync();
        var all = await conn.Table<JournalEntry>().ToListAsync();

        if (from != null)
        {
            var f = Key(from.Value);
            all = all.Where(e => string.Compare(e.EntryDateKey, f, StringComparison.Ordinal) >= 0).ToList();
        }
        if (to != null)
        {
            var t = Key(to.Value);
            all = all.Where(e => string.Compare(e.EntryDateKey, t, StringComparison.Ordinal) <= 0).ToList();
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var q = query.Trim();
            all = all.Where(e =>
                (e.Title?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (e.ContentMarkdown?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
            ).ToList();
        }

        if (primaryMood != null)
            all = all.Where(e => e.PrimaryMood == primaryMood.Value).ToList();

        if (tagNames is { Count: > 0 })
        {
            var tagsWanted = tagNames.Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
            if (tagsWanted.Count > 0)
            {
                var tags = await conn.Table<Tag>().ToListAsync();
                var wantedIds = tags
                    .Where(tg => tagsWanted.Any(w => tg.Name.Equals(w, StringComparison.OrdinalIgnoreCase)))
                    .Select(tg => tg.Id)
                    .ToHashSet();

                var links = await conn.Table<EntryTag>().ToListAsync();
                var entryIds = links.Where(l => wantedIds.Contains(l.TagId)).Select(l => l.EntryId).ToHashSet();

                all = all.Where(e => entryIds.Contains(e.Id)).ToList();
            }
        }

        all = all.OrderByDescending(e => e.EntryDateKey).ToList();

        var total = all.Count;
        var items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return (items, total);
    }

    public async Task<List<JournalEntry>> GetRangeAsync(DateOnly from, DateOnly to)
    {
        var conn = await _db.GetAsync();
        var all = await conn.Table<JournalEntry>().ToListAsync();

        var f = Key(from);
        var t = Key(to);

        return all
            .Where(e => string.Compare(e.EntryDateKey, f, StringComparison.Ordinal) >= 0
                     && string.Compare(e.EntryDateKey, t, StringComparison.Ordinal) <= 0)
            .OrderBy(e => e.EntryDateKey)
            .ToList();
    }
}
