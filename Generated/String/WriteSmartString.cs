using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.Abstract;

namespace SmartData.SmartString.Components {
	/// <summary>
	/// Serialised write access to a SmartString.
	/// </summary>
	[AddComponentMenu("SmartData/String/Write Smart String", 1)]
	public class WriteSmartString : WriteSmartBase<string, StringWriter> {}
}