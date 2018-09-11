using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SmartData.Editors {
	[CustomEditor(typeof(SmartTypeCreatorSettings))]
	public class SmartTypeCreatorSettingsEditor : Editor {
		public override void OnInspectorGUI(){
			EditorGUILayout.HelpBox("Cannot be edited manually - use advanced settings in SmartData type creator window.", MessageType.Info);
		}
	}
}