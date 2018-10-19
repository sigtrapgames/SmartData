// SMARTTYPE bool
// SMARTTEMPLATE SmartVarTemplate
// Do not move or delete the above lines

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SmartData.SmartBool.Data;
using SmartData.Abstract;
using SmartData.Interfaces;
using Sigtrap.Relays;

namespace SmartData.SmartBool.Data {
	/// <summary>
	/// ScriptableObject data which fires a Relay on data change.
	/// <summary>
	[CreateAssetMenu(menuName="SmartData/Bool/Bool Variable", order=0)]
	public partial class BoolVar : SmartVar<bool>, ISmartVar<bool> {	// partial to allow overrides that don't get overwritten on regeneration
		#if UNITY_EDITOR
		const string VALUETYPE = "bool";
		const string DISPLAYTYPE = "Bool";
		#endif

		[System.Serializable]
		public class BoolEvent : UnityEvent<bool>{}
	}
}

namespace SmartData.SmartBool {
	/// <summary>
	/// Read-only access to SmartBool or bool, with built-in UnityEvent.
	/// For write access make a BoolRefWriter reference.
	/// UnityEvent disabled by default. If enabled, remember to disable at end of life.
	/// </summary>
	[System.Serializable]
	public class BoolReader : SmartDataRefBase<bool, BoolVar, BoolConst, BoolMulti> {
		[SerializeField]
		Data.BoolVar.BoolEvent _onUpdate;
		
		protected sealed override System.Action<bool> GetUnityEventInvoke(){
			return _onUpdate.Invoke;
		}
	}
	/// <summary>
	/// Write access to SmartBoolWriter or bool, with built-in UnityEvent.
	/// For read-only access make a BoolRef reference.
	/// UnityEvent disabled by default. If enabled, remember to disable at end of life.
	/// </summary>
	[System.Serializable]
	public class BoolWriter : SmartDataRefWriter<bool, BoolVar, BoolConst, BoolMulti> {
		[SerializeField]
		Data.BoolVar.BoolEvent _onUpdate;
		
		protected sealed override System.Action<bool> GetUnityEventInvoke(){
			return _onUpdate.Invoke;
		}
		protected sealed override void InvokeUnityEvent(bool value){
			_onUpdate.Invoke(value);
		}
	}
}