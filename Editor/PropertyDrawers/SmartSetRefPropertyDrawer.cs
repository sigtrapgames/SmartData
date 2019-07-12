using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SmartData.Abstract;
using System.Reflection;
using System.Linq;

namespace SmartData.Editors {
	[CustomPropertyDrawer(typeof(SmartSetRefBase), true)]
	public class SmartSetRefPropertyDrawer : SmartRefPropertyDrawerBase {
		bool _useList;
		bool _forceEventable;
		string _listProp {get {return Application.isPlaying ? "_runtimeList" : "_list";}}
		const string SMART_PROP = "_smartSet";

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label){
			if (_useList && !_forceEventable){				
				var p = property.FindPropertyRelative(_listProp);
				string listPath = p.propertyPath;
				float h = 0;
				p.Next(true);
				bool expand = true;
				do {
					// Stop when we iterate past list
					if (!p.propertyPath.StartsWith(listPath)) break;
					
					h += BasePropertyHeight(p, GUIContent.none);
					h += EditorGUIUtility.standardVerticalSpacing;
					expand = p.isExpanded;
				} while (p.NextVisible(expand));

				return h;
			}
			return base.GetPropertyHeight(property, label);
		}
		protected override void DrawGUI(Rect position, SerializedProperty property, GUIContent label, Vector2 min, Vector2 max){
			position.height = BasePropertyHeight(property, label);
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			Rect fieldPos = position;
			Rect btnPos, rwPos;
			GetSmartFieldRects(property, label, max, ref fieldPos, out rwPos, out btnPos, false, !_forceEventable);

			bool forceExpand;
			_forceEventable = IsForceEventable(property, fieldInfo, out forceExpand);
			if (_forceEventable){
				_isEventable = true;
				GUI.enabled = !Application.isPlaying;	
				EditorGUI.PropertyField(fieldPos, property.FindPropertyRelative(SMART_PROP), GUIContent.none);
				DrawReadWriteLabel(rwPos, property, fieldInfo);
			} else {
				var useListProp = property.FindPropertyRelative("_useList");
				_useList = useListProp.boolValue;
				_isEventable = !_useList;

				GUI.enabled = !Application.isPlaying;
				if (GUI.Button(btnPos, new GUIContent(_useList ? "L" : "S", _useList ? "Using local set" : "Using SmartSet"))){
					_useList = !_useList;
					useListProp.boolValue = _useList;
				}

				DrawReadWriteLabel(rwPos, property, fieldInfo);

				if (_useList){
					GUI.enabled = true;
					fieldPos.xMin += 15;
					fieldPos.xMax = max.x;

					var lProp = property.FindPropertyRelative(_listProp);
					string listPath = lProp.propertyPath;
					lProp.Next(true);
					EditorGUI.PropertyField(fieldPos, lProp, new GUIContent("Local Set"), true);
				} else {
					DrawSmart(fieldPos, property.FindPropertyRelative(SMART_PROP), property, min, max, true);
				}
			}
			GUI.enabled = true;

			if ((_forceEventable || !_useList) &&  !IsForceHideEvent(property, fieldInfo)){
				DrawEvent(property, position, min, max, forceExpand);
			}
		}
	}
}
