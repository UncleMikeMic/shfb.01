using UnityEngine;
using System.Collections;
using LE_LevelEditor.UI;
using LE_LevelEditor.Core;
using LE_LevelEditor.Events;
using LE_LevelEditor.Logic;

namespace LE_LevelEditor.Commands
{
	public class LE_CmdCreateTerrain : LE_CmdBase
	{
		private LE_LogicTerrain m_logicTerrain;
		private int m_terrainBaseTextureIndex;
		private TerrainData m_terrainData;

		public LE_CmdCreateTerrain(LE_LogicTerrain p_logicTerrain, int p_terrainBaseTextureIndex, TerrainData p_terrainData)
		{
			m_logicTerrain = p_logicTerrain;
			m_terrainData = p_terrainData;
			m_terrainBaseTextureIndex = p_terrainBaseTextureIndex;
		}

		public override long GetStoredBytes()
		{
			return 50;
		}
		
		public override bool Execute()
		{
			if (!base.Execute()) { return false; }

			if (m_terrainData == null)
			{
				Debug.LogError("LE_CmdCreateTerrain: Execute: could not execute, m_terrainData is null!");
				return false;
			}

			if (m_logicTerrain == null)
			{
				Debug.LogError("LE_CmdCreateTerrain: Execute: could not execute, m_logicTerrain is null!");
				return false;
			}

			m_logicTerrain.CreateOrRecycleTerrainWithUIUpdate(m_terrainData, m_terrainBaseTextureIndex);

			return true;
		}
		
		public override bool Rollback()
		{
			if (!base.Rollback()) { return false; }

			if (m_logicTerrain == null)
			{
				Debug.LogError("LE_CmdCreateTerrain: Execute: could not execute, m_logicTerrain is null!");
				return false;
			}

			m_logicTerrain.DestroyOrResetTerrainWithUIUpdate();

			return true;
		}
	}
}
