using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SmartData.SmartEvent.Components {
	[AddComponentMenu("SmartData/Smart Event On Lifecycle", 103)]
	public class SmartEventOnLifecycle : MonoBehaviour {
		[SerializeField][ForceEventable][ForceNoAutoListen(hide=true)]
		EventDispatcher _onAwake;
		[SerializeField][ForceEventable][ForceNoAutoListen(hide=true)]
		EventDispatcher _onStart;
		[SerializeField][ForceEventable][ForceNoAutoListen(hide=true)]
		EventDispatcher _onEnable;
		[SerializeField][ForceEventable][ForceNoAutoListen(hide=true)]
		EventDispatcher _onDisable;
		[SerializeField][ForceEventable][ForceNoAutoListen(hide=true)]
		EventDispatcher _onDestroy;
		
		void Awake(){
			SetReceive(true, _onAwake, _onStart, _onEnable, _onDisable, _onDestroy);
			Dispatch(_onAwake);
		}
		void Start () {
			Dispatch(_onStart);
		}
		void OnEnable(){
			Dispatch(_onEnable);
		}
		void OnDisable(){
			Dispatch(_onDisable);
		}
		void OnDestroy(){
			Dispatch(_onDestroy);
			SetReceive(false, _onAwake, _onStart, _onEnable, _onDisable, _onDestroy);
		}

		void SetReceive(bool enable, params EventDispatcher[] es){
			foreach (var e in es){
				if (e.isValid){
					e.unityEventOnReceive = enable;
				}
			}
		}
		void Dispatch(EventDispatcher e){
			if (e.isValid){
				e.Dispatch();
			}
		}
	}
}