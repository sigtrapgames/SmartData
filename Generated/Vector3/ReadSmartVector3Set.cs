// SMARTTYPE Vector3
// SMARTTEMPLATE ReadSmartSetTemplate
// Do not move or delete the above lines

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.Abstract;

namespace SmartData.SmartVector3.Components {
	/// <summary>
	/// Automatically listens to a <cref>SmartVector3Set</cref> and fires a <cref>UnityEvent<Vector3></cref> when data changes.
	/// </summary>
	[AddComponentMenu("SmartData/Vector3/Read Smart Vector3 Set", 2)]
	public class ReadSmartVector3Set : ReadSmartBase<Vector3SetReader> {}
}