using System;
using System.Collections.Generic;
using UnityEngine;
using SmartData.Abstract;
using Sigtrap.Relays;

namespace SmartData.Editors {
	public class SmartDataRegistry : MonoBehaviour {
		static Dictionary<WeakReference, Type> _refs = new Dictionary<WeakReference, Type>();
		static Dictionary<SmartBase, Type> _smart = new Dictionary<SmartBase, Type>();

		public static Dictionary<WeakReference, Type> GetSmartReferences(){
			return new Dictionary<WeakReference, Type>(_refs);
		}
		public static Dictionary<SmartBase, Type> GetSmartDatas(){
			return new Dictionary<SmartBase, Type>(_smart);
		}
		public static void RegisterReference(SmartRefBase r){
			_refs.Add(new WeakReference(r), r.GetType());
		}
		public static void UnregisterReference(WeakReference r){
			_refs.Remove(r);
		}
		public static void RegisterSmart(SmartBase s){
			_smart[s] = s.GetType();
		}
		public static void UnregisterSmart(SmartBase s){
			_smart.Remove(s);
		}

		#if UNITY_EDITOR && !SMARTDATA_NO_GRAPH_HOOKS
		/// <summary>
		/// If false, SmartRef calls will not be intercepted.
		/// </summary>
		public static bool graphIsOpen = false;
		/// <summary>
		/// Dispatched when a SmartRef->SmartObject call is intercepted.
		/// First arg is caller. Second is direct callee - this may be a Multi. 
		/// Third is final callee. If direct is Multi, this is the Var it passes through to.
		/// </summary>
		public static IRelayLink<SmartRefBase, SmartBindableBase, SmartBindableBase> onRefCallToSmart {get {return _onRefCallToSmart.link;}}
		static Relay<SmartRefBase, SmartBindableBase, SmartBindableBase> _onRefCallToSmart = new Relay<SmartRefBase, SmartBindableBase, SmartBindableBase>();
		/// <summary>
		/// Intercepts SmartRef calls to SmartObjects to animate graph.
		/// </summary>
		/// <param name="r">Writer ref caller.</param>
		/// <param name="smart">Direct callee. May be a Multi.</param>
		/// <param name="leaf">Final callee. If Multi, this will be the Var element.</param>
		public static void OnRefCallToSmart(SmartRefBase r, SmartBindableBase smart, SmartBindableBase leaf=null){
			if (graphIsOpen){
				_onRefCallToSmart.Dispatch(r, smart, leaf);
			}
		}
		#endif
	}
}