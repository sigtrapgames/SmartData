using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SmartData.Examples {
	[RequireComponent(typeof(CharacterController))]
	public class SmartDataExample_CharacterController : MonoBehaviour {
		CharacterController _cc;
		public float speed = 10;
		void Awake () {
			_cc = GetComponent<CharacterController>();
		}
		
		void Update () {
			Vector2 move = Vector2.zero;
			if (Input.GetKey(KeyCode.W)){
				move.y += 1;
			}
			if (Input.GetKey(KeyCode.S)){
				move.y -= 1;
			}
			if (Input.GetKey(KeyCode.D)){
				move.x += 1;
			}
			if (Input.GetKey(KeyCode.A)){
				move.x -= 1;
			}
			_cc.Move(new Vector3(move.x, 0, move.y) * speed * Time.deltaTime);
		}
	}
}