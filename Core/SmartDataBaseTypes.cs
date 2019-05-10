using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sigtrap.Relays;
using SmartData.Abstract;
using SmartData.Interfaces;
using System.Linq;

namespace SmartData {
#region Enums and Structs
	public enum MultiElementType {
		/// <summary>This SmartObject is not an element in any SmartMultis.</summary>
		NONE,
		/// <summary>This SmartObject is an element of a SmartMulti and was created by it.</summary>
		DYNAMIC,
		/// <summary>This SmartObject is an element in one or more SmartMultis and exists as an asset.</summary>
		PERSISTENT
	}
	public enum RestoreMode {
		/// <summary>Data is not being reset - this is a regular value set call.</summary>
		NONE,
		/// <summary>Data is being initialized in OnEnable. Decorators will not be executed.</summary>
		INIT,
		/// <summary>Data is being reset to default automatically on scene load.</summary>
		AUTO,
		/// <summary>Data is being reset to default manually.</summary>
		MANUAL
	}
	public enum SetOperation {
		NONE = 0,
		/// <summary>Data was added to the SmartSet. Index is that of new element. Index will be out of range during decorator execution.</summary>
		ADDED = 1,
		/// <summary>Data was removed from the SmartSet. Index is where the element was before removal.</summary>
		REMOVED = 2,
		/// <summary>Data was changed within the SmartSet. Index is the element which was changed.</summary>
		CHANGED = 3,
		/// <summary>Data was inserted into the SmartSet. Index is that of new element. NOT YET USED.</summary>
		INSERTED = 4,
		/// <summary>SmartSet is being cleared. Callback will be fired for each element individually with its index.</summary>
		CLEARED = 5
	}
	public struct SetEventData<T> {
		/// <summary>The new value.</summary>
		public T value {get; private set;}
		/// <summary>The previous value, if operation is CHANGED.</summary>
		public T previousValue {get; private set;}
		/// <summary>What operation was called on the set to fire this callback?</summary>
		public SetOperation operation {get; private set;}
		/// <summary>Index of the element affected by this operation.</summary>
		public int index {get; private set;}
		public SetEventData(T data, T previousData, SetOperation operation, int index){
			this.value = data;
			this.previousValue = previousData;
			this.operation = operation;
			this.index = index;
		}
	}
#endregion
}

namespace SmartData.Abstract {
#region Base
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
		
	#region Multi Registration
		Dictionary<SmartMultiBase, int> _multiParentsPersistent = new Dictionary<SmartMultiBase, int>();
		/// <summary>
		/// Indicates whether this SmartObject is an element in any SmartMultis.
		/// </summary>
		public MultiElementType multiElementType {get; private set;}
		/// <summary>
		/// If this SmartObject was dynamically created by a SmartMulti, returns that SmartMulti.
		/// Check multiElementType is DYNAMIC first.
		/// </summary>
		public SmartMultiBase multiParentDynamic {get; private set;}
		int _multiParentDynamicIndex = -1;
		/// <summary>
		/// If this SmartObject was dynamically created by a SmartMulti, returns index of this SmartObject in that SmartMulti.
		/// Check multiElementType is DYNAMIC first.
		/// </summary>
		public int multiParentDynamicIndex {get {return _multiParentDynamicIndex;}}
		/// <summary>
		/// If this SmartObject is an asset and referenced by one or more SmartMultis, returns those SmartMultis.
		/// Creates and returns a copy of the array.
		/// Check multiElementType is PERSISTENT first.
		/// </summary>
		public SmartMultiBase[] GetMultiParentsPersistent(){
			if (multiElementType != MultiElementType.PERSISTENT) return null;
			var result = new SmartMultiBase[_multiParentsPersistent.Count];
			int i=0;
			foreach (var a in _multiParentsPersistent){
				result[i] = a.Key;
				++i;
			}
			return result;
		}
		/// <summary>
		/// Returns the index of this SmartObject in the specified SmartMulti.
		/// If not an element in specified SmartMulti, returns -1.
		/// <param name="parent">The SmartMulti. Can leave null if multiElementType is DYNAMIC.</param>
		public int GetMultiIndex(SmartMultiBase parent=null){
			switch (multiElementType){
				case MultiElementType.NONE:
					return -1;
				case MultiElementType.DYNAMIC:
					if (parent == null || parent == multiParentDynamic){
						return _multiParentDynamicIndex;
					}
					return -1;
				case MultiElementType.PERSISTENT:
					if (parent == null){
						throw new System.ArgumentException("Must specify SmartMulti parent when getting MultiIndex of a PERSISTENT SmartObject.");
					}
					int result;
					if (!_multiParentsPersistent.TryGetValue(parent, out result)){
						result = -1;
					}
					return result;
			}
			return -1;
		}
		/// <summary>
		/// Internal use only. Registers this SmartObject as an element of a SmartMulti.
		/// </summary>
		public void SetMulti(SmartMultiBase multi, int index, bool isDynamic){
			if (isDynamic){
				if (multiElementType != MultiElementType.NONE){
					throw new System.Exception(string.Format("SmartObject's MultiElementType is already {0} - cannot set DYNAMIC SmartMulti parent", multiElementType));
				}
				multiElementType = MultiElementType.DYNAMIC;
				multiParentDynamic = multi;
				_multiParentDynamicIndex = index;
			} else {
				if (multiElementType == MultiElementType.DYNAMIC){
					throw new System.Exception(string.Format("SmartObject's MultiElementType is already {0} - cannot set PERSISTENT SmartMulti parent", multiElementType));
				}
				if (_multiParentsPersistent.ContainsKey(multi)){
					throw new System.ArgumentException(string.Format("Trying to add SmartMulti {0} as a parent of SmartObject {1} when already registered as a parent", multi.name, name));
				}
				multiElementType = MultiElementType.PERSISTENT;
				_multiParentsPersistent.Add(multi, index);
			}
		}
	#endregion Multi Registration

