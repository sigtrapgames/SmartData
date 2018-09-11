using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sigtrap.Relays;
using SmartData.Abstract;
using SmartData.Interfaces;
using System.Linq;

namespace SmartData.Abstract {
	/// <summary>
	/// Non-generic abstract base for all Smart classes. Do not reference directly.
	/// </summary>
	public abstract class SmartBase : ScriptableObject {
		protected class AutoBindingHelper {
			List<ISmartRef> _refsToBind;
			public bool hasRefsToBind {get; private set;}
			public void RequestAutoBinding(ISmartRef r){
				if (_refsToBind == null){
					_refsToBind = new List<ISmartRef>();
				}
				if (!_refsToBind.Contains(r)){
					_refsToBind.Add(r);
					hasRefsToBind = true;
				}
			}
			public void AutoBind(){
				AutoBind(_refsToBind);
				hasRefsToBind = false;
			}
			public static void AutoBind(List<ISmartRef> refs){
				for (int i=0; i<refs.Count; ++i){
					refs[i].UnbindOnDestroy();
				}
				refs.Clear();
			}
		}

		[SerializeField]
		string _description;

		protected virtual void OnEnable(){
			#if UNITY_EDITOR
			SmartData.Editors.SmartDataRegistry.RegisterSmart(this);
			#endif
		}
		protected virtual void OnDisable(){
			#if UNITY_EDITOR
			SmartData.Editors.SmartDataRegistry.UnregisterSmart(this);
			#endif
		}

		#if UNITY_EDITOR
		protected virtual void _EDITOR_ForceDispatch(){}
		#endif
	}
	/// <summary>
	/// Abstract base class for all Smart types with modifiable data. Do not reference.
	/// </summary>
	public abstract class SmartBindableBase : SmartBase {
		[SerializeField]
		protected SmartDecoratorBase[] _decorators;

		static List<SmartDecoratorBase> _tempGetDecorators = new List<SmartDecoratorBase>();
		protected int _tempDecoratorCount {get {return _tempGetDecorators.Count;}}
		protected bool _hasDecorators;

		protected AutoBindingHelper _binder = new AutoBindingHelper();
		/// <summary>
		/// Internal use only.
		/// Called by SmartRefs during deserialization to auto-bind unity events.
		/// Auto-binding requires addition of SmartRefUnbinder component which cannot be created during deserialization.
		/// So Data waits for value to be changed, then binds event and add component.
		/// </summary>
		public void RequestCtorAutoBinding(ISmartRef r){
			_binder.RequestAutoBinding(r);
		}

		/// <summary>
		/// Fills _tempGetDecorators with decorators matching or derived from the specified type, from the passed array.
		/// </summary>
		/// <returns>The number of decorators added to _tempGetDecorators.</returns>
		protected int PopulateTempDecorators(System.Type t, SmartDecoratorBase[] input, bool clear){
			if (clear){
				_tempGetDecorators.Clear();
			}

			int results = 0;
			for (int i=0; i<input.Length; ++i){
				if (t.IsAssignableFrom(input[i].GetType())){
					_tempGetDecorators.Add(input[i]);
					++results;
				}
			}
			return results;
		}
		/// <summary>
		/// Transfers and casts decorators from _tempGetDecorators to specified array.
		/// </summary>
		/// <returns>The number of decorators filled - at most, the length of the passed array.</returns>
		protected int FillResultDecorators<TDecorator>(TDecorator[] results) where TDecorator : SmartDecoratorBase {
			int count = Mathf.Min(_tempGetDecorators.Count, results.Length);
			for (int i=0; i<count; ++i){
				results[i] = (TDecorator)_tempGetDecorators[i];
			}
			return count;
		}
		// Evaluate hasDecorators in OnEnable, but allow multidecorators to update this later as they'll be added after OnEnable
		protected virtual void EvalHasDecorators(){
			_hasDecorators = _decorators != null && _decorators.Length > 0;
		}
		protected override void OnEnable(){
			base.OnEnable();
			EvalHasDecorators();
		}
	}
	/// <summary>
	/// Abstract base for Smart types which use Decorators (i.e. not Multis). Do not reference.
	/// </summary>
	public abstract class SmartDecorableBase : SmartBindableBase {
		protected Dictionary<SmartMultiBase, SmartDecoratorBase[]> _multiDecorators = new Dictionary<SmartMultiBase, SmartDecoratorBase[]>();

