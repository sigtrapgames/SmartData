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
			var refTypeProp = property.FindPropertyRelative("_refType");
			var refType = (SmartDataRefBase.RefType)refTypeProp.intValue;
			_isEventable = refType != SmartDataRefBase.RefType.CONST;
			bool forceExpand, allowLocal;
			bool forceEventable = IsForceEventable(property, fieldInfo, out forceExpand, out allowLocal);
			bool eventable = _isEventable || forceEventable;
			if (eventable){
				eventable &= !IsForceHideEvent(property, fieldInfo);
			}

			// Current type
			SmartDataRefBase.RefType[] refTypes = null;
			string[] refTypeNames = null;

			object[] parms = new object[]{forceEventable, allowLocal};
			var ft = fieldInfo.FieldType.IsArray ? fieldInfo.FieldType.GetElementType() : fieldInfo.FieldType;
			
			refTypes = (SmartDataRefBase.RefType[])ft.GetMethodPrivate(
				"_EDITOR_GetUsableRefTypes",
				BindingFlags.Static | BindingFlags.NonPublic
			).Invoke(null, parms);
			refTypeNames = (string[])ft.GetMethodPrivate(
				"_EDITOR_GetUsableRefNames",
				BindingFlags.Static | BindingFlags.NonPublic
			).Invoke(null, parms);

			bool refTypeValid = false;
			for (int i=0; i<refTypes.Length; ++i){
				if (refTypeProp.intValue == (int)refTypes[i]){
					refTypeValid = true;
					break;
				}
			}
			if (!refTypeValid){
				refTypeProp.intValue = (int)refTypes[0];
				property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
			}
			int currentRefTypeIndex = -1;
			for (int i=0; i<refTypes.Length; ++i){
				if (refType == refTypes[i]){
					currentRefTypeIndex = i;
					break;
				}
			}

			
			Rect fieldPos = DrawLabel(position, property, label, eventable, forceExpand);

			// Prep rects
			Rect popupPos, readWritePos;
			GetSmartFieldRects(property, label, max, ref fieldPos, out readWritePos, out popupPos, true, true);
			
			Rect evtPos = position;
			evtPos.yMin = fieldPos.min.y;
			evtPos.height = fieldPos.height;

			// Select type
			GUI.enabled = !Application.isPlaying;
			int newIndex = EditorGUI.Popup(popupPos, currentRefTypeIndex, refTypeNames);
			if (newIndex != currentRefTypeIndex){
				refTypeProp.intValue = (int)refTypes[newIndex];
				property.serializedObject.ApplyModifiedProperties();
			}
			GUI.enabled = true;

			// RW indicator
			DrawReadWriteLabel(readWritePos, property, fieldInfo);

			// Smart ref or value field
			if (refType == SmartDataRefBase.RefType.MULTI){
				DrawMultiProperty(fieldPos, property, min, max);					
			} else {
				DrawSmart(
					fieldPos, property.FindPropertyRelative(refPropNames[refType]), property, 
					min, max, refType != SmartDataRefBase.RefType.LOCAL, refType
				);
			}

			// Draw event if type supports it
			if (_isEventable){
				if (!eventable) return;
				DrawEvent(property, evtPos, min, max, forceExpand);
			} else if (forceEventable){
				refTypeProp.intValue = (int)SmartDataRefBase.RefType.VAR;
			}
		}
	}
}