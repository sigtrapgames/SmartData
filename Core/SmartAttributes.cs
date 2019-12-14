using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace SmartData {
	/// <summary>
	/// Use on a SmartRef field to force use of a SmartData or SmartMulti.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class ForceEventableAttribute : Attribute {
		/// <summary>
		/// Force the UnityEvent attached to this SmartRef to always be visible in the editor.
		/// </summary>
		public bool forceExpand {get;set;}
		/// <summary>
		/// Allow LOCAL values, i.e. non-SmartObject. LOCAL mode still fires local Relay/UnityEvent. True by default.
		/// </summary>
		public bool allowLocal {get;set;}
		public ForceEventableAttribute(bool forceExpand=false, bool allowLocal=true){
			this.forceExpand = forceExpand;
			this.allowLocal = allowLocal;
		}
	}

	/// <summary>
	/// Use on a SmartRef field to hide the UnityEvent in the inspector.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class ForceHideEventAttribute : Attribute {}

	/// <summary>
	/// Use on a SmartRef field to disable the in-editor Auto Listen option.
	/// <para />Can still call UnbindOnDestroy from code.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class ForceNoAutoListenAttribute : Attribute {
		/// <summary>
		/// If true, hides checkbox and "Disabled from code" tooltip.
		/// <para />If false (default) checkbox is shown but greyed out.
		/// </summary>
		public bool hide {get; set;}
		public ForceNoAutoListenAttribute(bool hide=false){
			this.hide = hide;
		}
	}

	/// <summary>
	/// Use on a SmartDecoratorBase to show a description in editor.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class DecoratorDescriptionAttribute : Attribute {
		public string description {get; private set;}
		public DecoratorDescriptionAttribute(string description){
			this.description = description;
		}
	}

	/// <summary>
	/// Use on a SmartMultiRef to disable the in-editor index field.
	/// <para />Requires index selection from code.
	/// <para />Disables Auto Bind in editor.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class ForceNoMultiIndexAttribute : Attribute {}
}