// SMARTTYPE bool
// SMARTTEMPLATE ReadSmartSetTemplate
// Do not move or delete the above lines

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.Abstract;

namespace SmartData.SmartBool.Components {
	/// <summary>
	/// Automatically listens to a <cref>SmartBoolSet</cref> and fires a <cref>UnityEvent<bool></cref> when data changes.
	/// </summary>
	[AddComponentMenu("SmartData/Bool/Read Smart Bool Set", 2)]
	public class ReadSmartBoolSet : ReadSmartBase<BoolSetReader> {}
}