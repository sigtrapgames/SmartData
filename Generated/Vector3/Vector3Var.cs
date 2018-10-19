// SMARTTYPE Vector3
// SMARTTEMPLATE SmartVarTemplate
// Do not move or delete the above lines

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SmartData.SmartVector3.Data;
using SmartData.Abstract;
using SmartData.Interfaces;
using Sigtrap.Relays;

namespace SmartData.SmartVector3.Data {
	/// <summary>
	/// ScriptableObject data which fires a Relay on data change.
	/// <summary>
	[CreateAssetMenu(menuName="SmartData/Vector3/Vector3 Variable", order=0)]
	public partial class Vector3Var : SmartVar<Vector3>, ISmartVar<Vector3> {	// partial to allow overrides that don't get overwritten on regeneration
		#if UNITY_EDITOR
		const string VALUETYPE = "Vector3";
		const string DISPLAYTYPE = "Vector3";
		#endif

		[System.Serializable]
		public class Vector3Event : UnityEvent<Vector3>{}
	}
}

namespace SmartData.SmartVector3 {
	/// <summary>
	/// Read-only access to SmartVector3 or Vector3, with built-in UnityEvent.
	/// For write access make a Vector3RefWriter reference.
	/// UnityEvent disabled by default. If enabled, remember to disable at end of life.
	/// </summary>
	[System.Serializable]
	public class Vector3Reader : SmartDataRefBase<Vector3, Vector3Var, Vector3Const, Vector3Multi> {
		[SerializeField]
		Data.Vector3Var.Vector3Event _onUpdate;
		
		protected sealed override System.Action<Vector3> GetUnityEventInvoke(){
			return _onUpdate.Invoke;
		}
	}
	/// <summary>
	/// Write access to SmartVector3Writer or Vector3, with built-in UnityEvent.
	/// For read-only access make a Vector3Ref reference.
	/// UnityEvent disabled by default. If enabled, remember to disable at end of life.
	/// </summary>
	[System.Serializable]
	public class Vector3Writer : SmartDataRefWriter<Vector3, Vector3Var, Vector3Const, Vector3Multi> {
		[SerializeField]
		Data.Vector3Var.Vector3Event _onUpdate;
		
		protected sealed override System.Action<Vector3> GetUnityEventInvoke(){
			return _onUpdate.Invoke;
		}
		protected sealed override void InvokeUnityEvent(Vector3 value){
			_onUpdate.Invoke(value);
		}
	}
}