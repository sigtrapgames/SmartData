using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.SmartEvent.Data;

namespace SmartData.SmartEvent.Components {
	/// <summary>
	/// Serialised access to a SmartEventDispatcher.
	/// </summary>
	[AddComponentMenu("SmartData/Dispatch Smart Event", 101)]
	public class DispatchSmartEvent : MonoBehaviour {
		[SerializeField][ForceNoAutoListen(hide=true)][ForceHideEvent]
		EventDispatcher _event;

		/// <summary>Dispatch the SmartEvent.</summary>
		public void Dispatch(){
			_event.Dispatch();
		}
		/// <summary>Dispatch a SmartEvent specified by the caller.</summary>
		public void DispatchEvent(EventVar e){
			e.Dispatch();
		}
	}
}