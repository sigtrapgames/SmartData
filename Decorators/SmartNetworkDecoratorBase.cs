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
		public override TData OnUpdated(TData newValue){
			Send(newValue);
			return newValue;
		}
		public override void OnDispatched(TData value){
			Send(value);
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
		/// <summary>
		/// Call to receive an update from the network.
		/// </summary>
		public void Receive(){
			this.active = false;
			owner.Dispatch();
			this.active = true;
		}
	}
	public abstract class SmartNetworkSetDecoratorBase<TData> : SmartSetDecoratorBase<TData> {
		public override TData OnChanged(TData newValue, bool added){
			if (added){
				SendAdd(newValue);
			} else {
				SendRemove(newValue);
			}
			return newValue;
		}
		public override TData OnRemovedAt(TData value, int index){
			SendRemovedAt(value, index);
			return value;
		}
		protected abstract void SendAdd(TData value);
		protected abstract void SendRemove(TData value);
		protected abstract void SendRemovedAt(TData value, int index);
		/// <summary>
		/// Call to receive an update from the network.
		/// </summary>
		public void ReceiveAdd(TData value){
			this.active = false;
			owner.Add(value);
			this.active = true;
		}
		/// <summary>
		/// Call to receive an update from the network.
		/// </summary>
		public void ReceiveRemove(TData value){
			this.active = false;
			owner.Remove(value);
			this.active = true;
		}
		/// <summary>
		/// Call to receive an update from the network.
		/// </summary>
		public void ReceiveRemoveAt(int index){
			this.active = false;
			owner.RemoveAt(index);
			this.active = true;
		}
	}
}