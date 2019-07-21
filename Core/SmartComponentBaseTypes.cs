using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sigtrap.Relays;
using SmartData.Abstract;
using SmartData.Interfaces;
using System.Linq;

namespace SmartData.Abstract {
	public abstract class SmartComponentBase : MonoBehaviour {
		protected virtual bool bindEvents {get {return true;}}
		protected abstract void EnableUnityEvents(bool enable);
		void OnEnable(){
			if (!isValid){
				throw new MissingReferenceException(string.Format("{0}=>{1}  is missing a Smart object reference", name, GetType().Name));
			}
			if (bindEvents){
				EnableUnityEvents(true);
			}
		}
		void OnDisable(){
			if (bindEvents){
				EnableUnityEvents(false);
			}
		}
		void OnDestroy(){
			if (bindEvents){
				EnableUnityEvents(false);
			}
		}
		public abstract bool isValid {get;}
	}
	public abstract class ReadSmartBase<TRef> : SmartComponentBase
		where TRef : ISmartRef
	{

		[SerializeField][ForceEventable(true, false)][ForceNoAutoListen(true)]
		protected TRef[] _data;

		public override bool isValid {
			get {
				for (int i=0; i<_data.Length; ++i){
					if (!_data[i].isValid){
						return false;
					}
				}
				return true;
			}
		}

		protected override void EnableUnityEvents(bool enable){
			for (int i=0; i<_data.Length; ++i){
				_data[i].unityEventOnReceive = enable;
			}
		}
	}
	
	public abstract class WriteSmartBase<TData, TRef> : SmartComponentBase
		where TRef : ISmartRefWriter<TData>
	{
		[SerializeField][ForceEventable(false, false)][ForceNoAutoListen(true)][ForceHideEvent]
		protected TRef _data = default(TRef);

		[SerializeField, Tooltip("Value to set when Set() is called")]
		TData _valueToSet = default(TData);

		protected override bool bindEvents {get {return false;}}

		public override bool isValid {get {return _data.isValid;}}
		public TData value {
			get {return ((SmartData.Interfaces.ISmartDataWriter<TData>)_data).value;}
			set {((SmartData.Interfaces.ISmartDataWriter<TData>)_data).value = value;}
		}
		public void Dispatch(){
			_data.Dispatch();
		}
		/// <summary> For UnityEvent calls on data types UnityEvent won't serialize </summary>
		public void Set(){
			value = _valueToSet;
		}
		protected override void EnableUnityEvents(bool enable){}	// Write-only - UnityEvent never used.
	}

	public abstract class WriteSetBase<TData, TRef> : SmartComponentBase
		where TRef : ISmartSetRefWriter<TData>
	{
		[SerializeField][ForceEventable(false, false)][ForceNoAutoListen(true)][ForceHideEvent]
		protected TRef _data;
		public override bool isValid {get {return _data != null;}}
		protected override bool bindEvents {get {return false;}}

		public void Add(TData val){
			_data.Add(val);
		}
		public void Remove(TData val){
			_data.Remove(val);
		}
		public void RemoveAt(int index){
			_data.RemoveAt(index);
		}
		protected override void EnableUnityEvents(bool enable){}	// Write-only - UnityEvent never used.
	}
}