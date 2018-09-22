using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SmartData.Abstract;
using System.Reflection;

namespace SmartData.Editors {
	[CustomEditor(typeof(SmartVarBase), true)]
	public class SmartVarEditor : SmartEventEditor {
		SerializedProperty _rtValue, _value;
		SerializedProperty _resetOnLoad;

		MethodInfo _updateRtValue;
		MethodInfo _hasPersistent;


		protected override void OnEnable(){
			base.OnEnable();
			_rtValue = serializedObject.FindProperty("_runtimeValue");
			_value = serializedObject.FindProperty("_value");
			_resetOnLoad = serializedObject.FindProperty("_resetOnSceneChange");
			_updateRtValue = target.GetType().GetMethodPrivate("_EDITOR_UpdateRtValue", BindingFlags.NonPublic | BindingFlags.Instance);
		}
		
		override protected void DrawInspector(){
			DrawCommonHeader(true);
			// Not writable at runtime
			if (Application.isPlaying){
				GUI.enabled = false;
			}
			if (_resetOnLoad != null){
				EditorGUILayout.PropertyField(_resetOnLoad);
			}
			DrawValueField(_value);
			GUI.enabled = true;
			
			// Hidden at edit time
			if (Application.isPlaying && _rtValue != null){
				bool guiChanged = GUI.changed;
				GUI.changed = false;
				EditorGUI.BeginChangeCheck();

				// Handle scene objects being serialised (not supported in ScriptableObjects due to conflicting scope)
				// TODO try to fake this with a custom property field
				if (_rtValue.propertyType == SerializedPropertyType.ObjectReference && _rtValue.objectReferenceValue != null && _rtValue.objectReferenceInstanceIDValue > 0){
					var rtv = _rtValue.objectReferenceValue;
					EditorGUILayout.BeginHorizontal(); {
						EditorGUILayout.LabelField(new GUIContent(ObjectNames.NicifyVariableName(_rtValue.name), _rtValue.tooltip), GUILayout.Width(EditorGUIUtility.labelWidth));
						if (GUILayout.Button(rtv.name)){
							EditorGUIUtility.PingObject(rtv);
						}
					} EditorGUILayout.EndHorizontal();
					EditorGUILayout.HelpBox("Scene GameObjects cannot be serialized in ScriptableObjects.\nThis is only cosmetic - the reference still exists in code.", MessageType.Warning);
				} else {
					EditorGUILayout.PropertyField(_rtValue, true);
				}
				GUI.changed = guiChanged;
			}

			DrawEventDetails();

			DrawDecorators("Decorators modify SmartData behaviour at runtime, in order from top to bottom.");

			DrawMultiDecoratorDetails();
		}
	}
}