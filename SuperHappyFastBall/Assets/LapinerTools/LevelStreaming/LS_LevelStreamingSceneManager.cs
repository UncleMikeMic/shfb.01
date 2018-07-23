using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LS_LevelStreaming
{
	public class LS_LevelStreamingSceneManager : MonoBehaviour
	{
		private static LS_LevelStreamingSceneManager s_instance = null;
		public static bool IsInstanceSet { get{ return s_instance != null; } }
		public static LS_LevelStreamingSceneManager Instance
		{
			get
			{
				if (s_instance == null)
				{
					GameObject go = new GameObject("LS_LevelStreamingSceneManager");
					s_instance = go.AddComponent<LS_LevelStreamingSceneManager>();
				}
				return s_instance;
			}
		}

		private Dictionary<int, LS_ManagedObjectBase> m_managedObjects = new Dictionary<int, LS_ManagedObjectBase>();
		private List<int> m_managedObjectsToRemoveAtFrameEnd = new List<int>();

		public int AddManagedObject(LS_ManagedObjectBase p_managedObj)
		{
			if (p_managedObj != null)
			{
				if (!m_managedObjects.ContainsKey(p_managedObj.ID))
				{
					m_managedObjects.Add(p_managedObj.ID, p_managedObj);
					return p_managedObj.ID;
				}
				else
				{
					Debug.LogError("LS_LevelStreamingSceneManager: AddManagedObject: object with ID(" + p_managedObj.ID + ") is already added!");
					return -1;
				}
			}
			else
			{
				Debug.LogError("LS_LevelStreamingSceneManager: AddManagedObject: parameter is null!");
				return -1;
			}

		}

		public LS_ManagedObjectBase GetManagedObject(int p_managedObjID)
		{
			LS_ManagedObjectBase obj;
			m_managedObjects.TryGetValue(p_managedObjID, out obj);
			return obj;
		}

		public bool RemoveManagedObject(int p_managedObjID)
		{
			LS_ManagedObjectBase obj = GetManagedObject(p_managedObjID);
			if (obj != null)
			{
				m_managedObjects.Remove(p_managedObjID);
				obj.m_onShow = null;
				obj.m_onHide = null;
			}
			return false;
		}

		public void RemoveManagedObjectAtFrameEnd(int p_managedObjID)
		{
			m_managedObjectsToRemoveAtFrameEnd.Add(p_managedObjID);
		}

		public void RemoveAllManagedObjects()
		{
			Dictionary<int, LS_ManagedObjectBase>.KeyCollection keys = m_managedObjects.Keys;
			int[] keysArray = new int[keys.Count];
			keys.CopyTo(keysArray, 0);
			for (int i = 0; i < keysArray.Length; i++)
			{
				RemoveManagedObject(keysArray[i]);
			}
		}

		public void ForceShowAllManagedObjects()
		{
			foreach (KeyValuePair<int, LS_ManagedObjectBase> entry in m_managedObjects)
			{
				entry.Value.ForceShow();
			}
		}

		public void ForceHideAllManagedObjects()
		{
			foreach (KeyValuePair<int, LS_ManagedObjectBase> entry in m_managedObjects)
			{
				entry.Value.ForceHide();
			}
		}
		
		private void Update()
		{
			foreach (KeyValuePair<int, LS_ManagedObjectBase> entry in m_managedObjects)
			{
				entry.Value.Update();
			}
		}

		private void LateUpdate()
		{
			foreach (int id in m_managedObjectsToRemoveAtFrameEnd)
			{
				RemoveManagedObject(id);
			}
			m_managedObjectsToRemoveAtFrameEnd.Clear();
		}
	}
}
