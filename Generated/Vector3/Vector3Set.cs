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
	/// ScriptableObject data set which fires a Relay on data addition/removal.
	/// <summary>
	[CreateAssetMenu(menuName="SmartData/Vector3/Vector3 Set", order=2)]
	public class Vector3Set : SmartSet<Vector3>, ISmartDataSet<Vector3> {
		#if UNITY_EDITOR
		const string VALUETYPE = "Vector3";
		const string DISPLAYTYPE = "Vector3 Set";
		#endif
	}
}

namespace SmartData.SmartVector3 {
	/// <summary>
	/// Read-only access to Vector3Set or List<0>, with built-in UnityEvent.
	/// For write access make a Vector3SetWriter reference.
	/// UnityEvent disabled by default. If enabled, remember to disable at end of life.
	/// </summary>
	[System.Serializable]
	public class Vector3SetReader : SmartSetRefBase<Vector3, Vector3Set>, ISmartSetRefReader<Vector3> {
		[SerializeField]
		Data.Vector3Var.Vector3Event _onAdd;
		[SerializeField]
		Data.Vector3Var.Vector3Event _onRemove;
		
		protected override System.Action<Vector3, bool> GetUnityEventInvoke(){
			return (e,a)=>{
				if (a){
					_onAdd.Invoke(e);
				} else {
					_onRemove.Invoke(e);
				}
			};
		}
	}
	/// <summary>
	/// Write access to Vector3Set or List<Vector3>, with built-in UnityEvent.
	/// For read-only access make a Vector3SetRef reference.
	/// UnityEvent disabled by default. If enabled, remember to disable at end of life.
	/// </summary>
	[System.Serializable]
	public class Vector3SetWriter : SmartSetRefWriterBase<Vector3, Vector3Set>, ISmartSetRefReader<Vector3> {
		[SerializeField]
		Data.Vector3Var.Vector3Event _onAdd;
		[SerializeField]
		Data.Vector3Var.Vector3Event _onRemove;
		
		protected override System.Action<Vector3, bool> GetUnityEventInvoke(){
			return (e,a)=>{
				if (a){
					_onAdd.Invoke(e);
				} else {
					_onRemove.Invoke(e);
				}
			};
		}
		
		protected sealed override void InvokeUnityEvent(Vector3 value, bool added){
			if (added){
				_onAdd.Invoke(value);
			} else {
				_onRemove.Invoke(value);
			}
		}
		
	}
}