/*
 * Warning Icon provided by: http://findicons.com/icon/69369/warning 
 */

using UnityEngine;
using UnityEditor;

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

using GameDataEditor;

namespace GameDataEditor {
	public class GDEEncryption {
		const int SALT_LENGTH = 16;
		const int IV_LENGTH = 16;
		const int PASS_LENGTH = 32;

		static GDECrypto gdeCrypto;

		/// <summary>
		/// Encrypts a string with AES
		/// </summary>
		/// <param name="plainText">Text to be encrypted</param>
		/// <param name="path">The path to save the encrypted file</param> 
		/// <returns>An encrypted string</returns>
		public static void Encrypt(string plainText, string path)
		{
			GenerateKeys();

			byte[] cipher_text = EncryptToBytes(plainText, gdeCrypto.Pass);

			File.WriteAllBytes(path, cipher_text);
			AssetDatabase.Refresh();
		}
		
		/// <summary>
		/// Encrypts a string with AES
		/// </summary>
		/// <param name="plainText">Text to be encrypted</param>
		/// <param name="password">Password to encrypt with</param>
		/// <returns>An encrypted string</returns>
		public static byte[] EncryptToBytes(string plainText, string password)
		{
			byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
			return EncryptToBytes(plainTextBytes, password);
		}

		/// <summary>
		/// Encrypts a string with AES
		/// </summary>
		/// <param name="plainTextBytes">Bytes to be encrypted</param>
		/// <param name="password">Password to encrypt with</param>
		/// <returns>An encrypted string</returns>
		public static byte[] EncryptToBytes(byte[] plainTextBytes, string password)
		{
			byte[] keyBytes = new Rfc2898DeriveBytes(password, gdeCrypto.Salt).GetBytes(GDECrypto.KEY_LENGTH / 8);
			
			using (RijndaelManaged symmetricKey = new RijndaelManaged())
			{
				symmetricKey.Mode = CipherMode.CBC;
				
				using (ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, gdeCrypto.IV))
				{
					using (MemoryStream memStream = new MemoryStream())
					{
						using (CryptoStream cryptoStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write))
						{
							cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
							cryptoStream.FlushFinalBlock();
							
							return memStream.ToArray();
						}
					}
				}
			}
		}

		static void GenerateKeys()
		{
			string path = GDESettings.FullRootDir + Path.DirectorySeparatorChar + GDECodeGenConstants.CryptoFilePath;
			string dir = Path.GetDirectoryName(path);

			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			gdeCrypto = new GDECrypto();

			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			
			gdeCrypto.Salt = new Byte[SALT_LENGTH];
			rng.GetBytes(gdeCrypto.Salt);
		
			gdeCrypto.IV = new Byte[IV_LENGTH];
			rng.GetBytes(gdeCrypto.IV);

			byte[] passbytes = new Byte[PASS_LENGTH];
			rng.GetBytes(passbytes);

			gdeCrypto.Pass = Convert.ToBase64String(passbytes);

			using (var stream = new MemoryStream())
			{
				BinaryFormatter bin = new BinaryFormatter();
				bin.TypeFormat = FormatterTypeStyle.XsdString;
				bin.Serialize(stream, gdeCrypto);

				File.WriteAllText(path, Convert.ToBase64String(stream.ToArray()));
			}

			AssetDatabase.Refresh();
		}
	}
}
