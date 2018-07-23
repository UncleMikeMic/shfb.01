using UnityEngine;
using System.Collections;
using LE_LevelEditor.Core;
using LE_LevelEditor.Events;

namespace LE_LevelEditor.Commands
{
	public class LE_CmdChangeTerrainAlphamaps : LE_CmdBase
	{
		private LE_TerrainManager m_terrainMgr;
		private LE_TerrainManager.AlphamapData m_alphamapsDelta = null;

		public LE_CmdChangeTerrainAlphamaps(LE_TerrainManager p_terrainMgr, LE_TerrainManager.AlphamapData p_alphamapsDelta)
		{
			m_terrainMgr = p_terrainMgr;
			m_alphamapsDelta = p_alphamapsDelta;
			m_isExecuted = true;
		}
		
		public override long GetStoredBytes()
		{
			if (m_alphamapsDelta != null)
			{
				return 20 + 4*m_alphamapsDelta.m_alphamaps.Length;
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
			if (m_terrainMgr == null || m_alphamapsDelta == null)
			{
				Debug.LogError("LE_CmdChangeTerrainAlphamaps: Apply: could not execute, m_terrainMgr or m_alphamapsDelta are null!");
				return false;
			}
			
			int xBase = m_alphamapsDelta.m_xBase;
			int yBase = m_alphamapsDelta.m_yBase;
			int width = m_alphamapsDelta.m_alphamaps.GetLength(1);
			int height = m_alphamapsDelta.m_alphamaps.GetLength(0);
			int alphamapsCount = m_alphamapsDelta.m_alphamaps.GetLength(2);
			if (width > m_terrainMgr.TerrainData.alphamapWidth || height > m_terrainMgr.TerrainData.alphamapHeight)
			{
				Debug.LogError("LE_CmdChangeTerrainAlphamaps: Apply: could not execute, terrain alpha map resolution was reduced in the meantime!");
				return false;
			}
			if (alphamapsCount > m_terrainMgr.TerrainData.alphamapLayers)
			{
				Debug.LogError("LE_CmdChangeTerrainAlphamaps: Apply: could not execute, terrain alpha map layers count was reduced in the meantime!");
				return false;
			}
			
			float[,,] dataAfterChange = m_terrainMgr.TerrainData.GetAlphamaps(xBase, yBase, width, height);
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					for (int z = 0; z < alphamapsCount; z++)
					{
						dataAfterChange[y, x, z] += p_direction * m_alphamapsDelta.m_alphamaps[y, x, z];
					}
				}
			}
			m_terrainMgr.TerrainData.SetAlphamaps(m_alphamapsDelta.m_xBase, m_alphamapsDelta.m_yBase, dataAfterChange);
			// notify listeners that the level data was changed
			if (LE_EventInterface.OnChangeLevelData != null)
			{
				LE_EventInterface.OnChangeLevelData(m_terrainMgr, new LE_LevelDataChangedEvent(LE_ELevelDataChangeType.TERRAIN_ALPHAMAPS));
			}
			return true;
		}
	}
}
