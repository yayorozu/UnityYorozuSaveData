using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Yorozu.SaveData
{
	/// <summary>
	/// 嵌合化複合化を行う
	/// </summary>
	internal class AESEncryption
	{
		private byte[] _iv;
		private byte[] _key;
		private const int KeySize = 128;
		/// <summary>
		/// 128/192/256bit から選択
		/// </summary>
		private const int BlockSize = 128;

		private bool _valid;

		internal AESEncryption(string iv = "", string key = "")
		{
			_valid = !string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(iv);
			if (!_valid)
				return;

			_iv = Encoding.UTF8.GetBytes(iv);
			_key = Encoding.UTF8.GetBytes(key);
		}

		/// <summary>
		/// 暗号化
		/// </summary>
		internal string Encrypt(string text)
		{
			if (!_valid || string.IsNullOrEmpty(text))
				return text;

			using (RijndaelManaged rijAlg = new RijndaelManaged())
			{
				rijAlg.BlockSize = BlockSize;
				rijAlg.KeySize = KeySize;

				rijAlg.IV = _iv;
				rijAlg.Key = _key;

				var encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

				byte[] encrypted;
				using (var msEncrypt = new MemoryStream())
				{
					using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
					{
						using (var swEncrypt = new StreamWriter(csEncrypt))
						{
							swEncrypt.Write(text);
						}
						encrypted = msEncrypt.ToArray();
					}
				}

				return Convert.ToBase64String(encrypted);
			}
		}

		/// <summary>
		/// 復号化
		/// </summary>
		internal string Decrypt(string text)
		{
			if (!_valid || string.IsNullOrEmpty(text))
				return text;

			using (var rijndael = new RijndaelManaged())
			{
				rijndael.BlockSize = BlockSize;
				rijndael.KeySize = KeySize;

				rijndael.IV = _iv;
				rijndael.Key = _key;

				var decryptor = rijndael.CreateDecryptor(rijndael.Key, rijndael.IV);

				var plain = string.Empty;
				using (var mStream = new MemoryStream(Convert.FromBase64String(text)))
				{
					using (var ctStream = new CryptoStream(mStream, decryptor, CryptoStreamMode.Read))
					{
						using (var sr = new StreamReader(ctStream))
						{
							plain = sr.ReadLine();
						}
					}
				}
				return plain;
			}
		}
	}
}
