// SMARTTYPE Vector3
// SMARTTEMPLATE WriteSmartSetTemplate
// Do not move or delete the above lines

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.Abstract;

namespace SmartData.SmartVector3.Components {
	/// <summary>
	/// Serialised write access to a SmartVector3Set.
	/// </summary>
	[AddComponentMenu("SmartData/Vector3/Write Smart Vector3 Set", 3)]
	public class WriteSmartVector3Set : WriteSetBase<Vector3, Vector3SetWriter> {}
}