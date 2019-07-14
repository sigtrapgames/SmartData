// SMARTTYPE bool
// SMARTTEMPLATE SmartMultiTemplate
// Do not move or delete the above lines

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.SmartBool.Data;
using SmartData.Abstract;
using SmartData.Interfaces;
using Sigtrap.Relays;

namespace SmartData.SmartBool.Data {
	/// <summary>
	/// Dynamic collection of BoolVar assets.
	/// </summary>
	[CreateAssetMenu(menuName="SmartData/Bool/Bool Multi", order=1)]
	public class BoolMulti: SmartMulti<bool, BoolVar>, ISmartMulti<bool, BoolVar> {
		#if UNITY_EDITOR
		const string VALUETYPE = "bool";
		const string DISPLAYTYPE = "Bool Multi";
		#endif
	}
}

namespace SmartData.SmartBool {
	/// <summary>
	/// Indexed reference into a BoolMulti (read-only access).
	/// For write access make a reference to BoolMultiRefWriter.
	/// </summary>
	[System.Serializable]
	public class BoolMultiReader : SmartDataMultiRef<BoolMulti, bool, BoolVar>  {
		public static implicit operator bool(BoolMultiReader r){
            return r.value;
		}
		
		[SerializeField]
		Data.BoolVar.BoolEvent _onUpdate = null;
		
		protected override System.Action<bool> GetUnityEventInvoke(){
			return _onUpdate.Invoke;
		}
	}
	/// <summary>
	/// Indexed reference into a BoolMulti, with a built-in UnityEvent.
	/// For read-only access make a reference to BoolMultiRef.
	/// UnityEvent disabled by default. If enabled, remember to disable at end of life.
	/// </summary>
	[System.Serializable]
	public class BoolMultiWriter : SmartDataMultiRefWriter<BoolMulti, bool, BoolVar> {
		public static implicit operator bool(BoolMultiWriter r){
            return r.value;
		}
		
		[SerializeField]
		Data.BoolVar.BoolEvent _onUpdate = null;
		
		protected override System.Action<bool> GetUnityEventInvoke(){
			return _onUpdate.Invoke;
		}
		protected sealed override void InvokeUnityEvent(bool value){
			_onUpdate.Invoke(value);
		}
	}
}