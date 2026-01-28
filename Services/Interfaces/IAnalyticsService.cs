using JournalApp.Models;

namespace JournalApp.Services.Interfaces;

public record MoodDistribution(double PositivePct, double NeutralPct, double NegativePct);

public record StreakInfo(
    int CurrentStreak,
    int LongestStreak,
    int MissedDaysCount,
    List<DateOnly> MissedDays);

public interface IAnalyticsService
{
    Task<StreakInfo> GetStreakInfoAsync();
    Task<MoodDistribution> GetMoodDistributionAsync(DateOnly? from = null, DateOnly? to = null);
    Task<MoodName?> GetMostFrequentMoodAsync(DateOnly? from = null, DateOnly? to = null);
    Task<Dictionary<string, int>> GetTopTagsAsync(int topN = 10);
    Task<Dictionary<DateOnly, double>> GetWordCountTrendAsync(int lastNDays = 30);
}
