using System;

namespace LE_LevelEditor.Events
{
	public class LE_TerrainCreatedEvent : EventArgs
	{
		private readonly UnityEngine.GameObject m_terrainGameObject;
		public UnityEngine.GameObject TerrainGameObject { get{ return m_terrainGameObject; } }

		public LE_TerrainCreatedEvent(UnityEngine.GameObject p_terrainGameObject)
		{
			m_terrainGameObject = p_terrainGameObject;
		}
	}
}