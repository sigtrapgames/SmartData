using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.Abstract;

namespace SmartData.SmartVector3.Components {
	/// <summary>
	/// Serialised write access to a SmartVector3.
	/// </summary>
	[AddComponentMenu("SmartData/Vector3/Write Smart Vector3", 1)]
	public class WriteSmartVector3 : WriteSmartBase<Vector3, Vector3Writer> {}
}