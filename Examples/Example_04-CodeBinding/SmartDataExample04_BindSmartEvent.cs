using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SmartData.Examples {
	[RequireComponent(typeof(ParticleSystem))]
	public class SmartDataExample04_BindSmartEvent : MonoBehaviour {
		public SmartData.SmartEvent.EventListener myEvent;
		ParticleSystem _particles;
		Sigtrap.Relays.IRelayBinding _eventBinding;

		void Start(){
			_particles = GetComponent<ParticleSystem>();

			// This line adds OnEvent as a listener to the SmartEvent.
			// The return value is a link to the created binding.
			_eventBinding = myEvent.BindListener(OnEvent);

			// BindListener can also be used with inline lambdas/closures.
			// The returned binding makes it easy to remove/re-add the lambda/closure as a listener.
		}

		void OnEvent(){
			Debug.Log("Event Raised - playing particle system");
			_particles.Play();
		}

		void OnDestroy(){
			// This line removes OnEvent as a listener by disabling the binding.
			// This MUST be done at end-of-life to allow garbage collection.
			_eventBinding.Enable(false);
		}
	}
}