// SMARTTYPE bool
// SMARTTEMPLATE WriteSmartSetTemplate
// Do not move or delete the above lines

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.Abstract;

namespace SmartData.SmartBool.Components {
	/// <summary>
	/// Serialised write access to a SmartBoolSet.
	/// </summary>
	[AddComponentMenu("SmartData/Bool/Write Smart Bool Set", 3)]
	public class WriteSmartBoolSet : WriteSetBase<bool, BoolSetWriter> {}
}