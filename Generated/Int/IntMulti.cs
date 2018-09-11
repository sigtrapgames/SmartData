using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.SmartInt.Data;
using SmartData.Abstract;
using SmartData.Interfaces;
using Sigtrap.Relays;

namespace SmartData.SmartInt.Data {
	/// <summary>
	/// Dynamic collection of IntVar assets.
	/// </summary>
	[CreateAssetMenu(menuName="SmartData/Int/Int Multi", order=1)]
	public class IntMulti: SmartMulti<int, IntVar>, ISmartMulti<int, IntVar> {
		#if UNITY_EDITOR
		const string VALUETYPE = "int";
		const string DISPLAYTYPE = "Int Multi";
		#endif
	}
}

namespace SmartData.SmartInt {
	/// <summary>
	/// Indexed reference into a IntMulti (read-only access).
	/// For write access make a reference to IntMultiRefWriter.
	/// </summary>
	[System.Serializable]
	public class IntMultiReader : SmartDataMultiRef<IntMulti, int, IntVar>  {
		public static implicit operator int(IntMultiReader r){
            return r.value;
		}
		
		[SerializeField]
		Data.IntVar.IntEvent _onUpdate;
		
		protected override System.Action<int> GetUnityEventInvoke(){
			return _onUpdate.Invoke;
		}
	}
	/// <summary>
	/// Indexed reference into a IntMulti, with a built-in UnityEvent.
	/// For read-only access make a reference to IntMultiRef.
	/// UnityEvent disabled by default. If enabled, remember to disable at end of life.
	/// </summary>
	[System.Serializable]
	public class IntMultiWriter : SmartDataMultiRefWriter<IntMulti, int, IntVar> {
		public static implicit operator int(IntMultiWriter r){
            return r.value;
		}
		
		[SerializeField]
		Data.IntVar.IntEvent _onUpdate;
		
		protected override System.Action<int> GetUnityEventInvoke(){
			return _onUpdate.Invoke;
		}
		protected sealed override void InvokeUnityEvent(int value){
			_onUpdate.Invoke(value);
		}
	}
}