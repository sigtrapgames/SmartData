using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sigtrap.Relays;
using SmartData.Abstract;
using SmartData.Interfaces;
using System.Linq;

namespace SmartData.Abstract {
	#region General
	/// <summary>
	/// Non-generic abstract base for all SmartRefs. Do not reference.
	/// </summary>
	public abstract class SmartRefBase : ISmartRef {
		#if UNITY_EDITOR
		protected abstract SmartBase _EDITOR_GetSmartObject(out bool useMultiIndex);
		protected virtual bool _EDITOR_GetIsWritable(){return false;}
		public static SmartBase _EDITOR_GetSmartObject(System.Type t, SmartRefBase target, out bool useMultiIndex, out bool writeable){
			SmartBase smart = target._EDITOR_GetSmartObject(out useMultiIndex);
			writeable = target._EDITOR_GetIsWritable();
			return smart;
		}
		#endif

		[SerializeField][HideInInspector]
		Component _owner;
		[SerializeField][HideInInspector]
		string _propertyPath;
		[SerializeField]
		protected bool _autoListen = false;
		[SerializeField]
		private string _notes;

		public abstract bool isValid {get;}
		/// <summary>
		/// Gets the name of the underlying smart object, or null if using a local value.
		/// </summary>
		public abstract string name {get;}

		#region UnityEvent
		protected IRelayBinding _unityEventBinding;
		bool _unityEventEnabled = false;

		/// <summary>
		/// If enabled, built-in UnityEvent will respond to events raised by referenced SmartObject.
		/// <para />UnityEvent will always fire if SmartObject updated by this instance.
		/// <para />Code bindings via BindListener or relay are not affected.
		/// <para />Disabled by default. Remember to disable at end of life.
		/// </summary>
		public bool unityEventOnReceive {
			get {return _unityEventEnabled && _unityEventBinding != null && _unityEventBinding.enabled;}
			set {
				if (_unityEventEnabled == value) return;
				_unityEventEnabled = value;

				if (_unityEventEnabled && _unityEventBinding == null){
					_unityEventBinding = BindUnityEvent();
				}
				if (_unityEventBinding != null){
					_unityEventBinding.Enable(_unityEventEnabled);

					// Turn off auto bind as soon as binding is disabled.
					// This doesn't actually do anything but changes display in editor.
					if (!_unityEventEnabled){
						_autoListen = false;
					}
				}
			}
		}
		
		protected abstract IRelayBinding BindUnityEvent();
		#endregion

		public SmartRefBase(){
			#if UNITY_EDITOR
			SmartData.Editors.SmartDataRegistry.RegisterReference(this);
			#endif
		}

		/// <summary>
		/// Register this SmartRef for automatic event unbinding when gameobject is destroyed.
		/// Note: adds a MonoBehaviour to the gameobject when first called.
		/// Called automatically if Auto Bind checked in editor.
		/// </summary>
		public void UnbindOnDestroy(bool enableUnityEventNow=true){
			SmartData.Components.SmartRefUnbinder.UnbindOnDestroy(this, _owner.gameObject, enableUnityEventNow);
		}
	}
	/// <summary>
	/// Non-generic abstract base for SmartRefs which can use SmartMultis. Do not reference.
	/// </summary>
	public abstract class SmartRefMultiableBase : SmartRefBase {
		[SerializeField]
		protected int _multiIndex;
	}
	/// <summary>
	/// Non-generic abstract base for SmartDataRefs, for Editor purposes. Do not reference.
	/// </summary>
	public abstract class SmartDataRefBase : SmartRefMultiableBase {		
		public enum RefType {
			/// <summary>Reference uses a local value</summary>
			LOCAL,
			/// <summary>Reference uses a SmartConst</summary>
			CONST,
			/// <summary>Reference uses a SmartVar</summary>
			VAR,
			/// <summary>Reference uses a SmartMulti</summary>
			MULTI
		}

		protected static readonly RefType[] TYPES_ALL = new List<RefType>(
			(RefType[])System.Enum.GetValues(typeof(RefType))
		).ToArray();
		protected static readonly RefType[] TYPES_WRITABLE = 
			TYPES_ALL.Where((r)=>{return r != RefType.CONST;}).ToArray();
		protected static readonly RefType[] TYPES_EVENTABLE = 
			TYPES_WRITABLE.Where((r)=>{return r != RefType.LOCAL;}).ToArray();

