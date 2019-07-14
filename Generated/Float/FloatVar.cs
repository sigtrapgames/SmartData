// SMARTTYPE float
// SMARTTEMPLATE SmartVarTemplate
// Do not move or delete the above lines

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SmartData.SmartFloat.Data;
using SmartData.Abstract;
using SmartData.Interfaces;
using Sigtrap.Relays;

namespace SmartData.SmartFloat.Data {
	/// <summary>
	/// ScriptableObject data which fires a Relay on data change.
	/// <summary>
	[CreateAssetMenu(menuName="SmartData/Float/Float Variable", order=0)]
	public partial class FloatVar : SmartVar<float>, ISmartVar<float> {	// partial to allow overrides that don't get overwritten on regeneration
		#if UNITY_EDITOR
		const string VALUETYPE = "float";
		const string DISPLAYTYPE = "Float";
		#endif

		[System.Serializable]
		public class FloatEvent : UnityEvent<float>{}
	}
}

namespace SmartData.SmartFloat {
	/// <summary>
	/// Read-only access to SmartFloat or float, with built-in UnityEvent.
	/// For write access make a FloatRefWriter reference.
	/// UnityEvent disabled by default. If enabled, remember to disable at end of life.
	/// </summary>
	[System.Serializable]
	public class FloatReader : SmartDataRefBase<float, FloatVar, FloatConst, FloatMulti> {
		[SerializeField]
		Data.FloatVar.FloatEvent _onUpdate = null;
		
		protected sealed override System.Action<float> GetUnityEventInvoke(){
			return _onUpdate.Invoke;
		}
	}
	/// <summary>
	/// Write access to SmartFloatWriter or float, with built-in UnityEvent.
	/// For read-only access make a FloatRef reference.
	/// UnityEvent disabled by default. If enabled, remember to disable at end of life.
	/// </summary>
	[System.Serializable]
	public class FloatWriter : SmartDataRefWriter<float, FloatVar, FloatConst, FloatMulti> {
		[SerializeField]
		Data.FloatVar.FloatEvent _onUpdate = null;
		
		protected sealed override System.Action<float> GetUnityEventInvoke(){
			return _onUpdate.Invoke;
		}
		protected sealed override void InvokeUnityEvent(float value){
			_onUpdate.Invoke(value);
		}
	}
}