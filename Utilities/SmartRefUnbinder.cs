using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.Abstract;

namespace SmartData.Components {
	[AddComponentMenu("")]
	public class SmartRefUnbinder : MonoBehaviour {
		static Dictionary<GameObject, SmartRefUnbinder> _all = new Dictionary<GameObject, SmartRefUnbinder>();
		/// <summary>
		/// Register SmartRef for automatic event unbinding when gameobject is destroyed.
		/// Note: adds a MonoBehaviour to the gameobject when first called.
		/// </summary>
		public static void UnbindOnDestroy(SmartRefBase r, GameObject go, bool enableUnityEventNow=true){
			SmartRefUnbinder helper = null;
			if (!_all.TryGetValue(go, out helper)){
				helper = go.AddComponent<SmartRefUnbinder>();
				_all.Add(go, helper);
			}
			helper._refs.Add(r);
			if (enableUnityEventNow){
				r.unityEventOnReceive = true;
			}
		}

		List<SmartRefBase> _refs = new List<SmartRefBase>();

		void OnDestroy(){
			for (int i=0; i<_refs.Count; ++i){
				_refs[i].unityEventOnReceive = false;
			}
			_all.Remove(gameObject);
		}
	}
}