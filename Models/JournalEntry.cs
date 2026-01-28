using SQLite;

namespace JournalApp.Models;

public class JournalEntry
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // Enforce one entry per day
    [Indexed(Unique = true)]
    public string EntryDateKey { get; set; } = ""; // "yyyy-MM-dd"

    public string Title { get; set; } = "";
    public string ContentMarkdown { get; set; } = "";

    public MoodName PrimaryMood { get; set; }
    public MoodName? SecondaryMood1 { get; set; }
    public MoodName? SecondaryMood2 { get; set; }

    public string Category { get; set; } = "";

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public int WordCount { get; set; }
}
