using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace UnityEditor {
	public static class Ext_SerializedProperty {
		const BindingFlags FLAGS_ALL = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
		public static void DefaultLayoutField(this SerializedProperty p, GUIContent label=null, bool includeChildren=false, GUIStyle style=null, params GUILayoutOption[] options){
			if (label == null){
				label = new GUIContent(p.name, p.tooltip);
			}
			bool s = style != null;

			switch (p.propertyType){
				case SerializedPropertyType.Integer:
					if (s) 	EditorGUILayout.IntField(label, p.intValue, style, options);
					else	EditorGUILayout.IntField(label, p.intValue, options);
					break;
				case SerializedPropertyType.Boolean:
					if (s)	EditorGUILayout.Toggle(label, p.boolValue, style, options);
					else 	EditorGUILayout.Toggle(label, p.boolValue, options);
					break;
				case SerializedPropertyType.Float:
					if (s)	EditorGUILayout.FloatField(label, p.floatValue, style, options);
					else	EditorGUILayout.FloatField(label, p.floatValue, options);
					break;
				case SerializedPropertyType.String:
					if (s)	EditorGUILayout.TextField(label, p.stringValue, style, options);
					else	EditorGUILayout.TextField(label, p.stringValue, options);
					break;
				case SerializedPropertyType.Color:
					EditorGUILayout.ColorField(label, p.colorValue, options);
					break;
				case SerializedPropertyType.ObjectReference:
					EditorGUILayout.ObjectField(p, label, options);
					break;
				case SerializedPropertyType.LayerMask:
					if (s)	EditorGUILayout.LayerField(label, p.intValue, style, options);
					else	EditorGUILayout.LayerField(label, p.intValue, options);
					break;
				case SerializedPropertyType.Enum:
					if (s)	EditorGUILayout.IntField(label, p.intValue, style, options);
					else	EditorGUILayout.IntField(label, p.intValue, options);
					break;
				case SerializedPropertyType.Vector2:
					EditorGUILayout.Vector2Field(label, p.vector2Value, options);
					break;
				case SerializedPropertyType.Vector3:
					EditorGUILayout.Vector3Field(label, p.vector3Value, options);
					break;
				case SerializedPropertyType.Vector4:
					EditorGUILayout.Vector4Field(label, p.vector4Value, options);
					break;
				case SerializedPropertyType.Rect:
					EditorGUILayout.RectField(label, p.rectValue, options);
					break;
				case SerializedPropertyType.ArraySize:
					if (s)	EditorGUILayout.IntField(label, p.intValue, style, options);
					else	EditorGUILayout.IntField(label, p.intValue, options);
					break;
				case SerializedPropertyType.Character:
					if (s)	EditorGUILayout.IntField(label, p.intValue, style, options);
					else	EditorGUILayout.IntField(label, p.intValue, options);
					break;
				case SerializedPropertyType.AnimationCurve:
					if (s)	EditorGUILayout.LabelField(label, style, options);
					else	EditorGUILayout.LabelField(label, options);
					break;
				case SerializedPropertyType.Bounds:
					EditorGUILayout.BoundsField(label, p.boundsValue, options);
					break;
				case SerializedPropertyType.Gradient:
					if (s)	EditorGUILayout.LabelField(label, style, options);
					else	EditorGUILayout.LabelField(label, options);
					break;
				case SerializedPropertyType.Quaternion:
					EditorGUILayout.Vector4Field(label, p.vector4Value, options);
					break;
				case SerializedPropertyType.ExposedReference:
					EditorGUILayout.ObjectField(p, label, options);
					break;
				case SerializedPropertyType.FixedBufferSize:
					if (s)	EditorGUILayout.IntField(label, p.intValue, style, options);
					else	EditorGUILayout.IntField(label, p.intValue, options);
					break;
				case SerializedPropertyType.Vector2Int:
					EditorGUILayout.Vector2IntField(label, p.vector2IntValue, options);
					break;
				case SerializedPropertyType.Vector3Int:
					EditorGUILayout.Vector3IntField(label, p.vector3IntValue, options);
					break;
				case SerializedPropertyType.RectInt:
					EditorGUILayout.RectIntField(label, p.rectIntValue, options);
					break;
				case SerializedPropertyType.BoundsInt:
					EditorGUILayout.BoundsIntField(label, p.boundsIntValue, options);
					break;
				case SerializedPropertyType.Generic:
					if (s)	EditorGUILayout.LabelField(label, new GUIContent(p.type), style, options);
					else	EditorGUILayout.LabelField(label, new GUIContent(p.type), options);
					break;
				default:
					EditorGUILayout.PropertyField(p, label, includeChildren, options);
					break;
			}
		}
		/// <summary>
		/// Get the object this property points to
		/// </summary>
		public static object GetObject(this SerializedProperty p){
			object o = p.serializedObject.targetObject;
			// Friendly array syntax - one . per path element
			// e.g. propName.Array.data[0] => propName[0]
			string path = p.propertyPath.Replace(".Array.data[", "[");
			string[] elements = path.Split('.');
			// Iterate through property path
			for (int i=0; i<elements.Length; ++i){
				if (p.IsArrayElement(elements[i])){
					string arrayName;
					int j = GetArrayIndex(elements, i, out arrayName);
					var fi = o.GetType().GetFieldPrivate(arrayName, FLAGS_ALL);
					var arr = (System.Array)fi.GetValue(o);
					o = arr.GetValue(j);
				} else {
					var fi = o.GetType().GetFieldPrivate(elements[i], FLAGS_ALL);
					o = fi.GetValue(o);
				}
			}
			return o;
		}
		public static FieldInfo GetField(this SerializedProperty p){
			object o = p.serializedObject.targetObject;
			var t = o.GetType();
			FieldInfo fi = null;

			// Friendly array syntax - one . per path element
			// e.g. propName.Array.data[0] => propName[0]
			string[] elements = GetFriendlyPath(p);
			
			// Iterate through property path
			for (int i=0; i<elements.Length; ++i){
				if (p.IsArrayElement(elements[i])){
					string arrayName;
					GetArrayIndex(elements, i, out arrayName);
					fi = t.GetFieldPrivate(arrayName, FLAGS_ALL);
				} else {
					fi = t.GetFieldPrivate(elements[i], FLAGS_ALL);
				}
				if (fi == null) return null;
			}
			return fi;
		}
		public static System.Type GetFieldType(this SerializedProperty p){
			object o = p.serializedObject.targetObject;
			return GetField(p).FieldType;
		}
		/// <summary>
		/// Splits property path and simplifies array syntax. 
		/// E.g. propName.Array.data[0] => propName[0]
		/// </summary>
		public static string[] GetFriendlyPath(SerializedProperty p){
			string path = p.propertyPath.Replace(".Array.data[", "[");
			return path.Split('.');
		}
		public static bool IsArrayElement(this SerializedProperty p){
			return p.propertyPath.EndsWith("]");
		}
		public static bool IsArrayElement(this SerializedProperty p, string path){
			return path.EndsWith("]");
		}
		public static FieldInfo GetArrayField(this SerializedProperty p){
			return p.GetArrayField(-2);
		}
		public static FieldInfo GetArrayField(this SerializedProperty p, int pathElement){
			// Friendly array syntax - one . per path element
			string path = p.propertyPath.Replace("Array.data[", "[");
			string[] dirs = path.Split('.');
			// If index negative, count from end
			string fieldName = pathElement < 0 ? dirs[dirs.Length+pathElement] : dirs[pathElement];

			var t = p.serializedObject.targetObject.GetType();
			return t.GetFieldPrivate(fieldName, FLAGS_ALL);
		}
		static FieldInfo GetArrayField(this SerializedProperty p, string[] pathElements, int elementIndex){
			// If index negative, count from end
			string fieldName = elementIndex < 0 ? pathElements[pathElements.Length-elementIndex] : pathElements[elementIndex];
			var t = p.serializedObject.targetObject.GetType();
			return t.GetFieldPrivate(fieldName, FLAGS_ALL);
		}
		public static int GetArrayIndex(this SerializedProperty p){
			// Remove cruft
			string path = p.propertyPath.Replace("Array.data[", "");
			path = path.TrimEnd(']');
			string[] dirs = path.Split('.');
			// Final element should be array index
			return int.Parse(dirs[dirs.Length-1]);
		}
		static int GetArrayIndex(string[] pathElements, int elementIndex, out string arrayName){
			// Find opening array bracket to get first element of int substring
			string e = GetElement(pathElements, elementIndex);
			int first = e.Length-1;
			for (; first>=0; --first){
				if (e[first] == '['){
					++first;
					break;
				}
			}
			// Name is substring from [0] to [first-1] (remove [{index}])
			arrayName = e.Substring(0, first-1);
			// Get substring from [first] to [penultimate element] (get int within [])
			int len = e.Length-(first+1);
			return int.Parse(e.Substring(first, len));
		}
		static string GetElement(string[] elements, int index){
			return index < 0 ? elements[elements.Length-index] : elements[index];
		}
	}
}