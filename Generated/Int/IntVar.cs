// SMARTTYPE int
// SMARTTEMPLATE SmartVarTemplate
// Do not move or delete the above lines

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SmartData.SmartInt.Data;
using SmartData.Abstract;
using SmartData.Interfaces;
using Sigtrap.Relays;

namespace SmartData.SmartInt.Data {
	/// <summary>
	/// ScriptableObject data which fires a Relay on data change.
	/// <summary>
	[CreateAssetMenu(menuName="SmartData/Int/Int Variable", order=0)]
	public partial class IntVar : SmartVar<int>, ISmartVar<int> {	// partial to allow overrides that don't get overwritten on regeneration
		#if UNITY_EDITOR
		const string VALUETYPE = "int";
		const string DISPLAYTYPE = "Int";
		#endif

		[System.Serializable]
		public class IntEvent : UnityEvent<int>{}
	}
}

namespace SmartData.SmartInt {
	/// <summary>
	/// Read-only access to SmartInt or int, with built-in UnityEvent.
	/// For write access make a IntRefWriter reference.
	/// UnityEvent disabled by default. If enabled, remember to disable at end of life.
	/// </summary>
	[System.Serializable]
	public class IntReader : SmartDataRefBase<int, IntVar, IntConst, IntMulti> {
		[SerializeField]
		Data.IntVar.IntEvent _onUpdate;
		
		protected sealed override System.Action<int> GetUnityEventInvoke(){
			return _onUpdate.Invoke;
		}
	}
	/// <summary>
	/// Write access to SmartIntWriter or int, with built-in UnityEvent.
	/// For read-only access make a IntRef reference.
	/// UnityEvent disabled by default. If enabled, remember to disable at end of life.
	/// </summary>
	[System.Serializable]
	public class IntWriter : SmartDataRefWriter<int, IntVar, IntConst, IntMulti> {
		[SerializeField]
		Data.IntVar.IntEvent _onUpdate;
		
		protected sealed override System.Action<int> GetUnityEventInvoke(){
			return _onUpdate.Invoke;
		}
		protected sealed override void InvokeUnityEvent(int value){
			_onUpdate.Invoke(value);
		}
	}
}