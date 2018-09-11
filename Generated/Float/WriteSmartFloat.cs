using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.Abstract;

namespace SmartData.SmartFloat.Components {
	/// <summary>
	/// Serialised write access to a SmartFloat.
	/// </summary>
	[AddComponentMenu("SmartData/Float/Write Smart Float", 1)]
	public class WriteSmartFloat : WriteSmartBase<float, FloatWriter> {}
}