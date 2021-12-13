# UnitySaveData
永続データを管理するライブラリ

### 使い方

```cs
// 利用するための準備
SaveDataUtility.SetUp();

// 保存時の名前を指定する
SaveDataUtility.SetUp("MySaveData");

const string AesIV = @"123456789";
const string AesKey = @"ABCDEFG";
// AES暗号を利用してデータ保存時に暗号化を行う
SaveDataUtility.SetUp(AesIV, AesKey);
```

保存するクラスの作成

```cs
// PlayerPrefs に保存するクラス
[Serializable]
public class SaveDataTest1 : PrefsSaveData
{
	public enum Key
	{
		A,
		B,
	}
}

// Andorid以外は Appliction.persistentDataPathにファイルとして保存するクラス
[Serializable]
public class SaveDataTest2 : FileSaveData
{
	public enum Key
	{
		A,
		B,
	}
}
```

データの読み込み

int, bool, string は Key を指定して基底クラスに保存する処理が書いてるためクラスを作成するだけでよい

```cs
var saveData = SaveDataUtility.Load<SaveDataTest1>();
// データの取得 enum
var value = test.IntValue(SaveDataTest1.Key.A));
// データの取得 string
value = test.IntValue("A"));

// データのセット enum
test.Set(SaveDataTest1.Key.A, 10);
// データのセット string
test.Set("A", 10);
```

データの削除

```
// 指定したクラスのデータを削除
SaveDataUtility.Delete<SaveDataTest1>();

// 保存したデータをすべて削除
SaveDataUtility.DeleteAll();
````


可視化

`Tools/SaveDataViewer` より Editor で保存してあるデータの中身を見れたり、変更できるツールを利用できる

