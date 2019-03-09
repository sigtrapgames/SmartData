// SMARTTYPE bool
// SMARTTEMPLATE SmartSetTemplate
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
	/// ScriptableObject data set which fires a Relay on data addition/removal.
	/// <summary>
	[CreateAssetMenu(menuName="SmartData/Bool/Bool Set", order=2)]
	public class BoolSet : SmartSet<bool>, ISmartDataSet<bool> {
		#if UNITY_EDITOR
		const string VALUETYPE = "bool";
		const string DISPLAYTYPE = "Bool Set";
		#endif
	}
}

namespace SmartData.SmartBool {
	/// <summary>
	/// Read-only access to BoolSet or List<0>, with built-in UnityEvent.
	/// For write access make a BoolSetWriter reference.
	/// UnityEvent disabled by default. If enabled, remember to disable at end of life.
	/// </summary>
	[System.Serializable]
	public class BoolSetReader : SmartSetRefBase<bool, BoolSet>, ISmartSetRefReader<bool> {
		[SerializeField]
		Data.BoolVar.BoolEvent _onAdd;
		[SerializeField]
		Data.BoolVar.BoolEvent _onRemove;
		[SerializeField]
		Data.BoolVar.BoolEvent _onChange;
		
		protected override System.Action<SetEventData<bool>> GetUnityEventInvoke(){
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
	/// Write access to BoolSet or List<bool>, with built-in UnityEvent.
	/// For read-only access make a BoolSetRef reference.
	/// UnityEvent disabled by default. If enabled, remember to disable at end of life.
	/// </summary>
	[System.Serializable]
	public class BoolSetWriter : SmartSetRefWriterBase<bool, BoolSet>, ISmartSetRefReader<bool> {
		[SerializeField]
		Data.BoolVar.BoolEvent _onAdd;
		[SerializeField]
		Data.BoolVar.BoolEvent _onRemove;
		[SerializeField]
		Data.BoolVar.BoolEvent _onChange;
		
		protected override System.Action<SetEventData<bool>> GetUnityEventInvoke(){
			return InvokeUnityEvent;
		}
		
		protected sealed override void InvokeUnityEvent(SetEventData<bool> d){
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