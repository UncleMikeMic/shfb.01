// delayed LOD for terrain is supported since Unity 5.1.1p2 -> there are no compiler flags for this version -> we assume it is supported since 5.1.2
// http://unity3d.com/unity/qa/patch-releases/5.1.1p2
#if !(UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1_0 || UNITY_5_1_1)
#define IS_DELAY_LOD_SUPPORTED
#endif

using UnityEngine;
using System.Collections;
using LE_LevelEditor.Events;

namespace LE_LevelEditor.Core
{
	public class LE_TerrainManager
	{
		public class HeightData
		{
			public readonly int m_xBase;
			public readonly int m_yBase;
			public readonly float[,] m_heights;
			public HeightData(int p_xBase, int p_yBase, float[,] p_heights)
			{
				m_xBase = p_xBase;
				m_yBase = p_yBase;
				m_heights = p_heights;
			}
		}

		public class AlphamapData
		{
			public readonly int m_xBase;
			public readonly int m_yBase;
			public readonly float[,,] m_alphamaps;
			public AlphamapData(int p_xBase, int p_yBase, float[,,] p_alphamaps)
			{
				m_xBase = p_xBase;
				m_yBase = p_yBase;
				m_alphamaps = p_alphamaps;
			}
		}

		public System.Action<HeightData> OnBeforeChangeHeights;
		public System.Action<HeightData> OnAfterChangeHeights;
		public System.Action<AlphamapData> OnBeforeChangeAlphamaps;
		public System.Action<AlphamapData> OnAfterChangeAlphamaps;

		private TerrainData m_terrainData;
		public TerrainData TerrainData
		{
			get { return m_terrainData; }
		}
		
		public LE_TerrainManager(TerrainData p_terrainData)
		{
			m_terrainData = p_terrainData;
		}