	#region Decorators from Multis
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
	#endregion Decorators from Multis
		
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
#endregion

#region Const
	/// <summary>
	/// Abstract base for SmartConsts. Do not reference. Will not serialize.
	/// </summary>
	public abstract class SmartConst<TData> : SmartBase {
		[SerializeField]
		TData _value;
		public TData value {get {return _value;}}
	}
#endregion Const

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
	}

	/// <summary>
	/// Abstract base for SmartData classes. Do not reference. Will not serialize.
	/// </summary>
	public abstract class SmartVar<TData> : SmartVarBase, ISmartVar<TData> {
		// Set in editor
		[SerializeField]
		protected DataType _dataType;
		[SerializeField][Tooltip("Event will not be automatically fired if data changed from editor.")]
		protected TData _value;
		/// <summary>
		/// Separate copy of serialized value for non-destructive runtime use
		/// </summary>
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
			set {SetValue(value, RestoreMode.NONE);}
		}
		public TData defaultValue {get {return _value;}}
		void SetValue(TData v, RestoreMode restore){
			// When setting initial value, don't dispatch anything or use decorators
			if (restore == RestoreMode.INIT){
				_runtimeValue = v;
				return;
			}

			// Auto-bound SmartRefs get bound first time value changes after they're queued
			if (_binder.hasRefsToBind){
				_binder.AutoBind();
			}
			
			if (_hasDecorators){
				TData temp = v;
				temp = ExecuteDecoratorsOnUpdate(_decorators, _runtimeValue, temp, restore);
				if (_multiDecorators.Count > 0){
					foreach (var a in _multiDecorators){
						temp = ExecuteDecoratorsOnUpdate(a.Value, _runtimeValue, temp, restore);
					}
				}

				_runtimeValue = temp;
			} else {
				_runtimeValue = v;
			}
			
			_relay.Dispatch(_runtimeValue);
		}

		TData ExecuteDecoratorsOnUpdate(SmartDecoratorBase[] decorators, TData oldValue, TData newValue, RestoreMode restore){
			if (decorators != null && decorators.Length != 0){
				for (int i=0; i<decorators.Length; ++i){
					var d = decorators[i];
					if (d.active){
						newValue = (decorators[i] as SmartDataDecoratorBase<TData>).OnUpdated(oldValue, newValue, restore);
					}
				}
			}
			return newValue;
		}

	#region Lifecycle
		protected override void OnEnable(){
			base.OnEnable();

			// Set DataType for reset behaviour
			_dataType = GetDataType(typeof(TData));

			#if UNITY_EDITOR
			// Ensure serialization
			var so = new UnityEditor.SerializedObject(this);
			so.FindProperty("_dataType").intValue = (int)_dataType;
			so.ApplyModifiedPropertiesWithoutUndo();			
			#endif

			UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;

			Restore(RestoreMode.INIT);
		}
		protected override void OnDisable(){
			base.OnDisable();
			UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnSceneChanged;
		}
		void OnSceneChanged(UnityEngine.SceneManagement.Scene s0, UnityEngine.SceneManagement.Scene s1){
			if (_resetOnSceneChange){
				Restore(RestoreMode.AUTO);
			}
		}
		/// <summary>
		/// Safely try to restore to serialized value.
		/// </summary>
		/// <param name="init">Passed in OnEnable to set initial value without dispatching events or decorators.</summary>
		protected void Restore(RestoreMode restore){
			try {
				OnRestore(restore);
			} catch {
				Debug.LogErrorFormat("Automatic reset behaviour for {0}/{1} failed. Please override {0}.OnRestore().", GetType().Name, this.name);
				throw;
			}
		}
		/// <summary>
		/// Restore runtime value to default (serialized) value.
		/// <para />Override this to implement type-specific restore behaviour.
		/// </summary>
		/// <param name="init">Passed in OnEnable to set initial value without dispatching events or decorators.</summary>
		protected virtual void OnRestore(RestoreMode restore){
			// Storing and restoring defaults are same action
			// Storing actually means writing existing value to runtime value
			switch (_dataType){
				case DataType.STRUCT:
				case DataType.CLASS:
					SetValue(_value, restore);
					break;
				case DataType.ARRAY:
					SetValue((TData)(_value as System.Array).Clone(), restore);
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
						SetValue((TData)_dictionaryConstructor.Invoke(argsDict), restore);
					} else {
						// Empty dictionary - just create a new one
						SetValue((TData)System.Activator.CreateInstance(dType), restore);
					}
					break;
				case DataType.COLLECTION:
					object[] argsCol = new object[]{_value};
					SetValue((TData)System.Activator.CreateInstance(typeof(TData), argsCol), restore);
					break;
			}
		}
	#endregion Lifecycle

		/// <summary>
		/// Force a dispatch if value object doesn't change, e.g. when changing an element from a List.
		/// </summary>
		public void Dispatch(){
			_relay.Dispatch(value);
			#if UNITY_EDITOR && !SMARTDATA_NO_GRAPH_HOOKS
			SmartData.Editors.SmartDataRegistry.OnRefCallToSmart(null, this);
			#endif
		}
		/// <summary>
		/// Reset to initial serialized value. For reference and collection types, may not have desired effect.
		/// </summary>
		public void SetToDefault(){
			Restore(RestoreMode.MANUAL);
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
	#endif
	}
