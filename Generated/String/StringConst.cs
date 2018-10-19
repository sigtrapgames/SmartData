// SMARTTYPE string
// SMARTTEMPLATE SmartConstTemplate
// Do not move or delete the above lines

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.Abstract;
using SmartData.Interfaces;

namespace SmartData.SmartString.Data {
	/// <summary>
	/// ScriptableObject constant string.
	/// </summary>
	[CreateAssetMenu(menuName="SmartData/String/String Const", order=3)]
	public class StringConst : SmartConst<string>, ISmartConst<string> {
		#if UNITY_EDITOR
		const string VALUETYPE = "string";
		const string DISPLAYTYPE = "String Const";
		#endif
	}
}