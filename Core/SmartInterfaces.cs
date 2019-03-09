using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sigtrap.Relays;

namespace SmartData.Interfaces {
	#region Granular
	#region Base underlying Smart objects (valid for TRead/TWrite constraints)
	/// <summary>
	/// Lowest level Smart object that holds data or event.
	/// </summary>
	public interface ISmartObject {}

	/// <summary>
	/// <para />A stateless SmartEvent whose event can be bound to.
	/// <para />* relay get;
	/// <para />* BindListener(void)
	/// </summary>
	public interface ISmartEvent : ISmartObject {
		IRelayLink relay {get;}
		IRelayBinding BindListener(System.Action listener);
	}
	/// <summary>
	/// <para />A SmartData whose event can be bound to.
	/// <para />* relay TData
	/// <para />* BindListener(TData)
	/// <para />* BindListener(void)
	/// </summary>
	public interface ISmartEvent<TData> : ISmartObject {
		IRelayLink<TData> relay {get;}
		IRelayBinding BindListener(System.Action<TData> listener, bool callNow=false);
		IRelayBinding BindListener(System.Action listener);
	}
	/// <summary>
	/// <para />A Smart object whose event can be dispatched.
	/// <para />* Dispatch()
	/// </summary>
	public interface ISmartEventDispatcher : ISmartObject {
		void Dispatch();
	}

	/// <summary>
	/// <para />A SmartData whose value can be read.
	/// <para />* value get;
	/// </summary>
	public interface ISmartData<TData> : ISmartObject {
		TData value {get;}
	}
	/// <summary>
	/// <para />A SmartData whose value can be read and written.
	/// <para />* value get;set;
	/// <para />* defaultValue get;;
	/// <para />* SetToDefault()
	/// </summary>
	public interface ISmartDataWriter<TData> : ISmartObject {
		TData value {get; set;}
		TData defaultValue {get;}
		void SetToDefault();
	}
	/// <summary>
	/// <para />A SmartData which can auto-bind SmartRefs to its event
	/// <para />* RequestCtorAutoBinding(ISmartRef)
	/// </summary>
	public interface ISmartAutoBinder : ISmartObject {
		void RequestCtorAutoBinding(ISmartRef r);
	}
	/// <summary>
	/// <para />A SmartMulti which can auto-bind SmartRefs to the events of its elements
	/// <para />* RequestCtorAutoBinding(ISmartRef, index)
	/// </summary>
	public interface ISmartMultiAutoBinder : ISmartObject {
		void RequestCtorAutoBinding(ISmartRef r, int index);
	}
	/// <summary>
	/// <para />A typed SmartSet whose elements can be read.
	/// <para />* [] get;set;
	/// <para />* BindListener(void)
	/// <para />* BindListener(TData,bool)
	/// </summary>
	public interface ISmartSet<TData> : ISmartObject {
		int count {get;}
		
		TData this[int index]{get;}
		IRelayLink<SetEventData<TData>> relay {get;}
		IRelayBinding BindListener(System.Action listener);
		IRelayBinding BindListener(System.Action<SetEventData<TData>> listener);
	}
	/// <summary>
	/// <para />A typed SmartSet whose elements can be read and written.
	/// <para />* [] get;set;
	/// <para />* Add(TData, bool)
	/// <para />* Remove(TData)
	/// </summary>
	public interface ISmartSetWriter<TData> : ISmartObject {
		TData this[int index]{get;set;}
		bool Add(TData element, bool allowDuplicates=true);
		int Remove(TData element);
	}
	/// <summary>
	/// <para />A SmartSet whose elements can be removed by index.
	/// <para />* RemoveAt(index)
	/// </summary>
	public interface ISmartSetWriter : ISmartObject {
		bool RemoveAt(int index);
	}
	#endregion