		/// <summary>
		/// Get attached decorators of, or derived from, the specified type.
		/// </summary>
		/// <param name="includeMultis">If true, includes decorators from parent SmartMultis.</param>
		public TDecorator[] GetDecorators<TDecorator>(bool includeMultis=false) where TDecorator : SmartDecoratorBase {
			PopulateTempDecorators<TDecorator>(includeMultis);
			TDecorator[] results = new TDecorator[_tempDecoratorCount];
			FillResultDecorators(results);
			return results;
		}
		/// <summary>
		/// Get attached decorators of, or derived from, the specified type, without GC allocation.
		/// See Unity? It's easy.
		/// </summary>
		/// <param name="results">Pre-initialised array to pass matching decorators into.</param>
		/// <param name="includeMultis">If true, includes decorators from parent SmartMultis.</param>
		/// <returns>Number of decorators found and filled. If supplied array isn't long enough, this will be the length of the array.</returns>
		public int GetDecoratorsNonAlloc<TDecorator>(TDecorator[] results, bool includeMultis=false) where TDecorator : SmartDecoratorBase {
			PopulateTempDecorators<TDecorator>(includeMultis);
			return FillResultDecorators(results);
		}
		/// <summary>
		/// Fills _tempGetDecorators with decorators matching or derived from the specified type.
		/// </summary>
		/// <param name="includeMultis">If true, includes decorators from parent SmartMultis.</param>
		/// <returns>Internal decorators added first - returns the number thereof.</returns>
		protected int PopulateTempDecorators<TDecorator>(bool includeMultis){
			var t = typeof(TDecorator);
			int result = PopulateTempDecorators(t, _decorators, true);

			if (includeMultis){
				foreach (var a in _multiDecorators){
					PopulateTempDecorators(t, a.Value, false);
				}
			}

			return result;
		}
		

		/// <summary>
		/// Internal use only. Registers a SmartMulti's decorators with its child SmartDatas.
		/// </summary>
		public void SetMultiDecorators(SmartDecoratorBase[] decorators, SmartMultiBase multi){
			if (decorators == null || decorators.Length == 0){
				_multiDecorators.Remove(multi);
				WarnMultiDecorators();
				return;
			}

			SmartDecoratorBase[] d = new SmartDecoratorBase[decorators.Length];
			for (int i=0; i<decorators.Length; ++i){
				d[i] = decorators[i];
				d[i].SetOwner(this);
			}
			_multiDecorators[multi] = d;

			EvalHasDecorators();			
			WarnMultiDecorators();		
		}
		void WarnMultiDecorators(){
			if (_multiDecorators.Count > 1){
				string multis = "";
				foreach (var a in _multiDecorators){
					multis +=  string.Format("\n{0} : {1} decorator(s)", a.Key.name, a.Value.Length);
				}
				Debug.LogWarningFormat("SmartData {0} has external decorators from {1} SmartMultis. Order of these groups of decorators is undefined.{2}", name, _multiDecorators.Count, multis);
			}
		}
		protected override void EvalHasDecorators(){
			_hasDecorators = 
				(_multiDecorators != null && _multiDecorators.Count > 0) || 
				(_decorators != null && _decorators.Length > 0);
		}
		protected override void OnEnable(){
			base.OnEnable();
			if (_decorators != null){
				for (int i=0; i<_decorators.Length; ++i){
					_decorators[i].SetOwner(this);
				}
			}
		}

