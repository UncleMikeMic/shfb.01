using UnityEngine;
using System.Collections;
using UndoRedo;
using LE_LevelEditor.Core;
using LE_LevelEditor.Events;

namespace LE_LevelEditor.Commands
{
	public class LE_CmdChangeTerrainHeights : LE_CmdBase
	{
		private LE_TerrainManager m_terrainMgr;
		private LE_TerrainManager.HeightData m_heightsDelta = null;

		public LE_CmdChangeTerrainHeights(LE_TerrainManager p_terrainMgr, LE_TerrainManager.HeightData p_heightsDelta)
		{
			m_terrainMgr = p_terrainMgr;
			m_heightsDelta = p_heightsDelta;
			m_isExecuted = true;
		}

		public override long GetStoredBytes()
		{
			if (m_heightsDelta != null)
			{
				return 20 + 4*m_heightsDelta.m_heights.Length;
			}
			else
			{
				return 0;
			}
		}

		public override bool Execute()
		{
			return base.Execute() && Apply(1f);
		}

		public override bool Rollback()
		{
			return base.Rollback() && Apply(-1f);
		}

		private bool Apply(float p_direction)
		{
			if (m_terrainMgr == null || m_heightsDelta == null)
			{
				Debug.LogError("LE_CmdChangeTerrainHeight: Apply: could not execute, m_terrainMgr or m_heightsDelta are null!");
				return false;
			}
			
			int xBase = m_heightsDelta.m_xBase;
			int yBase = m_heightsDelta.m_yBase;
			int width = m_heightsDelta.m_heights.GetLength(1);
			int height = m_heightsDelta.m_heights.GetLength(0);
			if (width > m_terrainMgr.TerrainData.heightmapWidth || height > m_terrainMgr.TerrainData.heightmapHeight)
			{
				Debug.LogError("LE_CmdChangeTerrainHeight: Apply: could not execute, terrain height map resolution was reduced in the meantime!");
				return false;
			}

			float[,] dataAfterChange = m_terrainMgr.TerrainData.GetHeights(xBase, yBase, width, height);
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					dataAfterChange[y, x] += p_direction * m_heightsDelta.m_heights[y, x];
				}
			}
			m_terrainMgr.TerrainData.SetHeights(m_heightsDelta.m_xBase, m_heightsDelta.m_yBase, dataAfterChange);
			// notify listeners that the level data was changed
			if (LE_EventInterface.OnChangeLevelData != null)
			{
				LE_EventInterface.OnChangeLevelData(m_terrainMgr, new LE_LevelDataChangedEvent(LE_ELevelDataChangeType.TERRAIN_HEIGHTS));
			}
			return true;
		}
	}
}