		#if UNITY_EDITOR
		protected static readonly string[] TYPENAMES_ALL = GetTypenames(TYPES_ALL);
		protected static readonly string[] TYPENAMES_WRITABLE = GetTypenames(TYPES_WRITABLE);
		protected static readonly string[] TYPENAMES_EVENTABLE = GetTypenames(TYPES_EVENTABLE);

		static string[] GetTypenames(RefType[] rts){
			string[] result = new string[rts.Length];
			for (int i=0; i<rts.Length; ++i){
				result[i] = rts[i].ToString();
			}
			return result;
		}
		#endif

		[SerializeField]
		protected RefType _refType = RefType.VAR;	// Note: Never default to CONST as Writer doesn't support it

		protected bool _isEventable {
			get {
				for (int i=0; i<TYPES_EVENTABLE.Length; ++i){
					if (TYPES_EVENTABLE[i] == _refType) return true;
				}
				return false;
			}
		}
	}
	/// <summary>
	/// Abstract base for SmartDataRefs. Do not reference. Will not serialize.
	/// </summary>
	public abstract class SmartDataRefBase<TData, TVar, TConst, TMulti> : 
		SmartDataRefBase, ISmartRefReader<TData>, ISerializationCallbackReceiver
		where TVar : SmartVar<TData>, ISmartVar<TData>
		where TConst : SmartConst<TData>
		where TMulti : SmartMulti<TData, TVar>
	{
		#if UNITY_EDITOR
		static RefType[] _EDITOR_GetUsableRefTypes(){
			return TYPES_ALL;
		}
		static string[] _EDITOR_GetUsableRefNames(){
			return TYPENAMES_ALL;
		}
		protected sealed override SmartBase _EDITOR_GetSmartObject(out bool useMultiIndex){
			useMultiIndex = false;
			switch (_refType){
				case RefType.CONST: 
					return _smartConst;
				case RefType.VAR: 
					return _smartVar;
				case RefType.MULTI:
					useMultiIndex = true;
					return _smartMulti;
			}
			return null;
		}
		protected virtual void TriggerSmartRegistry(){
			if (_refType == RefType.MULTI){
				Editors.SmartDataRegistry.OnRefCallToSmart(this, _smartMulti, _writeable);
			} else {
				Editors.SmartDataRegistry.OnRefCallToSmart(this, _smartVar);
			}
		}
		#endif

		[SerializeField]
		protected TData _value;
		protected TData _default;
		[SerializeField]
		TConst _smartConst;
		[SerializeField]
		TVar _smartVar;
		[SerializeField]
		TMulti _smartMulti;

		public TData value {
			get {
				switch (_refType){
					case RefType.LOCAL: return _value;
					case RefType.CONST: return _smartConst.value;
					case RefType.VAR:
					case RefType.MULTI:
						return _writeable.value;
				}
				return default(TData);
			}
		}
		public override bool isValid {
			get {
				switch (_refType){
					case RefType.CONST:
						return _smartConst != null;
					case RefType.VAR:
						return _smartVar != null;
					case RefType.MULTI:
						return _smartMulti != null;
				}
				return true;
			}
		}
		public override string name {
			get {
				switch (_refType){
					case RefType.CONST:
						return _smartConst.name;
					case RefType.VAR:
						return _smartVar.name;
					case RefType.MULTI:
						return _smartMulti.name;
				}
				return "";
			}
		}

		protected TVar _writeable {
			get {
				switch (_refType){
					case RefType.VAR:
						return _smartVar;
					case RefType.MULTI:
						return _smartMulti[_multiIndex];
				}
				return null;
			}
		}

		/// <summary>
		/// Get decorators attached to the referenced SmartObject.
		/// </summary>
		/// <param name="includeMultis">If true, includes decorators from parent SmartMultis.</param>
		/// <returns>Decorators if decorable SmartObject referenced. Null otherwise.</returns>
		public TDecorator[] GetDecorators<TDecorator>(bool includeMultis=false) where TDecorator : SmartDecoratorBase {
			var w = _writeable;
			if (w != null){
				return w.GetDecorators<TDecorator>(includeMultis);
			}
			return null;
		}
		/// <summary>
		/// Get decorators attached to the referenced SmartObject without GC allocation.
		/// See Unity? It's easy.
		/// </summary>
		/// <param name="results">Pre-initialised array to pass matching decorators into.</param>
		/// <param name="includeMultis">If true, includes decorators from parent SmartMultis.</param>
		/// <returns>Number of decorators found and filled. If supplied array isn't long enough, this will be the length of the array. If a local value is referenced, returns -1.</returns>
		public int GetDecoratorsNonAlloc<TDecorator>(TDecorator[] results, bool includeMultis=false) where TDecorator : SmartDecoratorBase {
			var w = _writeable;
			if (w != null){
				return w.GetDecoratorsNonAlloc<TDecorator>(results, includeMultis);
			}
			return -1;
		}

		/// <summary>
		/// If underlying object supports events, gives listen-only access to advanced features.
		/// </summary>
		public IRelayLink<TData> relay {
			get {
				if (!_isEventable) return null;
				return _writeable.relay;
			}
		}
		/// <summary>
		/// If underlying object supports events, binds a listener to it which passes current value.
		/// <param name="callNow">If true, call just this listener with the current value of the SmartData.</param>
		/// </summary>
		public IRelayBinding BindListener(System.Action<TData> listener, bool callNow=false){
			if (!_isEventable) return null;
			return _writeable.BindListener(listener, callNow);
		}
		/// <summary>
		/// If underlying object supports events, binds a listener to it which passes nothing.
		/// </summary>
		public IRelayBinding BindListener(System.Action listener){
			if (!_isEventable) return null;
			return _writeable.BindListener(listener);
		}
		protected override IRelayBinding BindUnityEvent(){
			if (!_isEventable) return null;
			var w = _writeable;
			if (w != null){
				return w.BindListener(GetUnityEventInvoke());
			}
			return null;
		}
		protected abstract System.Action<TData> GetUnityEventInvoke();

		#region ISerializationCallbackReceiver Implementation
		bool _hasDefault;
		public void OnBeforeSerialize(){}
		public void OnAfterDeserialize(){
			if (!_hasDefault){
				// No need to restore on scene change
				_default = _value;
				_hasDefault = true;
			}

			if (_autoListen){
				switch (_refType){
					case RefType.VAR:
						if (_smartVar != null){
							_smartVar.RequestCtorAutoBinding(this);
						}
						break;
					case RefType.MULTI:
						if (_smartMulti != null){
							_smartMulti.RequestCtorAutoBinding(this, _multiIndex);
						}
						break;
				}
			}
		}
		#endregion
	}
	/// <summary>
	/// Abstract base for SmartRefWriters. Do not reference. Will not serialize.
	/// </summary>
	public abstract class SmartDataRefWriter<TData, TVar, TConst, TMulti> : 
		SmartDataRefBase<TData, TVar, TConst, TMulti>, ISmartRefWriter<TData>
		where TVar : SmartVar<TData>, ISmartVar<TData>
		where TConst : SmartConst<TData>
		where TMulti : SmartMulti<TData, TVar>
	{
		#if UNITY_EDITOR
		static RefType[] _EDITOR_GetUsableRefTypes(){{
			return TYPES_WRITABLE;
		}}
		static string[] _EDITOR_GetUsableRefNames(){{
			return TYPENAMES_WRITABLE;
		}}
		protected sealed override bool _EDITOR_GetIsWritable(){return true;}
		#endif

		new public TData value {
			get {return base.value;}
			set {
				switch (_refType){
					case RefType.LOCAL:
						_value = value;
						break;
					case RefType.VAR:
					case RefType.MULTI:
						_writeable.value = value;
						if (!unityEventOnReceive){
							InvokeUnityEvent(value);
						}
						#if UNITY_EDITOR && !SMARTDATA_NO_GRAPH_HOOKS
						TriggerSmartRegistry();
						#endif
						break;
				}
			}
		}
		/// <summary>
		/// Reset to initial serialized value. For reference types, may not have desired effect.
		/// </summary>
		public void SetToDefault(){
			switch (_refType){
				case RefType.LOCAL:
					_value = _default;
					break;
				case RefType.VAR:
				case RefType.MULTI:
					_writeable.SetToDefault();
					if (!unityEventOnReceive){
						InvokeUnityEvent(value);
					}
					#if UNITY_EDITOR && !SMARTDATA_NO_GRAPH_HOOKS
					TriggerSmartRegistry();
					#endif
					break;
			}
		}
		/// <summary>
		/// If using SmartVar or SmartMulti, force dispatch.
		/// Useful when changing contents of underlying reference-type data.
		/// </summary>
		public void Dispatch(){
			var w = _writeable;
			if (w != null){
				w.Dispatch();
			}
			if (!unityEventOnReceive){
				InvokeUnityEvent(value);
			}
		}

		protected abstract void InvokeUnityEvent(TData value);
	}
	#endregion

