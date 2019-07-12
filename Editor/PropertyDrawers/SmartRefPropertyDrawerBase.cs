using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using SmartData.Abstract;

namespace SmartData.Editors {
	public abstract class SmartRefPropertyDrawerBase : PropertyDrawer {
		protected const int SPACING = 2;
		static readonly GUIContent _gcCreateBtn = new GUIContent("+", "Create a new SmartObject asset for this field.");
		bool _forceExpand;
		protected virtual bool _isEventable {get; set;}
		float _lastHeight;

		protected SerializedProperty _baseProperty {get; private set;}
		public sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label){
			ProjectPanelPathUtil.Init();

			bool metadataGenerated = false;

			_baseProperty = property;
			var ownerProp = property.FindPropertyRelative("_owner");
			var owner = property.serializedObject.targetObject;
			int id = 0;
			if (owner is Component){
				// Owner is a Component - find GameObject
				var og = (owner as Component).gameObject;
				if (ownerProp.objectReferenceValue != og){
					ownerProp.objectReferenceValue = og;
					metadataGenerated = true;
				}
				id = og.GetInstanceID();
			} else {
				// Owner is an Object - use directly
				if (ownerProp.objectReferenceValue != owner){
					ownerProp.objectReferenceValue = owner;
					metadataGenerated = true;
				}
				id = owner.GetInstanceID();
			}

			var ownerName = property.FindPropertyRelative("_ownerName");
			if (ownerName.stringValue != owner.name){
				ownerName.stringValue = owner.name;
				metadataGenerated = true;
			}
			
			var pathProp = property.FindPropertyRelative("_propertyPath");
			string pp = pathProp.stringValue;
			if (pp != property.propertyPath){
				pathProp.stringValue = property.propertyPath;
				metadataGenerated = true;
			}
			if (metadataGenerated){
				property.serializedObject.ApplyModifiedProperties();
			}
			
			EditorGUI.BeginProperty(position, label, property);
			Vector2 min = position.min;
			Vector2 max = position.max;
			position.height = BasePropertyHeight(property, label);
			DrawGUI(position, property, label, min, max);
			EditorGUI.EndProperty();
		}
		protected abstract void DrawGUI(Rect position, SerializedProperty property, GUIContent label, Vector2 min, Vector2 max);

		protected Rect DrawLabel(Rect position, SerializedProperty property, GUIContent label){
			position.height = BasePropertyHeight(property, label);			
			return EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
		}

