// SMARTTYPE float
// SMARTTEMPLATE SmartSetTemplate
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
		Data.FloatVar.FloatEvent _onAdd = null;
		[SerializeField]
		Data.FloatVar.FloatEvent _onRemove = null;
		[SerializeField]
		Data.FloatVar.FloatEvent _onChange = null;
		
		protected override System.Action<SetEventData<float>> GetUnityEventInvoke(){
			return (d)=>{
				switch (d.operation){
					case SetOperation.ADDED:
						_onAdd.Invoke(d.value);
						break;
					case SetOperation.REMOVED:
						_onRemove.Invoke(d.value);
						break;
					case SetOperation.CHANGED:
						_onChange.Invoke(d.value);
						break;
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
		Data.FloatVar.FloatEvent _onAdd = null;
		[SerializeField]
		Data.FloatVar.FloatEvent _onRemove = null;
		[SerializeField]
		Data.FloatVar.FloatEvent _onChange = null;
		
		protected override System.Action<SetEventData<float>> GetUnityEventInvoke(){
			return InvokeUnityEvent;
		}
		
		protected sealed override void InvokeUnityEvent(SetEventData<float> d){
			switch (d.operation){
				case SetOperation.ADDED:
					_onAdd.Invoke(d.value);
					break;
				case SetOperation.REMOVED:
					_onRemove.Invoke(d.value);
					break;
				case SetOperation.CHANGED:
					_onChange.Invoke(d.value);
					break;
			}
		}
		
	}
}