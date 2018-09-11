using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using SmartData.Abstract;
using System.IO;

namespace SmartData.Editors {
	public static class SmartEditorUtils {
		public enum SmartObjectType {
			NONE,
			CONST,
			VAR,
			MULTI,
			SET,
			EVENT,
			EVENT_MULTI
		}

		static readonly char[] TRIM_SLASH = new char[]{'/','\\'};

		private static Texture2D _iconRead;
		public static Texture2D iconRead {
			get {
				if (_iconRead == null){
					_iconRead = Resources.Load<Texture2D>("GUI/IconReadHalf");
				}
				return _iconRead;
			}
		}
		static Texture2D _iconWrite;
		public static Texture2D iconWrite {
			get {
				if (_iconWrite == null){
					_iconWrite = Resources.Load<Texture2D>("GUI/IconWriteHalf");
				}
				return _iconWrite;
			}
		}
		static Texture2D _iconGameobject;
		public static Texture2D iconGameobject {
			get {
				if (_iconGameobject == null){
					_iconGameobject = (Texture2D) EditorGUIUtility.IconContent("Gameobject Icon").image;
				}
				return _iconGameobject;
			}
		}

		public static readonly float pixelsPerIndent = (float)(typeof(EditorGUI).GetField("kIndentPerLevel", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null));
		public static float indent {get {return pixelsPerIndent * EditorGUI.indentLevel;}}
		static string _rootPath {
			get {
				string root = Application.dataPath;
				// Remove Assets
				return root.Substring(0, root.Length-6);	
			}
		}

		static readonly Dictionary<SmartObjectType, string> iconNamesBySmartObjectType = new Dictionary<SmartObjectType, string>{
			{SmartObjectType.CONST, "Const"},
			{SmartObjectType.VAR, "Var"},
			{SmartObjectType.MULTI, "Multi"},
			{SmartObjectType.EVENT, "Event"},
			{SmartObjectType.EVENT_MULTI, "EventMulti"},
			{SmartObjectType.SET, "Set"}
		};
		static readonly Dictionary<SmartObjectType, Color> colorsBySmartObjectType = new Dictionary<SmartObjectType, Color>{
			{SmartObjectType.CONST, Color.grey},
			{SmartObjectType.VAR, new Color(0.1f,0.45f,1f)},
			{SmartObjectType.MULTI, Color.cyan},
			{SmartObjectType.EVENT, new Color(1,0.6f,0)},
			{SmartObjectType.EVENT_MULTI, Color.cyan},
			{SmartObjectType.SET, new Color(1,1,0.25f)}
		};

		public static string GetSmartIconResourcesPath(SmartObjectType sot, bool halfSize=false, bool themed=false){
			return string.Format("GUI/{0}Icon{1}{2}", (themed ? "Themed/" : ""), iconNamesBySmartObjectType[sot], (halfSize ? "Half" : ""));
		}
		public static Texture2D LoadSmartIcon(SmartObjectType sot, bool halfSize=false, bool themed=false){
			return Resources.Load<Texture2D>(GetSmartIconResourcesPath(sot, halfSize, themed));
		}
		public static Color GetSmartColor(SmartObjectType sot){
			return colorsBySmartObjectType[sot];
		}
		public static string GetMetaPath(Object asset, bool absolute=true){
			string ap = AssetDatabase.GetAssetPath(asset);
			if (!string.IsNullOrEmpty(ap)){
				if (absolute){
					ap = ToAbsolutePath(ap);
				}
				ap = ap + ".meta";
			}
			return ap;
		}

		#region Paths
		public static string ToAbsolutePath(string relativePath){
			return _rootPath + relativePath.TrimStart(TRIM_SLASH);
		}
		public static string ToRelativePath(string absolutePath, bool forceAssets=true){
			string result = absolutePath.Replace(_rootPath, "");
			// If rootPath was not found in full, we're outside project root. Use "" to force the highest level to be the project root.
			if (result == absolutePath){
				result = "";
			}
			// If forcing assets folder as highest level, check result starts with it.
			if (forceAssets && !result.StartsWith("Assets/")){
				result = "Assets/";
			}
			return result;
		}
		public static bool CheckPathExists(string path){
			bool result = Directory.Exists(path);
			if (!result){
				result = Directory.Exists(ToAbsolutePath(path));
			}
			return result;
		}
		public static string ToDirectory(string path){
			for (int i=path.Length-1; i>=0; --i){
				if (path[i]=='/' || path[i]=='\\'){
					return path.Substring(0, i);
				}
			}
			return path;
		}
		#endregion
		
