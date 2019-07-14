using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using NUnit.Framework;
using SE = SmartData.SmartEvent;
using SED = SmartData.SmartEvent.Data;
using SEC = SmartData.SmartEvent.Components;
using SF = SmartData.SmartFloat;
using SFD = SmartData.SmartFloat.Data;
using SFC = SmartData.SmartFloat.Components;
using SFA = SmartData.Abstract;
using System.Reflection;

namespace SmartData.Editors.Testing {
	public class SmartDataTests {
		static float _f;
		static Sigtrap.Relays.IRelayBinding _b;
		static int _raised;

		[TearDown]
		public void Teardown(){
			_f = 0;
			if (_b != null){
				_b.Enable(false);
			}
			_b = null;
			_raised = 0;
		}

		#region SmartObjects
		[Test]
		public void TestEvent(){
			var s = CreateEvent();
			_b = s.BindListener(OnEvent);
			s.Dispatch();
			Assert.That(_raised == 1);
			Destroy(s);
		}
		[Test]
		public void TestEventMulti(){
			var s = CreateEventMulti();

			_b = s.BindListener(OnEvent, 1);
			Assert.That(s.count == 2);
			s[1].Dispatch();
			Assert.That(_raised == 1);

			Destroy(s);
		}
		[Test]
		public void TestFloat(){
			var s = CreateFloat();

			_b = s.BindListener(OnUpdated);
			s.value = 100f;
			Assert.That(s.value == 100f);
			Assert.That(_f == 100f);

			Destroy(s);
		}
		[Test]
		public void TestFloatMulti(){
			var s = CreateFloatMulti();

			_b = s.BindListener(OnUpdated, 1);
			Assert.That(s.count == 2);
			s[1].value = 100f;
			Assert.That(s[1].value == 100f);
			Assert.That(_f == 100f);

			Destroy(s);
		}
		[Test]
		public void TestFloatSet(){
			var s = CreateFloatSet();

			_b = s.BindListener(OnSetChanged);
			s.Add(100f);
			Assert.That(_f == 100f);
			Assert.That(s.count == 1);
			s.Remove(100f);
			Assert.That(_f == -100f);
			Assert.That(s.count == 0);

			s.Add(10f);
			s.Add(50f);
			Assert.That(_f == 50f);
			Assert.That(s.count == 2);
			s.RemoveAt(0);
			Assert.That(s.count == 1);
			Assert.That(s[0] == 50f);
			Assert.That(_f == -10f);

			Destroy(s);
		}
		#endregion

		#region Components
		#region Event/Multi
		[Test]
		public void TestListenSmartEvent () {
			var s = CreateEvent();
			var l = CreateListenSmartEvent(s);
			
			s.Dispatch();
			Assert.That(_raised == 1);

			Destroy(s, l);
		}
		[Test]
		public void TestDispatchSmartEvent(){
			var s = CreateEvent();
			var w = CreateDispatchSmartEvent(s);
			_b = s.BindListener(OnEvent);

			w.Dispatch();
			Assert.That(_raised == 1);

			Destroy(s, w);
		}
		[Test]
		public void TestListenAndDispatchEvent(){
			var s = CreateEvent();
			var l = CreateListenSmartEvent(s);
			var w = CreateDispatchSmartEvent(s);

			w.Dispatch();
			Assert.That(_raised == 1);

			Destroy(s, l, w);
		}
		#endregion

		#region Var/Multi
		[Test]
		public void TestReadSmartFloat () {
			var s = CreateFloat();
			var l = CreateReadSmartFloat(s);
			
			s.value = 100f;
			Assert.That(s.value == 100f);
			Assert.That(_f == 100f);

			Destroy(s, l);
		}
		[Test]
		public void TestWriteFloat(){
			var s = CreateFloat();
			var w = CreateWriteFloat(s);
			_b = s.BindListener(OnUpdated);

			w.value = 100f;
			Assert.That(w.value == 100f);
			Assert.That(_f == 100f);

			Destroy(s, w);
		}
		[Test]
		public void TestReadAndWriteFloat(){
			var s = CreateFloat();
			var l = CreateReadSmartFloat(s);
			var w = CreateWriteFloat(s);

			w.value = 100f;
			Assert.That(s.value == 100f);
			Assert.That(w.value == 100f);
			Assert.That(_f == 100f);

			Destroy(s, l, w);
		}
		#endregion

