using SmartData.Editors;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;

namespace SmartData.Graph
{
	[System.Serializable]
	public class SmartGraphConnection
	{
		public enum EventCallType {
			// ADD STUFF
			REF_TO_SMART,
			SMART_TO_SMART
		}
		public EventCallType CallType { get; private set; }
		public Object Sender { get; private set; }
		public Object Receiver { get; private set; }
		public string EventName { get; private set; }
		public string Method { get; private set; }
		public string ReceiverComponentName { get; private set; }
		public bool ReadOnly { get; private set; }
		public bool UseFullMethodPath { get; private set; }

		public string MethodFullPath
		{
			get
			{
				if (UseFullMethodPath)
				{
					return ReceiverComponentName + Method;
				}
				return Method;
			}
		}

		public System.Action<bool> OnTriggered;

		private static Regex parenteshesPattern = new Regex(@"\((.*)\)");


		public SmartGraphConnection(
			Object sender,
			Object receiver,
			SmartData.Abstract.SmartRefBase smartref,
			string eventName,
			string methodName,
			UnityEventBase unityEvent,
			bool readOnly = true,
			bool useFullMethodPath = true)
		{
			Sender = sender as Component ? (sender as Component).gameObject : sender;
			Receiver = receiver as Component ? (receiver as Component).gameObject : receiver;
			EventName = eventName;
			Method = methodName;
			ReadOnly = readOnly;
			UseFullMethodPath = useFullMethodPath;

			if (smartref != null){
				CallType = EventCallType.REF_TO_SMART;
			} else if (Sender is SmartData.Abstract.SmartBase && Receiver is SmartData.Abstract.SmartBase){
				CallType = EventCallType.SMART_TO_SMART;
			} else {
				throw new System.Exception("EventCall must have a SmartRef or SmartObject sender, and a SmartObject receiver.");
			}

			UpdateReceiverComponentName(receiver);
			AttachTrigger(unityEvent);
			switch (CallType){
				case EventCallType.REF_TO_SMART:
					int refCode = smartref.GetHashCode();
					SmartDataRegistry.onRefCallToSmart.AddListener((caller, receiver1, receiver2) =>
					{
						if (caller != null && caller.GetHashCode() == refCode)
						{
							if (OnTriggered != null)
							{
								OnTriggered(true);
							}
						}
					});
					break;
				case EventCallType.SMART_TO_SMART:
					// TODO This isn't really implemented, just putting this here to prevent failure in the meantime
					int smartCode = Receiver.GetHashCode();
					SmartDataRegistry.onRefCallToSmart.AddListener((caller, receiver1, receiver2) =>
					{
						if (receiver1 != null && receiver1.GetHashCode() == smartCode)
						{
							if (OnTriggered != null)
							{
								OnTriggered(true);
							}
						}
					});
					break;
			}
		}

