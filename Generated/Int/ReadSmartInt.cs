using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.Abstract;

namespace SmartData.SmartInt.Components {
	/// <summary>
	/// Automatically listens to a <cref>SmartInt</cref> and fires a <cref>UnityEvent<int></cref> when data changes.
	/// </summary>
	[AddComponentMenu("SmartData/Int/Read Smart Int", 0)]
	public class ReadSmartInt : ReadSmartBase<IntReader> {}
}