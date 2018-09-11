using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.Abstract;
using SmartData.Interfaces;

namespace SmartData.SmartInt.Data {
	/// <summary>
	/// ScriptableObject constant int.
	/// </summary>
	[CreateAssetMenu(menuName="SmartData/Int/Int Const", order=3)]
	public class IntConst : SmartConst<int>, ISmartConst<int> {
		#if UNITY_EDITOR
		const string VALUETYPE = "int";
		const string DISPLAYTYPE = "Int Const";
		#endif
	}
}