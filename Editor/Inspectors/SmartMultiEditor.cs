using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SmartData.Abstract;
using System.Reflection;

namespace SmartData.Editors {
	[CustomEditor(typeof(SmartMultiBase), true)]
	public class SmartMultiEditor : SmartObjectEditorBase {
		MethodInfo _getElement;
		MethodInfo _getValue;

		SmartVarBase.DataType _dataType;
		object[] _args = new object[1];

		List<bool> _showIndividualListeners = new List<bool>();

		SerializedProperty _maxCount, _persistent;

		bool _isEvent;

		protected override void OnEnable(){
			System.Type t = serializedObject.targetObject.GetType();
			_getElement = t.GetMethod("Get", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			_getValue = t.GetMethod("GetValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);

			var gdt = t.GetMethod("_EDITOR_GetDataType", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			_isEvent = gdt == null;	// Doesn't exist for SmartEventMulti
			if (!_isEvent){
				_dataType = (SmartVarBase.DataType)gdt.Invoke(target, null);
				_isOnRestoreOverridden = IsOnRestoreOverridden(
					(System.Type)t.GetMethod(
						"_EDITOR_GetSmartType", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy
					).Invoke(target, null)
				);
			}

			_maxCount = serializedObject.FindProperty("_maxSize");
			_persistent = serializedObject.FindProperty("_persistent");

			base.OnEnable();
		}
		protected override FieldInfo CacheGetRelay(){
			return _getElement.ReturnType.GetField("_relay", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		}

		protected override void DrawInspector(){
			DrawTypeHeader();
			if (!_isOnRestoreOverridden){
				DrawRestoreWarning(_dataType);
			}
			DrawDescription();

			if (Application.isPlaying){
				GUI.enabled = false;
			}
			EditorGUILayout.PropertyField(_maxCount);
			EditorGUILayout.PropertyField(_persistent, true);
			GUI.enabled = true;

			if (Application.isPlaying) {
				var multi = (target as SmartMultiBase);
				int c = multi.count;

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Count: "+c.ToString());
				
				var wIndex = GUILayout.Width(50);
				var wValue = GUILayout.Width(50);
				GUIStyle foldout = new GUIStyle(EditorStyles.label);
				foldout.fixedWidth = 20;

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Index", wIndex);
				EditorGUILayout.LabelField("Value", wValue);
				EditorGUILayout.LabelField("Listeners");
				EditorGUILayout.EndHorizontal();
				
				++EditorGUI.indentLevel;
				for (int i=0; i<c; ++i){
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField(i.ToString(), wIndex);

					bool hasVal = false;
					if (_getValue != null){
						_args[0] = i;
						var val = _getValue.Invoke(multi, _args);
						if (val != null){
							var o = (val as UnityEngine.Object);
							if (o){
								if (GUILayout.Button(val.ToString())){
									EditorGUIUtility.PingObject(o);
								} 
							} else {
								EditorGUILayout.LabelField(val.ToString(), wValue);
							}
							hasVal = true;
						}
					}
					if (!hasVal){
						EditorGUILayout.LabelField("N/A", wIndex);
					}
					if (_showIndividualListeners.Count <= i){
						_showIndividualListeners.Add(false);
					}
				
					_args[0] = i;
					object smart = _getElement.Invoke(multi, _args);
					uint lCount = PopulateListeners(smart);
					if (_showIndividualListeners[i]){
						EditorGUILayout.BeginVertical();
					}
					_showIndividualListeners[i] = EditorGUILayout.Foldout(_showIndividualListeners[i], lCount.ToString());
					if (_showIndividualListeners[i]){
						foreach (var a in _listeners){
							EditorGUILayout.LabelField(ObjectToString(a.Key));
							for (int j=0; j<a.Value.Count; ++j){
								EditorGUILayout.LabelField("    "+MethodToString(a.Value[j]));
							}
						}
						EditorGUILayout.EndVertical();
					}
					
					EditorGUILayout.EndHorizontal();
				}
				--EditorGUI.indentLevel;

				EditorGUILayout.Space();
				if (GUILayout.Button("Dispatch All")){
					for (int i=0; i<c; ++i){
						_args[0] = i;
						object element = _getElement.Invoke(multi, _args);
						GetDispatchMethod(element).Invoke(element, null);
					}
				}
			}

			DrawDecorators("These decorators apply to all SmartData objects in this Multi. If persistent SmartDatas have their own decorators, they are applied before those here.");
		}
		protected override void DrawListeners(){}
	}
}
