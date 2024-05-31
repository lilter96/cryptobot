namespace CryptoBot.Service.Services.Interfaces;

public interface ICryptoService
{
    Task<string> EncryptAsync(string text);

    Task<string> DecryptAsync(string text);
}