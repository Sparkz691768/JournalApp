using SQLite;
using JournalApp.Models;

namespace JournalApp.Data;

public class AppDb
{
    private SQLiteAsyncConnection? _conn;
    private bool _initialized;

    public async Task<SQLiteAsyncConnection> GetAsync()
    {
        if (_conn != null && _initialized) return _conn;

        var path = Path.Combine(FileSystem.AppDataDirectory, "journal.db3");
        _conn = new SQLiteAsyncConnection(path);

        if (!_initialized)
        {
            await _conn.CreateTableAsync<JournalEntry>();
            await _conn.CreateTableAsync<Tag>();
            await _conn.CreateTableAsync<EntryTag>();
            _initialized = true;
        }

        return _conn;
    }
}