		public void PaintTexture(int p_splatPrototypeIndex, float p_delta, float p_targetValue, Texture2D p_alphaBrushTexture, float p_relativeBrushSize, Vector2 p_relativeLocalLocation)
		{
			if (p_splatPrototypeIndex < 0 || p_splatPrototypeIndex >= m_terrainData.splatPrototypes.Length)
			{
				Debug.LogError("LE_TerrainManager: PaintTexture: splat prototype index '"+p_splatPrototypeIndex+"' is out of bounds [0,"+m_terrainData.splatPrototypes.Length+"]");
				return;
			}
			float[,,] alphaMaps;
			int minX, maxX, minY, maxY;
			float relBrushMinX, relBrushMinY, alphamapMaxX, alphamapMaxY;
			alphamapMaxX = m_terrainData.alphamapWidth - 1;
			alphamapMaxY = m_terrainData.alphamapHeight - 1;
			GetAffectedAreaInternal(p_alphaBrushTexture, p_relativeBrushSize, p_relativeLocalLocation, alphamapMaxX, alphamapMaxY,
			                        out minX, out maxX, out minY, out maxY, out relBrushMinX, out relBrushMinY);
			// get the current alphamaps array
			alphaMaps = m_terrainData.GetAlphamaps(minY, minX, maxY-minY+1, maxX-minX+1);

			// inform listeners of the affected alphamaps array with its values before the change
			if (OnBeforeChangeAlphamaps != null)
			{
				OnBeforeChangeAlphamaps(new AlphamapData(minY, minX, alphaMaps));
			}

			// apply alpha change to every affected alpha map entry
			// according to given p_delta and the alpha value in p_alphaBrushTexture
			// alpha is always changed towards p_targetValue
			int iterateToX = Mathf.Min(maxX-minX, alphaMaps.GetLength(0)-1);
			int iterateToY = Mathf.Min(maxY-minY, alphaMaps.GetLength(1)-1);
			int iterateToZ = alphaMaps.GetLength(2)-1;
			float brushValue, alphaMapValue, signDiff;
			for (int indexX = 0; indexX <= iterateToX; indexX++)
			{
				for (int indexY = 0; indexY <= iterateToY; indexY++)
				{
					alphaMapValue = alphaMaps[indexX,indexY,p_splatPrototypeIndex];
					if (Mathf.Abs(alphaMapValue - p_targetValue) > 0.0001f)
					{
						signDiff = Mathf.Sign(p_targetValue - alphaMapValue);
						brushValue = p_delta * signDiff * p_alphaBrushTexture.GetPixelBilinear(
							(((float)(indexY+minY) / alphamapMaxY) - relBrushMinY) / p_relativeBrushSize,
							(((float)(indexX+minX) / alphamapMaxX) - relBrushMinX) / p_relativeBrushSize).a;

						// apply change in the selected layer
						if (signDiff != Mathf.Sign(p_targetValue - (alphaMapValue + brushValue)))
						{
							alphaMapValue = p_targetValue;
						}
						else
						{
							alphaMapValue += brushValue;
						}
						alphaMaps[indexX,indexY,p_splatPrototypeIndex] = alphaMapValue;
						// normilize the other layers
						float alphaMapsSum = 0;
						// calculate the sum of the other layers
						for (int indexZ = 0; indexZ <= iterateToZ; indexZ++)
						{
							if (indexZ != p_splatPrototypeIndex)
							{
								alphaMapsSum += alphaMaps[indexX,indexY,indexZ];
							}
						}
						// if the other layers have values, then reduce those to get a normalized result
						if (alphaMapsSum != 0)
						{
							float normalizer = (1f - alphaMapValue) / alphaMapsSum;
							for (int indexZ = 0; indexZ <= iterateToZ; indexZ++)
							{
								if (indexZ != p_splatPrototypeIndex)
								{
									alphaMaps[indexX,indexY,indexZ] *= normalizer;
								}
							}
						}
						// if the other layers have no values, but the target layers is not normalized, then...
						else if (alphaMapValue != 1)
						{
							if (p_splatPrototypeIndex != 0)
							{
								// fill up the base layer with the missing amount to normalization
								alphaMaps[indexX,indexY,0] = 1f - alphaMapValue;
							}
							else 
							{
								// and do not allow to decrease the base layer opacity
								alphaMaps[indexX,indexY,0] = 1f;
							}
						}
					}
				}	
			}

			// apply the changed alpha maps
			m_terrainData.SetAlphamaps(minY, minX, alphaMaps);

			// inform listeners of the affected alphamaps array with its values after the change
			if (OnAfterChangeAlphamaps != null)
			{
				OnAfterChangeAlphamaps(new AlphamapData(minY, minX, alphaMaps));
			}

			// notify listeners that the level data was changed
			if (LE_EventInterface.OnChangeLevelData != null)
			{
				LE_EventInterface.OnChangeLevelData(this, new LE_LevelDataChangedEvent(LE_ELevelDataChangeType.TERRAIN_ALPHAMAPS));
			}
		}

