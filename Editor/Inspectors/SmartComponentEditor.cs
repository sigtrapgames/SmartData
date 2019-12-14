using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SmartData.Abstract;
using System.Reflection;

namespace SmartData.Editors {
	[CustomEditor(typeof(SmartComponentBase), true)]
	public class SmartComponentEditor : Editor {
		MethodInfo _dispatch;
		PropertyInfo _value;
		FieldInfo _data;
		Color _green = new Color(0.25f,1,0.25f);
		Color _red = new Color(1,0.25f,0.25f);

		SerializedProperty _spScript;
		SerializedProperty _valueToSet;

		void OnEnable(){
			_dispatch = target.GetType().GetMethod("Dispatch", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			_value = target.GetType().GetProperty("value", BindingFlags.Public | BindingFlags.Instance);
			_data = target.GetType().GetFieldPrivate("_data", BindingFlags.NonPublic | BindingFlags.Instance);

			_spScript = serializedObject.FindProperty("m_Script");
			_valueToSet = serializedObject.FindProperty("_valueToSet");
		}

		public override void OnInspectorGUI(){
			EditorGUI.BeginChangeCheck();
			
			EditorGUILayout.Space();
			GUI.enabled = false;
			EditorGUILayout.PropertyField(_spScript);
			GUI.enabled = true;

			Color gc = GUI.color;
			bool changed = false;

			int toRemove = -1;
			var spData = serializedObject.FindProperty("_data");	// Get each time to reset iterator
			if (spData.isArray){
				EditorGUILayout.BeginHorizontal();
				GUI.color = _green;
				GUI.enabled = !Application.isPlaying;
				bool add = GUILayout.Button("+", GUILayout.MaxWidth(25));
				GUI.enabled = true;
				if (add){
					++spData.arraySize;
					changed = true;
				}
				GUI.color = gc;
                EditorGUILayout.LabelField("Elements: " + spData.arraySize.ToString());
				EditorGUILayout.EndHorizontal();
				
				if (!add){	// Expanding in same paint as drawing causes sync error
					spData.NextVisible(true);
					int i = 0;
					while (spData.NextVisible(false)){
                        EditorGUILayout.BeginHorizontal();{
							EditorGUILayout.BeginVertical(GUILayout.MaxWidth(25));{
								GUILayout.Space(20);
								GUI.color = _red;
								GUI.enabled = !Application.isPlaying;
								if (GUILayout.Button("-", GUILayout.Height(40))){
									toRemove = i;
									changed = true;
								}
								GUI.enabled = true;
								GUI.color = gc;
							} EditorGUILayout.EndVertical();
							EditorGUILayout.PropertyField(spData, true);
						} EditorGUILayout.EndHorizontal();
						++i;
					}
				}
			} else {
                EditorGUILayout.PropertyField(spData, true);
			}
			
			if (_valueToSet != null){
				EditorGUILayout.PropertyField(_valueToSet, true);
			}

			if (EditorGUI.EndChangeCheck() || changed){
				if (toRemove >= 0){
					serializedObject.FindProperty("_data").DeleteArrayElementAtIndex(toRemove);
				}
				serializedObject.ApplyModifiedProperties();
			}

			if (Application.isPlaying){
				if (_value != null){
					GUI.enabled = _value.CanWrite;
					var sRef = (SmartRefBase)_data.GetValue(target);
					if (sRef.isValid){
						EditorGUILayout.LabelField("Value", _value.GetValue(target, null).ToString());
					} else {
						GUI.color = Color.red;
						EditorGUILayout.LabelField("No SmartObject!");
						GUI.color = gc;
					}
				}
				GUI.enabled = true;
				if (_dispatch != null){
					EditorGUILayout.Space();
					if (GUILayout.Button("Dispatch")){
						_dispatch.Invoke(target, null);
					}
				}
			}

			GUI.color = gc;
		}
	}
}