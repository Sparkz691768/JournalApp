using SQLite;

namespace JournalApp.Models;

public class EntryTag
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed] public int EntryId { get; set; }
    [Indexed] public int TagId { get; set; }
}