		#if UNITY_EDITOR
		void _EDITOR_RemoveMultiDecorators(SmartMultiBase[] owners){
			foreach (var o in owners){
				_multiDecorators.Remove(o);
			}
			WarnMultiDecorators();
		}
		#endif
	}

	#region Const
	/// <summary>
	/// Abstract base for SmartConsts. Do not reference. Will not serialize.
	/// </summary>
	public abstract class SmartConst<TData> : SmartBase {
		[SerializeField]
		TData _value;
		public TData value {get {return _value;}}
	}
	#endregion

	#region Variable
	/// <summary>
	/// Non-generic abstract base for SmartData, for Editor purposes. Do not reference.
	/// </summary>
	public abstract class SmartVarBase : SmartDecorableBase {
		public enum DataType {
			/// <summary>Underlying data type not yet determined</summary>
			NONE,
			/// <summary>Underlying data is value type</summary>
			STRUCT,
			/// <summary>Underlying data is an array</summary>
			ARRAY,
			/// <summary>Underlying data is a Dictionary</summary>
			DICTIONARY,
			/// <summary>Underlying data is a collection</summary>
			COLLECTION,
			/// <summary>Underlying data is reference type</summary>
			CLASS
		}

		[SerializeField]
		protected bool _resetOnSceneChange = false;

		#if UNITY_EDITOR
		public static DataType GetDataType(System.Type tData){
			if (tData.IsValueType || tData == typeof(string) || tData.IsSubclassOf(typeof(string))){
				return DataType.STRUCT;
			} else if (tData.IsArray){
				return DataType.ARRAY;
			} else if (typeof(IDictionary).IsAssignableFrom(tData)){
				return DataType.DICTIONARY;
			} else if (typeof(IEnumerable).IsAssignableFrom(tData)){
				return DataType.COLLECTION;
			} else {
				return DataType.CLASS;
			}
		}
		#endif
	}

	/// <summary>
	/// Abstract base for SmartData classes. Do not reference. Will not serialize.
	/// </summary>
	public abstract class SmartVar<TData> : SmartVarBase, ISmartVar<TData> {
		[SerializeField]
		protected DataType _dataType;
		[SerializeField][Tooltip("Event will not be automatically fired if data changed from editor.")]
		protected TData _value;
		/// <summary>Separate copy of serialized value for non-destructive runtime use</summary>
		[SerializeField]
		protected TData _runtimeValue;

		/// <summary>
		/// Link to underlying event. Gives access to additional functionality.
		/// </summary>
		public IRelayLink<TData> relay {get {return _relay.link;}}
		protected Relay<TData> _relay = new Relay<TData>();

		System.Reflection.ConstructorInfo _dictionaryConstructor = null;

		public TData value {
			get {return _runtimeValue;}
			set {
				// Auto-bound SmartRefs get bound first time value changes after they're queued
				if (_binder.hasRefsToBind){
					_binder.AutoBind();
				}
				
				if (_hasDecorators){
					TData temp = value;
					temp = ExecuteDecoratorsOnUpdate(_decorators, temp);
					if (_multiDecorators.Count > 0){
						foreach (var a in _multiDecorators){
							temp = ExecuteDecoratorsOnUpdate(a.Value, temp);
						}
					}
					_runtimeValue = temp;
				} else {
					_runtimeValue = value;
				}
				
				_relay.Dispatch(_runtimeValue);
			}
		}
		TData ExecuteDecoratorsOnUpdate(SmartDecoratorBase[] decorators, TData value){
			if (decorators != null && decorators.Length != 0){
				for (int i=0; i<decorators.Length; ++i){
					var d = decorators[i];
					if (d.active){
						value = (decorators[i] as SmartDataDecoratorBase<TData>).OnUpdated(value);
					}
				}
			}
			return value;
		}

