using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sigtrap.Relays;
using SmartData.Abstract;
using SmartData.SmartEvent;
using SmartData.Interfaces;

namespace SmartData.SmartEvent.Components {
	[AddComponentMenu("SmartData/Listen Smart Event", 100)]
	public class ListenSmartEvent : ReadSmartBase<SmartEvent.EventListener> {}
}