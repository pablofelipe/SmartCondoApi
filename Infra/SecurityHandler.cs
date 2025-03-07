using System.Security.Cryptography;
using System.Text;

namespace SmartCondoApi.Infra
{
    public class SecurityHandler
    {
        private readonly string keyPath = "chave.key";
        private readonly string ivPath = "iv.iv";

        public SecurityHandler()
        {
            GenerateNewKeys();
        }

        public void GenerateNewKeys()
        {
            if (!File.Exists(keyPath) || !File.Exists(ivPath))
            {
                using TripleDES tripleDES = TripleDES.Create();
                
                tripleDES.GenerateKey();
                tripleDES.GenerateIV();

                File.WriteAllBytes(keyPath, tripleDES.Key);
                File.WriteAllBytes(ivPath, tripleDES.IV);
            }
        }

        public string EncryptText(string text)
        {
            byte[] chave = File.ReadAllBytes(keyPath);
            byte[] iv = File.ReadAllBytes(ivPath);

            using TripleDES tripleDES = TripleDES.Create();
            tripleDES.Key = chave;
            tripleDES.IV = iv;

            byte[] output = Encrypt(tripleDES, text);

            return Encoding.UTF8.GetString(output);
        }

        private static byte[] Encrypt(TripleDES tripleDES, string texto)
        {
            using ICryptoTransform encryptor = tripleDES.CreateEncryptor();
            byte[] textoBytes = Encoding.UTF8.GetBytes(texto);
            return encryptor.TransformFinalBlock(textoBytes, 0, textoBytes.Length);
        }

        public string DecryptText(string text)
        {
            byte[] chave = File.ReadAllBytes(keyPath);
            byte[] iv = File.ReadAllBytes(ivPath);

            using TripleDES tripleDES = TripleDES.Create();
            tripleDES.Key = chave;
            tripleDES.IV = iv;

            return Decrypt(tripleDES, Encoding.UTF8.GetBytes(text));
        }

        private static string Decrypt(TripleDES tripleDES, byte[] textoCriptografado)
        {
            using ICryptoTransform decryptor = tripleDES.CreateDecryptor();
            byte[] textoDescriptografado = decryptor.TransformFinalBlock(textoCriptografado, 0, textoCriptografado.Length);
            return Encoding.UTF8.GetString(textoDescriptografado);
        }
    }
}
