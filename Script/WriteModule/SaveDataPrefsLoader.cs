using System;
using UnityEngine;

namespace Yorozu.SaveData
{
	internal class SaveDataPrefsLoader : ISaveDataLoader
	{
		private static string GetKey(string prefix, Type t) => $"{prefix}.{t.Name}";

		bool ISaveDataLoader.Exist(string prefix, Type type)
		{
			var key = GetKey(prefix, type);
			var json = PlayerPrefs.GetString(key);
			return !string.IsNullOrEmpty(json);
		}

		void ISaveDataLoader.Save(string prefix, SaveDataBase data, Func<string, string> encrypt)
		{
			var key = GetKey(prefix, data.GetType());
			var json = JsonUtility.ToJson(data);
			var encryptText = encrypt.Invoke(json);
			PlayerPrefs.SetString(key, encryptText);
			Debug.Log($"Save key:{key}\njson:{json}");
		}

		SaveDataBase ISaveDataLoader.Load(string prefix, Type type, Func<string, string> dencrypt)
		{
			var key = GetKey(prefix, type);
			var json = PlayerPrefs.GetString(key);
			if (string.IsNullOrEmpty(json))
				return Activator.CreateInstance(type) as SaveDataBase;

			try
			{
				var dencryptText = dencrypt.Invoke(json);
				Debug.Log($"Load key:{key}\njson:{dencryptText}");
				var data = JsonUtility.FromJson(dencryptText, type);
				return data as SaveDataBase;
			}
			catch
			{
				// Json のパースが失敗することを考慮
				return Activator.CreateInstance(type) as SaveDataBase;
			}
		}

		void ISaveDataLoader.Delete(string prefix, Type type)
		{
			var key = GetKey(prefix, type);
			PlayerPrefs.DeleteKey(key);
		}
	}
}