		public static SmartObjectType GetSmartObjectType(SmartBase smart, out System.Type dataType){
			return GetSmartObjectType(smart.GetType(), out dataType);
		}
		public static SmartObjectType GetSmartObjectType(System.Type tSmart, out System.Type tData){
			tData = GetDataType(tSmart);
			
			if (tSmart == null) return SmartObjectType.NONE;

			#region SmartRef
			if (tSmart.IsSubclassOf(typeof(SmartRefBase))){
				if (tSmart.IsSubclassOf(typeof(SmartSetRefBase))){
					return SmartObjectType.SET;
				} else if (tSmart.IsSubclassOf(typeof(SmartEvent.EventListener))){
					return SmartObjectType.EVENT;
				} else if (tSmart.IsSubclassOf(typeof(SmartEvent.EventMultiListener))){
					return SmartObjectType.EVENT_MULTI;
				} else if (tSmart.IsSubclassOf(typeof(SmartMultiRef<,>))){
					return SmartObjectType.MULTI;
				}
				return SmartObjectType.VAR;
			}
			#endregion

			#region SmartComponent
			if (tSmart.IsSubclassOf(typeof(SmartComponentBase))){
				// Try to get reference type
				var tRef = GetDataType(tSmart, false);
				if (!tRef.IsSubclassOf(typeof(SmartRefBase))){
					// Not a ref type = attempt to get second generic arg (should be TRef)
					tRef = GetDataType(tSmart, false, 1);
				}
				return GetSmartObjectType(tRef, out tData);
			}
			#endregion

			#region SmartObject
			if (!tSmart.IsSubclassOf(typeof(SmartBase))){
				return SmartObjectType.NONE;
			}

			if (typeof(SmartEvent.Data.EventVar).IsAssignableFrom(tSmart)){
				// Event
				return SmartObjectType.EVENT;
			} else if (typeof(SmartEvent.Data.EventMulti).IsAssignableFrom(tSmart)){
				// Event Multi
				return SmartObjectType.EVENT_MULTI;
			} else {
				// Data or Set
				
				var tSmartSetBase = typeof(SmartSet<>).MakeGenericType(tData);
				var tSmartVarBase = typeof(SmartVar<>).MakeGenericType(tData);
				var tSmartMulBase = typeof(SmartMultiBase);
				if (tSmartSetBase.IsAssignableFrom(tSmart)){
					// Set
					return SmartObjectType.SET;	
				} else if (tSmartVarBase.IsAssignableFrom(tSmart)){
					// Var
					return SmartObjectType.VAR;
				} else if (tSmartMulBase.IsAssignableFrom(tSmart)){
					// Multi
					return SmartObjectType.MULTI;
				}
			}
			// Const
			return SmartObjectType.CONST;
			#endregion
		}
		public static System.Type GetDataType(System.Type tSmart, bool ignoreSmartTypes=true, int genericArgIndex=0){
			if (tSmart == null) return null;

			var tData = tSmart;
			
			// Go up to find non-concrete type to extract data type arg
			while (!tData.IsGenericType){
				tData = tData.BaseType;
				if (tData == null) return null;	// No data type
			}

			// Get type arg
			var gArgs = tData.GetGenericArguments();
			if (genericArgIndex < gArgs.Length){
				tData = gArgs[genericArgIndex];
			} else {
				return null;
			}
			
			// Component types will give tData of SmartRef or SmartObject, so run once more
			if (ignoreSmartTypes && tData.IsSubclassOf(typeof(SmartRefBase)) || tData.IsSubclassOf(typeof(SmartBase))){
				tData = GetDataType(tData);
			}

			return tData;
		}
	}
}