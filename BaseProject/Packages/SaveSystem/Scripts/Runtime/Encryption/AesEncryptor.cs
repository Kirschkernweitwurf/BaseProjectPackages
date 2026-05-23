using System;
using System.Security.Cryptography;
using System.Text;
using Base.SaveSystemPackage.Storage;
using Base.UtilityPackage.Logging;

namespace Base.SaveSystemPackage.Encryption
{
    /// <summary>
    /// Simple AES-256 encryption. A fresh random IV is created for every save
    /// and stored at the front of the file, so the same data never looks the same twice.
    /// </summary>
    public sealed class AesEncryptor : ISaveEncryptor
    {
        private const int IvSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100_000;
        private const string FallbackSalt = "3a3tsUZLSjZNTXR#7Tsgz3c95OIXsHsYia";

        public ESaveEncryption Mode => ESaveEncryption.Aes;

        private readonly byte[] _key;

        /// <param name="passphrase">Any secret string. Keep it the same across versions
        /// or old saves will fail to decrypt.</param>
        /// <param name="salt">Optional. Pass your own for extra safety.</param>
        public AesEncryptor(string passphrase, byte[] salt = null)
        {
            if (string.IsNullOrEmpty(passphrase))
            {
                CustomLogger.LogWarning("Passphrase is null or empty. This is not secure!" +
                                        " Please provide a strong passphrase.", null);
                return;
            }

            salt ??= Encoding.UTF8.GetBytes(FallbackSalt);
            using Rfc2898DeriveBytes kdf = new(passphrase, salt, Iterations, HashAlgorithmName.SHA256);
            _key = kdf.GetBytes(KeySize);
        }

        /// <inheritdoc />
        public byte[] Encrypt(byte[] plain)
        {
            using Aes aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();

            using ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] cipher = encryptor.TransformFinalBlock(plain, 0, plain.Length);

            byte[] result = new byte[IvSize + cipher.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, IvSize);
            Buffer.BlockCopy(cipher, 0, result, IvSize, cipher.Length);
            return result;
        }

        /// <inheritdoc />
        public byte[] Decrypt(byte[] data)
        {
            if (data is not { Length: > IvSize })
            {
                CustomLogger.LogError("Data is null or too short to contain an IV. Decryption failed.", null);
                return Array.Empty<byte>();
            }

            using Aes aes = Aes.Create();
            aes.Key = _key;

            byte[] iv = new byte[IvSize];
            Buffer.BlockCopy(data, 0, iv, 0, IvSize);
            aes.IV = iv;

            using ICryptoTransform decryptor = aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(data, IvSize, data.Length - IvSize);
        }
    }
}