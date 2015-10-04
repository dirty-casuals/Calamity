using UnityEngine;

using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

using GameDataEditor;

namespace GameDataEditor
{
    /// <summary>
    /// AES is a symmetric 256-bit encryption algorthm.
    /// Read more: http://en.wikipedia.org/wiki/Advanced_Encryption_Standard
    /// </summary>
    [Serializable]
	public class GDECrypto
    {
		public const int KEY_LENGTH = 256;
	
		public byte[] Salt;
		public byte[] IV;
		public string Pass;

        /// <summary>
        /// Decrypts an AES-encrypted string.
        /// </summary>
        /// <param name="cipherText">Text to be decrypted</param>
        /// <returns>A decrypted string</returns>
        public string Decrypt(byte[] cipherTextBytes)
        {
			string content = string.Empty;

			try
			{
				byte[] keyBytes = new Rfc2898DeriveBytes(Pass, Salt).GetBytes(KEY_LENGTH / 8);
	            byte[] plainTextBytes = new byte[cipherTextBytes.Length];

	            using (RijndaelManaged symmetricKey = new RijndaelManaged())
	            {
	                symmetricKey.Mode = CipherMode.CBC;

					using (ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, IV))
					{
	                    using (MemoryStream memStream = new MemoryStream(cipherTextBytes))
	                    {
	                        using (CryptoStream cryptoStream = new CryptoStream(memStream, decryptor, CryptoStreamMode.Read))
	                        {
	                            int byteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
	                            content = Encoding.UTF8.GetString(plainTextBytes, 0, byteCount);
	                        }
	                    }
	                }
	            }
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}

			return content;
        }
    }
}
