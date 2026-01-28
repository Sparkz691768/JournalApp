using JournalApp.Data;
using JournalApp.Models;
using JournalApp.Services.Interfaces;

namespace JournalApp.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly AppDb _db;
    public AnalyticsService(AppDb db) => _db = db;

    private static DateOnly ParseKey(string key) =>
        DateOnly.ParseExact(key, "yyyy-MM-dd");

    public async Task<StreakInfo> GetStreakInfoAsync()
    {
        var conn = await _db.GetAsync();
        var entries = await conn.Table<JournalEntry>().ToListAsync();

        if (entries.Count == 0) return new StreakInfo(0, 0, 0, new());

        var dates = entries.Select(e => ParseKey(e.EntryDateKey))
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        // missed days between first and last
        var missed = new List<DateOnly>();
        for (var d = dates.First(); d <= dates.Last(); d = d.AddDays(1))
            if (!dates.Contains(d)) missed.Add(d);

        // longest streak
        int longest = 1, current = 1;
        for (int i = 1; i < dates.Count; i++)
        {
            current = (dates[i] == dates[i - 1].AddDays(1)) ? current + 1 : 1;
            if (current > longest) longest = current;
        }

        // current streak ending today
        var today = DateOnly.FromDateTime(DateTime.Now);
        var set = dates.ToHashSet();
        int cur = 0;
        for (var d = today; set.Contains(d); d = d.AddDays(-1)) cur++;

        return new StreakInfo(cur, longest, missed.Count, missed);
    }

    public async Task<MoodDistribution> GetMoodDistributionAsync(DateOnly? from = null, DateOnly? to = null)
    {
        var conn = await _db.GetAsync();
        var entries = await conn.Table<JournalEntry>().ToListAsync();

        if (from != null) entries = entries.Where(e => ParseKey(e.EntryDateKey) >= from.Value).ToList();
        if (to != null) entries = entries.Where(e => ParseKey(e.EntryDateKey) <= to.Value).ToList();

        if (entries.Count == 0) return new MoodDistribution(0, 0, 0);

        int pos = 0, neu = 0, neg = 0;
        foreach (var e in entries)
        {
            switch (MoodMap.GetCategory(e.PrimaryMood))
            {
                case MoodCategory.Positive: pos++; break;
                case MoodCategory.Neutral: neu++; break;
                default: neg++; break;
            }
        }

        var total = (double)entries.Count;
        return new MoodDistribution(pos * 100 / total, neu * 100 / total, neg * 100 / total);
    }

    public async Task<MoodName?> GetMostFrequentMoodAsync(DateOnly? from = null, DateOnly? to = null)
    {
        var conn = await _db.GetAsync();
        var entries = await conn.Table<JournalEntry>().ToListAsync();

        if (from != null) entries = entries.Where(e => ParseKey(e.EntryDateKey) >= from.Value).ToList();
        if (to != null) entries = entries.Where(e => ParseKey(e.EntryDateKey) <= to.Value).ToList();

        if (entries.Count == 0) return null;

        return entries.GroupBy(e => e.PrimaryMood)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();
    }

    public async Task<Dictionary<string, int>> GetTopTagsAsync(int topN = 10)
    {
        var conn = await _db.GetAsync();
        var tags = await conn.Table<Tag>().ToListAsync();
        var links = await conn.Table<EntryTag>().ToListAsync();

        var countById = links.GroupBy(l => l.TagId).ToDictionary(g => g.Key, g => g.Count());

        return tags.Select(t => new { t.Name, Count = countById.TryGetValue(t.Id, out var c) ? c : 0 })
            .OrderByDescending(x => x.Count)
            .Take(topN)
            .ToDictionary(x => x.Name, x => x.Count);
    }

    public async Task<Dictionary<DateOnly, double>> GetWordCountTrendAsync(int lastNDays = 30)
    {
        var conn = await _db.GetAsync();
        var entries = await conn.Table<JournalEntry>().ToListAsync();

        var from = DateOnly.FromDateTime(DateTime.Now).AddDays(-(lastNDays - 1));

        return entries.Select(e => new { Date = ParseKey(e.EntryDateKey), e.WordCount })
            .Where(x => x.Date >= from)
            .GroupBy(x => x.Date)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.Average(v => (double)v.WordCount));
    }
}
