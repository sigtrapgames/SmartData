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
	/// ScriptableObject data set which fires a Relay on data addition/removal.
	/// <summary>
	[CreateAssetMenu(menuName="SmartData/Float/Float Set", order=2)]
	public class FloatSet : SmartSet<float>, ISmartDataSet<float> {
		#if UNITY_EDITOR
		const string VALUETYPE = "float";
		const string DISPLAYTYPE = "Float Set";
		#endif
	}
}

namespace SmartData.SmartFloat {
	/// <summary>
	/// Read-only access to FloatSet or List<0>, with built-in UnityEvent.
	/// For write access make a FloatSetWriter reference.
	/// UnityEvent disabled by default. If enabled, remember to disable at end of life.
	/// </summary>
	[System.Serializable]
	public class FloatSetReader : SmartSetRefBase<float, FloatSet>, ISmartSetRefReader<float> {
		[SerializeField]
		Data.FloatVar.FloatEvent _onAdd;
		[SerializeField]
		Data.FloatVar.FloatEvent _onRemove;
		
		protected override System.Action<float, bool> GetUnityEventInvoke(){
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
	/// Write access to FloatSet or List<float>, with built-in UnityEvent.
	/// For read-only access make a FloatSetRef reference.
	/// UnityEvent disabled by default. If enabled, remember to disable at end of life.
	/// </summary>
	[System.Serializable]
	public class FloatSetWriter : SmartSetRefWriterBase<float, FloatSet>, ISmartSetRefReader<float> {
		[SerializeField]
		Data.FloatVar.FloatEvent _onAdd;
		[SerializeField]
		Data.FloatVar.FloatEvent _onRemove;
		
		protected override System.Action<float, bool> GetUnityEventInvoke(){
			return (e,a)=>{
				if (a){
					_onAdd.Invoke(e);
				} else {
					_onRemove.Invoke(e);
				}
			};
		}
		
		protected sealed override void InvokeUnityEvent(float value, bool added){
			if (added){
				_onAdd.Invoke(value);
			} else {
				_onRemove.Invoke(value);
			}
		}
		
	}
}