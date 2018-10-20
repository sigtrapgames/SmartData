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
		void OnEnable(){
			_notes = serializedObject.FindProperty("_notes");
		}
		public override void OnInspectorGUI(){
			EditorGUILayout.Space();
			GUILayout.Label(_notes.stringValue.Replace("\\n","\n"), _textStyle);
		}
	}
}