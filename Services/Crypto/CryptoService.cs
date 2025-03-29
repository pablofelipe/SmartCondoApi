using Serilog;
using SmartCondoApi.Dto;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace SmartCondoApi.Services.Crypto
{
    public class CryptoService : ICryptoService
    {
        private static readonly ConcurrentDictionary<string, CryptoKeyDTO> _keyStore = new();
        private static readonly TimeSpan _keyValidity = TimeSpan.FromHours(1);

        public CryptoKeyDTO GenerateNewKey()
        {
            using var rsa = RSA.Create(2048);

            var keyId = Guid.NewGuid().ToString();
            var publicKeyPem = ExportPublicKeyToPem(rsa);

            var key = new CryptoKeyDTO
            {
                KeyId = keyId,
                PublicKeyPem = publicKeyPem,
                PrivateKey = rsa.ExportParameters(true),
                Expiration = DateTime.UtcNow.Add(_keyValidity)
            };

            _keyStore[keyId] = key;

            CleanExpiredKeys();

            return key;
        }

        public string EncryptData(string plainText, string publicKeyPem)
        {
            try
            {
                if (string.IsNullOrEmpty(plainText))
                    return plainText;

                using var rsa = RSA.Create();

                // Importar chave pública no formato PEM
                rsa.ImportFromPem(publicKeyPem);

                // Converter texto para bytes
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

                // Criptografar usando OAEP com SHA-256
                byte[] encryptedBytes = rsa.Encrypt(plainBytes, RSAEncryptionPadding.OaepSHA256);

                // Retornar como Base64
                return Convert.ToBase64String(encryptedBytes);
            }
            catch (Exception ex)
            {
                throw new CryptographicException("Falha ao criptografar dados", ex);
            }
        }

        public string DecryptData(string keyId, string encryptedDataBase64)
        {
            if (!_keyStore.TryGetValue(keyId, out var keyPair) || keyPair.Expiration < DateTime.UtcNow)
            {
                throw new Exception("Chave inválida ou expirada");
            }

            using var rsa = RSA.Create();
            rsa.ImportParameters(keyPair.PrivateKey);

            var encryptedData = Convert.FromBase64String(encryptedDataBase64);

            try
            {
                var decryptedData = rsa.Decrypt(encryptedData, RSAEncryptionPadding.OaepSHA256);
                return Encoding.UTF8.GetString(decryptedData);
            }
            catch (CryptographicException ex)
            {
                Log.Debug($"Tamanho dos dados criptografados: {encryptedData.Length} bytes");
                Log.Debug($"Chave usada: {keyId}");
                throw new Exception("Falha na decriptação: " + ex.Message, ex);
            }
        }

        private static string ExportPublicKeyToPem(RSA rsa)
        {
            var publicKey = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());
            return $"-----BEGIN PUBLIC KEY-----\n{publicKey}\n-----END PUBLIC KEY-----";
        }

        private static void CleanExpiredKeys()
        {
            var expiredKeys = _keyStore.Where(k => k.Value.Expiration < DateTime.UtcNow).ToList();
            foreach (var key in expiredKeys)
            {
                _keyStore.TryRemove(key.Key, out _);
            }
        }
    }
}
