using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SmartData.Abstract {
	/// <summary>
	/// Base for SmartData network replication decorator.
	/// <para />Override Send method to replicate data over network.
	/// <para />Implement other behaviour to call Receive when data is replicated.
	/// </summary>
	public abstract class SmartNetworkDataDecoratorBase<TData> : SmartDataDecoratorBase<TData> {
		[SerializeField, Tooltip("Only dispatch event across network, not locally.")]
		bool _networkOnlyDispatch = false;
		[SerializeField, Tooltip("Only update value across network, not locally.")]
		bool _networkOnlyValue = false;
		public override TData OnUpdated(TData oldValue, TData newValue, RestoreMode restoreMode, ref BlockFlags block){
			Send(newValue);
			// Stop event from being dispatched locally
			if (_networkOnlyDispatch){
				block |= BlockFlags.DISPATCH;
			}
			// Stop data from being updated locally
			if (_networkOnlyValue){
				block |= BlockFlags.DATA;
			}
			return newValue;
		}
		protected abstract void Send(TData value);
		/// <summary>
		/// Call to receive an update from the network.
		/// </summary>
		public void Receive(TData value){
			// Take self out of OnSetValue callbacks to avoid loop
			this.active = false;
			// Set value
			owner.value = value;
			// Re-sub to OnSetValue
			this.active = true;
		}
	}
	/// <summary>
	/// Base class for SmartEvent network replication decorator.
	/// <para />Override OnDispatched to send event to your network implementation.
	/// <para />Implement other behaviour to call Receive when event is replicated.
	/// </summary>
	public abstract class SmartNetworkEventDecoratorBase : SmartEventDecoratorBase {
		[SerializeField, Tooltip("Only dispatch event across network, not locally.")]
		bool _networkOnlyDispatch = false;

		/// <summary>
		/// Call to receive an update from the network.
		/// </summary>
		public void Receive(){
			this.active = false;
			owner.Dispatch();
			this.active = true;
		}
		protected abstract void Send();
		public override void OnDispatched(ref BlockFlags blockFlags){
			Send();
			if (_networkOnlyDispatch){
				blockFlags |= BlockFlags.DISPATCH;
			}
		}
	}
	public abstract class SmartNetworkSetDecoratorBase<TData> : SmartSetDecoratorBase<TData> {
		public override SetEventData<TData> OnChanged(SetEventData<TData> data){
			Send(data, _currentRestoreMode);
			return data;
		}
		protected abstract void Send(SetEventData<TData> data, RestoreMode restoreMode);
		/// <summary>
		/// Call to receive an update from the network.
		/// </summary>
		public void Receive(SetEventData<TData> data){
			this.active = false;
			
			switch (data.operation){
				case SetOperation.ADDED:
					owner.Add(data.value);
					break;
				case SetOperation.REMOVED:
					owner.RemoveAt(data.index);
					break;
				case SetOperation.CHANGED:
					owner[data.index] = data.value;
					break;
			}
			
			this.active = true;
		}
	}
}