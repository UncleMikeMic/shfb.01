using UnityEngine;
using System.Collections;
using LE_LevelEditor.UI;
using LE_LevelEditor.Core;
using LE_LevelEditor.Events;
using LE_LevelEditor.Logic;

namespace LE_LevelEditor.Commands
{
	public class LE_CmdAddTerrainTexture : LE_CmdBase
	{
		private LE_LogicTerrain m_logicTerrain;
		private Texture2D m_texToAdd;

		public LE_CmdAddTerrainTexture(LE_LogicTerrain p_logicTerrain, Texture2D p_texToAdd)
		{
			m_logicTerrain = p_logicTerrain;
			m_texToAdd = p_texToAdd;
		}

		public override long GetStoredBytes()
		{
			return 16;
		}
		
		public override bool Execute()
		{
			if (!base.Execute()) { return false; }

			if (m_logicTerrain == null)
			{
				Debug.LogError("LE_CmdAddTerrainTexture: Execute: could not execute, because m_logicTerrain is null!");
				return false;
			}

			m_logicTerrain.AddTerrainTexture(m_texToAdd);

			return true;
		}
		
		public override bool Rollback()
		{
			if (!base.Rollback()) { return false; }

			if (m_logicTerrain == null)
			{
				Debug.LogError("LE_CmdAddTerrainTexture: Rollback: could not rollback, because m_logicTerrain is null!");
				return false;
			}

			m_logicTerrain.RemoveTerrainTexture(m_texToAdd);

			return true;
		}
	}
}
