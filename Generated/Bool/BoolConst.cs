// SMARTTYPE bool
// SMARTTEMPLATE SmartConstTemplate
// Do not move or delete the above lines

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.Abstract;
using SmartData.Interfaces;

namespace SmartData.SmartBool.Data {
	/// <summary>
	/// ScriptableObject constant bool.
	/// </summary>
	[CreateAssetMenu(menuName="SmartData/Bool/Bool Const", order=3)]
	public class BoolConst : SmartConst<bool>, ISmartConst<bool> {
		#if UNITY_EDITOR
		const string VALUETYPE = "bool";
		const string DISPLAYTYPE = "Bool Const";
		#endif
	}
}