		#region Lifecycle
		protected override void OnEnable(){
			base.OnEnable();

			#if UNITY_EDITOR
			// Set DataType for reset behaviour
			DataType dt = GetDataType(typeof(TData));			
			var so = new UnityEditor.SerializedObject(this);
			so.FindProperty("_dataType").intValue = (int)dt;
			so.ApplyModifiedPropertiesWithoutUndo();
			#endif

			UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;

			Restore();
		}
		protected override void OnDisable(){
			base.OnDisable();
			UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnSceneChanged;
		}
		void OnSceneChanged(UnityEngine.SceneManagement.Scene s0, UnityEngine.SceneManagement.Scene s1){
			if (_resetOnSceneChange){
				Restore();
			}
		}
		protected void Restore(){
			try {
				OnRestore();
			} catch (System.Exception ex){
				Debug.LogErrorFormat(
					"Automatic reset behaviour for {0} failed. Please override {0}.OnRestore().", GetType().Name
				);
				throw ex;
			}
		}
		/// <summary>
		/// Restore runtime value to default (serialized) value.
		/// <para />Override this to implement type-specific restore behaviour.
		/// </summary>
		protected virtual void OnRestore(){
			// Storing and restoring defaults are same action
			// Storing actually means writing existing value to runtime value
			switch (_dataType){
				case DataType.STRUCT:
				case DataType.CLASS:
					value = _value;
					break;
				case DataType.ARRAY:
					value = (TData)(_value as System.Array).Clone();
					break;
				case DataType.DICTIONARY:
					var dType = typeof(TData);
					object[] argsDict = new object[]{_value};
					if (argsDict[0] != null){
						// Find Dictionary<T1,T1>(IDictionary<T1,T2>) copy constructor
						if (_dictionaryConstructor == null){
							var _iDictType = typeof(IDictionary<,>).MakeGenericType(
								dType.GetGenericArguments()
							);
							_dictionaryConstructor = dType.GetConstructor(new System.Type[]{_iDictType});
						}
						value = (TData)_dictionaryConstructor.Invoke(argsDict);
					} else {
						// Empty dictionary - just create a new one
						value = (TData)System.Activator.CreateInstance(dType);
					}
					break;
				case DataType.COLLECTION:
					object[] argsCol = new object[]{_value};
					value = (TData)System.Activator.CreateInstance(typeof(TData), argsCol);
					break;
			}
		}
		#endregion

		/// <summary>
		/// Force a dispatch if value object doesn't change, e.g. when changing an element from a List.
		/// </summary>
		public void Dispatch(){
			_relay.Dispatch(value);
		}
		/// <summary>
		/// Reset to initial serialized value. For reference and collection types, may not have desired effect.
		/// </summary>
		public void SetToDefault(){
			Restore();
		}

		/// <summary>
		/// Bind an event listener which passes the current value.
		/// <param name="callNow">If true, call just this listener with the current value of the SmartVar.</param>
		/// <returns>IRelayBinding for easy enabling/disabling, or null if failed.</returns>
		public IRelayBinding BindListener(System.Action<TData> listener, bool callNow=false){
			IRelayBinding result = relay.BindListener(listener);
			if (callNow){
				listener(value);
			}
			return result;
		}
		/// <summary>
		/// Bind an event listener which passes nothing.
		/// <returns>IRelayBinding for easy enabling/disabling, or null if failed.</returns>
		public IRelayBinding BindListener(System.Action listener){
			return relay.BindListener((x)=>{listener();});
		}

		public override string ToString(){
			if(_runtimeValue != null)
			{
				return _runtimeValue.ToString();
			}
			return "<missing runtime>";
		}

		#if UNITY_EDITOR
		void _EDITOR_UpdateRtValue(){
			value = value;
		}
		protected override void _EDITOR_ForceDispatch(){
			_relay.Dispatch(_runtimeValue);
			#if !SMARTDATA_NO_GRAPH_HOOKS
			SmartData.Editors.SmartDataRegistry.OnRefCallToSmart(null, this);
			#endif
		}
		#endif
	}
	#endregion

	#region Multi
	/// <summary>
	/// Non-generic base for SmartMulti, for Decorator purposes. Do not reference.
	/// </summary>
	public abstract class SmartMultiBase : SmartBindableBase {
		[SerializeField][Tooltip("Auto-adding Smart objects will cease at this length.\n<=0 for no limit.")]
		protected int _maxSize = -1;

