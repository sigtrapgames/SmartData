using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.Abstract;
using SmartData.Interfaces;

namespace SmartData.SmartVector3.Data {
	/// <summary>
	/// ScriptableObject constant Vector3.
	/// </summary>
	[CreateAssetMenu(menuName="SmartData/Vector3/Vector3 Const", order=3)]
	public class Vector3Const : SmartConst<Vector3>, ISmartConst<Vector3> {
		#if UNITY_EDITOR
		const string VALUETYPE = "Vector3";
		const string DISPLAYTYPE = "Vector3 Const";
		#endif
	}
}