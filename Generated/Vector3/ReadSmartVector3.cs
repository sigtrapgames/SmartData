// SMARTTYPE Vector3
// SMARTTEMPLATE ReadSmartVarTemplate
// Do not move or delete the above lines

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.Abstract;

namespace SmartData.SmartVector3.Components {
	/// <summary>
	/// Automatically listens to a <cref>SmartVector3</cref> and fires a <cref>UnityEvent<Vector3></cref> when data changes.
	/// </summary>
	[AddComponentMenu("SmartData/Vector3/Read Smart Vector3", 0)]
	public class ReadSmartVector3 : ReadSmartBase<Vector3Reader> {}
}