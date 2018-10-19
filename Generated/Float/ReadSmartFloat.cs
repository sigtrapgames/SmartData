// SMARTTYPE float
// SMARTTEMPLATE ReadSmartVarTemplate
// Do not move or delete the above lines

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.Abstract;

namespace SmartData.SmartFloat.Components {
	/// <summary>
	/// Automatically listens to a <cref>SmartFloat</cref> and fires a <cref>UnityEvent<float></cref> when data changes.
	/// </summary>
	[AddComponentMenu("SmartData/Float/Read Smart Float", 0)]
	public class ReadSmartFloat : ReadSmartBase<FloatReader> {}
}