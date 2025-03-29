using SmartCondoApi.Dto;

namespace SmartCondoApi.Services.Crypto
{
    public interface ICryptoService
    {
        CryptoKeyDTO GenerateNewKey();

        string EncryptData(string plainText, string publicKeyPem);

        string DecryptData(string keyId, string encryptedDataBase64);
    }
}
