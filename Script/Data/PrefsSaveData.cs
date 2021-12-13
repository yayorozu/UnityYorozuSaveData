using System;

namespace Yorozu.SaveData
{
    /// <summary>
    /// Prefs に保存するデータ
    /// </summary>
    [Serializable]
    public abstract class PrefsSaveData : SaveDataBase
    {
        protected override SaveDataDestination destination => SaveDataDestination.PlayerPrefs;
    }
}