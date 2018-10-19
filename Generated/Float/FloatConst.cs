// SMARTTYPE float
// SMARTTEMPLATE SmartConstTemplate
// Do not move or delete the above lines

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.Abstract;
using SmartData.Interfaces;

namespace SmartData.SmartFloat.Data {
	/// <summary>
	/// ScriptableObject constant float.
	/// </summary>
	[CreateAssetMenu(menuName="SmartData/Float/Float Const", order=3)]
	public class FloatConst : SmartConst<float>, ISmartConst<float> {
		#if UNITY_EDITOR
		const string VALUETYPE = "float";
		const string DISPLAYTYPE = "Float Const";
		#endif
	}
}