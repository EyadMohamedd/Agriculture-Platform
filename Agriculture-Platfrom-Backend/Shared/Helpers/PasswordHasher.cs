namespace AgriculturalMonitorSystem.Shared.Helpers;

/// <summary>BCrypt wrapper with work factor 11 as required.</summary>
public class PasswordHasher
{
    private const int WorkFactor = 11;

    public string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);

    public bool Verify(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);
}
