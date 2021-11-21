using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Yorozu.SaveData
{
	public static class SaveDataUtility
	{
		private static List<SaveDataBase> _data = new List<SaveDataBase>(1);
		/// <summary>
		/// Tool 用
		/// </summary>
		internal static List<SaveDataBase> Data => _data;

		private static string _keyPrefix = null;

		private static SaveDataPrefsLoader _prefsLoader;
		private static SaveDataPersistentDataLoader _fileLoader;

		private static AESEncryption _crypt;

		public static void SetUp(string iv, string key, string keyPrefix = "SaveData")
		{
			_crypt = new AESEncryption(iv, key);
			SetUp(keyPrefix);
		}

#if UNITY_EDITOR

		/// <summary>
		/// セーブデータの保存先を開く
		/// </summary>
		public static void OpenSaveDir(string keyPrefix)
		{
			var root = _fileLoader.SaveRoot();
			var path = Path.Combine(root, keyPrefix);
			UnityEditor.EditorUtility.RevealInFinder(path);
		}
#endif

		/// <summary>
		/// 保存先の Prefix | Dir をセット
		/// </summary>
		public static void SetUp(string keyPrefix = "SaveData")
		{
			if (_crypt == null)
				_crypt = new AESEncryption();

			_keyPrefix = keyPrefix;
			_prefsLoader = new SaveDataPrefsLoader();
			_fileLoader = new SaveDataPersistentDataLoader();

			var saveTypes = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(a => a.GetTypes())
				.Where(t => t.IsSubclassOf(typeof(SaveDataBase)) && !t.IsAbstract);

			_data = new List<SaveDataBase>(saveTypes.Count());
			foreach (var type in saveTypes)
			{
				var temp = Activator.CreateInstance(type) as SaveDataBase;
				var loader = Loader(temp.Destination);
				if (!loader.Exist(_keyPrefix, type))
					continue;

				var data = loader.Load(_keyPrefix, type, _crypt.Decrypt);
				_data.Add(data);
			}
		}

		/// <summary>
		/// ロード先に応じてモジュールを変える
		/// </summary>
		private static ISaveDataLoader Loader(SaveDataDestination destination)
		{
			switch (destination)
			{
				case SaveDataDestination.PlayerPrefs:
					return _prefsLoader;
				case SaveDataDestination.File:
					return _fileLoader;
				default:
					throw new ArgumentOutOfRangeException(nameof(destination), destination, null);
			}
		}

		public static SaveDataBase Load(Type type)
		{
			if (!type.IsSubclassOf(typeof(SaveDataBase)))
			{
				Debug.LogError($"Require SaveDataBase SubClass.");
				return null;
			}
			var find = _data.Where(d => d.GetType() == type);
			if (find.Any())
				return find.First();

			var data = Activator.CreateInstance(type) as SaveDataBase;
			_data.Add(data);

			return data;
		}

		/// <summary>
		/// 保存したデータをロードする
		/// インスタンスがない場合は新しく作成する
		/// </summary>
		public static T Load<T>() where T : SaveDataBase
		{
			return Load(typeof(T)) as T;
		}

		/// <summary>
		/// クラスを Json にして Prefs に書き込む
		/// </summary>
		internal static void Save(SaveDataBase data)
		{
			if (!data.GetType().IsSubclassOf(typeof(SaveDataBase)))
			{
				Debug.LogError($"Require SaveDataBase SubClass.");
				return ;
			}

			var loader = Loader(data.Destination);
			loader.Save(_keyPrefix, data, _crypt.Encrypt);

			// データの反映
			for (var i = 0; i < _data.Count; i++)
			{
				if (_data[i].GetType() == data.GetType())
				{
					_data[i] = data;
					return;
				}
			}

			// 見つからなかったら追加
			_data.Add(data);
		}

		/// <summary>
		/// データの削除
		/// </summary>
		public static void Delete<T>() where T : SaveDataBase
		{
			var type = typeof(T);
			// 消すデータを見つけ出して消す
			// ここにない場合は保存されてない
			foreach (var data in _data)
			{
				if (data.GetType() != type)
					continue;

				var loader = Loader(data.Destination);
				loader.Delete(_keyPrefix, type);
			}

			_data.RemoveAll(d => d.GetType() == type);
		}

		/// <summary>
		/// 全部のデータを削除
		/// </summary>
		public static void DeleteAll()
		{
			foreach (var data in _data)
			{
				var loader = Loader(data.Destination);
				loader.Delete(_keyPrefix, data.GetType());
			}

			_data.Clear();
		}
	}
}