#endregion Variable

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
		/// Raised when a element (SmartObject or local list entry) is added or removed (not currently supported) to/from this MultiRef.
		/// First int is new count, second is old count.
		/// </summary>
		public IRelayLink<int, int> onElementCountChanged {get {return _onElementCountChanged.link;}}
		protected Relay<int, int> _onElementCountChanged = new Relay<int, int>();

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
				for (int i=0; i<_persistent.Length; ++i){
					_persistent[i].SetMulti(this, i, false);
				}
				_onElementCountChanged.Dispatch(0, _runtimeList.Count);
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
						int oldCount = count;
						// Create runtime instance as new element
						// Will be destroyed on exit play mode in editor
						TSmart temp = ScriptableObject.CreateInstance<TSmart>();
						_runtimeList.Add(temp);
						// Register self as parent
						temp.SetMulti(this, count-1, true);
						_onElementCountChanged.Dispatch(oldCount, count);
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
	public abstract class SmartMulti<TData, TSmart> : SmartMulti<TSmart>, IEnumerable<TData> 
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

		IEnumerator<TData> IEnumerable<TData>.GetEnumerator(){
			return new MultiEnumerator<TData, SmartMulti<TData, TSmart>, TSmart>(this);
		}
		IEnumerator IEnumerable.GetEnumerator(){
			return new MultiEnumerator<TData, SmartMulti<TData, TSmart>, TSmart>(this);
		}

		public class MultiEnumerator<TData, TMulti, TSmart> : IEnumerator<TData> 
			where TMulti : SmartMulti<TData, TSmart>
			where TSmart : SmartVar<TData>
		{
			TMulti _multi;
			int _index;

			public MultiEnumerator(TMulti m){
				_multi = m;
			}

			TData IEnumerator<TData>.Current {
				get {
					return _multi.GetValue(_index);
				}
			}
			public bool MoveNext(){
				++_index;
				return _index < _multi.count;
			}
			public void Reset(){
				_index = 0;
			}
			object IEnumerator.Current {
				get {
					return _multi.GetValue(_index);
				}
			}
			public void Dispose(){}
		}
	}