		public void SmoothHeight(float p_amount, int p_neighbourCount, Texture2D p_alphaBrushTexture, float p_relativeBrushSize, Vector2 p_relativeLocalLocation, bool p_isDirected, float p_angle)
		{
			float[,] heights;
			int minX, maxX, minY, maxY;
			float relBrushMinX, relBrushMinY, heightmapMaxX, heightmapMaxY;

			heightmapMaxX = m_terrainData.heightmapWidth - 1;
			heightmapMaxY = m_terrainData.heightmapHeight - 1;
			// the affected area is bigger than the brush size, because of the neighbourcount
			float oversizedRelativeBrushSize = Mathf.Clamp01(p_relativeBrushSize + (float)(p_neighbourCount-1)/Mathf.Max(heightmapMaxX, heightmapMaxY));
			GetAffectedAreaInternal(p_alphaBrushTexture, oversizedRelativeBrushSize, p_relativeLocalLocation, heightmapMaxX, heightmapMaxY,
			                        out minX, out maxX, out minY, out maxY, out relBrushMinX, out relBrushMinY);
			// to calculate which part of the oversized affected area is really smoothed (there are borders that a read, but not smoothed)
			// we need to calculate the affected area of the brush
			int minXWrite, maxXWrite, minYWrite, maxYWrite;
			float relBrushMinXWrite, relBrushMinYWrite;
			GetAffectedAreaInternal(p_alphaBrushTexture, p_relativeBrushSize, p_relativeLocalLocation, heightmapMaxX, heightmapMaxY,
			                        out minXWrite, out maxXWrite, out minYWrite, out maxYWrite, out relBrushMinXWrite, out relBrushMinYWrite);
			// get the current read and write heights array
			heights = m_terrainData.GetHeights(minY, minX, maxY-minY+1, maxX-minX+1);

			// inform listeners of the affected heights array with its values before the change
			if (OnBeforeChangeHeights != null)
			{
				OnBeforeChangeHeights(new HeightData(minY, minX, heights));
			}

			// smoothing code inspired by Sándor Moldán's Unity Terrain Toolkit (Unity Summer of Code 2009)
			int iterateFromX = minXWrite - minX;
			int iterateFromY = minYWrite - minY;
			int iterateToXReadMax = heights.GetLength(0)-1;
			int iterateToYReadMax = heights.GetLength(1)-1;
			int iterateToX = Mathf.Min(iterateFromX + maxXWrite-minXWrite, iterateToXReadMax);
			int iterateToY = Mathf.Min(iterateFromY + maxYWrite-minYWrite, iterateToYReadMax);
			// smooth
			int neighbourCountHalf = (p_neighbourCount-1)/2;
			int xNeighbours, yNeighbours, xShift, yShift, Tx, Ty;
			float u, v, brushValue, oldValue;
			for (Ty = iterateFromY; Ty <= iterateToY; Ty++)
			{
				// get number of neighbours on Y
				if (Ty == 0) // Ty is on left edge of array -> go in one direction only
				{
					yNeighbours = neighbourCountHalf+1;
					yShift = 0;
				}
				else if (Ty == iterateToYReadMax) // Ty is on right edge of array -> go in one direction only
				{
					yNeighbours = neighbourCountHalf+1;
					yShift = -neighbourCountHalf;
				}
				else if (Ty - neighbourCountHalf < 0) // Ty is too close to left edge of array -> limit # of look ups in the left direction
				{
					int outRange = (neighbourCountHalf - Ty);
					yNeighbours = p_neighbourCount - outRange;
					yShift = -neighbourCountHalf + outRange;
				}
				else if (Ty + neighbourCountHalf >= iterateToYReadMax) // Ty is too close to right edge of array -> limit # of look ups in the right direction
				{
					int outRange = (neighbourCountHalf + Ty) - iterateToYReadMax;
					yNeighbours = p_neighbourCount - outRange;
					yShift = -neighbourCountHalf;
				}
				else // Ty is in the middle of array -> look as much as possible
				{
					yNeighbours = p_neighbourCount;
					yShift = -neighbourCountHalf;
				}
				for (Tx = iterateFromX; Tx <= iterateToX; Tx++)
				{
					// get number of neighbours on X
					if (Tx == 0) // Tx is on left edge of array -> go in one direction only
					{
						xNeighbours = neighbourCountHalf+1;
						xShift = 0;
					}
					else if (Tx == iterateToXReadMax) // Tx is on right edge of array -> go in one direction only
					{
						xNeighbours = neighbourCountHalf+1;
						xShift = -neighbourCountHalf;
					}
					else if (Tx - neighbourCountHalf < 0) // Tx is too close to left edge of array -> limit # of look ups in the left direction
					{
						int outRange = (neighbourCountHalf - Tx);
						xNeighbours = p_neighbourCount - outRange;
						xShift = -neighbourCountHalf + outRange;
					}
					else if (Tx + neighbourCountHalf >= iterateToXReadMax) // Tx is too close to right edge of array -> limit # of look ups in the right direction
					{
						int outRange = (neighbourCountHalf + Tx) - iterateToXReadMax;
						xNeighbours = p_neighbourCount - outRange;
						xShift = -neighbourCountHalf;
					}
					else // Tx is in the middle of array -> look as much as possible
					{
						xNeighbours = p_neighbourCount;
						xShift = -neighbourCountHalf;
					}
					// smooth
					int Ny, Nx;
					float hCumulative = 0.0f;
					int nNeighbours = 0;
					// calculate the sum of all heights in the neighbourhood
					for (Ny = 0; Ny < yNeighbours; Ny++)
					{
						for (Nx = 0; Nx < xNeighbours; Nx++)
						{
							if (p_isDirected)
							{
								int neighbourOffsetX = Nx + xShift;
								int neighbourOffsetY = Ny + yShift;
								if (neighbourOffsetX != 0 || neighbourOffsetY != 0)
								{
									Vector2 dir = new Vector2(neighbourOffsetX, neighbourOffsetY).normalized;
									float angle;
									if (dir.y >= 0)
									{
										angle = Mathf.Rad2Deg*Mathf.Acos(dir.x);
									}
									else
									{
										angle = Mathf.Rad2Deg*Mathf.Asin(dir.y);
										if (dir.x < 0)
										{
											angle = -90 - (90 + angle);
										}
									}
									if (Mathf.Abs(Mathf.DeltaAngle(p_angle, angle)) > 5f &&
									    Mathf.Abs(Mathf.DeltaAngle(p_angle, angle+180)) > 5f)
									{
										continue;
									}
								}
							}
							float heightAtPoint = heights[Tx + Nx + xShift, Ty + Ny + yShift];
							hCumulative += heightAtPoint;
							nNeighbours++;
						}
					}
					float hAverage = hCumulative / nNeighbours;
					// apply smoothed result
					oldValue = heights[Tx, Ty];
					v = (((float)(Tx+minX) / heightmapMaxX) - relBrushMinXWrite) / p_relativeBrushSize;
					u = (((float)(Ty+minY) / heightmapMaxY) - relBrushMinYWrite) / p_relativeBrushSize;
					brushValue = p_amount * p_alphaBrushTexture.GetPixelBilinear(u,v).a;
					heights[Tx, Ty] = oldValue * (1f - brushValue) + hAverage * brushValue;
				}
			}
			
			// apply the changed heights array
#if IS_DELAY_LOD_SUPPORTED
			m_terrainData.SetHeightsDelayLOD(minY, minX, heights);
#else
			m_terrainData.SetHeights(minY, minX, heights);
#endif

			// inform listeners of the affected heights array with its values after the change
			if (OnAfterChangeHeights != null)
			{
				OnAfterChangeHeights(new HeightData(minY, minX, heights));
			}

			// notify listeners that the level data was changed
			if (LE_EventInterface.OnChangeLevelData != null)
			{
				LE_EventInterface.OnChangeLevelData(this, new LE_LevelDataChangedEvent(LE_ELevelDataChangeType.TERRAIN_HEIGHTS));
			}
		}
		
