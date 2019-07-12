using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.IO;

namespace SmartData.Editors {

	/// <summary>
	/// Reports currently select folder in Project panel.
	/// Tries to get currently selected asset's folder first.
	/// If no asset selected, returns last selected asset's folder.
	/// </summary>
	public static class ProjectPanelPathUtil {
		static bool _updateSelectionSubbed = false;
		static string _lastPath = null;
		static void OnSelectionChanged(){
			if (Selection.activeObject == null) return;
			bool isAsset = !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(Selection.activeObject));
			if (isAsset){
				_lastPath = GetPathFromAsset(Selection.activeObject);
			}
		}
		public static void Init(){
			if (!_updateSelectionSubbed){
				Selection.selectionChanged += OnSelectionChanged;
				_updateSelectionSubbed = true;
			}
		}
		static readonly int ASSET_LENGTH = "Assets".Length;
		static string GetPathFromAsset(Object o){
			int dpLen = Application.dataPath.Length-ASSET_LENGTH;
			string path = Application.dataPath.Substring(0, dpLen) + AssetDatabase.GetAssetPath(o);
			bool success = true;
			if (!Directory.Exists(path)){
				path = Path.GetDirectoryName(path);
				success = (Directory.Exists(path));
			}
			if (success){
				return path.Substring(dpLen, path.Length-dpLen);
			}
			return null;
		}