		#region Set
		[Test]
		public void TestReadSmartFloatSet () {
			var s = CreateFloatSet();
			var l = CreateReadSmartFloatSet(s);
			
			s.Add(100f);
			Assert.That(s.count == 1);
			Assert.That(_f == 100f);
			s.Remove(100f);
			Assert.That(s.count == 0);
			Assert.That(_f == -100f);
			s.Add(50f);
			s.RemoveAt(0);
			Assert.That(_f == -50f);

			Destroy(s, l);
		}
		[Test]
		public void TestWriteFloatSet(){
			var s = CreateFloatSet();
			var w = CreateWriteFloatSet(s);
			_b = s.BindListener(OnSetChanged);

			w.Add(100f);
			Assert.That(s.count == 1);
			Assert.That(s[0] == 100f);
			Assert.That(_f == 100f);
			w.Remove(100f);
			Assert.That(s.count == 0);
			Assert.That(_f == -100f);
			w.Add(50f);
			w.RemoveAt(0);
			Assert.That(s.count == 0);
			Assert.That(_f == -50f);

			Destroy(s, w);
		}
		[Test]
		public void TestReadAndWriteFloatSet(){
			var s = CreateFloatSet();
			var l = CreateReadSmartFloatSet(s);
			var w = CreateWriteFloatSet(s);

			w.Add(100f);
			Assert.That(s.count == 1);
			Assert.That(s[0] == 100f);
			Assert.That(_f == 100f);
			w.Remove(100f);
			Assert.That(s.count == 0);
			Assert.That(_f == -100f);
			w.Add(50f);
			w.RemoveAt(0);
			Assert.That(s.count == 0);
			Assert.That(_f == -50f);

			Destroy(s, l, w);
		}
		#endregion
		#endregion

		#region Refs
		#region Event Var
		[Test]
		public void TestEventListenerVar(){
			var s = CreateEvent();
			var l = CreateListener(s);
			_b = l.BindListener(OnEvent);

			Assert.That(l.name == s.name);
			s.Dispatch();
			Assert.That(_raised == 1);

			Destroy(s);
		}
		[Test]
		public void TestEventListenerBindingVar(){
			var s = CreateEvent();
			var l = CreateListener(s);
			_b = l.BindListener(OnEvent);

			TestListenerBinding(s.Dispatch);

			Destroy(s);
		}
		[Test]
		public void TestEventDispatcherVar(){
			var s = CreateEvent();
			var r = CreateDispatcher(s);
			_b = r.BindListener(OnEvent);

			Assert.That(r.name == s.name);

			r.Dispatch();
			Assert.That(_raised == 1);

			Destroy(s);
		}
		#endregion