		/// <summary>
		/// Auto-adding Smart objects will cease at this length.
		/// No limit if <= 0.
		/// </summary>
		public int maxSize {get {return _maxSize;}}
		public abstract int count {get;}

		/// <summary>
		/// Get attached decorators of, or derived from, the specified type.
		/// </summary>
		public TDecorator[] GetDecorators<TDecorator>() where TDecorator : SmartDecoratorBase {
			PopulateTempDecorators(typeof(TDecorator), _decorators, true);
			TDecorator[] results = new TDecorator[_tempDecoratorCount];
			FillResultDecorators(results);
			return results;
		}
		/// <summary>
		/// Get attached decorators of, or derived from, the specified type, without GC allocation.
		/// See Unity? It's easy.
		/// </summary>
		/// <param name="results">Pre-initialised array to pass matching decorators into.</param>
		/// <returns>Number of decorators found and filled. If supplied array isn't long enough, this will be the length of the array.</returns>
		public int GetDecoratorsNonAlloc<TDecorator>(TDecorator[] results) where TDecorator : SmartDecoratorBase {
			PopulateTempDecorators(typeof(TDecorator), _decorators, true);
			return FillResultDecorators(results);
		}
	}
	/// <summary>
	/// Abstract base for SmartDataMultis and SmartEventMultis. Do not reference. Will not serialize.
	/// </summary>
	public abstract class SmartMulti<TSmart> : SmartMultiBase
		where TSmart:SmartDecorableBase
	{
		[SerializeField]
		TSmart[] _persistent;
		protected List<TSmart> _runtimeList;

		public override int count {get {return _runtimeList != null ? _runtimeList.Count : 0;}}

		Dictionary<int, List<ISmartRef>> _refsToBindByIndex = new Dictionary<int, List<ISmartRef>>();
		bool _hasRefsToBind = false;

		#if UNITY_EDITOR
		bool _EDITOR_HasPersistent(TSmart member){
			for (int i=0; i<_persistent.Length; ++i){
				if (_persistent[i] == member){
					return true;
				}
			}
			return false;
		}
		#endif

		protected override void OnEnable(){
			base.OnEnable();

			// Copy serialised data to runtime data
			// No scene change hook required
			// Persistent elements will handle their own restore behaviour
			// Non-persistent elements have no default anyway and shouldn't be treated as if they did
			if (_runtimeList == null){
				_runtimeList = new List<TSmart>();
			} else {
				_runtimeList.Clear();
			}
			if (_persistent != null){
				_runtimeList.AddRange(_persistent);
			}
			UpdateDecorators();
		}

		/// <summary>
		/// Internal use only!
		/// Called by SmartRefs during deserialization to auto-bind unity events.
		/// Auto-binding requires addition of SmartRefUnbinder component which cannot be created during deserialization.
		/// Multi elements created lazily and cannot be created during deserialization.
		/// So Multi waits for element to be accessed, then binds event and adds component.
		/// </summary>
		public void RequestCtorAutoBinding(ISmartRef r, int index){
			List<ISmartRef> refs = null;
			if (!_refsToBindByIndex.TryGetValue(index, out refs)){
				refs = new List<ISmartRef>();
				_refsToBindByIndex[index] = refs;
			}
			if (!refs.Contains(r)){
				refs.Add(r);
				_hasRefsToBind = true;
			}
		}

		/// <summary>
		/// Get a Smart object.
		/// If index out of range but below maxSize, new Smart objects will be auto-added.
		/// <summary>
		public TSmart this[int index]{
			get {return Get(index);}
		}
		protected TSmart Get(int index){
			// Semi-lazy - look for all requested indices and fill elements
			// Later, auto-bind queued SmartRefs, so all required elements must exist
			int maxIndex = index;
			if (_hasRefsToBind){
				foreach (var a in _refsToBindByIndex){
					index = Mathf.Max(a.Key, index);
				}
			}

			// Check for element existence - if no, create lazily
			if (count <= maxIndex){
				if (_maxSize <= 0 || index < _maxSize){
					while (count <= index){
						// Create runtime instance as new element
						// Will be destroyed on exit play mode in editor
						TSmart temp = ScriptableObject.CreateInstance<TSmart>();
						_runtimeList.Add(temp);
					}
					OnElementsAdded();
				} else {
					throw new System.AccessViolationException("Attempting to get index "+index.ToString()+" from a SmartMulti with a maximum size of "+_maxSize.ToString());
				}
			}

			// Iterate through ALL binding requests as can be queued after elements already exist
			if (_hasRefsToBind){
				// Set false FIRST
				// When refs bind, they recurse into this Get method to find the element
				// If _hasRefsToBind still true, infinite recursion through dictionary
				_hasRefsToBind = false;
				foreach (var a in _refsToBindByIndex){
					AutoBindingHelper.AutoBind(a.Value);
				}
				_refsToBindByIndex.Clear();
			}
			
			return _runtimeList[index];
		}

		void UpdateDecorators(){
			// Separate method for editor call when decorators modified during play
			for (int i=0; i<_runtimeList.Count; ++i){
				_runtimeList[i].SetMultiDecorators(_decorators, this);
			}
		}
		protected void OnElementsAdded(){
			UpdateDecorators();
		}
	}
	
