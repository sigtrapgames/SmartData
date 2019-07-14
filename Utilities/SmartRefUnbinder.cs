using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.Abstract;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

namespace SmartData.Components {
	[AddComponentMenu("")]
	public class SmartRefUnbinder : MonoBehaviour {
		#region Static
		#if UNITY_EDITOR
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		static void Init(){
			EditorApplication.playModeStateChanged += OnEditorChangePlayMode;
		}

		static PlayModeStateChange _playMode = PlayModeStateChange.EnteredEditMode;
		static bool _isPlaying {
			get {
				return 
					_playMode == PlayModeStateChange.EnteredPlayMode || 
					_playMode == PlayModeStateChange.ExitingPlayMode;
			}
		}
		static void OnEditorChangePlayMode(PlayModeStateChange change){
			_playMode = change;
			if (_isPlaying){
				// Go through registered SmartRefs and bind if necessary
				var refs = SmartData.Editors.SmartDataRegistry.GetSmartReferences();
				if (refs.Count > 0){
					FieldInfo autoListen = typeof(SmartData.Abstract.SmartRefBase).GetField("_autoListen", BindingFlags.NonPublic | BindingFlags.Instance);
					FieldInfo owner = typeof(SmartData.Abstract.SmartRefBase).GetField("_owner", BindingFlags.NonPublic | BindingFlags.Instance);
					FieldInfo ownerName = typeof(SmartData.Abstract.SmartRefBase).GetField("_ownerName", BindingFlags.NonPublic | BindingFlags.Instance);
					foreach (var a in refs){
						if (a.Key.IsAlive){
							var sr = (SmartData.Abstract.SmartRefBase)a.Key.Target;
							if ((bool)autoListen.GetValue(sr)){
								GameObject ownerGo = (GameObject)owner.GetValue(sr);
								if (!ownerGo){
									Debug.LogErrorFormat(
										"Error initialising SmartRefUnbinder on GameObject {0} (SmartRef type {1})",
										ownerName.GetValue(sr), sr.GetType().Name	
									);
								} else {
									SmartData.Components.SmartRefUnbinder.UnbindOnDestroy(sr, ownerGo, true);
								}
							}
						}
					}
				}
				EditorApplication.playModeStateChanged -= OnEditorChangePlayMode;
			}
		}
		#endif

		static Dictionary<GameObject, SmartRefUnbinder> _all = new Dictionary<GameObject, SmartRefUnbinder>();
		/// <summary>
		/// Register SmartRef for automatic event unbinding when gameobject is destroyed.
		/// Note: adds a MonoBehaviour to the gameobject when first called.
		/// </summary>
		public static void UnbindOnDestroy(SmartRefBase r, GameObject go, bool enableUnityEventNow=true){
			if (enableUnityEventNow){
				r.unityEventOnReceive = true;
			}

			#if UNITY_EDITOR
			// OnEditorChangePlayMode will automatically go through registered SmartRefs on Start
			if (!_isPlaying) return;
			#endif
			
			SmartRefUnbinder helper = null;
			if (!_all.TryGetValue(go, out helper)){
				helper = go.AddComponent<SmartRefUnbinder>();
				_all.Add(go, helper);
			}
			helper._refs.Add(r);
		}
		#endregion

		#region Instance
		List<SmartRefBase> _refs = new List<SmartRefBase>();

		void OnDestroy(){
			for (int i=0; i<_refs.Count; ++i){
				_refs[i].unityEventOnReceive = false;
			}
			_all.Remove(gameObject);
		}
		#endregion
	}
}