using UnityEngine;
using System.Collections;

namespace TT_TerrainTools
{
	public static class TT_TerrainHelpers
	{
		public enum EDirection { TOP, BOTTOM, LEFT, RIGHT }

		public static void FixBorders(Terrain p_terrainA, Terrain p_terrainB, EDirection p_borderA, EDirection p_borderB, float p_fade)
		{
			int heightsLength = p_terrainA.terrainData.heightmapResolution;
			if (p_terrainA.terrainData.heightmapResolution != heightsLength || p_terrainB.terrainData.heightmapResolution != heightsLength)
			{
				Debug.LogError("TT_Terrain9Patch: both TerrainData assets must have the same 'heightmapResolution'!");
				return;
			}
			p_fade = Mathf.Clamp01(p_fade);

			bool isChangedA = false;
			bool isChangedB = false;
			float terrainPosYA = p_terrainA.transform.position.y;
			float terrainSizeYA = p_terrainA.terrainData.size.y;
			float terrainPosYB = p_terrainB.transform.position.y;
			float terrainSizeYB = p_terrainB.terrainData.size.y;
			float[,] heightsA = GetDirectedHeights(p_terrainA.terrainData, p_borderA);
			float[,] heightsB = GetDirectedHeights(p_terrainB.terrainData, p_borderB);
			for (int i = 0; i < heightsLength; i++)
			{
				float valueA = GetDirectedWorldHeight(i, p_borderA, heightsA, p_terrainA);
				float valueB = GetDirectedWorldHeight(i, p_borderB, heightsB, p_terrainB);
				float fadedValue = valueA*(1f-p_fade)+valueB*p_fade;
				SetDirectedWorldHeight(fadedValue, i, p_borderA, heightsA, terrainPosYA, terrainSizeYA);
				SetDirectedWorldHeight(fadedValue, i, p_borderB, heightsB, terrainPosYB, terrainSizeYB);
				isChangedA |= fadedValue!=valueA;
				isChangedB |= fadedValue!=valueB;
			}
			if (isChangedA) { SetDirectedHeights(heightsA, p_terrainA.terrainData, p_borderA); }
			if (isChangedB) { SetDirectedHeights(heightsB, p_terrainB.terrainData, p_borderB); }
		}

		private static float GetDirectedWorldHeight(int p_index, EDirection p_border, float[,] p_heights, Terrain p_terrain)
		{
			return p_terrain.transform.position.y + p_terrain.terrainData.size.y*GetDirectedLocalHeight(p_index, p_border, p_heights);
		}

		private static float GetDirectedLocalHeight(int p_index, EDirection p_border, float[,] p_heights)
		{
			switch (p_border)
			{
				case EDirection.RIGHT:
				case EDirection.LEFT: return p_heights[p_index, 0];
				case EDirection.BOTTOM:
				case EDirection.TOP:
				default: return p_heights[0, p_index];
			}
		}

		private static void SetDirectedWorldHeight(float p_value, int p_index, EDirection p_border, float[,] p_heights, float p_terrainPosY, float p_terrainSizeY)
		{
			float localValue = (p_value - p_terrainPosY) / p_terrainSizeY;
			SetDirectedLocalHeight(localValue, p_index, p_border, p_heights);
		}

		private static void SetDirectedLocalHeight(float p_value, int p_index, EDirection p_border, float[,] p_heights)
		{
			switch (p_border)
			{
				case EDirection.RIGHT:
				case EDirection.LEFT: p_heights[p_index, 0] = p_value; break;
				case EDirection.BOTTOM:
				case EDirection.TOP:
				default: p_heights[0, p_index] = p_value; break;
			}
		}

		private static float[,] GetDirectedHeights(TerrainData p_terrainData, EDirection p_border)
		{
			int arraySize = p_terrainData.heightmapResolution;
			switch (p_border)
			{
				case EDirection.RIGHT: return p_terrainData.GetHeights(arraySize-1, 0, 1, arraySize);
				case EDirection.LEFT: return p_terrainData.GetHeights(0, 0, 1, arraySize);
				case EDirection.BOTTOM: return p_terrainData.GetHeights(0, 0, arraySize, 1);
				case EDirection.TOP:
				default: return p_terrainData.GetHeights(0, arraySize-1, arraySize, 1);
			}
		}

		private static void SetDirectedHeights(float[,] p_heights, TerrainData p_terrainData, EDirection p_border)
		{
			int arraySize = p_terrainData.heightmapResolution;
			switch (p_border)
			{
				case EDirection.RIGHT: p_terrainData.SetHeights(arraySize-1, 0, p_heights); break;
				case EDirection.LEFT: p_terrainData.SetHeights(0, 0, p_heights); break;
				case EDirection.BOTTOM: p_terrainData.SetHeights(0, 0, p_heights); break;
				case EDirection.TOP:
				default: p_terrainData.SetHeights(0, arraySize-1, p_heights); break;
			}
		}
	}
}