	#region Multi-specific
	/// <summary>
	/// Abstract base for SmartDataMultiRefs and SmartEventMultiRefs. Do not reference. Will not serialize.
	/// </summary>
	public abstract class SmartMultiRef<TMulti, TVar> : SmartRefMultiableBase
		where TMulti:SmartMulti<TVar>
		where TVar:SmartDecorableBase
	{
		[SerializeField]
		TMulti _smartMulti;

		public override bool isValid {get {return _smartMulti != null;}}
		/// <summary>Returns the name of the referenced SmartMulti (not the underlying SmartVar element)</summary>
		public override string name {get {return _smartMulti.name;}}

		/// <summary>
		/// Set index to change which Smart object is being referenced.
		/// Unity event binding will track changes. Remember to update manually bound listeners.
		/// </summary>
		public int index {
			get {return _multiIndex;}
			set {
				// Ensure UnityEvent binding tracks index
				if (value != _multiIndex){
					// Cache bound state
					bool bound = unityEventOnReceive;
					// Unlisten if listening
					if (_unityEventBinding != null){
						_unityEventBinding.Enable(false);
					}
					// Re-bind to new Smart object if was bound previously
					if (bound){
						_unityEventBinding = BindUnityEvent();
					} else {
						_unityEventBinding = null;
					}
					_multiIndex = value;
				}
			}
		}

		protected TMulti _multi {get {return _smartMulti;}}
	}
	/// <summary>
	/// Abstract base for SmartDataMultiRefs. Do not reference. Will not serialize.
	/// </summary>
	public abstract class SmartDataMultiRef<TList, TData, TVar> : 
		SmartMultiRef<TList, TVar>, ISmartDataMultiRefReader<TData, TVar>
		where TList:SmartMulti<TData, TVar>
		where TVar:SmartVar<TData>
	{
		#if UNITY_EDITOR
		protected sealed override SmartBase _EDITOR_GetSmartObject(out bool useMultiIndex){
			useMultiIndex = true;
			return _multi;
		}
		#endif

		/// <summary>
		/// Read-only access to the indexed SmartVar value.
		/// </summary>
		public TData value {
			get {return _multi[index].value;}
		}
		public IRelayLink<TData> relay {get {return _multi[index].relay;}}

		public IRelayBinding BindListener(System.Action<TData> listener, bool callNow=false){
			var result = relay.BindListener(listener);
			if (callNow){
				_multi[index].Dispatch();
			}
			return result;
		}
		public IRelayBinding BindListener(System.Action listener){
			return relay.BindListener((x)=>{listener();});
		}
		protected override IRelayBinding BindUnityEvent(){
			var r = _multi[index];
			if (r != null){
				return r.relay.BindListener(GetUnityEventInvoke());
			}
			return null;
		}
		protected abstract System.Action<TData> GetUnityEventInvoke();
	}
	public abstract class SmartDataMultiRefWriter<TList, TData, TVar> : 
		SmartDataMultiRef<TList, TData, TVar>, ISmartDataMultiRefWriter<TData, TVar>
		where TList:SmartMulti<TData, TVar>
		where TVar:SmartVar<TData>
	{
		#if UNITY_EDITOR
		protected sealed override bool _EDITOR_GetIsWritable(){return true;}
		#endif

		new public TData value {
			get {return base.value;}
			set {
				_smartVar.value = value;
				if (!unityEventOnReceive){
					InvokeUnityEvent(value);
				}
				#if UNITY_EDITOR && !SMARTDATA_NO_GRAPH_HOOKS
				Editors.SmartDataRegistry.OnRefCallToSmart(this, _multi, _smartVar);
				#endif
			}
		}
		
		protected TVar _smartVar {
			get {return _multi[index];}
		}
		/// <summary>
		/// Force a dispatch if value object doesn't change, e.g. when changing an element from a List.
		/// </summary>
		public void Dispatch(){
			_smartVar.Dispatch();
			if (!unityEventOnReceive){
				InvokeUnityEvent(value);
			}
		}
		public void SetToDefault(){
			_smartVar.SetToDefault();
			if (!unityEventOnReceive){
				InvokeUnityEvent(value);
			}
		}

		protected abstract void InvokeUnityEvent(TData value);
	}
	#endregion

