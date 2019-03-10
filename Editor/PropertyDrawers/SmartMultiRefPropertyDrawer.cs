using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SmartData.Abstract;
using System.Reflection;
using System.Linq;

namespace SmartData.Editors {
	[CustomPropertyDrawer(typeof(SmartDataMultiRef<,,>), true)]
	public class SmartMultiRefPropertyDrawer : SmartRefPropertyDrawerBase {
		protected override bool _isEventable {get {return true;}}
		protected override void DrawGUI(Rect position, SerializedProperty property, GUIContent label, Vector2 min, Vector2 max){
			position.height = BasePropertyHeight(property, label);
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
			
			DrawMultiProperty(position, property, min, max);
			if (IsForceHideEvent(property, fieldInfo)) return;
			bool forceExpand;
			IsForceEventable(property, fieldInfo, out forceExpand);
			DrawEvent(property, position, min, max, forceExpand);
		}
	}
}
