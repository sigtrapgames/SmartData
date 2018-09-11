using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sigtrap.Relays;
using SmartData.Abstract;
using SmartData.Interfaces;
using System.Linq;

namespace SmartData.Abstract {	
	public abstract class SmartDecoratorBase : ScriptableObject {
		[SerializeField]
		bool _active = true;
		/// <summary>
		/// If false, decorator won't be used. Works like MonoBehaviour.enabled.
		/// </summary>
		public bool active {
			get {return _active;}
			set {
				if (value != _active){
					_active = value;
					OnSetActive(_active);
				}
			}
		}

		/// <summary>
		/// Separate method for editor reflection call when toggling checkbox.
		/// </summary>
		void OnSetActive(bool a){
			if (_active){
				OnActivate();
			} else {
				OnDeactivate();
			}
			_onActivated.Dispatch(this, _active);
		}

		/// <summary>
		/// Dispatched when decorator is activated/deactivated. 
		/// <para /> Bool is true if activated, false if deactivated.
		/// <para /> Called after OnActivated/OnDeactivated.
		/// </summary>
		public IRelayLink<SmartDecoratorBase, bool> onActivated {get {return _onActivated.link;}}
		Relay<SmartDecoratorBase, bool> _onActivated = new Relay<SmartDecoratorBase, bool>();

		/// <summary>
		/// Called when decorator is activated.
		/// <para /> Works like MonoBehaviour.OnEnable().
		/// <para /> NOT called by ScriptableObject.OnEnable() as timing is ill-defined.
		/// </summary>
		protected virtual void OnActivate(){}
		/// <summary>
		/// Called when decorator is deactivated.
		/// <para /> Works like MonoBehaviour.OnDisable().
		/// <para /> NOT called by ScriptableObject.OnDisable() as timing is ill-defined.
		/// </summary>
		protected virtual void OnDeactivate(){}
		public abstract void SetOwner(SmartBindableBase owner);
	}
	/// <summary>
	/// Base class for custom SmartEvent Decorators.
	/// </summary>
	public abstract class SmartEventDecoratorBase : SmartDecoratorBase {
		SmartEvent.Data.EventVar _owner;
		public SmartEvent.Data.EventVar owner {get {return _owner;}}
		IRelayBinding _onDispatchBinding;

		public override void SetOwner(SmartBindableBase owner){
			this._owner = (SmartEvent.Data.EventVar)owner;
			_onDispatchBinding = this._owner.BindListener(OnDispatched);
		}
		protected override void OnActivate(){
			_onDispatchBinding.Enable(true);
		}
		protected override void OnDeactivate(){
			_onDispatchBinding.Enable(false);
		}
		protected virtual void OnDispatched(){}
	}

	/// <summary>
	/// Base class for custom SmartEvent Decorators.
	/// </summary>
	public abstract class SmartDataDecoratorBase<TData> : SmartDecoratorBase {
		SmartVar<TData> _owner;
		public SmartVar<TData> owner {get {return _owner;}}
		public virtual TData OnUpdated(TData newValue){return newValue;}
		public virtual void OnDispatched(TData value){}
		public override void SetOwner(SmartBindableBase owner){
			this._owner = (SmartVar<TData>)owner;
		}
	}

	/// <summary>
	/// Base class for custom SmartSet Decorators.
	/// </summary>
	public abstract class SmartSetDecoratorBase<TData> : SmartDecoratorBase {
		SmartSet<TData> _owner;
		public SmartSet<TData> owner {get {return _owner;}}
		public virtual TData OnChanged(TData value, bool added){return value;}
		public virtual TData OnRemovedAt(TData value, int index){return value;}
		public override void SetOwner(SmartBindableBase owner){
			this._owner = (SmartSet<TData>)owner;
		}
	}
}