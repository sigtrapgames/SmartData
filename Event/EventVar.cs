using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sigtrap.Relays;
using SmartData.Abstract;
using SmartData.Interfaces;
using SmartData.SmartEvent.Data;

namespace SmartData.SmartEvent.Data {
	/// <summary>
	/// ScriptableObject event with no parameters.
	/// </summary>
	[CreateAssetMenu(menuName="SmartData/Event", order=4)]
	public class EventVar : SmartDecorableBase, ISmartEvent {
		#if UNITY_EDITOR
		const string VALUETYPE = "void";
		const string DISPLAYTYPE = "Event";
		#endif

		/// <summary>
		/// Link to underlying event. Gives access to additional functionality.
		/// </summary>
		public IRelayLink relay {get {return _relay.link;}}
		protected Relay _relay = new Relay();

		/// <summary>
		/// Bind an event listener.
		/// <returns>IRelayBinding for easy enabling/disabling, or null if failed.</returns>
		public IRelayBinding BindListener(System.Action listener){
			return relay.BindListener(listener);
		}

		/// <summary> Fire event. </summary>
		public void Dispatch(){
			_relay.Dispatch();
			#if UNITY_EDITOR && !SMARTDATA_NO_GRAPH_HOOKS
			SmartData.Editors.SmartDataRegistry.OnRefCallToSmart(null, this);
			#endif
		}
	}
}
namespace SmartData.SmartEvent {
	/// <summary>
	/// Listen-only access to a SmartEvent.
	/// For dispatch access make a reference to EventDispatcher.
	/// </summary>	
	[System.Serializable]
	public class EventListener : SmartRefMultiableBase, ISmartEventRefListener, ISerializationCallbackReceiver {
		#if UNITY_EDITOR || DEVELOPMENT_BUILD
		protected sealed override SmartBase _EDITOR_GetSmartObject(out bool useMultiIndex){
			useMultiIndex = _useMulti;
			if (_useMulti) return _smartMulti;
			return _smartEvent;
		}
		#endif

		[SerializeField]
		protected EventVar _smartEvent = null;
		[SerializeField]
		protected EventMulti _smartMulti = null;
		[SerializeField]
		protected bool _useMulti = false;

		[SerializeField]
		UnityEvent _onEvent = null;

		public override bool isValid {get {return _event != null;}}
		public override string name {get {return _useMulti ? _smartMulti.name : _smartEvent.name;}}
		public virtual IRelayLink relay {
			get {
				if (isValid) return _event.relay;
				throw new System.Exception("SmartEvent object must be present in EventListener (but not EventDispatcher!) to subscribe to events.");
			}
		}
		protected EventVar _event {
			get {return _useMulti ? _smartMulti[_multiIndex] : _smartEvent;}
		}

		public void OnBeforeSerialize(){}
		public void OnAfterDeserialize(){
			if (_autoListen){
				if (_useMulti){
					if (_smartMulti != null){
						_smartMulti.RequestCtorAutoBinding(this, _multiIndex);
					}
				} else if (_smartEvent != null){
					_smartEvent.RequestCtorAutoBinding(this);
				}
			}
		}

		protected override IRelayBinding BindUnityEvent(){
			return _event.BindListener(_onEvent.Invoke);	
		}
		/// <summary>
		/// Bind an event listener.
		/// <returns>IRelayBinding for easy enabling/disabling, or null if failed.</returns>
		public IRelayBinding BindListener(System.Action listener){
			return relay.BindListener(listener);
		}
		protected void InvokeUnityEvent(){{
			_onEvent.Invoke();
		}}
	}
	/// <summary>
	/// Dispatch access to a SmartEvent, with a built-in UnityEvent.
	/// For listen-only access make a reference to EventListener.
	/// UnityEvent disabled by default. If enabled, remember to disable at end of life.
	/// </summary>
	[System.Serializable]
	public class EventDispatcher : EventListener, ISmartEventRefDispatcher {
		#if UNITY_EDITOR
		protected sealed override bool _EDITOR_GetIsWritable(){return true;}
		#endif

		Relay _localEvent = new Relay();
		public override IRelayLink relay {
			get {
				if (isValid) return base.relay;
				return _localEvent.link;
			}
		}
		
		public void Dispatch(){
			if (_event){
				_event.Dispatch();
				if (!unityEventOnReceive){
					InvokeUnityEvent();
				}
			} else {
				_localEvent.Dispatch();
				InvokeUnityEvent();
			}
			#if UNITY_EDITOR && !SMARTDATA_NO_GRAPH_HOOKS
			if (_useMulti){
				Editors.SmartDataRegistry.OnRefCallToSmart(this, _smartMulti, _event);
			} else {
				Editors.SmartDataRegistry.OnRefCallToSmart(this, _smartEvent);
			}
			#endif
		}
	}
}
