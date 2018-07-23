using UnityEngine;
using System.Collections;

namespace LE_LevelEditor.Core
{
	public class LE_SaveLoadDataPeek
	{
		private readonly byte m_version;
		public byte Version { get{ return m_version; } }
		
		private readonly TerrainData m_terrainData;
		public TerrainData TerrainDataPreview { get{ return m_terrainData; } }
		
		private readonly int m_levelObjectsCount;
		public int LevelObjectsCount { get{ return m_levelObjectsCount; } }

		public LE_SaveLoadDataPeek(byte p_version, TerrainData p_terrainData, int p_levelObjectsCount)
		{
			m_version = p_version;
			m_terrainData = p_terrainData;
			m_levelObjectsCount = p_levelObjectsCount;
		}
	}
}