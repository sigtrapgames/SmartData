using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.Abstract;

namespace SmartData.SmartInt.Components {
	/// <summary>
	/// Serialised write access to a SmartInt.
	/// </summary>
	[AddComponentMenu("SmartData/Int/Write Smart Int", 1)]
	public class WriteSmartInt : WriteSmartBase<int, IntWriter> {}
}