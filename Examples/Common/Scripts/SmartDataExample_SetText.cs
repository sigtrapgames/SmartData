using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SmartData.Examples {

	[RequireComponent(typeof(Text))]
	public class SmartDataExample_SetText : MonoBehaviour {
		public void SetText(int value){
			Set(value);
		}
		public void SetText(float value){
			Set(value);
		}
		public void SetText(bool value){
			Set(value);
		}
		public void SetText(Object value){
			Set(value);
		}
		void Set(object value){
			GetComponent<Text>().text = value.ToString();
		}
	}
}