		#region Event Multi
		[Test]
		public void TestEventListenerMulti(){
			var s = CreateEventMulti();
			var l = CreateListener(s,1);
			_b = l.BindListener(OnEvent);

			Assert.That(s.count == 2);
			s[1].Dispatch();
			Assert.That(_raised == 1);

			Destroy(s);
		}
		[Test]
		public void TestEventListenerBindingMulti(){
			var s = CreateEventMulti();
			var l = CreateListener(s,1);
			_b = s[1].BindListener(OnEvent);

			Assert.That(l.name == s.name);
			Assert.That(s.count == 2);
			TestListenerBinding(s[1].Dispatch);

			Destroy(s);
		}
		/// <summary>Tests an EventDispatcher referencing an EventMulti</summary>
		[Test]
		public void TestEventDispatcherMulti(){
			var s = CreateEventMulti();
			var r = CreateDispatcher(s,1);
			_b = r.BindListener(OnEvent);

			Assert.That(s.count == 2);
			Assert.That(r.name == s.name);

			r.Dispatch();
			Assert.That(_raised == 1);

			Destroy(s);
		}
		/// <summary>Tests an EventMultiListener</summary>
		[Test]
		public void TestEventMultiListener(){
			var s = CreateEventMulti();
			var r = CreateMultiListener(s,1);
			Assert.That(r.name == s.name);

			_b = r.BindListener(OnEvent);
			// Check has instantiated multi elements
			Assert.That(s.count == 2);

			s[1].Dispatch();
			Assert.That(_raised == 1);
			// Check changing ref index and rebinding adds another event
			r.index = 2;
			_b.Enable(false);
			_b = r.BindListener(OnEvent);
			Assert.That(s.count == 3);
			// Check binding has been updated - s[1] dispatch shouldn't affect us but s[2] should
			s[1].Dispatch();
			Assert.That(_raised == 1);
			s[2].Dispatch();
			Assert.That(_raised == 2);

			Destroy(s);
		}
		/// <summary>Tests an EventMultiDispatcher</summary>
		[Test]
		public void TestEventMultiDispatcher(){
			var s = CreateEventMulti();
			var r = CreateMultiDispatcher(s,1);
			Assert.That(r.name == s.name);

			_b = r.BindListener(OnEvent);
			// Check has instantiated multi elements
			Assert.That(s.count == 2);

			// Check ref dispatches element 1
			r.Dispatch();
			Assert.That(_raised == 1);
			// Check changing ref index and rebinding adds another event
			r.index = 2;
			_b.Enable(false);
			_b = r.BindListener(OnEvent);
			Assert.That(s.count == 3);
			// Check binding has been updated - s[1] dispatch shouldn't affect us but s[2] should
			s[1].Dispatch();
			Assert.That(_raised == 1);
			s[2].Dispatch();
			Assert.That(_raised == 2);
			// Check ref dispatches element 2
			r.Dispatch();
			Assert.That(_raised == 3);
			// Check smart event is indeed being dispatched
			_b.Enable(false);
			_b = s[2].BindListener(OnEvent);
			r.Dispatch();
			Assert.That(_raised == 4);

			Destroy(s);
		}
		#endregion

		#region Var
		[Test]
		public void TestFloatReaderVar(){
			var s = CreateFloat();
			var r = CreateReader(s, SFA.SmartDataRefBase.RefType.VAR);

			s.value = 100f;
			Assert.That(r.value == 100f);
			Assert.That(r.name == s.name);

			Destroy(s);
		}
		[Test]
		public void TestFloatReaderBindingVar(){
			var s = CreateFloat();
			var r = CreateReader(s, SFA.SmartDataRefBase.RefType.VAR);
			_b = r.BindListener(OnUpdated);

			TestReaderBinding((f)=>{s.value = f;});

			Destroy(s);
		}
		[Test]
		public void TestFloatWriterVar(){
			var s = CreateFloat();
			var r = CreateWriter(s, SFA.SmartDataRefBase.RefType.VAR);
			_b = r.BindListener(OnUpdated);

			Assert.That(r.name == s.name);

			r.value = 100f;
			Assert.That(s.value == 100f);
			Assert.That(_f == 100f);

			Destroy(s);
		}
		#endregion

