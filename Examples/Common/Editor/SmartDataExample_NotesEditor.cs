using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SmartData.Examples.Editors {
	[CustomEditor(typeof(SmartDataExample_Notes))]
	public class SmartDataExample_NotesEditor : Editor {
		SerializedProperty _notes;
		GUIStyle __textStyle;
		GUIStyle _textStyle {
			get {
				if (__textStyle == null){
					__textStyle = new GUIStyle(EditorStyles.textArea);
					__textStyle.wordWrap = true;
					__textStyle.richText = true;
				}
				return __textStyle;
			}
		}

		Vector2 _scroll = Vector2.zero;

		void OnEnable(){
			_notes = serializedObject.FindProperty("_notes");
		}
		public override void OnInspectorGUI(){
			EditorGUILayout.Space();
			_scroll = EditorGUILayout.BeginScrollView(_scroll);
			GUILayout.Label(_notes.stringValue.Replace("\\n","\n"), _textStyle);
			EditorGUILayout.EndScrollView();
		}
	}
}