		public void ChangeHeight(float p_delta, float p_targetHeight, Texture2D p_alphaBrushTexture, float p_relativeBrushSize, Vector2 p_relativeLocalLocation)
		{
			float[,] heights;
			int minX, maxX, minY, maxY;
			float relBrushMinX, relBrushMinY, heightmapMaxX, heightmapMaxY;
			ChangeHeightInternal(p_alphaBrushTexture, p_relativeBrushSize, p_relativeLocalLocation,
			                     out minX, out maxX, out minY, out maxY, out heights,
			                     out relBrushMinX, out relBrushMinY, out heightmapMaxX, out heightmapMaxY);
			
			// apply height change to every affected heights array entry
			// according to given p_delta and the alpha value in p_alphaBrushTexture
			// height is always changed towards p_targetHeight
			int iterateToX = Mathf.Min(maxX-minX, heights.GetLength(0)-1);
			int iterateToY = Mathf.Min(maxY-minY, heights.GetLength(1)-1);
			for (int indexX = 0; indexX <= iterateToX; indexX++)
			{
				for (int indexY = 0; indexY <= iterateToY; indexY++)
				{
					float height = heights[indexX,indexY];
					if (Mathf.Abs(height - p_targetHeight) > 0.0001f)
					{
						float v = (((float)(indexX+minX) / heightmapMaxX) - relBrushMinX) / p_relativeBrushSize;
						float u = (((float)(indexY+minY) / heightmapMaxY) - relBrushMinY) / p_relativeBrushSize;
						float brushValue = p_delta * p_alphaBrushTexture.GetPixelBilinear(u,v).a;
						float directionFactor = p_targetHeight - height > 0 ? 1f : -1f;
						
						if (Mathf.Sign(p_targetHeight - height) != Mathf.Sign(p_targetHeight - (height + directionFactor*brushValue)))
						{
							heights[indexX,indexY] = p_targetHeight;
						}
						else
						{
							heights[indexX,indexY] += directionFactor * brushValue;
						}
					}
				}	
			}
			
			// inform listeners of the affected heights array with its values after the change
			if (OnAfterChangeHeights != null)
			{
				OnAfterChangeHeights(new HeightData(minY, minX, heights));
			}
			
			// apply the changed heights array
#if IS_DELAY_LOD_SUPPORTED
			m_terrainData.SetHeightsDelayLOD(minY, minX, heights);
#else
			m_terrainData.SetHeights(minY, minX, heights);
#endif

			// notify listeners that the level data was changed
			if (LE_EventInterface.OnChangeLevelData != null)
			{
				LE_EventInterface.OnChangeLevelData(this, new LE_LevelDataChangedEvent(LE_ELevelDataChangeType.TERRAIN_HEIGHTS));
			}
		}
		
