using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SmartData.Examples {
	[RequireComponent(typeof(Collider))]
	public class SmartDataExample_OnHit : MonoBehaviour {
		public string nameMatch;
		public SmartEvent.EventDispatcher onHit;
		
		void OnCollisionEnter(Collision col){
			if (col.collider.name == nameMatch){
				onHit.Dispatch();
			}
		}
	}
}