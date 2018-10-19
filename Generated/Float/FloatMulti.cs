// SMARTTYPE float
// SMARTTEMPLATE SmartMultiTemplate
// Do not move or delete the above lines

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.SmartFloat.Data;
using SmartData.Abstract;
using SmartData.Interfaces;
using Sigtrap.Relays;

namespace SmartData.SmartFloat.Data {
	/// <summary>
	/// Dynamic collection of FloatVar assets.
	/// </summary>
	[CreateAssetMenu(menuName="SmartData/Float/Float Multi", order=1)]
	public class FloatMulti: SmartMulti<float, FloatVar>, ISmartMulti<float, FloatVar> {
		#if UNITY_EDITOR
		const string VALUETYPE = "float";
		const string DISPLAYTYPE = "Float Multi";
		#endif
	}
}

namespace SmartData.SmartFloat {
	/// <summary>
	/// Indexed reference into a FloatMulti (read-only access).
	/// For write access make a reference to FloatMultiRefWriter.
	/// </summary>
	[System.Serializable]
	public class FloatMultiReader : SmartDataMultiRef<FloatMulti, float, FloatVar>  {
		public static implicit operator float(FloatMultiReader r){
            return r.value;
		}
		
		[SerializeField]
		Data.FloatVar.FloatEvent _onUpdate;
		
		protected override System.Action<float> GetUnityEventInvoke(){
			return _onUpdate.Invoke;
		}
	}
	/// <summary>
	/// Indexed reference into a FloatMulti, with a built-in UnityEvent.
	/// For read-only access make a reference to FloatMultiRef.
	/// UnityEvent disabled by default. If enabled, remember to disable at end of life.
	/// </summary>
	[System.Serializable]
	public class FloatMultiWriter : SmartDataMultiRefWriter<FloatMulti, float, FloatVar> {
		public static implicit operator float(FloatMultiWriter r){
            return r.value;
		}
		
		[SerializeField]
		Data.FloatVar.FloatEvent _onUpdate;
		
		protected override System.Action<float> GetUnityEventInvoke(){
			return _onUpdate.Invoke;
		}
		protected sealed override void InvokeUnityEvent(float value){
			_onUpdate.Invoke(value);
		}
	}
}