		/// <summary>
		/// Changes the height.
		/// </summary>
		/// <param name='p_delta'>
		/// P_delta.
		/// </param>
		/// <param name='p_alphaBrushTexture'>
		/// p_alphaBrushTexture.
		/// </param>
		/// <param name='p_relativeBrushSize'>
		/// A value in range [0,1]. Relative size of the brush texture. Brush diameter is 10% of the terrain size when
		/// p_relativeBrushSize is 0.1.
		/// </param>
		/// <param name='p_relativeLocalLocation'>
		/// A vector in range ([0,1],[0,1]). Relative location of the brush texture center on the terrain.
		/// </param>
		public void ChangeHeight(float p_delta, Texture2D p_alphaBrushTexture, float p_relativeBrushSize, Vector2 p_relativeLocalLocation)
		{
			float[,] heights;
			int minX, maxX, minY, maxY;
			float relBrushMinX, relBrushMinY, heightmapMaxX, heightmapMaxY;
			ChangeHeightInternal(p_alphaBrushTexture, p_relativeBrushSize, p_relativeLocalLocation,
			                     out minX, out maxX, out minY, out maxY, out heights,
			                     out relBrushMinX, out relBrushMinY, out heightmapMaxX, out heightmapMaxY);

			// apply height change to every affected heights array entry
			// according to given p_delta and the alpha value in p_alphaBrushTexture
			int iterateToX = Mathf.Min(maxX-minX, heights.GetLength(0)-1);
			int iterateToY = Mathf.Min(maxY-minY, heights.GetLength(1)-1);
			for (int indexX = 0; indexX <= iterateToX; indexX++)
			{
				for (int indexY = 0; indexY <= iterateToY; indexY++)
				{
					float v = (((float)(indexX+minX) / heightmapMaxX) - relBrushMinX) / p_relativeBrushSize;
					float u = (((float)(indexY+minY) / heightmapMaxY) - relBrushMinY) / p_relativeBrushSize;
					float brushValue = p_alphaBrushTexture.GetPixelBilinear(u,v).a;
					heights[indexX,indexY] += p_delta*brushValue;
				}	
			}
			
			// apply the changed heights array
#if IS_DELAY_LOD_SUPPORTED
			m_terrainData.SetHeightsDelayLOD(minY, minX, heights);
#else
			m_terrainData.SetHeights(minY, minX, heights);
#endif

			// inform listeners of the affected heights array with its values after the change
			if (OnAfterChangeHeights != null)
			{
				OnAfterChangeHeights(new HeightData(minY, minX, heights));
			}

			// notify listeners that the level data was changed
			if (LE_EventInterface.OnChangeLevelData != null)
			{
				LE_EventInterface.OnChangeLevelData(this, new LE_LevelDataChangedEvent(LE_ELevelDataChangeType.TERRAIN_HEIGHTS));
			}
		}
		
