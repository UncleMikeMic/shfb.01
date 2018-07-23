using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LE_LevelEditor.Core
{
	[System.Serializable]
	public class LE_ObjectVariationActivateDeactivate : LE_ObjectVariationBase
	{
		[SerializeField]
		private string m_name = "Enter variation name here..";
		public string Name
		{
			get{ return m_name; }
			set{ m_name = value; }
		}

		[SerializeField]
		private GameObject[] m_objs = new GameObject[0];
		public GameObject[] Objs
		{
			get{ return m_objs; }
			set{ m_objs = value; }
		}

		[SerializeField]
		private bool[] m_objIsActivateStates = new bool[0];
		public bool[] ObjIsActivateStates
		{
			get{ return m_objIsActivateStates; }
			set{ m_objIsActivateStates = value; }
		}

		public LE_ObjectVariationActivateDeactivate(string p_name)
		{
			m_name = p_name;
		}

		public override string GetName()
		{
			return m_name;
		}

		public override void Apply(LE_Object p_object)
		{
			if (m_objs == null || m_objIsActivateStates == null || m_objs.Length != m_objIsActivateStates.Length)
			{
				Debug.LogWarning("LE_ObjectVariationActivateDeactivate: Apply: the 'Objs' array must be of same length as the 'ObjIsActivateStates' array! The active state of the game object at index i will be overwritten the 'ObjIsActivateStates' at index i!");
			}
			
			for (int i = 0; i < m_objs.Length && i < m_objIsActivateStates.Length; i++)
			{
				GameObject obj = m_objs[i];
				if (obj != null)
				{
					obj.SetActive(m_objIsActivateStates[i]);
				}
			}
		}

		public bool LoadNFixReferences(LE_Object p_object)
		{
			bool isChanged = false;
			
			Transform[] currTransforms = p_object.GetComponentsInChildren<Transform>(true);
			
			// check for nullpointers
			for (int i = m_objs.Length-1; i >= 0; i--)
			{
				if (m_objs[i] == null)
				{
					RemoveObjectAt(i);
					isChanged = true;
				}
			}
			
			// check if renderers where removed
			for (int i = m_objs.Length-1; i >= 0; i--)
			{
				if (System.Array.IndexOf(currTransforms, m_objs[i].transform) < 0)
				{
					RemoveObjectAt(i);
					isChanged = true;
				}
			}
			
			// check if renderers added
			for (int i = 0; i < currTransforms.Length; i++)
			{
				if (currTransforms[i] != p_object.transform && // don't add self
					System.Array.IndexOf(m_objs, currTransforms[i].gameObject) < 0 &&
					currTransforms[i].GetComponentInParent<LE_ObjectEditHandle>() == null) // don't select edit handles
				{
					AddObject(currTransforms[i].gameObject);
					isChanged = true;
				}
			}
			
			return isChanged;
		}
		
		private void AddObject(GameObject p_object)
		{
			List<GameObject> changedObjects = new List<GameObject>(m_objs);
			List<bool> changedObjectStates = new List<bool>(m_objIsActivateStates);
			
			changedObjects.Add(p_object);
			changedObjectStates.Add(p_object.activeSelf);
			
			m_objs = changedObjects.ToArray();
			m_objIsActivateStates = changedObjectStates.ToArray();
		}
		
		private void RemoveObjectAt(int p_index)
		{
			List<GameObject> changedObjects = new List<GameObject>(m_objs);
			List<bool> changedObjectStates = new List<bool>(m_objIsActivateStates);
			
			changedObjects.RemoveAt(p_index);
			changedObjectStates.RemoveAt(p_index);
			
			m_objs = changedObjects.ToArray();
			m_objIsActivateStates = changedObjectStates.ToArray();
		}
	}
}
