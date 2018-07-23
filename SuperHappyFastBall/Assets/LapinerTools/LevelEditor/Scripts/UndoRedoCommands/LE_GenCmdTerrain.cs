// delayed LOD for terrain is supported since Unity 5.1.1p2 -> there are no compiler flags for this version -> we assume it is supported since 5.1.2
// http://unity3d.com/unity/qa/patch-releases/5.1.1p2
#if !(UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1_0 || UNITY_5_1_1)
#define IS_DELAY_LOD_SUPPORTED
#endif

using UnityEngine;
using System.Collections;
using LE_LevelEditor.Core;
using LE_LevelEditor.UI;

namespace LE_LevelEditor.Commands
{
	public class LE_GenCmdTerrain
	{
		public enum Mode { HEIGHTS_CMD, ALPHAMAPS_CMD }

#if IS_DELAY_LOD_SUPPORTED
		private LE_GUI3dTerrain m_gui3d;
#endif
		private LE_TerrainManager m_terrainMgr;
		private Mode m_cmdMode;
		private float[,] m_heightsBeforeChange = null;
		private float[,,] m_alphamapsBeforeChange = null;
		private int m_xBase = -1;
		private int m_yBase = -1;
		private int m_xMax = -1;
		private int m_yMax = -1;

		private int m_lastEditedFrame;
		public int LastEditedFrame { get{ return m_lastEditedFrame; } }

		public LE_GenCmdTerrain(LE_GUI3dTerrain p_gui3d, LE_TerrainManager p_terrainMgr, Mode p_cmdMode)
		{
#if IS_DELAY_LOD_SUPPORTED
			m_gui3d = p_gui3d;
#endif
			m_terrainMgr = p_terrainMgr;
			m_cmdMode = p_cmdMode;
			if (m_cmdMode == Mode.HEIGHTS_CMD)
			{
				m_heightsBeforeChange = p_terrainMgr.TerrainData.GetHeights(0, 0, p_terrainMgr.TerrainData.heightmapWidth, p_terrainMgr.TerrainData.heightmapHeight);
			}
			else
			{
				m_alphamapsBeforeChange = p_terrainMgr.TerrainData.GetAlphamaps(0, 0, p_terrainMgr.TerrainData.alphamapWidth, p_terrainMgr.TerrainData.alphamapHeight);
			}
			m_lastEditedFrame = Time.frameCount;
		}

		public void ChangeHeight(float p_delta, Texture2D p_alphaBrushTexture, float p_relativeBrushSize, Vector2 p_relativeLocalLocation)
		{
			ExecuteHeightsSubCmd(()=>m_terrainMgr.ChangeHeight(p_delta, p_alphaBrushTexture, p_relativeBrushSize, p_relativeLocalLocation));
		}

		public void ChangeHeight(float p_delta, float p_targetHeight, Texture2D p_alphaBrushTexture, float p_relativeBrushSize, Vector2 p_relativeLocalLocation)
		{
			ExecuteHeightsSubCmd(()=>m_terrainMgr.ChangeHeight(p_delta, p_targetHeight, p_alphaBrushTexture, p_relativeBrushSize, p_relativeLocalLocation));
		}

		public void SmoothHeight(float p_amount, int p_neighbourCount, Texture2D p_alphaBrushTexture, float p_relativeBrushSize, Vector2 p_relativeLocalLocation, bool p_isDirected, float p_angle)
		{
			ExecuteHeightsSubCmd(()=>m_terrainMgr.SmoothHeight(p_amount, p_neighbourCount, p_alphaBrushTexture, p_relativeBrushSize, p_relativeLocalLocation, p_isDirected, p_angle));
		}

		public void PaintTexture(int p_splatPrototypeIndex, float p_delta, float p_targetValue, Texture2D p_alphaBrushTexture, float p_relativeBrushSize, Vector2 p_relativeLocalLocation)
		{
			if (m_cmdMode != Mode.ALPHAMAPS_CMD)
			{
				Debug.LogError("LE_GenCmdTerrain: ExecuteHeightsSubCmd: this instance was initialised in the '"+m_cmdMode+"' mode, but you are trying to change terrain's alphamap!");
				return;
			}

			m_terrainMgr.OnBeforeChangeAlphamaps += OnBeforeChangeAlphamaps;
			m_terrainMgr.PaintTexture(p_splatPrototypeIndex, p_delta, p_targetValue, p_alphaBrushTexture, p_relativeBrushSize, p_relativeLocalLocation);
			m_terrainMgr.OnBeforeChangeAlphamaps -= OnBeforeChangeAlphamaps;
			m_lastEditedFrame = Time.frameCount;
		}

