// SMARTTYPE int
// SMARTTEMPLATE SmartSetTemplate
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
	/// ScriptableObject data set which fires a Relay on data addition/removal.
	/// <summary>
	[CreateAssetMenu(menuName="SmartData/Int/Int Set", order=2)]
	public class IntSet : SmartSet<int>, ISmartDataSet<int> {
		#if UNITY_EDITOR
		const string VALUETYPE = "int";
		const string DISPLAYTYPE = "Int Set";
		#endif
	}
}

namespace SmartData.SmartInt {
	/// <summary>
	/// Read-only access to IntSet or List<0>, with built-in UnityEvent.
	/// For write access make a IntSetWriter reference.
	/// UnityEvent disabled by default. If enabled, remember to disable at end of life.
	/// </summary>
	[System.Serializable]
	public class IntSetReader : SmartSetRefBase<int, IntSet>, ISmartSetRefReader<int> {
		[SerializeField]
		Data.IntVar.IntEvent _onAdd = null;
		[SerializeField]
		Data.IntVar.IntEvent _onRemove = null;
		[SerializeField]
		Data.IntVar.IntEvent _onChange = null;
		
		protected override System.Action<SetEventData<int>> GetUnityEventInvoke(){
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
	/// Write access to IntSet or List<int>, with built-in UnityEvent.
	/// For read-only access make a IntSetRef reference.
	/// UnityEvent disabled by default. If enabled, remember to disable at end of life.
	/// </summary>
	[System.Serializable]
	public class IntSetWriter : SmartSetRefWriterBase<int, IntSet>, ISmartSetRefReader<int> {
		[SerializeField]
		Data.IntVar.IntEvent _onAdd = null;
		[SerializeField]
		Data.IntVar.IntEvent _onRemove = null;
		[SerializeField]
		Data.IntVar.IntEvent _onChange = null;
		
		protected override System.Action<SetEventData<int>> GetUnityEventInvoke(){
			return InvokeUnityEvent;
		}
		
		protected sealed override void InvokeUnityEvent(SetEventData<int> d){
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