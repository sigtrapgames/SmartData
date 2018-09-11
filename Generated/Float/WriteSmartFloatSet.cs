using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.Abstract;

namespace SmartData.SmartFloat.Components {
	/// <summary>
	/// Serialised write access to a SmartFloatSet.
	/// </summary>
	[AddComponentMenu("SmartData/Float/Write Smart Float Set", 3)]
	public class WriteSmartFloatSet : WriteSetBase<float, FloatSetWriter> {}
}