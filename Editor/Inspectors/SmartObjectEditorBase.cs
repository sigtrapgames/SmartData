using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using SmartData.Abstract;

namespace SmartData.Editors {
	public abstract class SmartObjectEditorBase : Editor {		
		string _typeName;
		string _displayType;
		string _valueType;
		protected bool _isOnRestoreOverridden = false;
		SerializedProperty _dataType;
		SerializedProperty _description;
		protected virtual bool _const {get {return false;}}

		protected FieldInfo _getRelay;
		protected FieldInfo _getListeners;
		protected PropertyInfo _getListenerCount;

		protected MethodInfo GetDispatchMethod(object o){
			return o.GetType().GetMethod("Dispatch", BindingFlags.Instance | BindingFlags.Public);
		}

		GUIStyle __gsFoldout;
		protected GUIStyle _gsFoldout {
			get {
				if (__gsFoldout == null){
					__gsFoldout = new GUIStyle(EditorStyles.foldout);
					__gsFoldout.fontStyle = FontStyle.Bold;
				}
				return __gsFoldout;
			}
		}
		protected Texture2D _iconWarn {get; private set;}
		protected Texture2D _iconSmart {get; private set;}
		protected Texture2D _iconGo {get; private set;}
		protected Texture2D _iconAsset {get; private set;}
		

		#region Decorators
		static List<System.Type> _decoratorTypes = new List<System.Type>();
		static Texture2D _iconChevronUp, _iconChevronDown;
		static GUILayoutOption[] _decBtnStyle;
		SerializedProperty _decorators;
		string _decoratorFilter;
		bool _showAddDecorators;
		MethodInfo _updateDecoratorsMulti;
		#endregion

		#region Listeners/Refs
		bool _showListeners = false;
		bool _showRefs = false;
		struct RefData {
			public string data {get; private set;}
			public bool rw {get; private set;}
			public RefData(string data, bool rw){
				this.data = data;
				this.rw = rw;
			}
		}
		Dictionary<Object, List<RefData>> _refs = new Dictionary<Object, List<RefData>>();
		protected Dictionary<object, List<MethodInfo>> _listeners = new Dictionary<object, List<MethodInfo>>();
		static List<System.WeakReference> _refsToRemove = new List<System.WeakReference>();
		#endregion

		SmartEditorUtils.SmartObjectType _smartType;
		protected System.Type _tData;
		
