using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SmartData.Examples {
	[RequireComponent(typeof(Collider))]
	public class SmartDataExample_DamageOnCollision : MonoBehaviour {
		public float damage;

		void OnCollisionEnter(Collision col){
			var damageable = col.rigidbody.GetComponentInChildren<SmartDataExample_Damageable>();
			if (damageable){
				damageable.Damage(damage);
			}
		}
	}
}