		protected bool GetEvents(SerializedProperty property, out SerializedProperty e0, out SerializedProperty e1){
			e0 = property.FindPropertyRelative("_onEvent");
			e1 = null;
			if (e0 == null){
				e0 = property.FindPropertyRelative("_onUpdate");
			}
			if (e0 == null){
				e0 = property.FindPropertyRelative("_onAdd");
				e1 = property.FindPropertyRelative("_onRemove");
			}
			return e0 != null;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label){
			float bph = BasePropertyHeight(property, label);
			float result = -1;
			if (_isEventable){
				SerializedProperty e0, e1;
				GetEvents(property, out e0, out e1);
				if (e0 != null){
					if (_forceExpand || e0.isExpanded){
						result = GetEventHeight(e0) + GetEventHeight(e1) + bph + SPACING + 5;
					} else {
						result = 2*bph;
					}
				}
			} else {
				result = bph;
			}
			if (result != _lastHeight){
				// Force repaint
				property.serializedObject.Update();
				EditorApplication.update();
			}
			_lastHeight = result;
			return result;
		}
		protected float BasePropertyHeight(SerializedProperty property, GUIContent label){
			return base.GetPropertyHeight(property, label);
		}
		static MethodInfo _getPersistentListeners;
		protected int GetPersistentListeners(SerializedProperty evtProp){
			if (_getPersistentListeners == null){
				_getPersistentListeners = evtProp.GetObject().GetType().GetMethod(
					"GetPersistentEventCount",
					BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy
				);
			}
			return (int)_getPersistentListeners.Invoke(evtProp.GetObject(), null);
		}
		float GetEventHeight(SerializedProperty evt, bool includeFooter=true){
			if (evt == null) return 0;
			// Manual event height calc
			int listeners = GetPersistentListeners(evt);
			listeners = Mathf.Max(1, listeners);
			// 42px per listener + header + [+/-]
			return (listeners * 43) + 18 + (includeFooter ? 20 : 0);
		}
		protected void DrawEvent(SerializedProperty property, Rect position, Vector2 min, Vector2 max, bool forceExpand){
			SerializedProperty e0, e1;
			GetEvents(property, out e0, out e1);
			if (e0 != null){
				min.x += 5;
				Rect evtPos = new Rect();
				evtPos.xMax = max.x;
				evtPos.height = position.height;
				_forceExpand = forceExpand;
				bool wasExpanded = e0.isExpanded;
				if (!_forceExpand){
					Rect togglePos = new Rect();
					togglePos.xMin = min.x + 15;
					togglePos.width = (e0.isExpanded ? 20 : 60) - SPACING;
					togglePos.yMin = position.max.y+SPACING;
					togglePos.height = position.height;

					evtPos.xMin = togglePos.max.x + SPACING + SmartEditorUtils.indent;
					evtPos.yMin = togglePos.yMin;

					e0.isExpanded = EditorGUI.Foldout(togglePos, e0.isExpanded, (e0.isExpanded ? "": "Event"));
				} else {
					e0.isExpanded = true;
					evtPos.xMin = min.x;
					evtPos.yMin = position.max.y+SPACING;
				}
				if (e0.isExpanded){
					evtPos.height = GetEventHeight(e0);
					EditorGUI.PropertyField(evtPos, e0);
					if (e1 != null){
						evtPos = new Rect(evtPos.xMin, evtPos.yMin+evtPos.height+EditorGUIUtility.standardVerticalSpacing, evtPos.width, GetEventHeight(e1));
						//evtPos.yMin = evtPos.yMax + EditorGUIUtility.standardVerticalSpacing;
						EditorGUI.PropertyField(evtPos, e1);
					}
					
					SerializedProperty lastEvt = (e1 == null) ? e0 : e1;
					Rect statusRect = new Rect(evtPos.xMin-SmartEditorUtils.indent, evtPos.yMin + GetEventHeight(lastEvt, false) + position.height - 10, 150, 15);
					var autoListen = property.FindPropertyRelative("_autoListen");
					bool writeable = IsWriteable(property, fieldInfo);
					if (Application.isPlaying){
						Color gc = GUI.color;
						if (IsEventEnabled(property, fieldInfo)){
							GUI.color = Color.green;
							EditorGUI.LabelField(
								statusRect, new GUIContent(
									(writeable ? 
										(autoListen.boolValue ? "LOCAL + REMOTE (Auto)" : "LOCAL + REMOTE"):
										"ENABLED"
									),
									"UnityEvent will fire when anything updates the SmartObject."
								),
								EditorStyles.whiteLabel
							);
						} else {
							GUI.color = Color.red;
							EditorGUI.LabelField(
								statusRect, new GUIContent(
									(writeable ? "LOCAL ONLY" : "DISABLED"),
									(writeable ? "UnityEvent will fire only when this instance updates the SmartObject." : "UnityEvent disabled")
								),
								EditorStyles.whiteLabel
							);
						}
						GUI.color = gc;
					} else {
						bool hideAutoListen;
						bool disableAutoListen = IsForceNoAutoListen(property, fieldInfo, out hideAutoListen);
						if (disableAutoListen){
							GUI.enabled = false;
							// Ensure set to false
							if (autoListen.boolValue){
								autoListen.boolValue = false;
								property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
							}
						}
						if (!hideAutoListen){
							string tooltip = 
								disableAutoListen ? 
								"Disabled in code" :
								"Fire UnityEvent when SmartObject is updated externally.\n"+(writeable ? "(Always fires when updated by this instance.)\n":"")+"Adds an extra MonoBehaviour on Awake, to auto-unbind in OnDestroy.";
							autoListen.boolValue = EditorGUI.ToggleLeft(statusRect, new GUIContent("Auto Listen", tooltip), autoListen.boolValue);
						}
						GUI.enabled = true;
					}
				}
				if (e0.isExpanded != wasExpanded){
					property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
				}
			}
		}
		protected void GetSmartFieldRects(SerializedProperty p, GUIContent l, Vector2 max, ref Rect fieldPos, out Rect rwPos, out Rect prePos, bool popup, bool usePre){
			prePos = new Rect();
			prePos.min = fieldPos.min;
			
			prePos.width = 26;
			if (popup){
				// PropertyField ignores indent while manual drawers (e.g. Popup) don't, or something
				prePos.xMin -= SmartEditorUtils.indent;
			}

			prePos.height = BasePropertyHeight(p, l);

			rwPos = new Rect();
			rwPos.xMin = (usePre ? prePos.xMax + SPACING : prePos.xMin) - SmartEditorUtils.indent;
			rwPos.yMin = prePos.yMin;
			rwPos.width = 20 + SmartEditorUtils.indent;
			rwPos.height = prePos.height;

			fieldPos.xMin = rwPos.xMax + SPACING - SmartEditorUtils.indent;
			fieldPos.xMax = max.x;
		}
		protected void DrawReadWriteLabel(Rect pos, SerializedProperty prop, FieldInfo fi, string r="R", string w="W", string rwTip="Read/Write", string rTip="Read-only"){
			GUI.enabled = false;
			EditorGUI.LabelField(pos, r+w, EditorStyles.miniLabel);
			GUI.enabled = true;
			if (IsWriteable(prop, fieldInfo)){
				EditorGUI.LabelField(pos, new GUIContent(r+w, rwTip), EditorStyles.miniBoldLabel);
			} else {
				EditorGUI.LabelField(pos, new GUIContent(r, rTip), EditorStyles.miniBoldLabel);
			}
		}
		
