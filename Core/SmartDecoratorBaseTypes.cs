using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sigtrap.Relays;
using SmartData.Abstract;
using SmartData.Interfaces;
using System.Linq;

namespace SmartData.Abstract {
	[System.Flags]
	public enum BlockFlags {
		/// <summary>This decorator does not block dispatch, following decorators or data update.</summary>
		NONE = 0,
		/// <summary>This decorator blocks event dispatch.</summary>
		DISPATCH = 1,
		/// <summary>This decorator blocks following decorators from being called.</summary>
		DECORATORS = 2,
		/// <summary>This decorator blocks data from being updated.</summary>
		DATA = 4
	}
	public static class Ext_BlockMode {
		public static bool Contains(this BlockFlags mask, BlockFlags value){
			return (mask & value) == value;
		}
	}
	public abstract class SmartDecoratorBase : ScriptableObject {
		[SerializeField]
		bool _active = true;
		bool _runtimeActive;

		/// <summary>
		/// If false, decorator won't be used. Works like MonoBehaviour.enabled.
		/// </summary>
		public bool active {
		#if UNITY_EDITOR
			get {
				return Application.isPlaying ? _runtimeActive : _active;
			}
			set {
				if (Application.isPlaying){
					if (value != _runtimeActive){
						_runtimeActive = value;
						OnSetActive(_runtimeActive);
					}
				} else {
					if (value != _active){
						_active = value;
						OnSetActive(_active);
					}
				}
			}
		#else
			get {return _runtimeActive;}
			set {
				if (value != _runtimeActive){
					_runtimeActive = value;
					OnSetActive(_runtimeActive);
				}
			}
		#endif
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
		public void SetOwner(SmartBindableBase owner){
			_runtimeActive = _active;
			OnSetOwner(owner);
		}
		/// <summary>
		/// Called when decorator is initially set up by its owner SmartObject.
		/// <para />Classes derived from SmartDecoratorBase must store the reference manually.
		/// </summary>
		protected abstract void OnSetOwner(SmartBindableBase owner);
	}
	/// <summary>
	/// Base class for custom SmartEvent Decorators.
	/// </summary>
	public abstract class SmartEventDecoratorBase : SmartDecoratorBase, ISmartRefOwnerRedirect {
		SmartEvent.Data.EventVar _owner;
		public SmartEvent.Data.EventVar owner {get {return _owner;}}
		IRelayBinding _onDispatchBinding;

		protected override void OnSetOwner(SmartBindableBase owner){
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

		public Object GetSmartRefOwner(){
			return _owner;
		}
		public System.Type GetOwnerType(){
			return GetType();
		}
	}

	/// <summary>
	/// Base class for custom SmartEvent Decorators.
	/// </summary>
	public abstract class SmartDataDecoratorBase<TData> : SmartDecoratorBase, ISmartRefOwnerRedirect {
		SmartVar<TData> _owner;
		public SmartVar<TData> owner {get {return _owner;}}
		public virtual TData OnUpdated(TData oldValue, TData newValue, RestoreMode restoreMode, ref BlockFlags block){return newValue;}
		protected override void OnSetOwner(SmartBindableBase owner){
			this._owner = (SmartVar<TData>)owner;
		}

		public Object GetSmartRefOwner(){
			return _owner;
		}
		public System.Type GetOwnerType(){
			return GetType();
		}
	}

	/// <summary>
	/// Base class for custom SmartSet Decorators.
	/// </summary>
	public abstract class SmartSetDecoratorBase<TData> : SmartDecoratorBase {
		SmartSet<TData> _owner;
		public SmartSet<TData> owner {get {return _owner;}}
		/// <summary>
		/// If NONE, data is being set normally.
		/// <para \>If AUTO, data is being reset automatically on scene change.
		/// <para \>If MANUAL, data is being reset manually by SetToDefault().
		/// <para \>Decorators aren't executed in INIT mode (runtime value initialisation in OnEnable).
		/// </summary>
		protected RestoreMode _currentRestoreMode {get; private set;}
		protected bool _isClearing {get; private set;}
		public virtual SetEventData<TData> OnChanged(SetEventData<TData> data){return data;}
		
		/// <summary>Called by SmartSet when Restore() is called. Do not call manually.</summary>
		public void BeginRestore(RestoreMode restore){
			if (restore == RestoreMode.NONE){
				throw new System.InvalidOperationException("Cannot begin restore with RestoreMode.NONE");
			}
			_currentRestoreMode = restore;
			OnBeginRestore();
		}
		/// <summary>Called by SmartSet when Restore() completes. Do not call manually.</summary>
		public void EndRestore(){
			if (_currentRestoreMode == RestoreMode.NONE){
				throw new System.InvalidOperationException("Cannot end restore - no restore in progress");
			}
			_currentRestoreMode = RestoreMode.NONE;
			OnEndRestore();
		}
		/// <summary>
		/// If Restore() has been called on owner, each element will be removed, then originals added.
		/// <para \>Before this process starts, this method will be called.
		/// </summary>
		protected virtual void OnBeginRestore(){}
		/// <summary>
		/// If Restore() has been called on owner, each element will be removed, then originals added.
		/// <para \>After this process ends, this method will be called (even if exceptions are thrown during Restore).
		/// </summary>
		protected virtual void OnEndRestore(){}

		/// <summary>Called by SmartSet when Clear() is called. Do not call manually.</summary>
		public void BeginClear(){
			_isClearing = true;
		}
		/// <summary>Called by SmartSet when Clear() completes. Do not call manually.</summary>
		public void EndClear(){
			_isClearing = false;
		}

		/// <summary>
		/// If Clear() has been called on owner, each element will be removed.
		/// <para \>Before this process starts, this method will be called.
		/// <para \>Clear() is also called during restore - check _currentRestoreMode to see if this is happening.
		/// </summary>
		protected virtual void OnBeginClear(){}
		/// <summary>
		/// If Clear() has been called on owner, each element will be removed.
		/// <para \>After this process ends, this method will be called (even if exceptions are thrown during Restore).
		/// <para \>Clear() is also called during restore - check _currentRestoreMode to see if this is happening.
		/// </summary>
		protected virtual void OnEndClear(){}

		protected override void OnSetOwner(SmartBindableBase owner){
			this._owner = (SmartSet<TData>)owner;
		}
	}
}