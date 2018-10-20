using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SmartData.Examples {
	[RequireComponent(typeof(Collider))]
	public class SmartDataExample02_Damageable : MonoBehaviour {
		public float damagePerHit;
		public SmartFloat.FloatWriter hp;

		void OnCollisionEnter(Collision c){
			if (c.collider.name == "Damager"){
				hp.value -= damagePerHit;
			}
		}
	}
}