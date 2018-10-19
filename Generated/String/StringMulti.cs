// SMARTTYPE string
// SMARTTEMPLATE SmartMultiTemplate
// Do not move or delete the above lines

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.SmartString.Data;
using SmartData.Abstract;
using SmartData.Interfaces;
using Sigtrap.Relays;

namespace SmartData.SmartString.Data {
	/// <summary>
	/// Dynamic collection of StringVar assets.
	/// </summary>
	[CreateAssetMenu(menuName="SmartData/String/String Multi", order=1)]
	public class StringMulti: SmartMulti<string, StringVar>, ISmartMulti<string, StringVar> {
		#if UNITY_EDITOR
		const string VALUETYPE = "string";
		const string DISPLAYTYPE = "String Multi";
		#endif
	}
}

namespace SmartData.SmartString {
	/// <summary>
	/// Indexed reference into a StringMulti (read-only access).
	/// For write access make a reference to StringMultiRefWriter.
	/// </summary>
	[System.Serializable]
	public class StringMultiReader : SmartDataMultiRef<StringMulti, string, StringVar>  {
		public static implicit operator string(StringMultiReader r){
            return r.value;
		}
		
		[SerializeField]
		Data.StringVar.StringEvent _onUpdate;
		
		protected override System.Action<string> GetUnityEventInvoke(){
			return _onUpdate.Invoke;
		}
	}
	/// <summary>
	/// Indexed reference into a StringMulti, with a built-in UnityEvent.
	/// For read-only access make a reference to StringMultiRef.
	/// UnityEvent disabled by default. If enabled, remember to disable at end of life.
	/// </summary>
	[System.Serializable]
	public class StringMultiWriter : SmartDataMultiRefWriter<StringMulti, string, StringVar> {
		public static implicit operator string(StringMultiWriter r){
            return r.value;
		}
		
		[SerializeField]
		Data.StringVar.StringEvent _onUpdate;
		
		protected override System.Action<string> GetUnityEventInvoke(){
			return _onUpdate.Invoke;
		}
		protected sealed override void InvokeUnityEvent(string value){
			_onUpdate.Invoke(value);
		}
	}
}