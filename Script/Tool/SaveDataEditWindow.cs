#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Yorozu.SaveData
{
	public class SaveDataEditWindow : EditorWindow
	{
		private BindingFlags Flags => BindingFlags.Public |
		                              BindingFlags.Instance |
		                              BindingFlags.NonPublic;

		private void OnGUI()
		{
			if (!Application.isPlaying)
			{
				EditorGUILayout.HelpBox("Editor Play And Call SaveDataUtility.SetUp", MessageType.Info);

				return;
			}

			var data = SaveDataUtility.Data;
			if (data.Count <= 0)
			{
				EditorGUILayout.LabelField("SaveData Not Found");

				return;
			}

			foreach (var d in data)
				DrawData(d);
		}

		[MenuItem("Tools/SaveDataViewer")]
		private static void ShowWindow()
		{
			var window = GetWindow<SaveDataEditWindow>();
			window.titleContent = new GUIContent("SaveDataViewer");
			window.Show();
		}

		private void DrawData(SaveDataBase instance)
		{
			var type = instance.GetType();

			instance.Foldout = CustomGUI.Foldout(type.Name, instance.Foldout);

			if (!instance.Foldout)
				return;

			using (new EditorGUI.IndentLevelScope())
			{
				using (new EditorGUILayout.VerticalScope())
				{
					DrawBaseClass(instance);
					var fields = type.GetFields(Flags);
					foreach (var field in fields)
					{
						if (field.IsNotSerialized)
							continue;

						DrawField(field, instance);
					}
				}

				// データ反映を適応
				if (GUILayout.Button("Save"))
				{
					instance.Save();
					GUIUtility.ExitGUI();
				}
			}
		}

		/// <summary>
		///     SaveDataBase の中身を描画
		/// </summary>
		private void DrawBaseClass(SaveDataBase instance)
		{
			var fields = typeof(SaveDataBase).GetFields(Flags);
			foreach (var field in fields)
			{
				if (field.IsNotSerialized)
					continue;

				var fieldType = field.FieldType;

				// データ保存するリスト
				if (!fieldType.IsGenericType || fieldType.GetGenericTypeDefinition() != typeof(List<>))
					continue;

				var value = field.GetValue(instance);
				var ilist = (IList) value;

				if (ilist.Count <= 0)
					continue;

				string GetName(string name)
				{
					switch (name)
					{
						case "_intList":
							return "IntData";
						case "_stringList":
							return "StringData";
						case "_boolList":
							return "BoolData";
					}

					return "";
				}

				EditorGUILayout.LabelField(GetName(field.Name));
				using (new EditorGUI.IndentLevelScope())
				{
					var elementType = fieldType.GetGenericArguments()[0];
					using (new EditorGUILayout.VerticalScope())
					{
						for (var i = 0; i < ilist.Count; i++)
							using (new EditorGUILayout.HorizontalScope())
							{
								DrawBaseClassData(instance, elementType, ilist[i]);
							}
					}
				}
			}
		}

		/// <summary>
		///     kv を表示
		/// </summary>
		private void DrawBaseClassData(SaveDataBase instance, Type elementType, object elementValue)
		{
			var keyField = elementType.GetField("Key");
			var key = keyField.GetValue(elementValue) as string;

			var valueField = elementType.GetField("Value");
			var v = valueField.GetValue(elementValue);

			if (v is int intValue)
			{
				var temp = intValue;
				temp = EditorGUILayout.IntField(key, temp);
				if (temp != intValue)
					instance.Set(key, temp, false);
			}
			else if (v is string stringValue)
			{
				var temp = stringValue;
				temp = EditorGUILayout.TextField(key, temp);
				if (temp != stringValue)
					instance.Set(key, temp, false);
			}
			else if (v is bool boolValue)
			{
				var temp = boolValue;
				temp = EditorGUILayout.Toggle(key, temp);
				if (temp != boolValue)
					instance.Set(key, temp, false);
			}
		}

		private void DrawField(FieldInfo field, object obj)
		{
			var value = field.GetValue(obj);
			var v2 = DrawValue(field.Name, field.FieldType, value);
			if (v2 != value)
				field.SetValue(obj, v2);
		}

		private object DrawClass(string fieldName, Type type, object value)
		{
			EditorGUILayout.LabelField(fieldName);
			using (new EditorGUI.IndentLevelScope())
			{
				if (value == null)
				{
					EditorGUILayout.LabelField("Null");
				}
				else
				{
					var fields = type.GetFields(Flags);
					using (new EditorGUILayout.VerticalScope())
					{
						foreach (var field in fields)
						{
							if (field.IsNotSerialized)
								continue;

							DrawField(field, value);
						}
					}
				}
			}

			return value;
		}

		private object DrawValue(string fieldName, Type type, object value)
		{
			if (value == null)
				EditorGUILayout.LabelField(fieldName, "null");
			else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
				value = DrawList(fieldName, type.GetGenericArguments()[0], (IList) value);
			else if (type.IsArray)
				value = DrawArray(fieldName, type.GetElementType(), (Array) value);
			else if (type.IsEnum)
				return EditorGUILayout.EnumPopup(fieldName, (Enum) value);
			else if (type == typeof(bool))
				return EditorGUILayout.Toggle(fieldName, (bool) value);
			else if (type == typeof(int))
				return EditorGUILayout.IntField(fieldName, (int) value);
			else if (type == typeof(uint))
				return EditorGUILayout.FloatField(fieldName, (uint) value);
			else if (type == typeof(float))
				return EditorGUILayout.FloatField(fieldName, (float) value);
			else if (type == typeof(string))
				return EditorGUILayout.TextField(fieldName, (string) value);
			else if (type == typeof(Vector2))
				return EditorGUILayout.Vector2Field(fieldName, (Vector2) value);
			else if (type == typeof(Vector2Int))
				return EditorGUILayout.Vector2IntField(fieldName, (Vector2Int) value);
			else if (type == typeof(Vector3))
				return EditorGUILayout.Vector3Field(fieldName, (Vector3) value);
			else if (type == typeof(Vector3Int))
				return EditorGUILayout.Vector3IntField(fieldName, (Vector3Int) value);
			else if (type == typeof(Vector4))
				return EditorGUILayout.Vector4Field(fieldName, (Vector4) value);
			else if (type == typeof(Color))
				return EditorGUILayout.ColorField(fieldName, (Color) value);
			else if (type == typeof(AnimationCurve))
				return EditorGUILayout.CurveField(fieldName, (AnimationCurve) value);
			else if (type == typeof(Gradient))
				return EditorGUILayout.GradientField(fieldName, (Gradient) value);
			else if (type.IsClass)
				value = DrawClass(fieldName, type, value);
			else
				EditorGUILayout.LabelField(fieldName, "invalid Type: " + type.Name);

			return value;
		}

		private IList DrawList(string fieldName, Type type, IList value)
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField(fieldName + "[" + value.Count + "]");
				if (GUILayout.Button("+", GUILayout.Width(20)))
					value.Add(Activator.CreateInstance(type));
			}

			using (new EditorGUI.IndentLevelScope())
			{
				using (new EditorGUILayout.VerticalScope())
				{
					for (var i = 0; i < value.Count; i++)
					{
						var v = value[i];
						value[i] = DrawValue(i.ToString(), type, v);
					}
				}
			}

			return value;
		}

		private Array DrawArray(string fieldName, Type type, Array value)
		{
			EditorGUILayout.LabelField(fieldName + "[" + value.Length + "]");

			using (new EditorGUI.IndentLevelScope())
			{
				using (new EditorGUILayout.VerticalScope())
				{
					for (var i = 0; i < value.Length; i++)
					{
						var v = value.GetValue(i);
						var v2 = DrawValue(i.ToString(), type, v);
						if (v2 != v)
							value.SetValue(v2, i);
					}
				}
			}

			return value;
		}
	}
}
#endif
