namespace JournalApp.Services;

public class AppState
{
    public bool IsUnlocked { get; set; } = true; // if PIN exists, Routes will force lock
}
