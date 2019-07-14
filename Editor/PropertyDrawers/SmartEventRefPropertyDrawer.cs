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
			Rect fieldPos = DrawLabel(position, property, label);

			var useMultiProp = property.FindPropertyRelative("_useMulti");
			bool useMulti = (bool)useMultiProp.boolValue;

			Rect ldRect, btnRect;
			GetSmartFieldRects(property, label, max, ref fieldPos, out ldRect, out btnRect, false, true);

			GUI.enabled = !Application.isPlaying;
			if (GUI.Button(btnRect, (useMulti ? _multiBtn : _eventBtn))){
				useMulti = !useMulti;
				useMultiProp.boolValue = useMulti;
			}
			GUI.enabled = true;

			DrawReadWriteLabel(ldRect, property, fieldInfo, "L", "D", "Listen/Dispatch", "Listen-only");

			if (useMulti){
				DrawMultiProperty(fieldPos, property, min, max);
			} else {
				DrawSmart(fieldPos, property.FindPropertyRelative("_smartEvent"), property, min, max, true);
			}

			if (IsForceHideEvent(property, fieldInfo)) return;
			bool forceExpand;
			IsForceEventable(property, fieldInfo, out forceExpand);
			DrawEvent(property, fieldPos, min, max, forceExpand);
		}
	}
}