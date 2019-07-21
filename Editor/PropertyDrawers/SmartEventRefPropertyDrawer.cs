using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SmartData.SmartEvent;

namespace SmartData.Editors {
	[CustomPropertyDrawer(typeof(EventListener), true)]
	public class SmartEventRefPropertyDrawer : SmartRefPropertyDrawerBase {
		GUIContent _multiBtn = new GUIContent("M", "Using SmartEventMulti");
		GUIContent _eventBtn = new GUIContent("E", "Using SmartEvent");
		protected override bool _isEventable {get {return true;}}
		protected override void DrawGUI(Rect position, SerializedProperty property, GUIContent label, Vector2 min, Vector2 max){
			bool forceHide = IsForceHideEvent(property, fieldInfo);
			bool forceShow = false;
			if (!forceHide){
				bool allowLocal;
				IsForceEventable(property, fieldInfo, out forceShow, out allowLocal);
			}
			Rect fieldPos = DrawLabel(position, property, label, !forceHide, forceShow);

			var useMultiProp = property.FindPropertyRelative("_useMulti");
			bool useMulti = (bool)useMultiProp.boolValue;

			Rect ldRect, btnRect;
			GetSmartFieldRects(property, label, max, ref fieldPos, out ldRect, out btnRect, false, true);

			GUI.enabled = !Application.isPlaying;
			if (GUI.Button(btnRect, (useMulti ? _multiBtn : _eventBtn))){
				useMulti = !useMulti;
				useMultiProp.boolValue = useMulti;
				property.serializedObject.ApplyModifiedProperties();
			}
			GUI.enabled = true;

			DrawReadWriteLabel(ldRect, property, fieldInfo, "L", "D", "Listen/Dispatch", "Listen-only");

			if (useMulti){
				DrawMultiProperty(fieldPos, property, min, max);
			} else {
				DrawSmart(fieldPos, property.FindPropertyRelative("_smartEvent"), property, min, max, true);
			}

			if (forceHide) return;
			DrawEvent(property, fieldPos, min, max, forceShow);
		}
	}
}