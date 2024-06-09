namespace CryptoBot.Service.Services.Cryptography;

public interface ICryptographyService
{
    Task<string> EncryptAsync(string text);

    Task<string> DecryptAsync(string text);
}