		public static string GetProjectPath(){
			Init();

			string path = "Assets";
			bool found = false;
			// Try to get current project panel path
			foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets)){
				var p = GetPathFromAsset(obj);
				if (!string.IsNullOrEmpty(p)){
					path = p;
					found = true;
					break;
				}
			}
			// Fallback on last selected
			if (!found){
				if (!string.IsNullOrEmpty(_lastPath)) {
					path = _lastPath;
					found = true;
				}
			}
			if (found){
				_lastPath = path;
			}
			return path;
		}
	}
	public class SmartTypeCreator : EditorWindow {
		#region Consts
		const string LEGALCHARS = "QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm1234567890_<>,.[]+ ";
		const string SMARTTYPE_HEADER = "// SMARTTYPE ";
		const string SMARTTEMPLATE_HEADER = "// SMARTTEMPLATE ";
		const string SMARTIGNORE_HEADER = "// SMARTIGNORE ";
		const int WIDTH = 640;
		const string ICON_MATCH = "icon: ";
		const string ICON_DEFAULT = "{instanceID: 0}";
		const string ICON_CUSTOM_HEAD = "{fileID: 2800000, guid: ";
		const string ICON_CUSTOM_TAIL = ", type: 3}";
		const string HELP_TEMPLATE =
@"Generate additional custom files when creating types.
Ouptut names must include {0} to include the capitalised type. Do not include the .cs extension.
    e.g. 'My{0}Class' for float generates MyFloatClass.cs

Template files are .txt with {0} for real underlying type name, {1} for capitalised underlying type name and {2} for 'inspector friendly' type name.
    e.g. 'public class My{1}Class : MyBaseClass<{0}>'
    For float this becomes 'public class MyFloatClass : MyBaseClass<float>'
Braces must be doubled except for type tags.
    e.g. 'if (true) {...}' must be 'if (true) {{...}}'

Store templates in <anything>/Editor/Resources and give just the filename without the .txt extension.";
const string HELP_EXCLUDE = 
@"Type auto-complete ignores types with full names containing these patterns.
Uses a simple string.Contains() match. Case-sensitive.
    e.g. 'ne.UI.' would ignore UnityEngine.UI.Text.";
		static readonly Dictionary<string,string> _templateToOutputBase = new Dictionary<string, string>{
			{"SmartVarTemplate", "{0}Var"},
			{"SmartConstTemplate", "{0}Const"},
			{"SmartMultiTemplate", "{0}Multi"},
			{"SmartSetTemplate", "{0}Set"},
			{"ReadSmartVarTemplate", "ReadSmart{0}"},
			{"WriteSmartVarTemplate", "WriteSmart{0}"},
			{"ReadSmartSetTemplate", "ReadSmart{0}Set"},
			{"WriteSmartSetTemplate", "WriteSmart{0}Set"}
		};
		static Dictionary<string,string> _templateToOutput;

		const string TYPE_FIELD_NAME = "SDTC_TF_{0}";
		const int MAX_AUTOCOMPLETE_LINES = 10;
		static readonly char[] SPLIT_TYPENAMES = new char[]{'.', '+'};
		#endregion

		#region Static
		public static bool settingsDirty = false;
		static SmartTypeCreator _i = null;
		static Texture2D _helpIcon;

		static Dictionary<string, string> scriptAbsPathToGuid = new Dictionary <string, string>();

		[MenuItem("Assets/Create/SmartData/Generate Types")]
		static void Init(){
			bool reset = _i == null;
			if (reset){
				_i = CreateInstance<SmartTypeCreator>();
				_i.titleContent = new GUIContent("Generate Smart Types");
			}

			_i.ShowUtility();
			SetHeight();
			
			if (reset){	
				Rect pos = _i.position;
				pos.width = WIDTH;
				_i.position = pos;

				_helpIcon = EditorGUIUtility.Load("icons/_Help.png") as Texture2D;

				_i._isParsingAssemblies = false;
				_i.ParseAssemblies();
			}

			PopulateTemplates();

			_i._path = ProjectPanelPathUtil.GetProjectPath();
			_i._settingsSerialized = new SerializedObject(SmartTypeCreatorSettings.DEFAULT);
			_i._settingsCustomTemplates = _i._settingsSerialized.FindProperty("_customTemplates");
			_i._settingsExclude = _i._settingsSerialized.FindProperty("_typeHelperExcludePatterns");
			_i._settingsFrameTime = _i._settingsSerialized.FindProperty("_asyncTypeLoadFrameTime");
		}

		static void SetHeight(float h=-1){
			if (_i == null) return;
			if (UnityEngine.Event.current != null && UnityEngine.Event.current.type != UnityEngine.EventType.Repaint) return;

			float height = _i.position.height;

			if (h < 0){
				int entries = _i._t.Count;
				if (_i._isRegenerating){
					int lines = 0;
					foreach (var a in _i._regenTypesByPath){
						++lines;
						foreach (var b in a.Value.data){
							++lines;
						}
					}
					height = 73 + (20*(Mathf.Max(0,lines-1)));
				} else {
					height =
						163 + 									// Base height
						(18*(Mathf.Max(0,entries-1))) + 		// 20 pixels per text field
						(_i._showTypeTooltip ? 44 : 0) + 		// 44 pixels for help box
						_i.GetTypeAutocompleteHeight();			// Autocomplete dropdown if visible
				}
			} else {
				height = h + 5;
			}

			Rect p = _i.position;
			p.yMin = Mathf.Max(22, p.yMin);
			_i.position = p;
			_i.minSize = _i.maxSize = new Vector2(_i._isRegenerating ? WIDTH * 2 : WIDTH, height);
		}
		static void CacheAssemblies(Assembly a0, List<Assembly> results){
			if (a0 == null) return;
			if (!results.Contains(a0)){
				results.Add(a0);
				foreach (var an in a0.GetReferencedAssemblies()){
					var a = Assembly.Load(an);
					CacheAssemblies(a, results);
				}
			}
		}
		/// <summary>
		/// Populate templates from settings. Return false if duplicates present.
		/// </summary>
		static bool PopulateTemplates(){
			_templateToOutput = new Dictionary<string, string>(_templateToOutputBase);
			var custom = SmartTypeCreatorSettings.DEFAULT.customTemplates;
			bool result = true;
			for (int i=0; i<custom.Count; ++i){
				if (!_templateToOutput.ContainsKey(custom[i].templateFile)){
					_templateToOutput.Add(custom[i].templateFile, custom[i].outputFile);
				} else {
					result = false;
				}
			}
			return result;
		}
		#endregion

		List<string> _t = new List<string>();
		List<string> _toRemove = new List<string>();
		string _path;
		bool _overwrite = false;
		bool _subfolders = true;
		bool _isParsingAssemblies = false;
		bool _subscribedToUpdate = false;
		float _parseProgress = 0;
		bool _showTypeTooltip = false;

		Vector2 _scroll;
		bool _showSettings = false;
		SerializedObject _settingsSerialized;
		SerializedProperty _settingsCustomTemplates;
		bool _showSettingsCustomTemplateTooltip = false;
		SerializedProperty _settingsExclude;
		bool _showSettingsExcludeTooltip = false;
		SerializedProperty _settingsFrameTime;
		bool _settingsValid = true;

		List<Assembly> _assemblies = new List<Assembly>();
		System.Type[] _types = null;
		System.Text.StringBuilder _genericPattern = new System.Text.StringBuilder();
		System.Text.StringBuilder _genericReplace = new System.Text.StringBuilder();
		int _assemblyIndex = 0;
		int _typeIndex = 0;
		int _typesCount = 0;
		System.Diagnostics.Stopwatch _parseSw = new System.Diagnostics.Stopwatch();

		string _lastTypeName;
		Vector2 _typeAutocompleteScroll;
		List<string> _typesAutocompleted = new List<string>();
		List<string> _typeNames = new List<string>();
		System.Diagnostics.Stopwatch _acSw = new System.Diagnostics.Stopwatch();

		bool _matching = false;
		int _typeMatchIndex = 0;

		protected class RegenData {
			public enum Mode {OVERWRITE, NEW, UNSURE}
			public Mode mode;
			public string typeName;
			public string outputFileName;
			public string templateFileName;
			public bool regen = true;
		}
		protected class RegenCategory {
			public bool show;
			public bool regen = true;
			public string path;
			public List<RegenData> data = new List<RegenData>();

			public RegenCategory(string path){
				this.path = path;
			}
		}

		Dictionary<string, RegenCategory> _regenTypesByPath = new Dictionary<string, RegenCategory>();
		bool _isRegenerating {get {return _regenTypesByPath.Count > 0;}}
		
		#region Messages
		void OnFocus(){
			Refresh();
		}
		void OnGUI(){
			bool repaint = false;

			if (_t.Count == 0){
				_t.Add("");
			}

			Color gbc = GUI.backgroundColor;
			EditorGUILayout.BeginVertical();{	// Used to GetLastRect to control total height of window
				EditorGUILayout.Space();

				if (!_isRegenerating){
					EditorGUILayout.LabelField("Data Types", GUILayout.MaxWidth(70));
					if (InlineHelpButton()){
						_showTypeTooltip = !_showTypeTooltip;
					}

					if (_isParsingAssemblies){
						EditorGUI.ProgressBar(GUILayoutUtility.GetLastRect(), _parseProgress, "Parsing Types");
					}

					++EditorGUI.indentLevel;
					if (_showTypeTooltip){
						EditorGUILayout.HelpBox("Case-sensitive. Include any namespaces/nesting.\nBasic types (e.g. int, float) and Unity types (e.g. Vector3) don't need namespaces.", MessageType.Info);
					}
					_toRemove.Clear();

					int focusedField = -1;
					var btnOptions = new GUILayoutOption[]{GUILayout.Width(20), GUILayout.Height(13)};
					bool missingGenericArgs = false;
					for (int i=0; i<_t.Count; ++i){
						bool typeIsOpenGeneric = false;
						string focusName = string.Format(TYPE_FIELD_NAME, i);
						EditorGUILayout.BeginHorizontal();{
							GUI.SetNextControlName(focusName);
							if (_t[i].Contains("#") || _t[i].Contains("<,") || _t[i].Contains(",,") || _t[i].Contains("<>") || _t[i].Contains(",>")){
								GUI.backgroundColor = Color.red;
								missingGenericArgs = true;
								typeIsOpenGeneric = true;
							}
							_t[i] = WithoutSelectAll(()=>EditorGUILayout.TextField(_t[i]));
							GUI.backgroundColor = gbc;

							GUI.backgroundColor = Color.red;
							GUI.enabled = _t.Count > 1;
							if (GUILayout.Button("-", btnOptions)){
								_toRemove.Add(_t[i]);
							}
							GUI.enabled = true;
							GUI.backgroundColor = gbc;
						} EditorGUILayout.EndHorizontal();
						
						if (typeIsOpenGeneric){
							EditorGUILayout.HelpBox("Replace all # with type arguments", MessageType.Error);
						}

						// Get focused field for autocomplete
						string focus = GUI.GetNameOfFocusedControl();
						if (focus == focusName){
							if (settingsDirty){
								OnFocus();
							}
							if (!string.IsNullOrEmpty(_t[i])){
								focusedField = i;
							} else {
								_lastTypeName = null;
							}
						}
					}

					// Autocomplete
					if (!_isParsingAssemblies && focusedField >= 0){
						// Check if changed
						string t = _t[focusedField];
						if (t != _lastTypeName){
							_typeAutocompleteScroll = Vector2.zero;
							_lastTypeName = t;
							// Start match
							_typeMatchIndex = 0;
							_typesAutocompleted.Clear();
							_matching = true;
						}
						
						// Match
						if (_matching){
							_acSw.Start();
							bool skip = false;
							while (_typeMatchIndex<_typeNames.Count){							
								if (_typeNames[_typeMatchIndex].Contains(t)){
									_typesAutocompleted.Add(_typeNames[_typeMatchIndex]);
								}
								++_typeMatchIndex;
								if (_acSw.Elapsed.TotalSeconds > SmartTypeCreatorSettings.DEFAULT.asyncTypeLoadFrameTime){
									skip = true;
									repaint = true;
									break;
								}
							}
							if (!skip){
								// Finished
								_matching = false;
							}
							_acSw.Stop();
							_acSw.Reset();
						}

						// Autocomplete box
						if (_typesAutocompleted.Count > 0){
							_typeAutocompleteScroll = EditorGUILayout.BeginScrollView(
								_typeAutocompleteScroll,
								GUILayout.Height(GetTypeAutocompleteHeight()),
								GUILayout.ExpandWidth(true)
							);{
								EditorGUILayout.BeginHorizontal(); {
									GUILayout.Space(30);
									GUI.backgroundColor = _matching ? Color.yellow : Color.green;
									EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Height(_typesAutocompleted.Count*20)); {
										// Fake header space
										// Don't render occluded elements, according to scroll
										int offset = Mathf.Max(0, Mathf.FloorToInt(_typeAutocompleteScroll.y / 20)-1);
										GUILayout.Space(offset * 20);
										GUI.backgroundColor = gbc;
										for (int i=offset; i<_typesAutocompleted.Count; ++i){
											EditorGUILayout.BeginHorizontal();{
												if (GUILayout.Button("Select", GUILayout.MaxWidth(75))){
													_t[focusedField] = _typesAutocompleted[i];
													GUI.FocusControl(null);
													repaint = true;
												}
												EditorGUILayout.LabelField(_typesAutocompleted[i]);
											} EditorGUILayout.EndHorizontal();
											if (i > offset+MAX_AUTOCOMPLETE_LINES+1) break;
										}
									} EditorGUILayout.EndVertical();
								} EditorGUILayout.EndHorizontal();
							} EditorGUILayout.EndScrollView();
						}
					} else {
						_matching = false;
						_lastTypeName = null;
						_typesAutocompleted.Clear();
					}

					if (_toRemove.Count > 0){
						foreach (string s in _toRemove){
							_t.Remove(s);
						}
						_toRemove.Clear();
					}

					EditorGUILayout.BeginHorizontal(); {
						GUI.enabled = false;
						EditorGUILayout.TextField("");
						GUI.enabled = true;
						GUI.backgroundColor = Color.green;
						if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(14))){
							_t.Add("");
						}
						GUI.backgroundColor = gbc;
					} EditorGUILayout.EndHorizontal();
					--EditorGUI.indentLevel;

					EditorGUILayout.BeginHorizontal(); {
						_path = EditorGUILayout.TextField(
							new GUIContent("Path", "Folder for generated scripts. If Individual Subfolders ticked, subfolders will be created here."),
							_path
						);
						if (GUILayout.Button("Select...", GUILayout.Width(75))){
							string p = EditorUtility.OpenFolderPanel("Select folder for generated Smart Types", _path, "");
							if (!string.IsNullOrEmpty(p)){
								_path = SmartEditorUtils.ToRelativePath(p);
							}
						}
					} EditorGUILayout.EndHorizontal();

					_overwrite = EditorGUILayout.Toggle(new GUIContent("Overwrite", "If false, will ignore types that already exist in the target path."), _overwrite);
					_subfolders = EditorGUILayout.Toggle(new GUIContent("Individual Subfolders", "Create subfolders for each type within the target path."), _subfolders);

					EditorGUILayout.Space();
					EditorGUILayout.BeginHorizontal(EditorStyles.helpBox); {
						EditorGUILayout.BeginVertical(GUILayout.Width(150)); {
							GUI.backgroundColor = Color.green;
							GUI.enabled = !missingGenericArgs && _settingsValid;
							if (GUILayout.Button("Create")){
								PopulateTemplates();
								int successes = 0;
								scriptAbsPathToGuid.Clear();
								for (int i=0; i<_t.Count; ++i){
									if (CreateType(_t[i], _overwrite)){
										++successes;
										_toRemove.Add(_t[i]);
									}
								}
								if (successes > 0){
									if (successes == _t.Count){
										ClosePopup();
									} else {
										// Remove successful types
										foreach (string s in _toRemove){
											_t.Remove(s);
										}
									}
									AssetDatabase.Refresh();
									PostProcessMeta();
								}
							}
							
							GUI.enabled = _settingsValid;
							GUI.backgroundColor = Color.cyan;
							if (GUILayout.Button("Regenerate...")){
								PopulateRegen();
							}
							GUI.backgroundColor = Color.magenta;
							if (GUILayout.Button("Regenerate All")){
								PopulateRegen();
								RegenNow();
							}
							GUI.backgroundColor = gbc;
							GUI.enabled = true;
						} EditorGUILayout.EndVertical();

						DrawSettings();
					} EditorGUILayout.EndHorizontal();
				} else {
					// Regen mode
					_scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.MinHeight(300));{
						EditorGUILayout.LabelField("Regenerate types:");
						EditorGUILayout.Space();
						++EditorGUI.indentLevel;

						GUIStyle infoStyle = new GUIStyle(EditorStyles.helpBox);
						infoStyle.fontSize = 9;
						infoStyle.fontStyle = FontStyle.Bold;
						RectOffset margin = infoStyle.margin;
						margin.top = 0;
						margin.bottom = 1;
						infoStyle.margin = margin;

						foreach (var a in _regenTypesByPath){
							EditorGUILayout.BeginHorizontal(); {
								a.Value.regen = EditorGUILayout.Toggle(a.Value.regen, GUILayout.Width(25));
								a.Value.show = EditorGUILayout.Foldout(a.Value.show, a.Key);
							} EditorGUILayout.EndHorizontal();
							
							if (a.Value.show){
								GUI.enabled = a.Value.regen;
								EditorGUI.indentLevel += 2;
								EditorGUILayout.BeginHorizontal(); {
									// Filename
									EditorGUILayout.BeginVertical(); {
										EditorGUILayout.LabelField("Filename", EditorStyles.miniBoldLabel);
										foreach (var data in a.Value.data){
											switch (data.mode){
												case RegenData.Mode.NEW:
														GUI.backgroundColor = Color.blue;
														break;
													case RegenData.Mode.UNSURE:
														GUI.backgroundColor = Color.yellow;
														break;
											}
											data.regen = EditorGUILayout.ToggleLeft(data.outputFileName, data.regen);	
											GUI.backgroundColor = gbc;
										}
									} EditorGUILayout.EndVertical();

									// SmartType
									EditorGUILayout.BeginVertical(); {
										EditorGUILayout.LabelField("SmartType", EditorStyles.miniBoldLabel);
										foreach (var data in a.Value.data){
											EditorGUILayout.LabelField(data.typeName);
										}
									} EditorGUILayout.EndVertical();

									// Template file
									EditorGUILayout.BeginVertical(); {
										EditorGUILayout.LabelField("Template", EditorStyles.miniBoldLabel);
										foreach (var data in a.Value.data){
											EditorGUILayout.LabelField(data.templateFileName);
										}
									} EditorGUILayout.EndVertical();

									// Additional info
									EditorGUILayout.BeginVertical(); {
										EditorGUILayout.LabelField("Misc", EditorStyles.miniBoldLabel);
										foreach (var data in a.Value.data){
											switch (data.mode){
												case RegenData.Mode.NEW:
													GUI.backgroundColor = Color.blue;
													EditorGUILayout.LabelField("New", infoStyle);
													GUI.backgroundColor = gbc;
													break;
												case RegenData.Mode.UNSURE:
													GUI.backgroundColor = Color.yellow;
													EditorGUILayout.LabelField("Check if this is the right file to overwrite!", infoStyle);
													GUI.backgroundColor = gbc;
													break;
												case RegenData.Mode.OVERWRITE:
													EditorGUILayout.LabelField("");
													break;
											}
										}
									} EditorGUILayout.EndVertical();
								} EditorGUILayout.EndHorizontal();
								EditorGUI.indentLevel -= 2;
								GUI.enabled = true;
							}
						}
						--EditorGUI.indentLevel;
					} EditorGUILayout.EndScrollView();

					EditorGUILayout.Space();
					EditorGUILayout.Space();

					EditorGUILayout.BeginHorizontal(EditorStyles.helpBox); {
						EditorGUILayout.BeginVertical(GUILayout.Width(150)); {
							GUI.backgroundColor = Color.green;
							if (GUILayout.Button("Regenerate!")){
								RegenNow();
							}

							GUI.backgroundColor = Color.red;
							if (GUILayout.Button("Cancel")){
								_regenTypesByPath.Clear();
							}
							GUI.backgroundColor = gbc;
						} EditorGUILayout.EndVertical();

						DrawSettings();
					} EditorGUILayout.EndHorizontal();
				}
			} EditorGUILayout.EndVertical();

			// GetLastRect for entire vertical group to control total height of window
			SetHeight(GUILayoutUtility.GetLastRect().height);			

			if (repaint){
				Repaint();
			}
		}
		#endregion

		void PopulateRegen(){
			var files = AssetDatabase.FindAssets("t:Script");
			var typesToTemplatesFound = new Dictionary<string, List<string>>();
			for (int i=0; i<files.Length; ++i){
				var f = files[i];
				EditorUtility.DisplayProgressBar("Detecting Smart Types", string.Format("Reading script {0} / {1}", i, files.Length), (float)i/(float)files.Length);
				string path = AssetDatabase.GUIDToAssetPath(f);
				using (var file = File.OpenText(SmartEditorUtils.ToAbsolutePath(path))){
					var line = file.ReadLine();
					if (!string.IsNullOrEmpty(line) && line.StartsWith(SMARTTYPE_HEADER)){
						RegenData r = new RegenData();
						
						// Get data type from header
						// SMARTTYPE {data type}
						r.typeName = line.Replace(SMARTTYPE_HEADER, "").Trim();
						var nextLine = file.ReadLine();
						if (nextLine.StartsWith(SMARTTEMPLATE_HEADER)){
							// Get template file
							// SMARTTEMPLATE {template name}
							r.templateFileName = nextLine.Replace(SMARTTEMPLATE_HEADER, "").Trim();
						}

						r.outputFileName = path;
						path = SmartEditorUtils.ToDirectory(path);
						r.outputFileName = r.outputFileName.Replace(path+"/", "");
						RegenCategory cat = null;
						if (!_regenTypesByPath.TryGetValue(path, out cat)){
							cat = new RegenCategory(path);
							_regenTypesByPath.Add(path, cat);
						}
						cat.data.Add(r);

						if (!string.IsNullOrEmpty(r.templateFileName)){
							List<string> typeTemplates = null;
							if (!typesToTemplatesFound.TryGetValue(r.typeName, out typeTemplates)){
								typeTemplates = new List<string>();
								typesToTemplatesFound.Add(r.typeName, typeTemplates);
							}
							if (!typeTemplates.Contains(r.templateFileName)){
								typeTemplates.Add(r.templateFileName);
							} else {
								Debug.LogWarning("Multiple SmartData types using template "+r.templateFileName);
							}
						}
					}
				}
			}

			int j=0;
			List<string> allTemplates = new List<string>();
			foreach (var a in typesToTemplatesFound){
				EditorUtility.DisplayProgressBar("Finding Smart Types without all current templated types", string.Format("Finding untemplated files for type {0} / ({1} / {2})", a.Key, j, files.Length), (float)j/(float)typesToTemplatesFound.Count);
				// Get all known templates, knock them off one by one
				allTemplates.Clear();
				foreach (var b in _templateToOutput){
					allTemplates.Add(b.Key);
				}
				foreach (var template in a.Value){
					if (allTemplates.Contains(template)){
						allTemplates.Remove(template);
					} else {
						Debug.LogWarningFormat("Missing template file: {0}.txt. Is it missing from Custom Templates, or has it been renamed?", template);
					}
				}
				// Check for remaining templates				
				foreach (var t in allTemplates){
					string filename = string.Format(_templateToOutput[t], PrettyTypeName(a.Key));
					// Look for files that would have resulted from each template
					var missingFiles = AssetDatabase.FindAssets("t:Script "+filename);
					if (missingFiles.Length > 0){
						// Found candidates for regen
						foreach (var mf in missingFiles){
							string path = SmartEditorUtils.ToDirectory(AssetDatabase.GUIDToAssetPath(mf));
							RegenCategory cat = null;
							if (!_regenTypesByPath.TryGetValue(path, out cat)){
								cat = new RegenCategory(path);
								_regenTypesByPath.Add(path, cat);
							}
							RegenData data = new RegenData();
							data.mode = RegenData.Mode.UNSURE;
							data.regen = false;
							data.outputFileName = filename+".cs";
							data.templateFileName = t;
							data.typeName = a.Key;
							
							cat.data.Add(data);
							// Default to showing categories with new files
							cat.show = true;
						}
					} else {
						// None found - need to make new one
						// Get path - guess at putting next to existing files
						string newPath = null;
						// Loop over existing paths
						foreach (var b in _regenTypesByPath){
							// Loop over files being regenned at path
							foreach (var r in b.Value.data){
								// If regenned file has this underlying type, let's put the new file next to it
								if (r.typeName == a.Key){
									newPath = b.Key;
								}
							}
							if (!string.IsNullOrEmpty(newPath)) break;
						}
						if (!string.IsNullOrEmpty(newPath)){
							RegenData newGen = new RegenData();
							newGen.mode = RegenData.Mode.NEW;
							newGen.outputFileName = filename+".cs";
							newGen.templateFileName = t;
							newGen.typeName = a.Key;

							_regenTypesByPath[newPath].data.Add(newGen);
							// Default to showing categories with unsure overwrites
							_regenTypesByPath[newPath].show = true;
						} else {
							Debug.LogErrorFormat("Found no existing SmartTypes for type {0} template {1}", a.Key, t);
						}
					}
				}
				++j;
			}

			
			foreach (var a in _regenTypesByPath){
				foreach (var r in a.Value.data){
					if (r.mode != RegenData.Mode.OVERWRITE){
						a.Value.show = true;
					}
				}
			}

			EditorUtility.ClearProgressBar();
		}
		void RegenNow(){
			if (!PopulateTemplates()){
				Debug.LogError("Duplicate template file in custom template settings - cannot regenerate SmartTypes");
			}
			scriptAbsPathToGuid.Clear();
			foreach (var a in _regenTypesByPath){
				// Ignore entire folder
				if (!a.Value.regen) continue;
				// Check per type
				foreach (var r in a.Value.data){
					if (r.regen){
						CreateType(r.typeName, true, a.Key, r.templateFileName);
					}
				}
			}
			_regenTypesByPath.Clear();
			AssetDatabase.Refresh();
			PostProcessMeta();
			ClosePopup();
		}
		void PostProcessMeta(){
			#region Repair GUIDS
			bool refresh = false;
			foreach (var a in scriptAbsPathToGuid){
				// Get latest guid of script from assetdatabase and compare to cached to see if changed
				string newGuid = AssetDatabase.AssetPathToGUID(SmartEditorUtils.ToRelativePath(a.Key));
				if (newGuid != a.Value){
					Debug.LogWarningFormat("Repairing changed GUID for script {0}", a.Key);
					string metaPath = a.Key + ".meta";

					#region Repair meta file
					if (File.Exists(metaPath)){
						// Find line with latest guid and replace with cached guid
						var lines = File.ReadAllLines(a.Key);
						for (int i=0; i<lines.Length; ++i){
							string l = lines[i];
							if (l.Contains(newGuid)){
								Debug.LogFormat("\n\tRestoring {0} to {1}", newGuid, a.Value);
								lines[i] = l.Replace(newGuid, a.Value);

								// Save and flag for asset database refresh
								File.WriteAllLines(a.Key, lines);
								refresh = true;
								break;
							}
						}
					}
					#endregion
				}
			}
			if (refresh){
				AssetDatabase.Refresh();
			}
			#endregion

			#region Set icons
			refresh = false;
			foreach (var a in scriptAbsPathToGuid){
				string metaPath = a.Key + ".meta";
				if (File.Exists(metaPath)){
					var lines = File.ReadAllLines(metaPath);

					// Read meta file, look for line with "icon:"
					for (int i=0; i<lines.Length; ++i){
						string l = lines[i];
						
						// Line must have "icon: " and default icon stuff
						if (l.Contains(ICON_MATCH)){
							// Get type of script, then get associated icon from utilities
							string pathToScript = AssetDatabase.GUIDToAssetPath(a.Value);
							MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(pathToScript);
							System.Type tData;
							var sot = SmartEditorUtils.GetSmartObjectType(script.GetClass(), out tData);
							
							//TODO Implement icons for component types
							if (sot == SmartEditorUtils.SmartObjectType.NONE) break;
							Texture2D icon = SmartEditorUtils.LoadSmartIcon(sot, false, true);
							
							// Get guid of icon and jam it in there
							string pathToIcon = AssetDatabase.GetAssetPath(icon);
							string iconGuid = AssetDatabase.AssetPathToGUID(pathToIcon);

							bool modified = false;
							if (l.Contains(ICON_DEFAULT)){
								// Replace entire default icon
								l = l.Replace(ICON_DEFAULT, string.Format("{{fileID: 2800000, guid: {0}, type: 3}}", iconGuid));
								modified = true;
							} else if (l.Contains(ICON_CUSTOM_HEAD)){
								// Isolate guid to check if changed
								string check = l.Trim().Replace(ICON_MATCH,"").Replace(ICON_CUSTOM_HEAD,"").Replace(ICON_CUSTOM_TAIL,"");
								if (check != iconGuid){var sb = new System.Text.StringBuilder();

									// Whitespace
									for (int j = 0; j<l.Length; ++j){
										if (l[j] != ' ') break;
										sb.Append(' ');
									}
									
									// Build with new guid
									sb.Append(ICON_MATCH);
									sb.Append(ICON_CUSTOM_HEAD);
									sb.Append(iconGuid);
									sb.Append(ICON_CUSTOM_TAIL);
									l = sb.ToString();

									modified = true;
								}
							}				
							
							if (modified){
								Debug.Log("Setting icon for script "+a.Key);
								// Save and flag for asset database refresh
								lines[i] = l;
								File.WriteAllLines(metaPath, lines);
								refresh = true;
							}
							break;
						}
					}
				} else {
					Debug.LogError("Meta file not found: "+metaPath);
				}
			}
			#endregion

			if (refresh){
				AssetDatabase.Refresh();
			}
		}

		#region Internal Methods
		void Refresh(){
			if (settingsDirty){
				settingsDirty = false;
				_lastTypeName = null;
				if (!_isParsingAssemblies){
					ParseAssemblies();
				}
				PopulateTemplates();
			}
		}
		bool InlineHelpButton(){
			var btnRect = GUILayoutUtility.GetLastRect();
			btnRect.xMin = btnRect.xMax;
			btnRect.width = 20;
			return GUI.Button(btnRect, _helpIcon, EditorStyles.label);
		}
		bool HelpButton(params GUILayoutOption[] options){
			return GUILayout.Button(_helpIcon, EditorStyles.label, options);
		}

		static List<string> _tempCheckDupTemplateNames = new List<string>();
		void DrawSettings(){
			var templates = SmartTypeCreatorSettings.DEFAULT.customTemplates;
			int badOutputIndex = -1;
			int dupInputIndex = -1;
			_tempCheckDupTemplateNames.Clear();
			for (int i=0; i<templates.Count; ++i){
				if (badOutputIndex < 0 && !templates[i].outputFile.Contains("{0}")){
					badOutputIndex = i;
				}
				if (dupInputIndex < 0 && _tempCheckDupTemplateNames.Contains(templates[i].templateFile)){
					dupInputIndex = i;
				}
				_tempCheckDupTemplateNames.Add(templates[i].templateFile);
			}
			_settingsValid = badOutputIndex < 0 && dupInputIndex < 0;

			Color gbc = GUI.backgroundColor;
			if (!_showSettings){
				GUI.backgroundColor = (_settingsValid) ? gbc : Color.red;
			}
			EditorGUILayout.BeginVertical(EditorStyles.helpBox); {
				GUI.backgroundColor = gbc;
				_showSettings = EditorGUILayout.Foldout(_showSettings, "Advanced");
				
				if (_showSettings){
					EditorGUI.BeginChangeCheck();
					DrawWithHelp(_settingsCustomTemplates, HELP_TEMPLATE, ref _showSettingsCustomTemplateTooltip);
					bool templatesChanged = EditorGUI.EndChangeCheck();

					// Check output names
					if (badOutputIndex >= 0){
						GUI.backgroundColor = Color.red;
						EditorGUILayout.HelpBox(string.Format("Custom template [{0}] output filename must include '{{0}}'", badOutputIndex), MessageType.Error);
						GUI.backgroundColor = gbc;
					}
					// Check duplicates
					if (dupInputIndex >= 0){
						GUI.backgroundColor = Color.red;
						EditorGUILayout.HelpBox(string.Format("Custom template [{0}] name must be unique", dupInputIndex), MessageType.Error);
						GUI.backgroundColor = gbc;
					}

					EditorGUI.BeginChangeCheck();
					DrawWithHelp(_settingsExclude, HELP_EXCLUDE, ref _showSettingsExcludeTooltip);
					EditorGUILayout.PropertyField(_settingsFrameTime);
					if (EditorGUI.EndChangeCheck() || templatesChanged){
						_settingsSerialized.ApplyModifiedProperties();
						_settingsSerialized.Update();
					}
					if (templatesChanged){
						PopulateTemplates();
					}
				}
			} EditorGUILayout.EndVertical();
			GUI.backgroundColor = gbc;
		}
		void DrawWithHelp(SerializedProperty p, string help, ref bool showHelp){
			EditorGUILayout.BeginHorizontal();
			if (HelpButton(GUILayout.Width(20))){
				showHelp = !showHelp;
			}
			if (showHelp){
				Color gbc = GUI.backgroundColor;
				GUI.backgroundColor = Color.yellow;
				EditorGUILayout.HelpBox(help, MessageType.None);
				GUI.backgroundColor = gbc;
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(20);
			}
			EditorGUILayout.PropertyField(p, true);
			EditorGUILayout.EndHorizontal();
		}
		void LoadAssembly(int index){
			_types = _assemblies[index].GetTypes();
			_typesCount = _types.Length;
			_typeIndex = 0;
		}

		/// <summary>
		/// Asynchronously get names of all types
		/// </summary>
		void ParseAssemblies(){
			if (!_subscribedToUpdate){
				// Add self to update delegate
				EditorApplication.update += ParseAssemblies;
				_subscribedToUpdate = true;
			}
			Repaint();
			if (!_isParsingAssemblies){
				_assemblies.Clear();
				CacheAssemblies(Assembly.GetExecutingAssembly(), _assemblies);
				_assemblyIndex = 0;
				_isParsingAssemblies = true;
				_typeNames.Clear();

				LoadAssembly(0);
			}

			_parseSw.Start();
			var exclude = SmartTypeCreatorSettings.DEFAULT.typeHelperExcludePatterns;
			double frameTime = SmartTypeCreatorSettings.DEFAULT.asyncTypeLoadFrameTime;
			System.Type attrType = typeof(System.Attribute);
			
			while (_assemblyIndex < _assemblies.Count){
				while (_typeIndex < _typesCount){
					float numerator = (float)_assemblyIndex + ((float)_typeIndex/(float)_typesCount);
					_parseProgress = numerator / (float)_assemblies.Count;
					System.Type t = _types[_typeIndex];
					string tn = t.FullName;
					bool add = true;

					// Only use public, non-abstract (also non-static), non-interface, non-attribute types
					if (t.IsNotPublic || t.IsAbstract || t.IsInterface || attrType.IsAssignableFrom(t)){
						add = false;
					}
					
					// Check type isn't abstract
					if (add){
						if (t.IsAbstract){
							add = false;
						}
					}

					// Check type isn't a coroutine implementation
					if (add){
						var split = tn.Split(SPLIT_TYPENAMES);
						if (split[split.Length-1].StartsWith("<")){
							add = false;
						}
					}

					// Check type isn't excluded by pattern
					if (add){
						for (int i=0; i<exclude.Length; ++i){
							if (tn.Contains(exclude[i])){
								add = false;
								break;
							}
						}
					}

					if (add){
						// Check for generic args and make more friendly
						while (tn.Contains("`")){
							for (int i=0; i<tn.Length; ++i){
								if (tn[i] == '`'){
									#region Find `1 or `2 etc etc
									_genericPattern.Length = 0;
									
									for (int j=i+1; j<tn.Length; ++j){
										if (!System.Char.IsDigit(tn[j])) break;
										_genericPattern.Append(tn[j]);
									}
									// Parse resulting int
									int numArgs = int.Parse(_genericPattern.ToString());
									// Insert ` at front to form replace pattern
									_genericPattern.Insert(0, '`');
									#endregion

									#region Create <#,#,#> replacement
									_genericReplace.Length = 0;
									_genericReplace.Append('<');
									for (int j=0; j<numArgs-1; ++j){
										_genericReplace.Append("#,");
									}
									_genericReplace.Append("#>");
									#endregion

									tn = tn.Replace(_genericPattern.ToString(), _genericReplace.ToString());
									break;
								}
							}
						}
						_typeNames.Add(tn);
					}

					++_typeIndex;
					
					// If frame budget up, break out of this execution, wait for update again
					if (_parseSw.Elapsed.TotalSeconds > frameTime){
						_parseSw.Stop();
						_parseSw.Reset();
						return;
					}
				}

				// Enumerated all types in assembly - load next
				++_assemblyIndex;
				if (_assemblyIndex < _assemblies.Count){
					LoadAssembly(_assemblyIndex);
				}
			}

			// If we got to here without returning by going over frame budget, we're all done!
			_isParsingAssemblies = false;
			// Unsub self
			EditorApplication.update -= ParseAssemblies;
			_subscribedToUpdate = false;
		}
		T WithoutSelectAll<T>(System.Func<T> guiCall){
			// https://answers.unity.com/questions/210808/using-guifocuscontrol-on-textfield-selects-all-tex.html
			bool preventSelection = (Event.current.type == EventType.MouseDown);
			Color oldCursorColor = GUI.skin.settings.cursorColor;
		
			if (preventSelection){
				GUI.skin.settings.cursorColor = new Color(0, 0, 0, 0);
			}
		
			T value = guiCall();
		
			if (preventSelection){
				GUI.skin.settings.cursorColor = oldCursorColor;
			}
		
			return value;
		}
		float GetTypeAutocompleteHeight(int forceCount=-1){
			int count = forceCount > 0 ? forceCount : _typesAutocompleted.Count;
			if (count <= 0) return 0;
			return 13 + (Mathf.Min(MAX_AUTOCOMPLETE_LINES, count) * 20);
		}
		void ClosePopup(){
			Close();
			Object.DestroyImmediate(_i);
			_i = null;
		}
		#endregion

		#region Type Creation
		bool CreateType(string typeName, bool overwrite, string path="", string forceTemplate=""){
			if (string.IsNullOrEmpty(typeName)) return false;
			foreach (var c in typeName){
				if (!LEGALCHARS.Contains(c.ToString())){
					Debug.LogError("Type name "+typeName+" contains illegal characters.");
					return false;
				}
			}
			// Nested types
			typeName = typeName.Replace('+','.');
			
			string prettyName = PrettyTypeName(typeName);
			if (string.IsNullOrEmpty(prettyName)) return false;

			bool globPath = string.IsNullOrEmpty(path);
			
			string fullPath = SmartEditorUtils.ToAbsolutePath(globPath ? _path : path) + "/";
			if (globPath && _subfolders){
				if (!fullPath.EndsWith(prettyName + "/")){
					fullPath += prettyName + "/";
				}
			}

			Debug.LogFormat("Creating classes in {0}", fullPath);
			
			List<string> filenames = new List<string>();

			bool singleTemplateMode = !string.IsNullOrEmpty(forceTemplate);

			if (singleTemplateMode){
				string filename = null;
				if (!_templateToOutput.TryGetValue(forceTemplate, out filename)){
					Debug.LogError("No output filename found for SmartTemplate "+forceTemplate);
					return false;
				}
				filenames.Add(string.Format(filename, prettyName) + ".cs");
			} else {
				foreach (var a in _templateToOutput){
					filenames.Add(string.Format(a.Value, prettyName) + ".cs");
				}
			}

			bool filesExist = false;
			string log = overwrite ? "Overwriting file(s):" : "File(s) already exist:";
			foreach (string f in filenames){
				filesExist |= CheckFile(f, fullPath, ref log);
			}

			bool makeFiles = true;
			if (filesExist){
				if (overwrite){
					Debug.LogWarning(log);
				} else {
					EditorUtility.DisplayDialog("Error: Files exist", log + "\n\nOperation cancelled, no files written.\n\nTick Overwrite to force.", "OK");
					makeFiles = false;
				}
			}
			if (makeFiles){
				string writeLog = "Created files:";
				if (singleTemplateMode){
					try {
						Write(filenames[0], forceTemplate, typeName, fullPath, ref writeLog);
					} catch (System.Exception e){
						Debug.LogErrorFormat("Skipping file due to error: {0}\nCheck custom template settings - template filename '{1}' may be incorrect.\nError:\n{2}\n{3}", fullPath+filenames[0], forceTemplate, e.Message, e.StackTrace);
						return false;
					}
				} else {
					int i=0;
					foreach (var a in _templateToOutput){
						try {
							Write(filenames[i], a.Key, typeName, fullPath, ref writeLog);
						} catch (System.Exception e){
							Debug.LogErrorFormat("Skipping file due to error: {0}\nCheck custom template settings - template filename '{1}' may be incorrect.\nError:\n{2}\n{3}", fullPath+filenames[i], a.Key, e.Message, e.StackTrace);
						}
						++i;
					}
				}

				Debug.Log(writeLog);
			}

			return makeFiles;
		}
		string PrettyTypeName(string typeName){
			// Trim whitespace
			typeName = typeName.Replace(" ","");

			// Remove namespaces
			var split = typeName.Split(new char[]{'.','+'});
			typeName = split[split.Length-1];

			typeName = Capitalize(typeName);

			// Convert array
			typeName = typeName.Replace("[]", "Array");

			// Convert angle brackets/type args
			if (typeName.Contains("<")){
				var pretty = new System.Text.StringBuilder();
				bool capNext = false;
				foreach (var c in typeName){
					switch (c){
						case '<':
							capNext = true;
							pretty.Append("_");
							break;
						case ',':
							capNext = true;
							break;
						case '>':
							pretty.Append("_");
							break;
						case '[':
						case ']':
							Debug.LogError("Type name "+typeName+" has an unmatched "+c);
							return null;
						default:
							if (capNext){
								pretty.Append(char.ToUpper(c));
							} else {
								pretty.Append(c);
							}
							capNext = false;
							break;
					}
				}
				if (pretty[pretty.Length-1] == '_'){
					pretty.Remove(pretty.Length-1, 1);
				}
				return pretty.ToString();
			}
			return typeName;
		}

		void Write(string outputFileName, string templateFileName, string t, string fullPath, ref string log){
			try {
				// Check type exclusions
				TextAsset template = Resources.Load<TextAsset>(templateFileName);
				var templateLines = template.text.Split('\n','\r');
				bool skip = false;
				foreach (var l in templateLines){
					if (l.StartsWith(SMARTIGNORE_HEADER)){
						var header = l.Replace(SMARTIGNORE_HEADER,"");
						var ignores = header.Split(' ');
						foreach (var i in ignores){
							if (i == t){
								skip = true;
								break;
							}
						}
						break;
					}
				}
				if (skip){
					Debug.LogWarningFormat("Ignoring type {0} for template {1}", t, templateFileName);
					return;
				}

				string format = string.Format(
					"{0}{{0}}\n{1}{{3}}\n// Do not move or delete the above lines\n\n{2}",
					SMARTTYPE_HEADER, SMARTTEMPLATE_HEADER, template.text
				);
				if (!Directory.Exists(fullPath)){
					Directory.CreateDirectory(fullPath);
				}
				string absPathToScript = fullPath+outputFileName;
				string relPathToScript = SmartEditorUtils.ToRelativePath(absPathToScript);
				scriptAbsPathToGuid[absPathToScript] = AssetDatabase.AssetPathToGUID(relPathToScript);

				File.WriteAllText(absPathToScript, string.Format(format, t, PrettyTypeName(t), Capitalize(t), templateFileName));
				log += "\n  " + outputFileName;
			} catch (System.FormatException e){
				Debug.LogError("Bad formatting in template "+templateFileName);
				throw e;
			}
		}
		bool CheckFile(string filename, string fullPath, ref string log){
			bool filesExist = false;
			if (File.Exists(fullPath+filename)){
				filesExist = true;
				log += string.Format("\n  {0}", filename);
			}
			return filesExist;
		}
		string Capitalize(string s){
			char cap = char.ToUpper(s[0]);
			return cap + s.Substring(1, s.Length-1);
		}
		#endregion
	}
}