		protected bool IsWriteable(SerializedProperty p, FieldInfo f){
			System.Type pType = f.FieldType;
			if (f.FieldType.IsArray){
				pType = f.FieldType.GetElementType();
			}
			var w = pType.GetMethod("_EDITOR_GetIsWritable", BindingFlags.NonPublic | BindingFlags.Instance);
			var result = w.Invoke(p.GetObject(), null);
			return (bool)result;
		}
		protected bool IsForceEventable(SerializedProperty p, FieldInfo f, out bool forceExpand){
			// Does the field have a ForceEventable attribute directly on it?
			var ats = GetCustomAttributes(p, fieldInfo, typeof(ForceEventableAttribute), true);

			if (ats != null && ats.Length > 0){
				forceExpand = ((ForceEventableAttribute)ats[0]).forceExpand;
				return true;
			}

			forceExpand = false;
			return false;
		}
		protected bool IsForceHideEvent(SerializedProperty p, FieldInfo f){
			var ats = GetCustomAttributes(p, fieldInfo, typeof(ForceHideEventAttribute), true);
			return ats != null && ats.Length > 0;
		}
		protected bool IsForceNoAutoListen(SerializedProperty p, FieldInfo f, out bool hide){
			bool result = false;
			hide = false;

			var ats = GetCustomAttributes(p, fieldInfo, typeof(ForceNoAutoListenAttribute), true);
			if (ats != null && ats.Length > 0){
				hide = ((ForceNoAutoListenAttribute)ats[0]).hide;
				result = true;
			}
			if (!result){
				// No multi index disallows auto bind as no way to know index will be assigned in time for binding
				result = IsForceNoMultiIndex(p, f);
			}
			return result;
		}
		protected bool IsForceNoMultiIndex(SerializedProperty p, FieldInfo f){
			// Attribute only valid on SmartMultiRefs
			if (!typeof(SmartData.Abstract.SmartDataMultiRef<,,>).IsAssignableFrom(f.FieldType)) return false;

			var ats = GetCustomAttributes(p, fieldInfo, typeof(ForceNoMultiIndexAttribute), true);
			return (ats != null && ats.Length > 0);
		}