		#region Multi
		[Test]
		public void TestFloatReaderMulti(){
			var s = CreateFloatMulti();
			var r = CreateReader(s, SFA.SmartDataRefBase.RefType.MULTI, 1);

			Assert.That(r.name == s.name);

			s[1].value = 100f;
			Assert.That(s.count == 2);
			Assert.That(r.value == 100f);

			for (int i=0; i<s.count; ++i){
				Destroy(s[i]);
			}
			Destroy(s);
		}
		[Test]
		public void TestFloatReaderBindingMulti(){
			var s = CreateFloatMulti();
			var r = CreateReader(s, SFA.SmartDataRefBase.RefType.MULTI, 1);
			_b = r.BindListener(OnUpdated);

			Assert.That(s.count == 2);
			TestReaderBinding((f)=>{s[1].value = f;});

			Destroy(s);
		}
		[Test]
		public void TestFloatWriterMulti(){
			var s = CreateFloatMulti();
			var r = CreateWriter(s, SFA.SmartDataRefBase.RefType.MULTI, 1);
			_b = r.BindListener(OnUpdated);

			Assert.That(r.name == s.name);

			r.value = 100f;
			Assert.That(s.count == 2);
			Assert.That(s[1].value == 100f);
			Assert.That(_f == 100f);

			Destroy(s);
		}
		/// <summary>Tests a FloatMultiReader</summary>
		[Test]
		public void TestFloatMultiReader(){
			var s = CreateFloatMulti();
			var r = CreateMultiReader(s,1);
			Assert.That(r.name == s.name);

			_b = r.BindListener(OnUpdated);
			// Check has instantiated multi elements
			Assert.That(s.count == 2);

			s[1].value = 100f;
			Assert.That(_f == 100f);
			// Check changing ref index and rebinding adds another event
			r.index = 2;
			_b.Enable(false);
			_b = r.BindListener(OnUpdated);
			Assert.That(s.count == 3);
			// Check binding has been updated - s[1] dispatch shouldn't affect us but s[2] should
			s[1].value = 50f;
			Assert.That(_f == 100f);
			s[2].value = 250f;
			Assert.That(_f == 250f);

			Destroy(s);
		}
		/// <summary>Tests a FloatMultiWriter</summary>
		[Test]
		public void TestFloatMultiWriter(){
			var s = CreateFloatMulti();
			var r = CreateMultiWriter(s,1);
			Assert.That(r.name == s.name);

			_b = r.BindListener(OnUpdated);
			// Check has instantiated multi elements
			Assert.That(s.count == 2);

			// Check ref dispatches element 1
			r.value = 100f;
			Assert.That(_f == 100f);
			// Check changing ref index and rebinding adds another event
			r.index = 2;
			_b.Enable(false);
			_b = r.BindListener(OnUpdated);
			Assert.That(s.count == 3);
			// Check binding has been updated - s[1] dispatch shouldn't affect us but s[2] should
			s[1].value = 50f;
			Assert.That(_f == 100f);
			s[2].value = 250f;
			Assert.That(_f == 250f);
			// Check ref dispatches element 2
			r.value = 300f;
			Assert.That(_f == 300f);
			// Check smart float is indeed being dispatched
			_b.Enable(false);
			_b = s[2].BindListener(OnUpdated);
			r.value = 400f;
			Assert.That(_f == 400f);

			Destroy(s);
		}
		#endregion

		#region Set
		[Test]
		public void TestFloatSetReader(){
			var s = CreateFloatSet();
			var r = CreateSetRef(s);

			Assert.That(r.name == s.name);

			s.Add(100f);
			Assert.That(r.count == 1);
			Assert.That(r[0] == 100f);

			Destroy(s);
		}
		[Test]
		public void TestFloatSetReaderBinding(){
			var s = CreateFloatSet();
			var r = CreateSetRef(s);
			_b = r.BindListener(OnSetChanged);

			TestReaderBinding((f)=>{s.Add(f);});

			Destroy(s);
		}
		[Test]
		public void TestFloatSetWriter(){
			var s = CreateFloatSet();
			var r = CreateSetRefWriter(s);
			_b = r.BindListener(OnSetChanged);

			Assert.That(r.name == s.name);

			r.Add(100f);
			Assert.That(s.count == 1);
			Assert.That(s[0] == 100f);
			Assert.That(_f == 100f);
			r[0] = 5f;
			Assert.That(s.count == 1);
			Assert.That(s[0] == 5f);
			Assert.That(_f == 10f);
			r.Remove(5);
			Assert.That(s.count == 0);
			Assert.That(_f == -5);
			r.Add(100f);
			r.Add(200f);
			r.RemoveAt(0);
			Assert.That(s.count == 1);
			Assert.That(s[0] == 200f);
			Assert.That(_f == -100f);

			Destroy(s);
		}
		#endregion
		#endregion

