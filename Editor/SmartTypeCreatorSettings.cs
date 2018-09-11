using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using System.IO;

namespace SmartData.Editors {
	public class SmartTypeCreatorSettings : ScriptableObject, ISerializationCallbackReceiver {
		#if UNITY_EDITOR
		const string DEFAULT_FILEPATH = "Assets/Editor/Resources/SmartTypeCreatorSettings.asset";
		static SmartTypeCreatorSettings _default;
		public static SmartTypeCreatorSettings DEFAULT {
			get {
				if (_default == null){
					_default = UnityEditor.AssetDatabase.LoadAssetAtPath<SmartTypeCreatorSettings>(DEFAULT_FILEPATH);
					if (_default == null){
						string path = SmartEditorUtils.ToDirectory(DEFAULT_FILEPATH);
						if (!SmartEditorUtils.CheckPathExists(path)){
							string dir = SmartEditorUtils.ToAbsolutePath(path);
							Debug.Log("SmartData: Creating directory "+dir);
							Directory.CreateDirectory(dir);
						}
						Debug.Log("SmartData: Creating default type creator settings at "+DEFAULT_FILEPATH);
						var d = CreateInstance<SmartTypeCreatorSettings>();
						UnityEditor.AssetDatabase.CreateAsset(d, DEFAULT_FILEPATH);
						UnityEditor.AssetDatabase.Refresh();
						_default = UnityEditor.AssetDatabase.LoadAssetAtPath<SmartTypeCreatorSettings>(DEFAULT_FILEPATH);
					}
				}
				return _default;
			}
		}
		#endif

		/// <summary>
		/// Relates a template file to an output script filename for auto type generation.
		/// </summary>
		[System.Serializable]
		public struct TemplateConfig {
			/// <summary>
			/// Place templates in &lt;anything&gt;/Editor/Resources
			/// <para />Filename only - do not include path or '.txt' extension.
			/// </summary>
			[Tooltip("Place templates in <anything>/Editor/Resources.\nFilename only - do not include path or '.txt' extension.")]
			public string templateFile;
			/// <summary>
			/// Must include '{0}' for type name.
			/// <para />Do not include '.cs' extension.
			/// <para />If a MonoBehaviour or ScriptableObject, filename must match class name.
			/// </summary>
			[Tooltip("Must include '{0}' for type name.\nDo not include '.cs' extension.\nIf a MonoBehaviour or ScriptableObject, filename must match class name.")]
			public string outputFile;
		}

		[SerializeField]
		List<TemplateConfig> _customTemplates = new List<TemplateConfig>();
		public ReadOnlyCollection<TemplateConfig> customTemplates {get {return _customTemplates.AsReadOnly();}}

		[SerializeField][Tooltip("Type creator auto-complete will ignore types matching these patterns.\nUses string.Contains().")]
		string[] _typeHelperExcludePatterns = new string[]{
			"UnityScript.Lang.", "Boo.Lang.", "<PrivateImplementationDetails>",
			"UnityEditor.", "UnityEditorInternal.", "CompilerGenerated.", "SmartData."
		};
		public string[] typeHelperExcludePatterns {get {return _typeHelperExcludePatterns;}}

		[SerializeField][Tooltip("Frame budget for async loading types for auto-complete")]
		float _asyncTypeLoadFrameTime = 0.05f;
		public double asyncTypeLoadFrameTime {get {return (double)_asyncTypeLoadFrameTime;}}

		public void OnBeforeSerialize(){
			SmartTypeCreator.settingsDirty = true;
		}
		public void OnAfterDeserialize(){
			SmartTypeCreator.settingsDirty = true;
		}
	}
}