using System;
using System.IO;
using UnityEngine;

namespace Yorozu.SaveData
{
	/// <summary>
	/// Application.persistentDataPath にデータを保存する
	/// </summary>
	internal class SaveDataPersistentDataLoader : ISaveDataLoader
	{
		/// <summary>
		/// 保存ファイルの拡張子
		/// </summary>
		private static string extension = "dat";

		/// <summary>
		/// 保存先を取得
		/// Android: /data/data/ProductName/files
		/// iOS    : /var/mobile/Containers/Data/Application/<guid>/Documents/ProductName/
		/// MacOS  : /Users/user名/Library/Application Support/DefaultCompany/ProductName/
		/// </summary>
		internal string SaveRoot()
		{
			// Androidの場合は
#if !UNITY_EDITOR && UNITY_ANDROID
			try
			{
				using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
				using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
				using (var getFilesDir = currentActivity.Call<AndroidJavaObject>("getFilesDir"))
				{
					return getFilesDir.Call<string>("getCanonicalPath");
				}
			}
			catch (Exception e)
			{
				Debug.LogError(e);
				return Application.persistentDataPath;
			}
#else
			return Application.persistentDataPath;
#endif
		}

		/// <summary>
		/// 保存パス
		/// </summary>
		private string SavePath(string prefix, Type type, bool isCheckDirectory)
		{
			var p1 = Path.Combine(SaveRoot(), prefix);
			if (isCheckDirectory && !Directory.Exists(p1))
				Directory.CreateDirectory(p1);

			var p2 = Path.Combine(p1, type.ToString()) + "." + extension;
			return p2;
		}

		bool ISaveDataLoader.Exist(string prefix, Type type)
		{
			var path = SavePath(prefix, type, false);
			return File.Exists(path);
		}

		void ISaveDataLoader.Save(string prefix, SaveDataBase data, Func<string, string> encrypt)
		{
			var path = SavePath(prefix, data.GetType(), true);
			using (var file = new FileStream(path, FileMode.Create, FileAccess.Write))
			{
				var json = JsonUtility.ToJson(data);
				var encryptText = encrypt.Invoke(json);
				var bytes = System.Text.Encoding.UTF8.GetBytes(encryptText);
				file.Write(bytes, 0, bytes.Length);
			}
		}

		SaveDataBase ISaveDataLoader.Load(string prefix, Type type, Func<string, string> dencrypt)
		{
			var path = SavePath(prefix, type, false);
			using (var file = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				var bytes = new byte[file.Length];
				file.Read(bytes, 0, bytes.Length);
				var json = System.Text.Encoding.UTF8.GetString(bytes);
				var dencryptText = dencrypt.Invoke(json);
				Debug.Log($"Load json:\n{dencryptText}");
				return JsonUtility.FromJson(dencryptText, type) as SaveDataBase;
			}
		}

		void ISaveDataLoader.Delete(string prefix, Type type)
		{
			var path = SavePath(prefix, type, false);
			File.Delete(path);
		}
	}
}
