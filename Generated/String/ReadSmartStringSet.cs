using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmartData.Abstract;

namespace SmartData.SmartString.Components {
	/// <summary>
	/// Automatically listens to a <cref>SmartStringSet</cref> and fires a <cref>UnityEvent<string></cref> when data changes.
	/// </summary>
	[AddComponentMenu("SmartData/String/Read Smart String Set", 2)]
	public class ReadSmartStringSet : ReadSmartBase<StringSetReader> {}
}