	/// <summary>
	/// Abstract base for SmartDataMultis. Do not reference. Will not serialize.
	/// </summary>
	public abstract class SmartMulti<TData, TSmart> : SmartMulti<TSmart> 
		where TSmart : SmartVar<TData>
	{
		#if UNITY_EDITOR
		protected SmartVarBase.DataType _EDITOR_GetDataType(){
			return SmartVarBase.GetDataType(typeof(TData));
		}
		protected System.Type _EDITOR_GetSmartType(){
			return typeof(TSmart);
		}
		#endif

		public TData GetValue(int index){
			return this[index].value;
		}
		/// <summary>
		/// Bind an event listener to indexed SmartVar which passes current value.
		/// <param name="callNow">If true, call just this listener with the current value of the SmartVar.</param>
		/// <returns>IRelayBinding for easy enabling/disabling, or null if failed.</returns>
		public IRelayBinding BindListener(System.Action<TData> listener, int index, bool callNow=false){
			return this[index].BindListener(listener, callNow);
		}
		/// <summary>
		/// Bind an event listener to indexed SmartVar which passes nothing.
		/// <returns>IRelayBinding for easy enabling/disabling, or null if failed.</returns>
		public IRelayBinding BindListener(System.Action listener, int index){
			return this[index].BindListener(listener);
		}
	}
	#endregion

	#region Sets
	/// <summary>
	/// Abstract base for SmartSets. Do not reference.
	/// </summary>
	public abstract class SmartSet<TData> : SmartDecorableBase, IEnumerable<TData> {
		[SerializeField]
		protected bool _resetOnSceneChange = false;
		
		/// <summary>
		/// Only used by editor to add elements from inspector.
		/// Can't be #if'd as would create mismatch with serialized data in build.
		/// </summary>
		[SerializeField]
		TData _toAdd;

		[SerializeField]
		List<TData> _set;
		protected List<TData> _runtimeSet;
		
		/// <summary>
		/// Link to underlying event. Gives access to additional functionality.
		/// </summary>
		public IRelayLink<TData, bool> relay {get {return _relay.link;}}
		protected Relay<TData, bool> _relay = new Relay<TData, bool>();
		
		public TData this[int index]{
			get {return _runtimeSet[index];}
			set {
				_runtimeSet[index] = value;
				_relay.Dispatch(value, true);
			}
		}
		public int count {get {return _runtimeSet.Count;}}