		private void ChangeHeightInternal(Texture2D p_alphaBrushTexture, float p_relativeBrushSize, Vector2 p_relativeLocalLocation,
		                                  out int o_minX, out int o_maxX, out int o_minY, out int o_maxY, out float[,] o_heights,
		                                  out float o_relBrushMinX, out float o_relBrushMinY, out float o_heightmapMaxX, out float o_heightmapMaxY)
		{	
			o_heightmapMaxX = m_terrainData.heightmapWidth - 1;
			o_heightmapMaxY = m_terrainData.heightmapHeight - 1;
			GetAffectedAreaInternal(p_alphaBrushTexture, p_relativeBrushSize, p_relativeLocalLocation, o_heightmapMaxX, o_heightmapMaxY,
			                        out o_minX, out o_maxX, out o_minY, out o_maxY, out o_relBrushMinX, out o_relBrushMinY);
			// get the current heights array
			o_heights = m_terrainData.GetHeights(o_minY, o_minX, o_maxY-o_minY+1, o_maxX-o_minX+1);

			// inform listeners of the affected heights array with its values before the change
			if (OnBeforeChangeHeights != null)
			{
				OnBeforeChangeHeights(new HeightData(o_minY, o_minX, o_heights));
			}
		}

		private void GetAffectedAreaInternal(Texture2D p_alphaBrushTexture, float p_relativeBrushSize, Vector2 p_relativeLocalLocation, float p_maxX, float p_maxY,
		                                     out int o_minX, out int o_maxX, out int o_minY, out int o_maxY, out float o_relBrushMinX, out float o_relBrushMinY)
		{	
			// fallback values for out variables
			o_minX = 0;
			o_maxX = 0;
			o_minY = 0;
			o_maxY = 0;
			o_relBrushMinX = p_relativeLocalLocation.x - p_relativeBrushSize * 0.5f;
			float relBrushMaxX = p_relativeLocalLocation.x + p_relativeBrushSize * 0.5f;
			o_relBrushMinY = p_relativeLocalLocation.y - p_relativeBrushSize * 0.5f;
			float relBrushMaxY = p_relativeLocalLocation.y + p_relativeBrushSize * 0.5f;
			
			// check input values
			if (p_alphaBrushTexture == null)
			{
				Debug.LogError("LE_TerrainManager: Parameter 'p_alphaBrushTexture' is null!");
				return;
			}
			if (p_relativeBrushSize != Mathf.Clamp01(p_relativeBrushSize))
			{
				Debug.LogError("LE_TerrainManager: Parameter 'p_relativeBrushSize' is out of bounds expected range [0,1], but was '" + p_relativeBrushSize + "'!");
				return;
			}
			if (p_relativeLocalLocation.x != Mathf.Clamp01(p_relativeLocalLocation.x) || p_relativeLocalLocation.y != Mathf.Clamp01(p_relativeLocalLocation.y))
			{
				Debug.LogError("LE_TerrainManager: Parameter 'p_relativeLocalLocation' is out of bounds expected range ([0,1],[0,1]), but was '" + p_relativeLocalLocation + "'!");
				return;
			}
			
			// calculate the smallest start and the highest end indices of the affected
			// height array fields concerning the brush size and brush location
			o_minX = Mathf.Clamp(Mathf.FloorToInt(p_maxX * o_relBrushMinX), 0, (int)p_maxX);
			o_maxX = Mathf.Clamp(Mathf.CeilToInt(p_maxX * relBrushMaxX), 0, (int)p_maxX);
			o_minY = Mathf.Clamp(Mathf.FloorToInt(p_maxY * o_relBrushMinY), 0, (int)p_maxY);
			o_maxY = Mathf.Clamp(Mathf.CeilToInt(p_maxY * relBrushMaxY), 0, (int)p_maxY);
		}
	}
}
