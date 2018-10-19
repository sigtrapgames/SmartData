// SMARTTYPE bool
// SMARTTEMPLATE WriteSmartVarTemplate
// Do not move or delete the above lines

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.Abstract;

namespace SmartData.SmartBool.Components {
	/// <summary>
	/// Serialised write access to a SmartBool.
	/// </summary>
	[AddComponentMenu("SmartData/Bool/Write Smart Bool", 1)]
	public class WriteSmartBool : WriteSmartBase<bool, BoolWriter> {}
}