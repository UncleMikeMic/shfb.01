using UnityEngine;
using System.Collections;

namespace LE_LevelEditor.Core
{
	public class LE_ObjectMap : ScriptableObject
	{
		[SerializeField]
		private LE_Object[] m_objectPrefabs = new LE_Object[0];
		public LE_Object[] ObjectPrefabs { get{ return m_objectPrefabs; } }

		[SerializeField, HideInInspector]
		private string[] m_objectPrefabResourcePaths = new string[0];
		public string[] ObjectPrefabResourcePaths
		{
			get{ return m_objectPrefabResourcePaths; }
			set{ m_objectPrefabResourcePaths = value; }
		}

		[SerializeField]
		private LE_ObjectMap[] m_subObjectMaps = new LE_ObjectMap[0];
		public LE_ObjectMap[] SubObjectMaps { get{ return m_subObjectMaps; } }
	}
}