// SMARTTYPE Vector3
// SMARTTEMPLATE SmartMultiTemplate
// Do not move or delete the above lines

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.SmartVector3.Data;
using SmartData.Abstract;
using SmartData.Interfaces;
using Sigtrap.Relays;

namespace SmartData.SmartVector3.Data {
	/// <summary>
	/// Dynamic collection of Vector3Var assets.
	/// </summary>
	[CreateAssetMenu(menuName="SmartData/Vector3/Vector3 Multi", order=1)]
	public class Vector3Multi: SmartMulti<Vector3, Vector3Var>, ISmartMulti<Vector3, Vector3Var> {
		#if UNITY_EDITOR
		const string VALUETYPE = "Vector3";
		const string DISPLAYTYPE = "Vector3 Multi";
		#endif
	}
}

namespace SmartData.SmartVector3 {
	/// <summary>
	/// Indexed reference into a Vector3Multi (read-only access).
	/// For write access make a reference to Vector3MultiRefWriter.
	/// </summary>
	[System.Serializable]
	public class Vector3MultiReader : SmartDataMultiRef<Vector3Multi, Vector3, Vector3Var>  {
		public static implicit operator Vector3(Vector3MultiReader r){
            return r.value;
		}
		
		[SerializeField]
		Data.Vector3Var.Vector3Event _onUpdate = null;
		
		protected override System.Action<Vector3> GetUnityEventInvoke(){
			return _onUpdate.Invoke;
		}
	}
	/// <summary>
	/// Indexed reference into a Vector3Multi, with a built-in UnityEvent.
	/// For read-only access make a reference to Vector3MultiRef.
	/// UnityEvent disabled by default. If enabled, remember to disable at end of life.
	/// </summary>
	[System.Serializable]
	public class Vector3MultiWriter : SmartDataMultiRefWriter<Vector3Multi, Vector3, Vector3Var> {
		public static implicit operator Vector3(Vector3MultiWriter r){
            return r.value;
		}
		
		[SerializeField]
		Data.Vector3Var.Vector3Event _onUpdate = null;
		
		protected override System.Action<Vector3> GetUnityEventInvoke(){
			return _onUpdate.Invoke;
		}
		protected sealed override void InvokeUnityEvent(Vector3 value){
			_onUpdate.Invoke(value);
		}
	}
}