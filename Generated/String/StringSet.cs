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
		
		protected override System.Action<string, bool> GetUnityEventInvoke(){
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
		
		protected override System.Action<string, bool> GetUnityEventInvoke(){
			return (e,a)=>{
				if (a){
					_onAdd.Invoke(e);
				} else {
					_onRemove.Invoke(e);
				}
			};
		}
		
		protected sealed override void InvokeUnityEvent(string value, bool added){
			if (added){
				_onAdd.Invoke(value);
			} else {
				_onRemove.Invoke(value);
			}
		}
		
	}
}