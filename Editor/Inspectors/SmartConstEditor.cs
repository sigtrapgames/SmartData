using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using SmartData.Abstract;

namespace SmartData.Editors {
	[CustomEditor(typeof(SmartConst<>), true)]
	public class SmartConstEditor : SmartObjectEditorBase {
		SerializedProperty _value;
		bool _serializable = true;
		protected sealed override bool _const {get {return true;}}

		protected override void OnEnable(){
			base.OnEnable();
			_value = serializedObject.FindProperty("_value");
			_serializable = _value != null;
		}

		override protected void DrawInspector(){
			if (_serializable){
				DrawCommonHeader(false);
			}
			DrawValueField(_value, true);
		}
	}
}
