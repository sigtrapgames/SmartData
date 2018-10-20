using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SmartData.Examples {
	public class SmartDataExample_Damageable : MonoBehaviour {
		public SmartFloat.FloatWriter hp;

		public void Damage(float damage){
			hp.value -= damage;
			if (hp.value <= 0){
				Destroy(gameObject);
			}
		}
	}
}