		void TestReaderBinding(System.Action<float> setter){
			setter(100f);
			Assert.That(_f == 100f);
			_b.Enable(false);
			setter(1f);
			Assert.That(_f == 100f);
			_b.Enable(true);
			setter(10f);
			Assert.That(_f == 10f);
		}
		void TestListenerBinding(System.Action raiser){
			raiser();
			Assert.That(_raised == 1);
			_b.Enable(false);
			raiser();
			Assert.That(_raised == 1);
			_b.Enable(true);
			raiser();
			Assert.That(_raised == 2);
		}

		#region Creation
		#region Events
		SED.EventVar CreateEvent(){
			var s = ScriptableObject.CreateInstance<SED.EventVar>();
			s.name = "TestEvent";
			return s;
		}
		SED.EventMulti CreateEventMulti(){
			var s = ScriptableObject.CreateInstance<SED.EventMulti>();
			s.name = "TestEventMulti";
			return s;
		}
		SE.EventListener CreateListener(SFA.SmartBase smart, int multiIndex=-1){
			var l = new SE.EventListener();
			SetupEventRef(l, smart, multiIndex);
			return l;
		}
		SE.EventDispatcher CreateDispatcher(SFA.SmartBase smart, int multiIndex=-1){
			var l = new SE.EventDispatcher();
			SetupEventRef(l, smart, multiIndex);
			return l;
		}
		SE.EventMultiListener CreateMultiListener(SFA.SmartBase smart, int multiIndex){
			var r = new SE.EventMultiListener();
			SetupEventRef(r, smart, multiIndex);
			return r;
		}
		SE.EventMultiDispatcher CreateMultiDispatcher(SFA.SmartBase smart, int multiIndex){
			var r = new SE.EventMultiDispatcher();
			SetupEventRef(r, smart, multiIndex);
			return r;
		}

		void SetupEventRef(object r, SFA.SmartBase smart, int multiIndex=-1){
			var b = BindingFlags.Instance | BindingFlags.NonPublic;
			
			try {
				// Will throw for EventMultiReader/Writer but doesn't matter
				r.GetType().GetFieldPrivate("_useMulti", b).SetValue(r, multiIndex>=0);
			} catch {}

			if (multiIndex >= 0){
				r.GetType().GetFieldPrivate("_smartMulti", b).SetValue(r, smart);
				r.GetType().GetField("_multiIndex", b).SetValue(r, multiIndex);
			} else {
				r.GetType().GetField("_smartEvent", b).SetValue(r, smart);
			}

			r.GetType().GetFieldPrivate("_onEvent", b).SetValue(r, new UnityEvent());

			if (r is ISerializationCallbackReceiver){
				((ISerializationCallbackReceiver)r).OnAfterDeserialize();
			}
			(r as SmartData.Abstract.SmartRefBase).unityEventOnReceive = true;
		}

