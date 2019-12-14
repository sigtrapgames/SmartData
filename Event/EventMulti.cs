using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SmartData.Abstract;
using Sigtrap.Relays;
using SmartData.SmartEvent.Data;
using SmartData.Interfaces;

namespace SmartData.SmartEvent.Data {
	/// <summary>
	/// Dynamic collection of <cref>SmartEvents</cref>.
	/// </summary>
	[CreateAssetMenu(menuName = "SmartData/Event Multi", order=5)]
	public class EventMulti : SmartMulti<EventVar> {
		#if UNITY_EDITOR
		const string VALUETYPE = "void";
		const string DISPLAYTYPE = "Event Multi";
		#endif

		/// <summary>
		/// Bind an event listener to indexed SmartVar which passes current value.
		/// <returns>IRelayBinding for easy enabling/disabling, or null if failed.</returns>
		public IRelayBinding BindListener(System.Action listener, int index){
			return this[index].BindListener(listener);
		}
	}
}

namespace SmartData.SmartEvent {
	/// <summary>
	/// Indexed reference into an <cref>EventMulti</cref> (read-only access).
	/// For write access make a reference to <cref>EventMultiDispatcher</cref>.
	/// </summary>
	[System.Serializable]
	public class EventMultiListener : SmartMultiRef<EventMulti,  EventVar> {
		#if UNITY_EDITOR || DEVELOPMENT_BUILD
		protected sealed override SmartBase _EDITOR_GetSmartObject(out bool useMultiIndex){
			useMultiIndex = true;
			return _multi;
		}
		#endif

		[SerializeField]
		UnityEvent _onEvent = null;

		protected override IRelayBinding BindUnityEvent(){
			return _event.relay.BindListener(_onEvent.Invoke);
		}
		protected EventVar _event {
			get {return _multi[index];}
		}
		/// <summary>
		/// Bind an event listener to SmartData[index].
		/// <returns>IRelayBinding for easy enabling/disabling, or null if failed.</returns>
		public IRelayBinding BindListener(System.Action listener){
			return _event.BindListener(listener);
		}
		protected void InvokeUnityEvent(){{
			_onEvent.Invoke();
		}}
	}
	/// <summary>
	/// Indexed reference into an <cref>EventMulti</cref> (dispatch access).
	/// For read-only access make a reference to <cref>EventMultiListener</cref>.
	/// </summary>
	[System.Serializable]
	public class EventMultiDispatcher : EventMultiListener {
		public void Dispatch(){
			var b = _event.Dispatch();
			if (!unityEventOnReceive && !b.Contains(BlockFlags.DISPATCH)){
				InvokeUnityEvent();
			}
		#if UNITY_EDITOR && !SMARTDATA_NO_GRAPH_HOOKS
			Editors.SmartDataRegistry.OnRefCallToSmart(this, _multi, _event);
		#endif
		}
	}
}
