// SMARTTYPE string
// SMARTTEMPLATE SmartVarTemplate
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
	/// ScriptableObject data which fires a Relay on data change.
	/// <summary>
	[CreateAssetMenu(menuName="SmartData/String/String Variable", order=0)]
	public partial class StringVar : SmartVar<string>, ISmartVar<string> {	// partial to allow overrides that don't get overwritten on regeneration
		#if UNITY_EDITOR
		const string VALUETYPE = "string";
		const string DISPLAYTYPE = "String";
		#endif

		[System.Serializable]
		public class StringEvent : UnityEvent<string>{}
	}
}

namespace SmartData.SmartString {
	/// <summary>
	/// Read-only access to SmartString or string, with built-in UnityEvent.
	/// For write access make a StringRefWriter reference.
	/// UnityEvent disabled by default. If enabled, remember to disable at end of life.
	/// </summary>
	[System.Serializable]
	public class StringReader : SmartDataRefBase<string, StringVar, StringConst, StringMulti> {
		[SerializeField]
		Data.StringVar.StringEvent _onUpdate;
		
		protected sealed override System.Action<string> GetUnityEventInvoke(){
			return _onUpdate.Invoke;
		}
	}
	/// <summary>
	/// Write access to SmartStringWriter or string, with built-in UnityEvent.
	/// For read-only access make a StringRef reference.
	/// UnityEvent disabled by default. If enabled, remember to disable at end of life.
	/// </summary>
	[System.Serializable]
	public class StringWriter : SmartDataRefWriter<string, StringVar, StringConst, StringMulti> {
		[SerializeField]
		Data.StringVar.StringEvent _onUpdate;
		
		protected sealed override System.Action<string> GetUnityEventInvoke(){
			return _onUpdate.Invoke;
		}
		protected sealed override void InvokeUnityEvent(string value){
			_onUpdate.Invoke(value);
		}
	}
}