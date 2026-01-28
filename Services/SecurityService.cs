using System.Security.Cryptography;
using System.Text;
using JournalApp.Services.Interfaces;

namespace JournalApp.Services;

public class SecurityService : ISecurityService
{
    private const string PinKey = "journal_pin_hash";

    public async Task<bool> HasPinAsync()
    {
        var hash = await GetStoredHashAsync();
        return !string.IsNullOrWhiteSpace(hash);
    }

    public async Task SetPinAsync(string pin)
    {
        if (string.IsNullOrWhiteSpace(pin) || pin.Length != 4 || !pin.All(char.IsDigit))
            throw new ArgumentException("PIN must be exactly 4 digits.");

        var hash = Hash(pin);

        // Try SecureStorage first, fallback to Preferences
        try
        {
            await SecureStorage.SetAsync(PinKey, hash);

        }
        catch
        {
            Preferences.Set(PinKey, hash);
        }
    }

    public async Task<bool> VerifyPinAsync(string pin)
    {
        var saved = await GetStoredHashAsync();
        if (string.IsNullOrWhiteSpace(saved))
            return true; // no PIN set

        return saved == Hash(pin);
    }

    public Task ClearPinAsync()
    {
        try
        {
            SecureStorage.Remove(PinKey);
        }
        catch
        {
            // ignore
        }

        Preferences.Remove(PinKey);
        return Task.CompletedTask;
    }

    private static string Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }

    private static async Task<string?> GetStoredHashAsync()
    {
        // Try SecureStorage
        try
        {
            var secure = await SecureStorage.GetAsync(PinKey);
            if (!string.IsNullOrWhiteSpace(secure))
                return secure;
        }
        catch
        {
            // ignore and fallback
        }

        // Fallback to Preferences
        var pref = Preferences.Get(PinKey, "");
        return string.IsNullOrWhiteSpace(pref) ? null : pref;
    }
}
