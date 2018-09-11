using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SmartData.SmartEvent.Components {
	[AddComponentMenu("SmartData/Smart Event On Lifecycle", 103)]
	public class SmartEventOnLifecycle : MonoBehaviour {
		[SerializeField][ForceEventable][ForceNoAutoListen]
		EventDispatcher _onAwake;
		[SerializeField][ForceEventable][ForceNoAutoListen]
		EventDispatcher _onStart;
		[SerializeField][ForceEventable][ForceNoAutoListen]
		EventDispatcher _onEnable;
		[SerializeField][ForceEventable][ForceNoAutoListen]
		EventDispatcher _onDisable;
		[SerializeField][ForceEventable][ForceNoAutoListen]
		EventDispatcher _onDestroy;
		
		void Awake(){
			_onAwake.unityEventOnReceive = true;
			_onStart.unityEventOnReceive = true;
			_onEnable.unityEventOnReceive = true;
			_onDisable.unityEventOnReceive = true;
			_onDestroy.unityEventOnReceive = true;

			_onAwake.Dispatch();
		}
		void Start () {
			_onStart.Dispatch();
		}
		void OnEnable(){
			_onEnable.Dispatch();
		}
		void OnDisable(){
			_onDisable.Dispatch();
		}
		void OnDestroy(){
			_onDestroy.Dispatch();

			_onAwake.unityEventOnReceive = false;
			_onStart.unityEventOnReceive = false;
			_onEnable.unityEventOnReceive = false;
			_onDisable.unityEventOnReceive = false;
			_onDestroy.unityEventOnReceive = false;
		}
	}
}