		#region Lifecycle
		protected override void OnEnable(){
			base.OnEnable();

			if (_set != null) {
				_runtimeSet = new List<TData>(_set);
			} else {
				_runtimeSet = new List<TData>();
			}
			UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;
		}
		void OnSceneChanged(UnityEngine.SceneManagement.Scene s0, UnityEngine.SceneManagement.Scene s1){
			if (_resetOnSceneChange){
				Restore();
			}
		}
		protected void Restore(){
			// Remove all
			while (_runtimeSet.Count > 0){
				RemoveAt(_runtimeSet.Count-1);
			}
			// Copy persistent data to runtime set
			for (int i=0; i<_set.Count; ++i){
				Add(_set[i]);
			}
		}
		protected override void OnDisable(){
			base.OnDisable();
			UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnSceneChanged;
		}
		#endregion

		#region Data
		public bool Add(TData element, bool allowDuplicates=true){
			if (allowDuplicates || !_runtimeSet.Contains(element)){
				// Auto-bound SmartRefs get bound first time value added/removed after they're queued
				if (_binder.hasRefsToBind){
					_binder.AutoBind();
				}

				if (_hasDecorators){
					TData temp = element;
					temp = ExecuteDecoratorsOnChanged(temp, true);
					_runtimeSet.Add(temp);
				} else {
					_runtimeSet.Add(element);
				}
				_relay.Dispatch(element, true);
				return true;
			}
			return false;
		}
		public bool Remove(TData element){
			bool result = false;
			if (_hasDecorators){
				TData temp = element;
				temp = ExecuteDecoratorsOnChanged(temp, false);
				result = _runtimeSet.Remove(temp);
			} else {
				result = _runtimeSet.Remove(element);
			}

			if (result){
				// Auto-bound SmartRefs get bound first time value added/removed after they're queued
				if (_binder.hasRefsToBind){
					_binder.AutoBind();
				}

				_relay.Dispatch(element, false);
			}
			return result;
		}
		TData ExecuteDecoratorsOnChanged(TData element, bool added){
			for (int i=0; i<_decorators.Length; ++i){
				element = ((SmartSetDecoratorBase<TData>)_decorators[i]).OnChanged(element, true);
			}
			return element;
		}
		TData ExecuteDecoratorsOnRemoveAt(TData element, int index){
			for (int i=0; i<_decorators.Length; ++i){
				element = ((SmartSetDecoratorBase<TData>)_decorators[i]).OnRemovedAt(element, index);
			}
			return element;
		}

		/// <summary>
		/// Removes an element of the set by index.
		/// <para />Warning: Decorators which alter the data on remove will not change which element is removed.
		/// </summary>
		public bool RemoveAt(int index){
			TData element = this[index];
			bool result = _runtimeSet.Count > index;
			if (_hasDecorators){
				TData temp = element;
				temp = ExecuteDecoratorsOnRemoveAt(temp, index);
			}
			_runtimeSet.RemoveAt(index);
			
			_relay.Dispatch(element, false);
			return result;
		}
		/// <summary>
		/// Reset to initial serialized value.
		/// </summary>
		public void SetToDefault(){
			Restore();
		}
		/// <summary>
		/// Clear and replace all elements.
		/// <para />Largely intended for decorator use.
		/// </summary>
		public void SetAll(List<TData> set){
			
		}
		#endregion

		/// <summary>
		/// Bind an event listener which passes the element and whether it was added (true) or removed (false).
		/// <returns>IRelayBinding for easy enabling/disabling, or null if failed.</returns>
		public IRelayBinding BindListener(System.Action<TData, bool> listener){
			return relay.BindListener(listener);
		}
		/// <summary>
		/// Bind an event listener which passes nothing.
		/// <returns>IRelayBinding for easy enabling/disabling, or null if failed.</returns>
		public IRelayBinding BindListener(System.Action listener){
			return relay.BindListener((x,y)=>{listener();});
		}

		public IEnumerator<TData> GetEnumerator(){
			return _runtimeSet.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator(){
			return this.GetEnumerator();
		}
	}
	#endregion
}