		protected object[] GetCustomAttributes(SerializedProperty property, FieldInfo field, System.Type type, bool inherit){
			var ats = field.GetCustomAttributes(type, true);
			
			if (ats == null || ats.Length == 0){
				// Is the property an element of a collection?
				if (property.IsArrayElement()){
					// If so, does the collection field have the ForceEventable attribute?
					ats = property.GetArrayField().GetCustomAttributes(type, inherit);
				}
			}

			return ats;
		}

		public void DrawMultiProperty(Rect position, SerializedProperty property, Vector2 min, Vector2 max){
			// Multi field rects
			Rect l = new Rect();
			l.min = position.min;
			l.height = position.height;
			l.width = position.width - (25 + SPACING);

			// Index rect
			Rect indexRect = new Rect();
			indexRect.xMin = l.max.x + SPACING - SmartEditorUtils.indent;
			indexRect.xMax = max.x;
			indexRect.yMin = position.min.y;
			indexRect.height = position.height;
			
			DrawSmart(
				l, property.FindPropertyRelative("_smartMulti"), property, min,
				new Vector2(l.max.x, max.y), true, SmartDataRefBase.RefType.MULTI
			);

			// Disable index if necessary
			bool forceNoMultiIndex = false;
			if (!Application.isPlaying){
				forceNoMultiIndex = IsForceNoMultiIndex(property, fieldInfo);
				if (forceNoMultiIndex){
					GUI.enabled = false;
				}
			} else {
				// At runtime only allow index change via property to ensure binding tracking
				GUI.enabled = false;
			}

			EditorGUI.PropertyField(indexRect, property.FindPropertyRelative("_multiIndex"), GUIContent.none);
			if (forceNoMultiIndex){
				GUI.Label(indexRect, new GUIContent("",  "Disabled in code"));
			}
			GUI.enabled = true;
		}
		
		public void DrawSmart(
			Rect position, SerializedProperty valProp, SerializedProperty refProp, 
			Vector2 min, Vector2 max, bool showCreateBtn, SmartDataRefBase.RefType? rt=null
		){
			Rect fieldPos = new Rect(position.x, position.y, position.width - (18 + SPACING), position.height);

			// In LOCAL mode, trigger dispatch if value changed from editor
			EditorGUI.BeginChangeCheck();
			EditorGUI.PropertyField(fieldPos, valProp, GUIContent.none);
			if (rt.HasValue && rt.Value == SmartDataRefBase.RefType.LOCAL && Application.isPlaying && EditorGUI.EndChangeCheck()){
				var o = refProp.GetObject();
				var d = o.GetType().GetMethod("Dispatch", BindingFlags.Instance | BindingFlags.Public);
				if (d != null){
					d.Invoke(o, null);
				}
			}

			if (showCreateBtn && !Application.isPlaying){
				Rect createBtnPos = new Rect();
				createBtnPos.xMin = fieldPos.max.x + SPACING;// - SmartEditorUtils.indent;
				createBtnPos.xMax = max.x;
				createBtnPos.yMin = fieldPos.yMin;
				createBtnPos.height = fieldPos.height;
				Color gbc = GUI.backgroundColor;
				GUI.backgroundColor = Color.green;
				if (GUI.Button(createBtnPos, _gcCreateBtn, EditorStyles.miniButton)){
					var s = ScriptableObject.CreateInstance(valProp.GetFieldType().FullName);
					s.name = ObjectNames.NicifyVariableName(_baseProperty.name).Replace(" ","");
					valProp.objectReferenceValue = s;
					AssetDatabase.CreateAsset(s, string.Format("{0}/{1}.asset", ProjectPanelPathUtil.GetProjectPath(), s.name));
					EditorGUIUtility.PingObject(s);
				}
				GUI.backgroundColor = gbc;
			}
		}

		protected bool IsEventEnabled(SerializedProperty prop, FieldInfo fieldInfo){
			var o = prop.GetObject();
			return (bool)o.GetType().GetProperty("unityEventOnReceive", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy).GetValue(o, null);
		}
	}
}