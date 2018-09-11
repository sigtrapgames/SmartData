using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.Abstract;

namespace SmartData.SmartBool.Components {
	/// <summary>
	/// Automatically listens to a <cref>SmartBool</cref> and fires a <cref>UnityEvent<bool></cref> when data changes.
	/// </summary>
	[AddComponentMenu("SmartData/Bool/Read Smart Bool", 0)]
	public class ReadSmartBool : ReadSmartBase<BoolReader> {}
}