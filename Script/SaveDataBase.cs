using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Yorozu.SaveData
{
	/// <summary>
	/// 保存するデータの基盤となるクラス
	/// Json に変換するため Serialize できる必要がある
	/// </summary>
	[Serializable]
	public abstract class SaveDataBase
	{
		[SerializeField]
		private List<DataInt> _intList = new List<DataInt>();
		[SerializeField]
		private List<DataString> _stringList = new List<DataString>();
		[SerializeField]
		private List<DataBool> _boolList = new List<DataBool>();

#if UNITY_EDITOR
		internal bool Foldout
		{
			get => UnityEditor.EditorPrefs.GetBool("SaveDataFoldout" + GetType().FullName, false);
			set => UnityEditor.EditorPrefs.SetBool("SaveDataFoldout" + GetType().FullName, value);
		}
#endif

		/// <summary>
		/// 保存先を指定
		/// </summary>
		protected abstract SaveDataDestination destination { get; }
		internal SaveDataDestination Destination => destination;

		[Serializable]
		private class DataInt
		{
			public string Key;
			public int Value;

			public DataInt(string key, int value)
			{
				Key = key;
				Value = value;
			}
		}

		[Serializable]
		private class DataString
		{
			public string Key;
			public string Value;

			public DataString(string key, string value)
			{
				Key = key;
				Value = value;
			}
		}

		[Serializable]
		private class DataBool
		{
			public string Key;
			public bool Value;

			public DataBool(string key, bool value)
			{
				Key = key;
				Value = value;
			}
		}

		/// <summary>
		/// int を保存
		/// </summary>
		public void Save(Enum key, int value, bool save = true) => Save(key.ToString(), value, save);
		public void Save(string keyString, int value, bool save = true)
		{
			var find = _intList.FirstOrDefault(d => d.Key == keyString);
			if (find != null)
				find.Value = value;
			else
				_intList.Add(new DataInt(keyString, value));

			if (save)
				Save();
		}

		/// <summary>
		/// int を取得
		/// </summary>
		public int IntValue(Enum key, int defaultValue = 0)
		{
			var keyString = key.ToString();
			var find = _intList.FirstOrDefault(d => d.Key == keyString);
			return find == null ? defaultValue : find.Value;
		}

		/// <summary>
		/// string を保存
		/// </summary>
		public void Save(Enum key, string value, bool save = true) => Save(key.ToString(), value, save);
		public void Save(string key, string value, bool save = true)
		{
			var find = _stringList.FirstOrDefault(d => d.Key == key);
			if (find != null)
				find.Value = value;
			else
				_stringList.Add(new DataString(key, value));

			if (save)
				Save();
		}

		public string StringValue(Enum key, string defaultValue = "")
		{
			var keyString = key.ToString();
			var find = _stringList.FirstOrDefault(d => d.Key == keyString);
			return find == null ? defaultValue : find.Value;
		}

		/// <summary>
		/// bool を保存
		/// </summary>
		public void Save(Enum key, bool value, bool save = true) => Save(key.ToString(), value, save);
		public void Save(string keyString, bool value, bool save = true)
		{
			var find = _boolList.FirstOrDefault(d => d.Key == keyString);
			if (find != null)
				find.Value = value;
			else
				_boolList.Add(new DataBool(keyString, value));

			if (save)
				Save();
		}

		public bool BoolValue(Enum key, bool defaultValue = false)
		{
			var keyString = key.ToString();
			var find = _boolList.FirstOrDefault(d => d.Key == keyString);
			return find == null ? defaultValue : find.Value;
		}

		/// <summary>
		/// 保存
		/// </summary>
		public void Save()
		{
			SaveDataUtility.Save(this);
		}
	}

	/// <summary>
	/// Prefs に保存するデータ
	/// </summary>
	[Serializable]
	public abstract class SaveDataPrefs : SaveDataBase
	{
		protected override SaveDataDestination destination => SaveDataDestination.PlayerPrefs;
	}

	/// <summary>
	/// File に保存するデータ
	/// </summary>
	[Serializable]
	public abstract class SaveDataFile : SaveDataBase
	{
		protected override SaveDataDestination destination => SaveDataDestination.File;
	}
}
