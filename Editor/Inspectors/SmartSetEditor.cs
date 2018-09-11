using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SmartData.Abstract;
using SmartData.Interfaces;
using System.Reflection;
using System.Reflection.Emit;

namespace SmartData.Editors {
	[CustomEditor(typeof(SmartSet<>), true)]
	public class SmartSetEditor : SmartObjectEditorBase {
		FieldInfo _getRtSet;
		SerializedProperty _set;
		SerializedProperty _resetOnLoad;
		bool _showRtValues = false;
		bool? _typeIsUnityObject;

		MethodInfo _add;
		FieldInfo _getToAdd;
		static object[] _addArgs = new object[2];
		SerializedProperty _toAdd;

		protected override void OnEnable(){
			base.OnEnable();
			System.Type t = target.GetType();
			_getRtSet = t.GetField("_runtimeSet", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			_set = serializedObject.FindProperty("_set");
			_resetOnLoad = serializedObject.FindProperty("_resetOnSceneChange");
			_toAdd = serializedObject.FindProperty("_toAdd");
			_add = t.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);

			while (_getToAdd == null){
				_getToAdd = t.GetField("_toAdd", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
				t = t.BaseType;
			}
		}

		protected override void DrawInspector(){
			Color gc = GUI.color;

			DrawCommonHeader();

			// Not writable at runtime
			if (Application.isPlaying){
				GUI.enabled = false;
			}
			EditorGUILayout.PropertyField(_resetOnLoad);
			DrawValueField(_set);
			GUI.enabled = true;
			
			// Hidden at edit time
			if (Application.isPlaying){
				int toRemove = -1;
				IList rtVals = (IList)_getRtSet.GetValue(target);
				if (rtVals != null){
					int c = rtVals.Count;
					if (c > 0){
						if (!_typeIsUnityObject.HasValue){
							object o = rtVals[0];
							_typeIsUnityObject = (o is Object);
						}
					}
					_showRtValues = EditorGUILayout.Foldout(_showRtValues, c.ToString() + " Runtime Values");
					if (_showRtValues && c>0){
						++EditorGUI.indentLevel;

						#region List/remove elements
						for (int i=0; i<c; ++i){
							object o = rtVals[i];
							EditorGUILayout.BeginHorizontal();
							GUI.color = Color.red;
							if (GUILayout.Button("-", GUILayout.MaxWidth(25))){
								toRemove = i;
							}
							GUI.color = gc;
							if (_typeIsUnityObject.Value){
								GUI.enabled = false;	// Disallow editing - this isn't serialized
								EditorGUILayout.ObjectField((Object)o, typeof(Object), true);
								GUI.enabled = true;
							} else {
								EditorGUILayout.LabelField(o.ToString());
							}
							EditorGUILayout.EndHorizontal();
						}
						#endregion

						#region Add element
						EditorGUILayout.BeginHorizontal();
						GUI.color = Color.green;
						if (GUILayout.Button("+", GUILayout.MaxWidth(25))){
							_addArgs[0] = _getToAdd.GetValue(target);
							_addArgs[1] = true;
							_add.Invoke(target, _addArgs);
						}
						GUI.color = gc;
						EditorGUILayout.PropertyField(_toAdd, new GUIContent(""), true);
						EditorGUILayout.EndHorizontal();
						#endregion

						--EditorGUI.indentLevel;
					}
				}

				DrawSetToDefault();
				DrawListeners();

				if (toRemove >= 0){
					((ISmartSetWriter)target).RemoveAt(toRemove);
				}
			}

			DrawDecorators("Decorators modify SmartSet behaviour at runtime, in order from top to bottom.");
		}
	}
}