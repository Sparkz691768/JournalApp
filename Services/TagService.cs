using JournalApp.Data;
using JournalApp.Models;
using JournalApp.Services.Interfaces;

namespace JournalApp.Services;

public class TagService : ITagService
{
    private readonly AppDb _db;

    public TagService(AppDb db) => _db = db;

    public async Task<List<Tag>> GetAllAsync()
    {
        var conn = await _db.GetAsync();
        return await conn.Table<Tag>().OrderBy(t => t.Name).ToListAsync();
    }

    public async Task<Tag?> GetByIdAsync(int id)
    {
        var conn = await _db.GetAsync();
        return await conn.Table<Tag>().FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tag?> GetByNameAsync(string name)
    {
        var conn = await _db.GetAsync();
        return await conn.Table<Tag>()
            .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());
    }

    public async Task<Tag> CreateAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tag name cannot be empty.", nameof(name));

        var conn = await _db.GetAsync();
        var trimmedName = name.Trim();

        var existing = await GetByNameAsync(trimmedName);
        if (existing != null)
            throw new InvalidOperationException("A tag with this name already exists.");

        var tag = new Tag { Name = trimmedName };
        await conn.InsertAsync(tag);
        return tag;
    }

    public async Task<Tag> UpdateAsync(int id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tag name cannot be empty.", nameof(name));

        var conn = await _db.GetAsync();
        var tag = await GetByIdAsync(id);
        
        if (tag == null)
            throw new KeyNotFoundException($"Tag with id {id} not found.");

        var trimmedName = name.Trim();
        var existing = await conn.Table<Tag>()
            .FirstOrDefaultAsync(t => t.Name.ToLower() == trimmedName.ToLower() && t.Id != id);

        if (existing != null)
            throw new InvalidOperationException("A tag with this name already exists.");

        tag.Name = trimmedName;
        await conn.UpdateAsync(tag);
        return tag;
    }

    public async Task DeleteAsync(int id)
    {
        var conn = await _db.GetAsync();
        var tag = await GetByIdAsync(id);
        
        if (tag == null)
            throw new KeyNotFoundException($"Tag with id {id} not found.");

        await conn.DeleteAsync(tag);
    }
}
