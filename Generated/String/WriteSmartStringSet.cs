// SMARTTYPE string
// SMARTTEMPLATE WriteSmartSetTemplate
// Do not move or delete the above lines

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.Abstract;

namespace SmartData.SmartString.Components {
	/// <summary>
	/// Serialised write access to a SmartStringSet.
	/// </summary>
	[AddComponentMenu("SmartData/String/Write Smart String Set", 3)]
	public class WriteSmartStringSet : WriteSetBase<string, StringSetWriter> {}
}