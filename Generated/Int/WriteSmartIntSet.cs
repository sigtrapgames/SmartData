using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.Abstract;

namespace SmartData.SmartInt.Components {
	/// <summary>
	/// Serialised write access to a SmartIntSet.
	/// </summary>
	[AddComponentMenu("SmartData/Int/Write Smart Int Set", 3)]
	public class WriteSmartIntSet : WriteSetBase<int, IntSetWriter> {}
}