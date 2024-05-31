using System.Security.Cryptography;
using System.Text;
using CryptoBot.Service.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CryptoBot.Service.Services.Implementations;

public class CryptoService : ICryptoService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public CryptoService(IConfiguration configuration)
    {
        var cryptoSettings = configuration.GetSection("CryptoSettings");
        
        _key = Encoding.UTF8.GetBytes(cryptoSettings["Key"]!);
        _iv = Encoding.UTF8.GetBytes(cryptoSettings["IV"]!);
    }
    
    public async Task<string> EncryptAsync(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            throw new ArgumentException("The text to encrypt cannot be null or empty.", nameof(plainText));
        }

        using var aesAlg = Aes.Create();
        
        aesAlg.Key = _key;
        aesAlg.IV = _iv;

        var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        using var msEncrypt = new MemoryStream();
        
        await using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        await using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            await swEncrypt.WriteAsync(plainText);
        }

        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    public async Task<string> DecryptAsync(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
        {
            throw new ArgumentException("The text to decrypt cannot be null or empty.", nameof(cipherText));
        }

        var buffer = Convert.FromBase64String(cipherText);

        using var aesAlg = Aes.Create();
        aesAlg.Key = _key;
        aesAlg.IV = _iv;

        var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using var msDecrypt = new MemoryStream(buffer);
        
        await using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        
        using var srDecrypt = new StreamReader(csDecrypt);
        
        return await srDecrypt.ReadToEndAsync();
    }
}