	#region Sets
	/// <summary>
	/// Abstract non-generic base for SmartSetRefs for editor purposes. Do not reference.
	/// </summary>
	public abstract class SmartSetRefBase : SmartRefBase {}

	/// <summary>
	/// Abstract base for SmartSetRefs. Do not reference.
	/// </summary>
	public abstract class SmartSetRefBase<TData, TWrite> : SmartSetRefBase, ISerializationCallbackReceiver
		where TWrite : SmartSet<TData>, ISmartSet<TData>
	{
		#if UNITY_EDITOR
		protected sealed override SmartBase _EDITOR_GetSmartObject(out bool useMultiIndex){
			useMultiIndex = false;
			if (_useList) return null;
			return _smartSet;
		}
		#endif

		[SerializeField]
		protected bool _useList = false;
		[SerializeField]
		protected TWrite _smartSet;
		[SerializeField]
		List<TData> _list = new List<TData>();
		[SerializeField]
		protected List<TData> _runtimeList;

		protected bool _isEventable {get {return !_useList;}}
		public override bool isValid {get {return (_useList ? true : _smartSet != null);}}
		public override string name {
			get {
				if (_useList) return null;
				return _smartSet.name;
			}
		}

		public void OnBeforeSerialize(){}
		public void OnAfterDeserialize(){
			if (_runtimeList == null){
				// Copy serialised data to runtime list
				_runtimeList = new List<TData>(_list);
			}

			if (_autoListen && _useList && _smartSet != null){
				_smartSet.RequestCtorAutoBinding(this);
			}
		}
		protected void Restore(){
			_runtimeList.Clear();
			for (int i=0; i<_list.Count; ++i){
				_runtimeList.Add(_list[i]);
			}
		}

		public TData this[int index]{
			get {return _useList ? _runtimeList[index] : _smartSet[index];}
		}
		public int count {
			get {return _useList ? _runtimeList.Count : _smartSet.count;}
		}
		public IRelayLink<TData, bool> relay {
			get {
				if (!_isEventable) return null;
				return _smartSet.relay;
			}
		}
		public IRelayBinding BindListener(System.Action<TData, bool> listener){
			if (!_isEventable) return null;
			return _smartSet.BindListener(listener);
		}
		public IRelayBinding BindListener(System.Action listener){
			if (!_isEventable) return null;
			return _smartSet.BindListener(listener);
		}
		protected abstract System.Action<TData, bool> GetUnityEventInvoke();
		protected override IRelayBinding BindUnityEvent(){
			if (_smartSet == null) return null;
			return relay.BindListener(GetUnityEventInvoke());
		}
	}

