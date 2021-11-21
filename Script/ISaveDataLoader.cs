using System;

namespace Yorozu.SaveData
{
	public interface ISaveDataLoader
	{
		/// <summary>
		/// 保存データが存在するか
		/// </summary>
		bool Exist(string prefix, Type type);

		/// <summary>
		/// データを保存する
		/// </summary>
		void Save(string prefix, SaveDataBase data, Func<string, string> encrypt);

		/// <summary>
		/// データをロードする
		/// </summary>
		/// <param name="prefix"></param>
		/// <returns></returns>
		SaveDataBase Load(string prefix, Type type, Func<string, string> dencrypt);

		/// <summary>
		/// データの削除
		/// </summary>
		void Delete(string prefix, Type type);


	}
}
