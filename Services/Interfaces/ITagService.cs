using JournalApp.Models;

namespace JournalApp.Services.Interfaces;

public interface ITagService
{
    Task<List<Tag>> GetAllAsync();
    Task<Tag?> GetByIdAsync(int id);
    Task<Tag?> GetByNameAsync(string name);
    Task<Tag> CreateAsync(string name);
    Task<Tag> UpdateAsync(int id, string name);
    Task DeleteAsync(int id);
}
