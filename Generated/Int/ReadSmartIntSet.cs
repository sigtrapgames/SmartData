using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.Abstract;

namespace SmartData.SmartInt.Components {
	/// <summary>
	/// Automatically listens to a <cref>SmartIntSet</cref> and fires a <cref>UnityEvent<int></cref> when data changes.
	/// </summary>
	[AddComponentMenu("SmartData/Int/Read Smart Int Set", 2)]
	public class ReadSmartIntSet : ReadSmartBase<IntSetReader> {}
}