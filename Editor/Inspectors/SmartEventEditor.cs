using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using SmartData.SmartEvent.Data;
using SmartData.Abstract;

namespace SmartData.Editors {
	[CustomEditor(typeof(EventVar), true)]
	public class SmartEventEditor : SmartObjectEditorBase {
		FieldInfo _multiDecorators;

		protected override void OnEnable(){
			base.OnEnable();

			_multiDecorators = target.GetType().GetFieldPrivate("_multiDecorators", BindingFlags.NonPublic | BindingFlags.Instance);

			// Check multi decorator owners to see if any have removed this SmartData from persistent list
			List<SmartMultiBase> multisToRemove = new List<SmartMultiBase>();
			Dictionary<SmartMultiBase, SmartDecoratorBase[]> extDec = (Dictionary<SmartMultiBase, SmartDecoratorBase[]>)_multiDecorators.GetValue(target);
			object[] me = new object[]{target};
			foreach (var a in extDec){
				bool containsThis = (bool)a.Key.GetType().GetMethodPrivate(
					"_EDITOR_HasPersistent", BindingFlags.NonPublic | BindingFlags.Instance
				).Invoke(
					a.Key, me
				);
				if (!containsThis){
					multisToRemove.Add(a.Key);
				}
			}
			if (multisToRemove.Count > 0){
				// Remove any that no longer include this SmartData
				target.GetType().GetMethodPrivate("_EDITOR_RemoveMultiDecorators", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(
					target, new object[]{multisToRemove.ToArray()}
				);
			}
		}

		override protected void DrawInspector(){
			DrawCommonHeader(false);
			DrawEventDetails(false);
			DrawDecorators("Decorators modify SmartEvent behaviour at runtime, in order from top to bottom.");
			DrawMultiDecoratorDetails();
		}

		protected void DrawEventDetails(bool allowReset=true){
			if (Application.isPlaying){
				EditorGUILayout.Space();
				if (GUILayout.Button("Dispatch")){
					GetDispatchMethod(target).Invoke(target, null);
				}
				if (allowReset){
					EditorGUILayout.Space();
					DrawSetToDefault();
				}
			}
		}

		protected void DrawMultiDecoratorDetails(){
			// Multi decorators details
			Dictionary<SmartMultiBase, SmartDecoratorBase[]> extDec = (Dictionary<SmartMultiBase, SmartDecoratorBase[]>)_multiDecorators.GetValue(target);
			if (extDec != null && extDec.Count > 0){
				GUIStyle warnBox = new GUIStyle(EditorStyles.helpBox);
				warnBox.fixedHeight = 18f;
				Color gbc = GUI.backgroundColor;
				foreach (var a in extDec){
					EditorGUILayout.BeginHorizontal();{
						GUILayout.Space(25);
						GUI.backgroundColor = Color.yellow;
						EditorGUILayout.LabelField(string.Format("{0} decorators from {1}\n", a.Value.Length, a.Key.name), warnBox);

						GUI.backgroundColor = Color.cyan;
						if (GUILayout.Button("Select", GUILayout.Width(55))){
							Selection.activeObject = a.Key;
						}
						GUI.backgroundColor = Color.magenta;
						if (GUILayout.Button("Ping", GUILayout.Width(55))){
							EditorGUIUtility.PingObject(a.Key);
						}
						GUI.backgroundColor = gbc;
					} EditorGUILayout.EndHorizontal();
				}
			}
		}
	}
}