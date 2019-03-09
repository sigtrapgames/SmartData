// SMARTTYPE string
// SMARTTEMPLATE SmartSetTemplate
// Do not move or delete the above lines

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SmartData.SmartString.Data;
using SmartData.Abstract;
using SmartData.Interfaces;
using Sigtrap.Relays;

namespace SmartData.SmartString.Data {
	/// <summary>
	/// ScriptableObject data set which fires a Relay on data addition/removal.
	/// <summary>
	[CreateAssetMenu(menuName="SmartData/String/String Set", order=2)]
	public class StringSet : SmartSet<string>, ISmartDataSet<string> {
		#if UNITY_EDITOR
		const string VALUETYPE = "string";
		const string DISPLAYTYPE = "String Set";
		#endif
	}
}

namespace SmartData.SmartString {
	/// <summary>
	/// Read-only access to StringSet or List<0>, with built-in UnityEvent.
	/// For write access make a StringSetWriter reference.
	/// UnityEvent disabled by default. If enabled, remember to disable at end of life.
	/// </summary>
	[System.Serializable]
	public class StringSetReader : SmartSetRefBase<string, StringSet>, ISmartSetRefReader<string> {
		[SerializeField]
		Data.StringVar.StringEvent _onAdd;
		[SerializeField]
		Data.StringVar.StringEvent _onRemove;
		[SerializeField]
		Data.StringVar.StringEvent _onChange;
		
		protected override System.Action<SetEventData<string>> GetUnityEventInvoke(){
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
	/// Write access to StringSet or List<string>, with built-in UnityEvent.
	/// For read-only access make a StringSetRef reference.
	/// UnityEvent disabled by default. If enabled, remember to disable at end of life.
	/// </summary>
	[System.Serializable]
	public class StringSetWriter : SmartSetRefWriterBase<string, StringSet>, ISmartSetRefReader<string> {
		[SerializeField]
		Data.StringVar.StringEvent _onAdd;
		[SerializeField]
		Data.StringVar.StringEvent _onRemove;
		[SerializeField]
		Data.StringVar.StringEvent _onChange;
		
		protected override System.Action<SetEventData<string>> GetUnityEventInvoke(){
			return InvokeUnityEvent;
		}
		
		protected sealed override void InvokeUnityEvent(SetEventData<string> d){
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