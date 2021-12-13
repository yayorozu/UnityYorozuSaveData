using System;

namespace Yorozu.SaveData
{
    /// <summary>
    /// File に保存するデータ
    /// </summary>
    [Serializable]
    public abstract class FileSaveData : SaveDataBase
    {
        protected override SaveDataDestination destination => SaveDataDestination.File;
    }
}