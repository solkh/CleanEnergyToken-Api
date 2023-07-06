using System.Security.Cryptography;
using System.Text;
using System.Text.Unicode;

namespace CleanEnergyToken_Api.Extentions
{
    public static class CETEncryption
    {
        private const string StaticSalt = "35b7d487-f8e0-433a-9355-318f9d779fd0";
        private const int iterations = 1024;
        private const int keySize = 256;
        private const int blockSize = 128;

        private static string Stringify(this byte[] bytes) => Convert.ToBase64String(bytes);

        private static byte[] Byteify(this string str) => Convert.FromBase64String(str);

        public static string Hash(string input) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input + StaticSalt)));


        public static string Enc(string plainText, params string[] keys)
        {
            var aes = Aes.Create();
            aes.KeySize = keySize;
            aes.BlockSize = blockSize;
            aes.Padding = PaddingMode.Zeros;

            var keyAndIv = Keygen(keys);

            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(keyAndIv.key, keyAndIv.iv), CryptoStreamMode.Write))
            {
                using var sw = new StreamWriter(cs, Encoding.UTF8);
                sw.Write(plainText);
            }
            return ms.ToArray().Stringify();
        }

        public static string Dec(string cipherText, params string?[] keys)
        {
            var aes = Aes.Create();
            aes.KeySize = keySize;
            aes.BlockSize = blockSize;
            aes.Padding = PaddingMode.Zeros;

            var keyAndIv = Keygen(keys);
            string result = null;
            using var ms = new MemoryStream(cipherText.Byteify());
            using (var cs = new CryptoStream(ms, aes.CreateDecryptor(keyAndIv.key, keyAndIv.iv), CryptoStreamMode.Read))
            {
                using var sr = new StreamReader(cs, Encoding.UTF8);
                result = sr.ReadToEnd();
            }
            return result.TrimEnd('\0');
        }

        public static (byte[] key, byte[] iv) Keygen(params string?[] keys)
        {
            var keygen = new Rfc2898DeriveBytes(string.Join(StaticSalt, keys),
                Encoding.UTF8.GetBytes(StaticSalt),
                iterations,
                HashAlgorithmName.SHA512);
            var keyAndIV = keygen.GetBytes((keySize + blockSize) / 8);
            var key = keyAndIV.Take(keySize / 8).ToArray();
            var iv = keyAndIV.Skip(keySize / 8).Take(blockSize / 8).ToArray();
            return (key, iv);
        }
    }
}
