// SMARTTYPE float
// SMARTTEMPLATE ReadSmartSetTemplate
// Do not move or delete the above lines

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.Abstract;

namespace SmartData.SmartFloat.Components {
	/// <summary>
	/// Automatically listens to a <cref>SmartFloatSet</cref> and fires a <cref>UnityEvent<float></cref> when data changes.
	/// </summary>
	[AddComponentMenu("SmartData/Float/Read Smart Float Set", 2)]
	public class ReadSmartFloatSet : ReadSmartBase<FloatSetReader> {}
}