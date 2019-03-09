using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SmartData.Abstract;
using System.Reflection;

namespace SmartData.Editors {
	[CustomPropertyDrawer(typeof(SmartDataRefBase), true)]
	public class SmartDataRefPropertyDrawer : SmartRefPropertyDrawerBase {
		static readonly System.Type _bt = typeof(SmartDataRefBase);
		static readonly SmartDataRefBase.RefType[] RTS_EVENTABLE = 
			(SmartDataRefBase.RefType[])_bt.GetField("TYPES_EVENTABLE", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
		static readonly string[] RT_NAMES_EVENTABLE = 
			(string[])_bt.GetField("TYPENAMES_EVENTABLE", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

		/*static readonly GUIContent[] _refTypeLabels = new GUIContent[10];
		static readonly Dictionary<SmartRefBase.RefType, string> _tooltips = new Dictionary<SmartRefBase.RefType, string>{
			{SmartRefBase.RefType.VALUE, "Manual value."},
			{SmartRefBase.RefType.CONST, "Smart Const."},
			{SmartRefBase.RefType.DATA, "Smart Data."},
			{SmartRefBase.RefType.MULTI, "Smart MULTI."}
		};*/
		public static readonly Dictionary<SmartDataRefBase.RefType, string> refPropNames = new Dictionary<SmartDataRefBase.RefType, string>{
			{SmartDataRefBase.RefType.LOCAL, "_value"},
			{SmartDataRefBase.RefType.CONST, "_smartConst"},
			{SmartDataRefBase.RefType.VAR, "_smartVar"},
			{SmartDataRefBase.RefType.MULTI, "_smartMulti"}
		};
		
		protected override void DrawGUI(Rect position, SerializedProperty property, GUIContent label, Vector2 min, Vector2 max){
			Rect fieldPos = DrawLabel(position, property, label);

			var rtProp = property.FindPropertyRelative("_refType");
			var rt = (SmartDataRefBase.RefType)rtProp.intValue;
			bool forceExpand;
			bool forceEventable = IsForceEventable(property, fieldInfo, out forceExpand);

			// Current type
			SmartDataRefBase.RefType[] rts = null;
			string[] rtNames = null;

			if (forceEventable){
				rts = RTS_EVENTABLE;
				rtNames = RT_NAMES_EVENTABLE;
			} else {
				rts = (SmartDataRefBase.RefType[])fieldInfo.FieldType.GetMethodPrivate(
					"_EDITOR_GetUsableRefTypes",
					BindingFlags.Static | BindingFlags.NonPublic
				).Invoke(null, null);
				rtNames = (string[])fieldInfo.FieldType.GetMethodPrivate(
					"_EDITOR_GetUsableRefNames",
					BindingFlags.Static | BindingFlags.NonPublic
				).Invoke(null, null);
			}
			bool rtValid = false;
			for (int i=0; i<rts.Length; ++i){
				if (rtProp.intValue == (int)rts[i]){
					rtValid = true;
					break;
				}
			}
			if (!rtValid){
				rtProp.intValue = (int)rts[0];
				property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
			}
			/*for (int i=0; i<rtNames.Length; ++i){
				_refTypeLabels[i] = new GUIContent(
					rtNames[i], _tooltips[rts[i]] + "\nClick to change."
				);
			}*/

			int currentIndex = -1;
			for (int i=0; i<rts.Length; ++i){
				if (rt == rts[i]){
					currentIndex = i;
					break;
				}
			}

			// Prep rects
			Rect popPos, rwPos;
			GetSmartFieldRects(property, label, max, ref fieldPos, out rwPos, out popPos, true, true);
			
			Rect evtPos = position;
			evtPos.yMin = fieldPos.min.y;
			evtPos.height = fieldPos.height;

			// Select type
			GUI.enabled = !Application.isPlaying;
			int newIndex = EditorGUI.Popup(popPos, currentIndex, rtNames/*_refTypeLabels*/);
			if (newIndex != currentIndex){
				rtProp.intValue = (int)rts[newIndex];
			}
			GUI.enabled = true;

			// RW indicator
			DrawReadWriteLabel(rwPos, property, fieldInfo);

			// Smart ref or value field
			if (rt == SmartDataRefBase.RefType.MULTI){
				DrawMultiProperty(fieldPos, property, min, max);					
			} else {
				DrawSmart(fieldPos, property.FindPropertyRelative(refPropNames[rt]), min, max);
			}

			// Draw event if type supports it
			bool eventable = false;
			for (int i=0; i<RTS_EVENTABLE.Length; ++i){
				if (RTS_EVENTABLE[i] == rt){
					eventable = true;
					break;
				}
			}
			if (eventable){
				if (IsForceHideEvent(property, fieldInfo)) return;
				DrawEvent(property, evtPos, min, max, forceExpand);
			} else if (forceEventable){
				rtProp.intValue = (int)SmartDataRefBase.RefType.VAR;
			}
		}
	}
}