	public abstract class SmartSetRefWriterBase<TData, TWrite> :
		SmartSetRefBase<TData, TWrite>, ISmartSetRefWriter<TData>
		where TWrite : SmartSet<TData>, ISmartSet<TData>
	{
		#if UNITY_EDITOR
		protected sealed override bool _EDITOR_GetIsWritable(){return true;}
		#endif

		new public TData this[int index]{
			get {return base[index];}
			set {
				if (_useList){
					_runtimeList[index] = value;
				} else {
					_smartSet[index] = value;
				}
			}
		}
		public bool Add(TData element, bool allowDuplicates=true){
			if (_useList){
				if (allowDuplicates || !_runtimeList.Contains(element)){
					_runtimeList.Add(element);
					return true;
				}
			} else {
				bool result = _smartSet.Add(element, allowDuplicates);
				if (!unityEventOnReceive){
					InvokeUnityEvent(_smartSet[_smartSet.count-1], true);
				}
				#if UNITY_EDITOR
				Editors.SmartDataRegistry.OnRefCallToSmart(this, _smartSet);
				#endif
				return result;
			}
			return false;
		}
		public bool Remove(TData element){
			if (_useList){
				return _runtimeList.Remove(element);
			} else {
				bool result = _smartSet.Remove(element);
				if (!unityEventOnReceive){
					InvokeUnityEvent(element, false);
				}
				#if UNITY_EDITOR
				Editors.SmartDataRegistry.OnRefCallToSmart(this, _smartSet);
				#endif
				return result;
			}
		}
		public bool RemoveAt(int index){
			bool result = false;
			if (_useList){
				result = _runtimeList.Count > index;
				TData element = _runtimeList[index];
				_runtimeList.RemoveAt(index);
				if (!unityEventOnReceive){
					InvokeUnityEvent(element, false);
				}
				#if UNITY_EDITOR
				Editors.SmartDataRegistry.OnRefCallToSmart(this, _smartSet);
				#endif
			} else {
				result = _smartSet.RemoveAt(index);
			}
			return result;
		}
		/// <summary>
		/// Reset to initial serialized values.
		/// </summary>
		public void SetToDefault(){
			if (_useList) {
				Restore();
			} else {
				_smartSet.SetToDefault();
			}
		}

		protected abstract void InvokeUnityEvent(TData value, bool added);
	}
	#endregion
}