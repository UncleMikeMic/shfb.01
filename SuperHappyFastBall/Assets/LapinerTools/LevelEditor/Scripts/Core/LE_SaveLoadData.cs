using UnityEngine;
using System.Collections;

namespace LE_LevelEditor.Core
{
	public class LE_SaveLoadData
	{
		/// <summary>
		/// LE_SaveLoadData.ObjectData contains the result of the loading operation for each object. Depending on the Result property you can access the Instance, the StreamedLevelObjectID or an Error message from the loaded object.
		/// </summary>
		public class ObjectData
		{
			public enum EResult { INSTANCE, STREAMED, ERROR }
			
			private readonly EResult m_loadResult;
			public EResult Result { get{ return m_loadResult; } }

			private readonly LE_Object m_instance;
			public LE_Object Instance { get{ return m_instance; } }

			private readonly int m_streamedLevelObjectID;
			public int StreamedLevelObjectID { get{ return m_streamedLevelObjectID; } }

			private readonly string m_error;
			public string Error { get{ return m_error; } }

			public ObjectData(LE_Object p_instance)
			{
				if (p_instance != null)
				{
					m_loadResult = EResult.INSTANCE;
					m_instance = p_instance;
					m_streamedLevelObjectID = -1;
					m_error = null;
				}
				else
				{
					m_loadResult = EResult.ERROR;
					m_instance = null;
					m_streamedLevelObjectID = -1;
					m_error = "Internal Error (instance is null)";
					Debug.LogError("ObjectData: p_instance is null!");
				}
			}

			public ObjectData(int p_streamedLevelObjectID)
			{
				if (p_streamedLevelObjectID != -1)
				{
					m_loadResult = EResult.STREAMED;
					m_instance = null;
					m_streamedLevelObjectID = p_streamedLevelObjectID;
					m_error = null;
				}
				else
				{
					m_loadResult = EResult.ERROR;
					m_instance = null;
					m_streamedLevelObjectID = -1;
					m_error = "Internal Error (streamed level object ID is -1)";
					Debug.LogError("ObjectData: p_streamedLevelObjectID is -1!");
				}
			}

			public ObjectData(string p_error)
			{
				if (p_error != null)
				{
					m_loadResult = EResult.ERROR;
					m_instance = null;
					m_streamedLevelObjectID = -1;
					m_error = p_error;
				}
				else
				{
					m_loadResult = EResult.ERROR;
					m_instance = null;
					m_streamedLevelObjectID = -1;
					m_error = "Internal Error (error message is null)";
					Debug.LogError("ObjectData: p_error is null!");
				}
			}
		}
		
		private readonly byte m_version;
		public byte Version { get{ return m_version; } }
		
		private readonly GameObject m_terrainObject;
		public GameObject TerrainObject { get{ return m_terrainObject; } }
		
		private readonly ObjectData[] m_levelObjects;
		public ObjectData[] LevelObjects { get{ return m_levelObjects; } }

		public LE_SaveLoadData(byte p_version, GameObject p_terrainObject, ObjectData[] p_levelObjects)
		{
			m_version = p_version;
			m_terrainObject = p_terrainObject;
			m_levelObjects = p_levelObjects;
		}
	}
}