		protected virtual void OnEnable(){			
			_typeName = target.GetType().Name;
			_displayType = (string)target.GetType().GetField("DISPLAYTYPE", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
			_valueType = (string)target.GetType().GetField("VALUETYPE", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
			_dataType = serializedObject.FindProperty("_dataType");
			_description = serializedObject.FindProperty("_description");
			
			_updateDecoratorsMulti = target.GetType().GetMethodPrivate("UpdateDecorators", BindingFlags.NonPublic | BindingFlags.Instance);
			_decorators = serializedObject.FindProperty("_decorators");

			if (!_const){
				_getRelay = CacheGetRelay();
				_getListenerCount = _getRelay.FieldType.GetProperty("listenerCount", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
				_getListeners = _getRelay.FieldType.GetField("_listeners", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
				_isOnRestoreOverridden = IsOnRestoreOverridden(target.GetType());
			}

			#region Cache available decorator types
			_decoratorTypes.Clear();

			System.Type tDecBase = null;
			_smartType = SmartEditorUtils.GetSmartObjectType(target as SmartBase, out _tData);
			switch (_smartType){
				case SmartEditorUtils.SmartObjectType.EVENT:
				case SmartEditorUtils.SmartObjectType.EVENT_MULTI:
					// Event decorators
					tDecBase = typeof(SmartEventDecoratorBase);
					break;
				case SmartEditorUtils.SmartObjectType.VAR:
				case SmartEditorUtils.SmartObjectType.MULTI:
					tDecBase = typeof(SmartDataDecoratorBase<>).MakeGenericType(_tData);
					break;
				case SmartEditorUtils.SmartObjectType.SET:
					tDecBase = typeof(SmartSetDecoratorBase<>).MakeGenericType(_tData);
					break;
			}

			if (tDecBase != null){
				foreach (var a in System.AppDomain.CurrentDomain.GetAssemblies()){
					foreach (var t in a.GetTypes()){
						if (!t.IsAbstract && t.IsSubclassOf(tDecBase)){
							_decoratorTypes.Add(t);
						}
					}
				}
			}
			#endregion
		
			_decBtnStyle = new GUILayoutOption[]{
				GUILayout.MaxWidth(18), GUILayout.MaxHeight(18)
			};
			_iconChevronUp = Resources.Load<Texture2D>("GUI/chevron_up");
			_iconChevronDown = Resources.Load<Texture2D>("GUI/chevron_down");
			_iconWarn = EditorGUIUtility.Load("icons/_Help.png") as Texture2D;
			_iconGo = EditorGUIUtility.IconContent("GameObject Icon").image as Texture2D;
			_iconAsset = EditorGUIUtility.IconContent("Prefab Icon").image as Texture2D;
			_iconSmart = SmartEditorUtils.LoadSmartIcon(_smartType);

			PopulateRefs();

			EditorApplication.update += Update;
		}

		public sealed override void OnInspectorGUI(){
			EditorGUI.BeginChangeCheck();
			DrawInspector();
			if (EditorGUI.EndChangeCheck()){
				serializedObject.ApplyModifiedProperties();
			}

			if (!_const){
				#region Draw References
				int count = 0;
				foreach (var a in _refs){
					count += a.Value.Count;
				}
				EditorGUILayout.Space();
				EditorGUILayout.BeginVertical(EditorStyles.helpBox);
				++EditorGUI.indentLevel;
				_showRefs = EditorGUILayout.Foldout(_showRefs, string.Format("GameObject References [{0}]", count), _gsFoldout);
				--EditorGUI.indentLevel;

				if (_showRefs){
					if (GUILayout.Button("Refresh")){
						PopulateRefs();
					}

					EditorGUILayout.Space();
					Color gbc = GUI.backgroundColor;
					GUIStyle goName = new GUIStyle(EditorStyles.largeLabel);
					goName.fontStyle = FontStyle.Bold;
					foreach (var a in _refs){
						EditorGUILayout.BeginVertical(EditorStyles.helpBox);{
							EditorGUILayout.BeginHorizontal();{								
								GUIContent icon = null;
								if (a.Key is GameObject){
									icon = new GUIContent(_iconGo, "GameObject");
								} else {
									icon = new GUIContent(_iconAsset, "Asset");
								}
								EditorGUILayout.LabelField(icon, GUILayout.Width(30), GUILayout.Height(25));
								EditorGUILayout.LabelField(a.Key.name, goName);
								GUILayout.FlexibleSpace();
								GUI.backgroundColor = Color.cyan;
								if (GUILayout.Button("Select", GUILayout.Width(55))){
									Selection.activeObject = a.Key;
								}
								GUI.backgroundColor = Color.magenta;
								if (GUILayout.Button("Ping", GUILayout.Width(55))){
									EditorGUIUtility.PingObject(a.Key);
								}
								GUI.backgroundColor = gbc;
							} EditorGUILayout.EndHorizontal();

							++EditorGUI.indentLevel;
							Color gcc = GUI.contentColor;
							GUILayout.Space(5);
							foreach (var b in a.Value){
								EditorGUILayout.BeginHorizontal();{
									if (b.rw){
										GUI.contentColor = Color.cyan;
										EditorGUILayout.LabelField(new GUIContent(SmartEditorUtils.iconWrite, "Read / Write"), EditorStyles.boldLabel, GUILayout.Width(35), GUILayout.Height(20));
										GUI.contentColor = gcc;
									} else {
										GUI.contentColor = new Color(1,0.6f,0.2f);
										EditorGUILayout.LabelField(new GUIContent(SmartEditorUtils.iconRead, "Read-only"), GUILayout.Width(35), GUILayout.Height(20));
										GUI.contentColor = gcc;
									}
									EditorGUILayout.LabelField(b.data);
								} EditorGUILayout.EndHorizontal();
							}
							--EditorGUI.indentLevel;
						} EditorGUILayout.EndVertical();
					}
				}

				EditorGUILayout.EndVertical();

				if (Application.isPlaying){
					DrawListeners();
				}
				#endregion
			}
		}

		protected abstract void DrawInspector();

		void Update(){
			serializedObject.UpdateIfRequiredOrScript();
			Repaint();
		}
		protected virtual void OnDisable(){
			EditorApplication.update -= Update;
		}
		protected virtual FieldInfo CacheGetRelay(){
			return target.GetType().GetField("_relay", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		}

		protected bool IsOnRestoreOverridden(System.Type t){
			// Determine if OnRestore method has been overridden
			// Get all methods in type and all ancestor types
			foreach (var m in t.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)){
				// Check all overloads if present. Select the one with zero params
				if (m.Name == "OnRestore" && m.GetParameters().Length == 0){
					// Check if method is the base version of the method
					if (m != m.GetBaseDefinition()){
						return true;
					}
				}
			}
			return false;
		}

		#region Draw helpers
		protected void DrawRestoreWarning(SmartVarBase.DataType dt){
			bool showWarning = false;
			string warning = "";
			switch (dt){
				case SmartVarBase.DataType.CLASS:
					showWarning = true;
					warning = "Default reset behaviour for reference types may not be correct.";
					break;
				case SmartVarBase.DataType.DICTIONARY:
					showWarning = true;
					warning = "Default reset behaviour for Dictionaries is suboptimal.";
					break;
				case SmartVarBase.DataType.COLLECTION:
					showWarning = true;
					warning = "Default reset behaviour for collections is suboptimal.";
					break;
			}
			if (showWarning){
				EditorGUILayout.HelpBox(
					string.Format(
						"{0}\nOverride {1}.OnRestore to implement custom behaviour.",
						warning, _typeName
					),
					MessageType.Info
				);
				EditorGUILayout.Space();
			}
		}

		protected void DrawTypeHeader(){
			EditorGUILayout.Space();
			var gs = new GUIStyle(EditorStyles.largeLabel);
			gs.fontSize = 18;
			EditorGUILayout.LabelField("Smart "+_displayType, gs, GUILayout.MaxHeight(25));

			EditorGUILayout.BeginHorizontal();
			gs.fontSize = 16;
			Color gcc = GUI.contentColor;
			GUI.contentColor = SmartEditorUtils.GetSmartColor(_smartType);
			GUILayout.Label(_iconSmart, GUILayout.Width(24));
			GUI.contentColor = gcc;
			EditorGUILayout.LabelField(target.name, gs, GUILayout.MaxHeight(24));
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.Space();
		}
		protected void DrawDescription(){
			if (Application.isPlaying){
				GUI.enabled = false;
			}
			EditorGUILayout.PropertyField(_description);
			GUI.enabled = true;
		}

		protected void DrawCommonHeader(bool canRestore=false){
			DrawTypeHeader();

			if (canRestore){
				if (!_isOnRestoreOverridden){
					SmartVarBase.DataType dt = (SmartVarBase.DataType)_dataType.intValue;
					DrawRestoreWarning(dt);
				}
			}

			DrawDescription();
		}

		protected bool DrawValueField(SerializedProperty valProp, bool critical=false){
			if (valProp != null){
				EditorGUILayout.PropertyField(valProp, true);
				return true;
			} else {
				EditorGUILayout.HelpBox(
					string.Format(
						"{0} is not serializable.\n{1}",
						_valueType,
						critical ? "Const value cannot be set." : "Data can only be set at runtime."
					),
					critical ? MessageType.Error : MessageType.Warning
				);
				return false;
			}
		}

		protected void DrawSetToDefault(){
			if (GUILayout.Button("Reset")){
				target.GetType().GetMethod("SetToDefault", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy).Invoke(target,null);
			}
		}
		#endregion

		protected virtual void DrawListeners(){
			EditorGUILayout.Space();
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			uint c = PopulateListeners(target);
			++EditorGUI.indentLevel;
			_showListeners = EditorGUILayout.Foldout(_showListeners, string.Format("Code Listeners [{0}]", c), _gsFoldout);
			--EditorGUI.indentLevel;
			if (_showListeners){
				foreach (var a in _listeners){
					EditorGUILayout.LabelField(ObjectToString(a.Key));
					for (int j=0; j<a.Value.Count; ++j){
						EditorGUILayout.LabelField("    "+MethodToString(a.Value[j]));
					}
				}
			}
			EditorGUILayout.EndVertical();
		}

		protected void DrawDecorators(string tooltip){
			bool changed = false;
			EditorGUILayout.Space();
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);{
				++EditorGUI.indentLevel;
				_decorators.isExpanded = EditorGUILayout.Foldout(
					_decorators.isExpanded, 
					new GUIContent(string.Format("Decorators [{0}]", _decorators.arraySize), tooltip), _gsFoldout
				);
				--EditorGUI.indentLevel;
				if (_decorators.isExpanded){
					var gs = new GUIStyle(EditorStyles.largeLabel);
					gs.fontSize = 14;
					EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
					for (int i=0; i<_decorators.arraySize; ++i){
						var pd = _decorators.GetArrayElementAtIndex(i);
						var decorator = (SmartDecoratorBase)pd.objectReferenceValue;
						// Array elements are just references
						// Need a separate SO to edit the actual instance
						var so = new SerializedObject(decorator);

						// Nice(r) type name
						var dt = decorator.GetType();
						string dn = dt.Name;
						if (dn.StartsWith("Smart")){
							dn = dn.Substring(5, dn.Length-5);
						}
						if (dn.EndsWith("Decorator")){
							dn = dn.Substring(0, dn.Length-9);
						}

						// Draw decorator name and buttons
						EditorGUILayout.BeginHorizontal(); {
							var spa = so.FindProperty("_active");
							bool active = spa.boolValue;
							active = EditorGUILayout.ToggleLeft(new GUIContent(dn, GetDecoratorDescription(dt)), active, gs, GUILayout.MaxHeight(25));
							if (active != spa.boolValue){
								spa.boolValue = active;
								so.ApplyModifiedProperties();
								if (Application.isPlaying){
									// Call OnActivated/Deactivated and dispatch relay
									typeof(SmartDecoratorBase).GetMethodPrivate("OnSetActive", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(so.targetObject, new object[]{active});
								}
							}

							// Move up
							GUI.enabled = (i!=0);
							if (GUILayout.Button(_iconChevronUp, _decBtnStyle)){
								changed = true;
								var d0 = _decorators.GetArrayElementAtIndex(i-1).objectReferenceValue;
								_decorators.GetArrayElementAtIndex(i-1).objectReferenceValue = decorator;
								pd.objectReferenceValue = d0;
								break;
							}

							// Move down
							GUI.enabled = (i<_decorators.arraySize-1);
							if (GUILayout.Button(_iconChevronDown, _decBtnStyle)){
								changed = true;
								var d1 = _decorators.GetArrayElementAtIndex(i+1).objectReferenceValue;
								_decorators.GetArrayElementAtIndex(i+1).objectReferenceValue = decorator;
								pd.objectReferenceValue = d1;
								break;
							}

							GUI.enabled = true;
							
							// Delete
							Color gc = GUI.color;
							GUI.color = Color.red;
							if (GUILayout.Button("-", _decBtnStyle)){
								changed = true;
								// For some reason Unity won't actually delete non-null entries
								// See https://answers.unity.com/questions/555724/serializedpropertydeletearrayelementatindex-leaves.html
								pd.objectReferenceValue = null;
								_decorators.DeleteArrayElementAtIndex(i);
								// Destroy sub-asset as well or it'll leak
								DestroyImmediate(decorator, true);
								
								serializedObject.ApplyModifiedProperties();
								AssetDatabase.Refresh();
								break;
							}
							GUI.color = gc;
						} EditorGUILayout.EndHorizontal();

						// Draw sub-asset editor, with isolated gui change check
						bool guiChanged = GUI.changed;
						GUI.changed = false;

						EditorGUI.BeginChangeCheck();
						++EditorGUI.indentLevel;
						var sp = so.GetIterator();
						sp.Next(true);
						while (sp.NextVisible(false)){
							if (sp.name == "_active") continue;
							EditorGUILayout.PropertyField(sp, true);
						}
						--EditorGUI.indentLevel;
						if (EditorGUI.EndChangeCheck()){
							so.ApplyModifiedProperties();
						}
						
						GUI.changed = guiChanged;

						EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
					}
				
					// Add decorators
					++EditorGUI.indentLevel;
					_showAddDecorators = EditorGUILayout.Foldout(_showAddDecorators, "Add Decorators...");
					if (_showAddDecorators){
						++EditorGUI.indentLevel;
						_decoratorFilter = EditorGUILayout.TextField(new GUIContent("Filter", "Show decorator type names that contain this string"), _decoratorFilter);
						bool found = false;
						foreach (var t in _decoratorTypes){
							// Filter
							if (string.IsNullOrEmpty(_decoratorFilter) || t.Name.ToLower().Contains(_decoratorFilter.ToLower())){
								found = true;
								EditorGUILayout.BeginHorizontal(); {
									GUILayout.Space(SmartEditorUtils.indent);
									if (GUILayout.Button(new GUIContent(t.Name, GetDecoratorDescription(t)))){
										changed = true;
										// Create decorator
										var s = ScriptableObject.CreateInstance(t);
										// Don't show in project window as sub-asset
										s.hideFlags = HideFlags.HideInHierarchy;
										AssetDatabase.AddObjectToAsset(s, AssetDatabase.GetAssetPath(target.GetInstanceID()));

										// Add reference to new decorator
										++_decorators.arraySize;
										_decorators.GetArrayElementAtIndex(_decorators.arraySize-1).objectReferenceValue = s;
										serializedObject.ApplyModifiedProperties();
										AssetDatabase.Refresh();
									}
								} EditorGUILayout.EndHorizontal();
							}
						}
						if (!found){
							EditorGUILayout.LabelField("No matching decorators");
						}
						--EditorGUI.indentLevel;
					}
					--EditorGUI.indentLevel;
				}
			} EditorGUILayout.EndVertical();

			if (changed && _updateDecoratorsMulti != null){
				_updateDecoratorsMulti.Invoke(target, null);
			}
		}
		string GetDecoratorDescription(System.Type tDecorator){
			foreach (var d in tDecorator.GetCustomAttributes(typeof(SmartData.DecoratorDescriptionAttribute), true)){
				return ((SmartData.DecoratorDescriptionAttribute)d).description;
			}
			return null;
		}

		protected uint PopulateListeners(object smart){
			object relay = _getRelay.GetValue(smart);
			
			var arr = _getListeners.GetValue(relay);
			uint c = (uint)_getListenerCount.GetValue(relay, null);
			var lsnrs = (System.Array)arr;
			
			PropertyInfo delMethod = null;
			PropertyInfo delTarget = null;
			_listeners.Clear();
			uint toIgnore = 0;
			for (int i=0; i<c; ++i){
				var v = lsnrs.GetValue(i);
				if (v != null){
					if (delMethod == null){
						delMethod = v.GetType().GetProperty("Method", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
						delTarget = v.GetType().GetProperty("Target", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
					}
					
					var tgt = delTarget.GetValue(v, null);
					var mth = (MethodInfo)delMethod.GetValue(v, null);
					if (mth.Name.Contains("GetUnityEventInvoke")) {
						++toIgnore;
						continue;
					}

					List<MethodInfo> methods = null;
					if (!_listeners.TryGetValue(tgt, out methods)){
						methods = new List<MethodInfo>();
						_listeners.Add(tgt, methods);
					}
					methods.Add(mth);
				}
			}

			return c - toIgnore;
		}
		void PopulateRefs(){
			_refsToRemove.Clear();
			_refs.Clear();
			foreach (var r in SmartDataRegistry.GetSmartReferences()){
				if (!r.Key.IsAlive){
					// Mark ref for removal
					_refsToRemove.Add(r.Key);
					continue;
				}

				BindingFlags binding = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
				SmartRefBase smartRef = (SmartRefBase)r.Key.Target;
				bool useMultiIndex = false;
				bool writeable = false;
				SmartBase smart = SmartRefBase._EDITOR_GetSmartObject(r.Value, smartRef, out useMultiIndex, out writeable);

				if (smart == target){
					ISmartRefOwnerRedirect redirect = null;
					try {
						Object owner = (Object)r.Value.GetFieldPrivate("_owner", binding).GetValue(smartRef);
						string typeName = owner.GetType().Name;
						if (owner is ISmartRefOwnerRedirect){
							redirect = (owner as ISmartRefOwnerRedirect);
							var redirectedOwner = redirect.GetSmartRefOwner();
							if (redirectedOwner){
								owner = redirect.GetSmartRefOwner();
								typeName = redirect.GetOwnerType().Name;
							} else {
								// ISmartRefOwnerRedirect probably hasn't had its owner populated yet
								Debug.LogWarning("Warning: ISmartRefOwnerRedirect owner probably null", redirect as Object);
								typeName = "MISSING REDIRECT FROM "+typeName;
							}
						} else if (owner is Component){
							owner = (owner as Component).gameObject;
							typeName = typeof(GameObject).Name;
						}
						string pPath = ((string)r.Value.GetFieldPrivate("_propertyPath", binding).GetValue(smartRef)).Replace(".Array.data","");
						List<RefData> data = null;
						if (!_refs.TryGetValue(owner, out data)){
							data = new List<RefData>();
							_refs.Add(owner, data);
						}
						data.Add(new RefData(string.Format("{0}.{1}", typeName, pPath), writeable));
					#pragma warning disable 0168
					} catch (MissingReferenceException e) {
						// Gameobject probably destroyed - remove ref from registry.
						_refsToRemove.Add(r.Key);
						continue;
					}
					#pragma warning restore 0168
				}
			}
			// Sweep marked refs
			foreach (var r in _refsToRemove){
				SmartData.Editors.SmartDataRegistry.UnregisterReference(r);
			}
		}
		protected string MethodToString(MethodInfo m){
			return string.Format("{0}.{1}", m.DeclaringType.Name, m.Name);
		}
		protected string ObjectToString(object o){
			Object u = (o as Object);
			if (u != null){
				return u.name;
			}
			return o.ToString().Replace(o.GetType().Namespace+".", "");
		}
	}
}