		private bool AttachTrigger(UnityEventBase unityEvent)
		{
			if (unityEvent == null)
			{
				return false;
			}
			MethodInfo eventRegisterMethod = unityEvent.GetType().GetMethod("AddListener");
			if (eventRegisterMethod != null)
			{
				System.Type eventType = eventRegisterMethod.GetParameters()[0].ParameterType;
				ParameterInfo[] eventParameters = eventType.GetMethod("Invoke").GetParameters();

				if (eventParameters.Length == 0)
				{
					MethodInfo methodInfo = this.GetType()
						.GetMethod("TriggerZeroArgs", BindingFlags.Public | BindingFlags.Instance);

					System.Type actionT = typeof(UnityAction);
					System.Delegate triggerAction = System.Delegate.CreateDelegate(actionT, this, methodInfo);

					eventRegisterMethod.Invoke(unityEvent, new object[]
					{
						triggerAction
					});
					return true;
				}
				else if (eventParameters.Length == 1)
				{
					System.Type t0 = eventParameters[0].ParameterType;

					MethodInfo methodInfo = this.GetType()
						.GetMethod("TriggerOneArg", BindingFlags.Public | BindingFlags.Instance)
						.MakeGenericMethod(t0);

					System.Type actionT = typeof(UnityAction<>).MakeGenericType(t0);
					System.Delegate triggerAction = System.Delegate.CreateDelegate(actionT, this, methodInfo);

					eventRegisterMethod.Invoke(unityEvent, new object[]
					{
						triggerAction
					});
					return true;
				}
				else if (eventParameters.Length == 2)
				{
					System.Type t0 = eventParameters[0].ParameterType;
					System.Type t1 = eventParameters[1].ParameterType;

					MethodInfo methodInfo = this.GetType()
						.GetMethod("TriggerTwoArgs", BindingFlags.Public | BindingFlags.Instance)
						.MakeGenericMethod(t0, t1);

					System.Type actionT = typeof(UnityAction<,>).MakeGenericType(t0, t1);
					System.Delegate triggerAction = System.Delegate.CreateDelegate(actionT, this, methodInfo);

					eventRegisterMethod.Invoke(unityEvent, new object[]
					{
						triggerAction
					});
					return true;
				}
				else if (eventParameters.Length == 3)
				{
					System.Type t0 = eventParameters[0].ParameterType;
					System.Type t1 = eventParameters[1].ParameterType;
					System.Type t2 = eventParameters[2].ParameterType;

					MethodInfo methodInfo = this.GetType()
						.GetMethod("TriggerThreeArgs", BindingFlags.Public | BindingFlags.Instance)
						.MakeGenericMethod(t0, t1, t2);

					System.Type actionT = typeof(UnityAction<,,>).MakeGenericType(t0, t1, t2);
					System.Delegate triggerAction = System.Delegate.CreateDelegate(actionT, this, methodInfo);

					eventRegisterMethod.Invoke(unityEvent, new object[]
					{
						triggerAction
					});
					return true;
				}
				else if (eventParameters.Length == 2)
				{
					System.Type t0 = eventParameters[0].ParameterType;
					System.Type t1 = eventParameters[1].ParameterType;
					System.Type t2 = eventParameters[2].ParameterType;
					System.Type t3 = eventParameters[3].ParameterType;

					MethodInfo methodInfo = this.GetType()
						.GetMethod("TriggerFourArgs", BindingFlags.Public | BindingFlags.Instance)
						.MakeGenericMethod(t0, t1, t2, t3);

					System.Type actionT = typeof(UnityAction<,,,>).MakeGenericType(t0, t1, t2, t3);
					System.Delegate triggerAction = System.Delegate.CreateDelegate(actionT, this, methodInfo);

					eventRegisterMethod.Invoke(unityEvent, new object[]
					{
						triggerAction
					});
					return true;
				}
			}
			return false;
		}

		#region generic callers
		public void TriggerZeroArgs()
		{
			if (OnTriggered != null)
			{
				OnTriggered(true);
			}
		}

		public void TriggerOneArg<T0>(T0 arg0)
		{
			if (OnTriggered != null)
			{
				OnTriggered(true);
			}
		}
		public void TriggerTwoArgs<T0, T1>(T0 arg0, T1 arg1)
		{
			if (OnTriggered != null)
			{
				OnTriggered(true);
			}
		}
		public void TriggerThreeArgs<T0, T1, T2>(T0 arg, T1 arg1, T2 arg2)
		{
			if (OnTriggered != null)
			{
				OnTriggered(true);
			}
		}
		public void TriggerFourArgs<T0, T1, T2, T3>(T0 arg, T1 arg1, T2 arg2, T3 arg3)
		{
			if (OnTriggered != null)
			{
				OnTriggered(true);
			}
		}
		#endregion

		private void UpdateReceiverComponentName(Object component)
		{
			if (Receiver != null && component != null)
			{
				MatchCollection matches = parenteshesPattern.Matches(component.ToString());
				if (matches != null && matches.Count > 0)
				{
					ReceiverComponentName = matches[matches.Count - 1].Value;
					if (ReceiverComponentName.Length > 1)
					{
						ReceiverComponentName = ReceiverComponentName.Remove(0, 1);
					}
					ReceiverComponentName = ReceiverComponentName.Replace(")", ".");
				}
			}
		}
	}
}