		public LE_CmdBase GetCmd()
		{
			if (m_xBase < 0 || m_yBase < 0 || m_xMax < 0 || m_yMax < 0)
			{
				return null; // no valid command
			}

			// calculate the delta values of the affected area
			int width = m_xMax - m_xBase;
			int height = m_yMax - m_yBase;

			if (m_cmdMode == Mode.HEIGHTS_CMD)
			{
#if IS_DELAY_LOD_SUPPORTED
				// apply LOD changes now (player has stopped changing the terrain)
				m_gui3d.TerrainInstance.ApplyDelayedHeightmapModification();
#endif

				float[,] delta = m_terrainMgr.TerrainData.GetHeights(m_xBase, m_yBase, width, height);
				for (int x = 0; x < width; x++)
				{
					for (int y = 0; y < height; y++)
					{
						delta[y, x] = delta[y, x] - m_heightsBeforeChange[m_yBase + y, m_xBase + x];
					}
				}
				return new LE_CmdChangeTerrainHeights(m_terrainMgr, new LE_TerrainManager.HeightData(m_xBase, m_yBase, delta));
			}
			else
			{
				float[,,] delta = m_terrainMgr.TerrainData.GetAlphamaps(m_xBase, m_yBase, width, height);
				int alphamapsCount = delta.GetLength(2);
				for (int x = 0; x < width; x++)
				{
					for (int y = 0; y < height; y++)
					{
						for (int z = 0; z < alphamapsCount; z++)
						{
							delta[y, x, z] = delta[y, x, z] - m_alphamapsBeforeChange[m_yBase + y, m_xBase + x, z];
						}
					}
				}
				return new LE_CmdChangeTerrainAlphamaps(m_terrainMgr, new LE_TerrainManager.AlphamapData(m_xBase, m_yBase, delta));
			}
		}

		private void ExecuteHeightsSubCmd(System.Action p_subCmd)
		{
			if (m_cmdMode != Mode.HEIGHTS_CMD)
			{
				Debug.LogError("LE_GenCmdTerrain: ExecuteHeightsSubCmd: this instance was initialised in the '"+m_cmdMode+"' mode, but you are trying to change terrain's height!");
				return;
			}

			m_terrainMgr.OnBeforeChangeHeights += OnBeforeChangeHeights;
			if (p_subCmd != null) { p_subCmd(); }
			m_terrainMgr.OnBeforeChangeHeights -= OnBeforeChangeHeights;
			m_lastEditedFrame = Time.frameCount;
		}

		private void OnBeforeChangeHeights(LE_TerrainManager.HeightData p_data)
		{
			m_xBase = m_xBase == -1 ? p_data.m_xBase : Mathf.Min(m_xBase, p_data.m_xBase);
			m_yBase = m_yBase == -1 ? p_data.m_yBase : Mathf.Min(m_yBase, p_data.m_yBase);
			m_xMax = Mathf.Max(m_xMax, p_data.m_xBase + p_data.m_heights.GetLength(1));
			m_yMax = Mathf.Max(m_yMax, p_data.m_yBase + p_data.m_heights.GetLength(0));
		}

		private void OnBeforeChangeAlphamaps(LE_TerrainManager.AlphamapData p_data)
		{
			m_xBase = m_xBase == -1 ? p_data.m_xBase : Mathf.Min(m_xBase, p_data.m_xBase);
			m_yBase = m_yBase == -1 ? p_data.m_yBase : Mathf.Min(m_yBase, p_data.m_yBase);
			m_xMax = Mathf.Max(m_xMax, p_data.m_xBase + p_data.m_alphamaps.GetLength(1));
			m_yMax = Mathf.Max(m_yMax, p_data.m_yBase + p_data.m_alphamaps.GetLength(0));
		}
	}
}