#endregion Multi

#region Sets
	/// <summary>
	/// Abstract base for SmartSets. Do not reference.
	/// <para />IEnumerable is not implemented to avoid unexpected lack of callbacks.
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
		public IRelayLink<SetEventData<TData>> relay {get {return _relay.link;}}
		protected Relay<SetEventData<TData>> _relay = new Relay<SetEventData<TData>>();
		
		public TData this[int index]{
			get {return _runtimeSet[index];}
			set {
				// Auto-bound SmartRefs get bound first time value added/removed after they're queued
				if (_binder.hasRefsToBind){
					_binder.AutoBind();
				}

				var data = new SetEventData<TData>(value, _runtimeSet[index], SetOperation.CHANGED, index);
				if (_hasDecorators){
					var temp = data;
					temp = ExecuteDecoratorsOnChanged(temp);
					data = temp;
					value = temp.value;
				}

				_runtimeSet[index] = value;
				_relay.Dispatch(data);
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
				Restore(RestoreMode.AUTO);
			}
		}
		protected void Restore(RestoreMode restore){
			// Auto-bound SmartRefs get bound first time value added/removed after they're queued
			if (_binder.hasRefsToBind){
				_binder.AutoBind();
			}
			
			if (_hasDecorators){
				for (int i=0; i<_decorators.Length; ++i){
					((SmartSetDecoratorBase<TData>)_decorators[i]).BeginRestore(restore);
				}
			}

			// Outer try-finally to allow throw within catch while still ensuring inner finally is executed.
			try {
				try {
					SetAll(_set);
				} catch {
					Debug.LogError("Exception thrown during "+name+".Restore(). OnEndRestore will still be called on all decorators. Exception follows.");
					throw;
				} finally {
					// Ensure decorator state always reset correctly
					for (int i=0; i<_decorators.Length; ++i){
						((SmartSetDecoratorBase<TData>)_decorators[i]).EndRestore();
					}
				}
			} finally {}
		}
		protected override void OnDisable(){
			base.OnDisable();
			UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnSceneChanged;
		}
	#endregion Lifecycle

	#region Data
		IEnumerator<TData> IEnumerable<TData>.GetEnumerator(){
			return _runtimeSet.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator(){
			return _runtimeSet.GetEnumerator();
		}
		public bool Add(TData element, bool allowDuplicates=true){
			if (allowDuplicates || !_runtimeSet.Contains(element)){
				// Auto-bound SmartRefs get bound first time value added/removed after they're queued
				if (_binder.hasRefsToBind){
					_binder.AutoBind();
				}

				SetEventData<TData> data = new SetEventData<TData>(element, default(TData), SetOperation.ADDED, count);
				if (_hasDecorators){
					var temp = data;
					temp = ExecuteDecoratorsOnChanged(temp);
					data = temp;
					element = temp.value;
				}
				
				_runtimeSet.Add(element);
				_relay.Dispatch(data);
				return true;
			}
			return false;
		}
		/// <summary>
		/// Removes element from the set.
		/// <para />Warning: Decorators which alter the data on remove will not change which element is removed.
		/// </summary>
		/// <returns>If successful, index of removed element. Otherwise -1.</returns>
		public int Remove(TData element){
			// Auto-bound SmartRefs get bound first time value added/removed after they're queued
			if (_binder.hasRefsToBind){
				_binder.AutoBind();
			}
			
			int index = _runtimeSet.IndexOf(element);
			bool result = index >= 0;
			var data = new SetEventData<TData>(element, element, SetOperation.REMOVED, index);
			if (_hasDecorators){
				// Execute decorators without altering data
				for (int i=0; i<_decorators.Length; ++i){
					((SmartSetDecoratorBase<TData>)_decorators[i]).OnChanged(data);
				}
			}

			if (result){
				_runtimeSet.Remove(element);

				// Auto-bound SmartRefs get bound first time value added/removed after they're queued
				if (_binder.hasRefsToBind){
					_binder.AutoBind();
				}

				_relay.Dispatch(data);
			}
			return index;
		}

		/// <summary>
		/// Removes an element of the set by index.
		/// <para />Warning: Decorators which alter the data on remove will not change which element is removed.
		/// </summary>
		public bool RemoveAt(int index){
			// Auto-bound SmartRefs get bound first time value added/removed after they're queued
			if (_binder.hasRefsToBind){
				_binder.AutoBind();
			}

			return RemoveAt(index, SetOperation.REMOVED);
		}
		bool RemoveAt(int index, SetOperation operation){
			TData element = this[index];
			bool result = _runtimeSet.Count > index;
			var data = new SetEventData<TData>(element, element, operation, index);
			if (_hasDecorators){
				data = ExecuteDecoratorsOnChanged(data);
			}

			_runtimeSet.RemoveAt(index);
			_relay.Dispatch(data);
			return result;
		}
		public void Clear(){
			// Auto-bound SmartRefs get bound first time value added/removed after they're queued
			if (_binder.hasRefsToBind){
				_binder.AutoBind();
			}

			if (_hasDecorators){
				for (int i=0; i<_decorators.Length; ++i){
					((SmartSetDecoratorBase<TData>)_decorators[i]).BeginClear();
				}
			}

			// Outer try-finally to allow throw within catch while still ensuring inner finally is executed.
			try {
				try {
					// Remove all, with all appropriate callbacks
					while (_runtimeSet.Count > 0){
						RemoveAt(_runtimeSet.Count-1, SetOperation.CLEARED);
					}
				} catch {
					Debug.LogError("Exception thrown during "+name+".Clear(). OnEndClear will still be called on all decorators. Exception follows.");
					throw;
				} finally {
					// Ensure decorator state always reset correctly
					for (int i=0; i<_decorators.Length; ++i){
						((SmartSetDecoratorBase<TData>)_decorators[i]).EndClear();
					}
				}
			} finally {}
		}
		/// <summary>
		/// Reset to initial serialized value.
		/// </summary>
		public void SetToDefault(){
			// Auto-bound SmartRefs get bound first time value added/removed after they're queued
			if (_binder.hasRefsToBind){
				_binder.AutoBind();
			}

			Restore(RestoreMode.MANUAL);
		}
		/// <summary>
		/// Clear and replace all elements.
		/// <para />Largely intended for decorator use.
		/// </summary>
		public void SetAll(List<TData> set){
			Clear();

			// Copy new data to runtime set
			for (int i=0; i<set.Count; ++i){
				Add(_set[i]);
			}
		}
	#endregion Data

		/// <summary>
		/// Bind an event listener which passes the element and related metadata.
		/// <returns>IRelayBinding for easy enabling/disabling, or null if failed.</returns>
		public IRelayBinding BindListener(System.Action<SetEventData<TData>> listener){
			return relay.BindListener(listener);
		}
		/// <summary>
		/// Bind an event listener which passes nothing.
		/// <returns>IRelayBinding for easy enabling/disabling, or null if failed.</returns>
		public IRelayBinding BindListener(System.Action listener){
			return relay.BindListener((x)=>{listener();});
		}

		SetEventData<TData> ExecuteDecoratorsOnChanged(SetEventData<TData> data){
			for (int i=0; i<_decorators.Length; ++i){
				data = ((SmartSetDecoratorBase<TData>)_decorators[i]).OnChanged(data);
			}
			return data;
		}
	}
#endregion Sets
}