		SEC.ListenSmartEvent CreateListenSmartEvent(SED.EventVar s){
			var go = new GameObject("LISTENER");
			var l = go.AddComponent<SEC.ListenSmartEvent>();

			var sol = new SerializedObject(l);
			var arr = sol.FindProperty("_data");
			arr.arraySize++;
			var el = arr.GetArrayElementAtIndex(0);
			el.FindPropertyRelative("_smartEvent").objectReferenceValue = s;
			sol.ApplyModifiedProperties();
			sol.Update();
			var evt = ((UnityEvent)el.FindPropertyRelative("_onEvent").GetObject());
			evt.AddListener(OnEvent);
			var fr = (SmartEvent.EventListener)el.GetObject();
			fr.GetType().GetField("_useMulti", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(fr, false);
			fr.unityEventOnReceive = true;

			return l;
		}
		SEC.DispatchSmartEvent CreateDispatchSmartEvent(SED.EventVar s){
			var go = new GameObject("DISPATCHER");
			var w = go.AddComponent<SEC.DispatchSmartEvent>();

			var sow = new SerializedObject(w);
			sow.FindProperty("_event").FindPropertyRelative("_smartEvent").objectReferenceValue = s;
			sow.ApplyModifiedProperties();
			sow.Update();

			return w;
		}
		#endregion

		#region Floats
		SF.FloatReader CreateReader(SFA.SmartBase smart, SFA.SmartDataRefBase.RefType type, int multiIndex=-1){
			var r = new SF.FloatReader();
			SetupFloatRef(r, smart, type, multiIndex);
			return r;
		}
		SF.FloatWriter CreateWriter(SFA.SmartBase smart, SFA.SmartDataRefBase.RefType type, int multiIndex=-1){
			var r = new SF.FloatWriter();
			SetupFloatRef(r, smart, type, multiIndex);
			return r;
		}
		SF.FloatMultiReader CreateMultiReader(SFA.SmartBase smart, int multiIndex){
			var r = new SF.FloatMultiReader();
			SetupFloatRef(r, smart, SFA.SmartDataRefBase.RefType.MULTI, multiIndex);
			return r;
		}
		SF.FloatMultiWriter CreateMultiWriter(SFA.SmartBase smart, int multiIndex){
			var r = new SF.FloatMultiWriter();
			SetupFloatRef(r, smart, SFA.SmartDataRefBase.RefType.MULTI, multiIndex);
			return r;
		}
		SF.FloatSetReader CreateSetRef(SFD.FloatSet smart){
			var r = new SF.FloatSetReader();
			SetupSetRef(r, smart);
			return r;
		}
		SF.FloatSetWriter CreateSetRefWriter(SFD.FloatSet smart){
			var r = new SF.FloatSetWriter();
			SetupSetRef(r, smart);
			return r;
		}

		void SetupFloatRef(object r, SFA.SmartBase smart, SFA.SmartDataRefBase.RefType type, int multiIndex){
			Debug.Log("BLARGH");
			var b = BindingFlags.Instance | BindingFlags.NonPublic;
			r.GetType().GetFieldPrivate(SmartDataRefPropertyDrawer.refPropNames[type], b).SetValue(r, smart);
			try {
				// Will throw for FloatMulti but doesn't matter
				r.GetType().GetFieldPrivate("_refType", b).SetValue(r, type);
			} catch {}
			if (multiIndex >= 0){
				r.GetType().GetField("_multiIndex", b).SetValue(r, multiIndex);
			}

			r.GetType().GetFieldPrivate("_onUpdate", b).SetValue(r, new SmartFloat.Data.FloatVar.FloatEvent());

			if (r is ISerializationCallbackReceiver){
				((ISerializationCallbackReceiver)r).OnAfterDeserialize();
			}
			(r as SmartData.Abstract.SmartRefBase).unityEventOnReceive = true;
		}
		void SetupSetRef(object r, SFD.FloatSet smart){
			var b = BindingFlags.Instance | BindingFlags.NonPublic;
			r.GetType().GetFieldPrivate("_smartSet", b).SetValue(r, smart);
			r.GetType().GetFieldPrivate("_onAdd", b).SetValue(r, new SmartFloat.Data.FloatVar.FloatEvent());
			r.GetType().GetFieldPrivate("_onRemove", b).SetValue(r, new SmartFloat.Data.FloatVar.FloatEvent());
			r.GetType().GetFieldPrivate("_onChange", b).SetValue(r, new SmartFloat.Data.FloatVar.FloatEvent());
			if (r is ISerializationCallbackReceiver){
				((ISerializationCallbackReceiver)r).OnAfterDeserialize();
			}
			(r as SmartData.Abstract.SmartRefBase).unityEventOnReceive = true;
		}
		SFD.FloatVar CreateFloat(){
			var s = ScriptableObject.CreateInstance<SFD.FloatVar>();
			s.name = "TestSmartFloat";
			return s;
		}
		SFD.FloatMulti CreateFloatMulti(){
			var s = ScriptableObject.CreateInstance<SFD.FloatMulti>();
			s.name = "TestSmartFloatMulti";
			return s;
		}
		SFD.FloatSet CreateFloatSet(){
			var s = ScriptableObject.CreateInstance<SFD.FloatSet>();
			s.name = "TestSmartFloatSet";
			return s;
		}
		
		SFC.ReadSmartFloat CreateReadSmartFloat(SFD.FloatVar s){
			var go = new GameObject("READER");
			var l = go.AddComponent<SFC.ReadSmartFloat>();

			var sol = new SerializedObject(l);
			var arr = sol.FindProperty("_data");
			arr.arraySize++;
			var el = arr.GetArrayElementAtIndex(0);
			el.FindPropertyRelative("_smartVar").objectReferenceValue = s;
			sol.ApplyModifiedProperties();
			sol.Update();
			var evt = ((UnityEvent<float>)el.FindPropertyRelative("_onUpdate").GetObject());
			evt.AddListener(OnUpdated);
			var fr = (SmartFloat.FloatReader)el.GetObject();
			fr.GetType().GetField("_refType", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(fr, SmartData.Abstract.SmartDataRefBase.RefType.VAR);
			fr.unityEventOnReceive = true;

			return l;
		}
		SFC.ReadSmartFloatSet CreateReadSmartFloatSet(SFD.FloatSet s){
			var go = new GameObject("READER");
			var l = go.AddComponent<SFC.ReadSmartFloatSet>();

			var sol = new SerializedObject(l);
			var arr = sol.FindProperty("_data");
			arr.arraySize++;
			var el = arr.GetArrayElementAtIndex(0);
			el.FindPropertyRelative("_smartSet").objectReferenceValue = s;
			sol.ApplyModifiedProperties();
			sol.Update();

			var evt = ((UnityEvent<float>)el.FindPropertyRelative("_onAdd").GetObject());
			evt.AddListener(OnAdded);
			evt = ((UnityEvent<float>)el.FindPropertyRelative("_onRemove").GetObject());
			evt.AddListener(OnRemoved);

			((SmartFloat.FloatSetReader)el.GetObject()).unityEventOnReceive = true;

			return l;
		}
		SFC.WriteSmartFloat CreateWriteFloat(SFD.FloatVar s){
			var go = new GameObject("WRITER");
			var w = go.AddComponent<SFC.WriteSmartFloat>();

			var sow = new SerializedObject(w);
			sow.FindProperty("_data").FindPropertyRelative("_smartVar").objectReferenceValue = s;
			sow.ApplyModifiedProperties();
			sow.Update();

			return w;
		}
		SFC.WriteSmartFloatSet CreateWriteFloatSet(SFD.FloatSet s){
			var go = new GameObject("WRITER");
			var w = go.AddComponent<SFC.WriteSmartFloatSet>();

			var sow = new SerializedObject(w);
			sow.FindProperty("_data").FindPropertyRelative("_smartSet").objectReferenceValue = s;
			sow.ApplyModifiedProperties();
			sow.Update();

			return w;
		}
		#endregion
		#endregion

		#region Callbacks
		void OnUpdated(float f){
			_f = f;
		}
		void OnSetChanged(SetEventData<float> d){
			switch (d.operation){
				case SetOperation.ADDED:
					OnAdded(d.value);
					break;
				case SetOperation.REMOVED:
					OnRemoved(d.value);
					break;
				case SetOperation.CHANGED:
					OnChanged(d.value);
					break;
			}
		}
		void OnAdded(float f){
			_f = f;
		}
		void OnRemoved(float f){
			_f = -f;
		}
		void OnChanged(float f){
			_f = 2*f;
		}
		void OnEvent(){
			++_raised;
		}
		#endregion

		void Destroy(params Object[] objs){
			foreach (var o in objs){
				if (o is Component){
					Object.DestroyImmediate((o as Component).gameObject);
				} else {
					Object.DestroyImmediate(o);
				}
			}
		}
	}
}
