using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.Abstract;

namespace SmartData.SmartString.Components {
	/// <summary>
	/// Automatically listens to a <cref>SmartString</cref> and fires a <cref>UnityEvent<string></cref> when data changes.
	/// </summary>
	[AddComponentMenu("SmartData/String/Read Smart String", 0)]
	public class ReadSmartString : ReadSmartBase<StringReader> {}
}