	#region Base wrapper objects
	/// <summary>
	/// <para />A SmartRef with a built-in UnityEvent.
	/// <para />* isValid get;
	/// <para />* unityEventOnReceive get; set;
	/// <para />* UnbindOnDestroy()
	/// </summary>
	public interface ISmartRef {
		bool isValid {get;}
		bool unityEventOnReceive {get; set;}
		void UnbindOnDestroy(bool enableUnityEventNow=true);
	}
	/// <summary>
	/// <para />A SmartMulti whose Smart objects can be accessed by index.
	/// <para />* [] get;
	/// </summary>
	public interface ISmartMulti<TSmart> where TSmart : ISmartObject {
		TSmart this[int index] {get;}
	}
	/// <summary>
	/// <para />A SmartMulti whose underlying Smart object events can be bound to by index.
	/// <para />* BindListener(TDelegate, int)
	/// </summary>
	public interface ISmartMultiBindable<TDelegate> {
		IRelayBinding BindListener(TDelegate listener, int index, bool callNow=false);
	}
	/// <summary>
	/// <para />A self-indexed SmartRef to a SmartMulti.
	/// <para />* index get; set;
	/// </summary>
	public interface ISmartMultiRef {
		int index {get; set;}
	}
	#endregion
	#endregion

	#region Composite
	#region Variable
	public interface ISmartConst<TData> : 
		ISmartData<TData>
	{}
	public interface ISmartVar<TData> : 
		ISmartDataWriter<TData>,
		ISmartEvent<TData>,
		ISmartEventDispatcher,
		ISmartAutoBinder
	{}
	#endregion
	
	#region Multis
	public interface ISmartMulti<TData, TSmart> :
		ISmartMulti<TSmart>,
		ISmartMultiBindable<System.Action<TData>>,
		ISmartMultiAutoBinder
		where TSmart : ISmartVar<TData>, ISmartEventDispatcher
	{}
	public interface ISmartEventMulti :
		ISmartMulti<ISmartEvent>,
		ISmartMultiBindable<System.Action>,
		ISmartMultiAutoBinder
	{}
	#endregion

	#region Sets
	public interface ISmartDataSet<TData> :
		ISmartSet<TData>,
		ISmartSetWriter<TData>,
		ISmartSetWriter,
		ISmartAutoBinder
	{}
	#endregion
	
	#region Refs
	public interface ISmartEventRefListener :
		ISmartRef, 
		ISmartEvent
	{}
	public interface ISmartEventRefDispatcher :
		ISmartRef, 
		ISmartEvent,
		ISmartEventDispatcher
	{}
	
	public interface ISmartRefReader<TData> :
		ISmartRef,
		ISmartData<TData>,
		ISmartEvent<TData>
	{}
	public interface ISmartRefWriter<TData> :
		ISmartRef,
		ISmartData<TData>,
		ISmartEvent<TData>,
		ISmartEventDispatcher,
		ISmartDataWriter<TData>
	{}

	public interface ISmartEventMultiRefReader :
		ISmartRef,
		ISmartMultiRef,
		ISmartEvent
	{}
	public interface ISmartEventMultiRefWriter :
		ISmartRef,
		ISmartMultiRef,
		ISmartEvent,
		ISmartEventDispatcher
	{}

	public interface ISmartDataMultiRefReader<TData, TSmart> : 
		ISmartRef,
		ISmartMultiRef,
		ISmartEvent<TData>
		where TSmart : ISmartVar<TData>
	{}
	public interface ISmartDataMultiRefWriter<TData, TSmart> : 
		ISmartRef,
		ISmartMultiRef,
		ISmartEvent<TData>,
		ISmartDataWriter<TData>
		where TSmart : ISmartVar<TData>
	{}
	public interface ISmartSetRefReader<TData> : 
		ISmartRef,
		ISmartSet<TData>
	{}
	public interface ISmartSetRefWriter<TData> : 
		ISmartRef,
		ISmartSet<TData>,
		ISmartSetWriter<TData>,
		ISmartSetWriter
	{}
		
	#endregion

	#endregion
}