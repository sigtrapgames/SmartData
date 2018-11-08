using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using System.Reflection;
using UnityEngine.Events;
using SmartData.Abstract;

namespace SmartData.Graph
{

	public static class SmartGraphPopulator
	{
		static List<System.WeakReference> _refsToRemove = new List<System.WeakReference>();
		public static List<SmartGraphConnection> FindAllEvents()
		{
			List<SmartGraphConnection> calls = new List<SmartGraphConnection>();

			#region SmartRefs
			_refsToRemove.Clear();
			var refs = SmartData.Editors.SmartDataRegistry.GetSmartReferences();
			foreach (var r in refs){
				if (!r.Key.IsAlive){
					// Mark ref for removal
					_refsToRemove.Add(r.Key);
					continue;
				}

				BindingFlags binding = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
				SmartRefBase target = (SmartRefBase)r.Key.Target;
				bool useMultiIndex = false;
				bool writeable = false;
				SmartBase smart = SmartRefBase._EDITOR_GetSmartObject(r.Value, target, out useMultiIndex, out writeable);
				
				if (smart != null)
				{
					string recMeta = "";
					if (useMultiIndex){
						FieldInfo multiIndex = r.Value.GetFieldPrivate("_multiIndex", binding);
						if (multiIndex != null){
							recMeta = string.Format("[{0}] ", multiIndex.GetValue(target));
						}
					}
					recMeta += (writeable ? "Read / Write" : "Read Only");
					Object owner = (Object)r.Value.GetFieldPrivate("_owner", binding).GetValue(target);
					string ownerType = owner.GetType().Name;
					if (owner is Component){
						owner = (owner as Component).gameObject;
					} else if (owner is ISmartRefOwnerRedirect){
						var redirect = (owner as ISmartRefOwnerRedirect);
						var redirectedOwner = redirect.GetSmartRefOwner();
						if (redirectedOwner){
							owner = redirectedOwner;
							ownerType = redirect.GetOwnerType().Name;
						} else {
							// ISmartRefOwnerRedirect probably hasn't had its owner populated yet
							Debug.LogWarning("Warning: ISmartRefOwnerRedirect owner probably null", redirect as Object);
						}
					}

					try {
						calls.Add(
							new SmartGraphConnection(
								owner, smart, target,
								string.Format("{0}::{1}", ownerType, ((string)r.Value.GetFieldPrivate("_propertyPath", binding).GetValue(target)).Replace(".Array.data","")),
								recMeta, null, !writeable, false
							)
						);
					#pragma warning disable 0168
					} catch (MissingReferenceException e){
						// Gameobject probably destroyed - remove ref from registry.
						_refsToRemove.Add(r.Key);
						continue;
					}
					#pragma warning restore 0168
				}
			}
			// Sweep marked refs
			foreach (var r in _refsToRemove){
				SmartData.Editors.SmartDataRegistry.UnregisterReference(r);
			}
			#endregion

			#region Get links from Multis to persistent child Vars
			var tm = typeof(SmartData.Abstract.SmartMultiBase); 
			foreach (var a in SmartData.Editors.SmartDataRegistry.GetSmartDatas()){
				if (tm.IsAssignableFrom(a.Value)){
					var persistent = (System.Array)a.Key.GetType().GetFieldPrivate("_persistent", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(a.Key);
					if (persistent != null){
						int i=0;
						foreach (var p in persistent){
							var rec = p as Object;
							calls.Add(
								new SmartGraphConnection (
									a.Key, 
									rec, 
									null,
									string.Format("[{0}]", i),
									string.Format("Multi[{0}]", i), null, false, false
								)
							);
							++i;
						}
					}
				}
			}
			#endregion

			return calls;
		}

		private static Object TryGetSmartObject(object r, System.Type t, string fieldName, bool isProperty){
			if (isProperty)
			{
				var p = t.GetProperty(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
				if (p != null)
				{
					var result = p.GetValue(r, null);
					if (result != null){
						return result as Object;
					}
				}
			}
			else
			{
				var f = t.GetFieldPrivate(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
				if (f != null)
				{
					var result = f.GetValue(r);
					if (result != null){
						return result as Object;
					}
				}
			}
			
			return null;
		}
		
		public static List<GameObject> GetAllObjectsInScene()
		{
			GameObject[] pAllObjects = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));

			List<GameObject> pReturn = new List<GameObject>();

			foreach (GameObject pObject in pAllObjects)
			{

				if (pObject.hideFlags == HideFlags.NotEditable
					|| pObject.hideFlags == HideFlags.HideAndDontSave)
				{
					continue;
				}

				if (Application.isEditor)
				{
					string sAssetPath = AssetDatabase.GetAssetPath(pObject.transform.root.gameObject);
					if (!string.IsNullOrEmpty(sAssetPath))
					{
						continue;
					}
				}

				pReturn.Add(pObject);
			}

			return pReturn;
		}
		
		private static UnityEventBase FindEvent(Component caller, SerializedProperty iterator)
		{
			PropertyInfo eventPropertyInfo = caller.GetType().GetProperty(iterator.propertyPath);
			if (eventPropertyInfo == null)
			{
				string fieldToPropertyName = iterator.propertyPath.Replace("m_", "");
				fieldToPropertyName = fieldToPropertyName[0].ToString().ToLower() + fieldToPropertyName.Substring(1);

				eventPropertyInfo = caller.GetType().GetProperty(fieldToPropertyName);
			}
			if (eventPropertyInfo != null)
			{
				return eventPropertyInfo.GetValue(caller, null) as UnityEventBase;
			}

			FieldInfo eventFieldInfo = caller.GetType().GetField(iterator.propertyPath);
			if (eventFieldInfo == null)
			{
				string fieldToFieldName = iterator.propertyPath.Replace("m_", "");
				fieldToFieldName = fieldToFieldName[0].ToString().ToLower() + fieldToFieldName.Substring(1);

				eventFieldInfo = caller.GetType().GetField(fieldToFieldName);
			}
			if (eventFieldInfo != null)
			{
				return eventFieldInfo.GetValue(caller) as UnityEventBase;
			}
			return null;
		}
	}
}