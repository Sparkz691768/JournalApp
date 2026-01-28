namespace JournalApp.Services.Interfaces;

public interface ISecurityService
{
    Task<bool> HasPinAsync();
    Task SetPinAsync(string pin);
    Task<bool> VerifyPinAsync(string pin);
    Task ClearPinAsync();
}
