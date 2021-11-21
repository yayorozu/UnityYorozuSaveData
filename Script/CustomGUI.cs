#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Yorozu.SaveData
{
	public static class CustomGUI
	{
		private static readonly GUIStyle _style;

		static CustomGUI()
		{
			_style = new GUIStyle("ShurikenModuleTitle");
			_style.font = new GUIStyle(EditorStyles.label).font;
			_style.border = new RectOffset(15, 7, 4, 4);
			_style.fixedHeight = 22;
			_style.contentOffset = new Vector2(20f, -2f);
		}

		public static bool Foldout(string title, bool foldout)
		{
			var rect = GUILayoutUtility.GetRect(16f, 22f, _style);
			GUI.Box(rect, title, _style);

			var @event = Event.current;

			var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
			if (@event.type == EventType.Repaint)
				EditorStyles.foldout.Draw(toggleRect, false, false, foldout, false);

			if (@event.type == EventType.MouseDown && rect.Contains(@event.mousePosition))
			{
				foldout = !foldout;
				@event.Use();
			}

			return foldout;
		}
	}
}

#endif
