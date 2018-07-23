using UnityEngine;
using System.Collections;
using LE_LevelEditor.Core;
using LE_LevelEditor.Events;
using TT_TerrainTools;
using UndoRedo;
using LE_LevelEditor.Commands;

namespace LE_LevelEditor.UI
{
	public class LE_GUI3dTerrain : LE_GUI3dBase
	{
		public int TERRAIN_LAYER = 28;

		private Terrain m_terrain = null;
		public Terrain TerrainInstance
		{
			get { return m_terrain; }
		}
		private LE_TerrainManager m_terrainMgr = null;
		public LE_TerrainManager TerrainManager
		{
			get { return m_terrainMgr; }
		}
		public void SetTerrain(Terrain p_terrain)
		{
			if (m_terrainMgr != null)
			{
				Debug.LogError("LE_GUI3dTerrain: SetTerrain: a terrain manager was already set and will be overwritten! Use 'RemoveTerrainManager' to reset the instance.");
			}
			m_terrain = p_terrain;
			m_terrainMgr = new LE_TerrainManager(p_terrain.terrainData);

			// notify listeners that the level data was changed
			if (LE_EventInterface.OnChangeLevelData != null)
			{
				LE_EventInterface.OnChangeLevelData(m_terrain.gameObject, new LE_LevelDataChangedEvent(LE_ELevelDataChangeType.TERRAIN_SELECTION));
			}
		}
		public void RemoveTerrainManager()
		{
			// notify listeners that the level data was changed
			if (m_terrain != null && LE_EventInterface.OnChangeLevelData != null)
			{
				LE_EventInterface.OnChangeLevelData(m_terrain.gameObject, new LE_LevelDataChangedEvent(LE_ELevelDataChangeType.TERRAIN_SELECTION));
			}
			m_terrain = null;
			m_terrainMgr = null;
		}

		private TerrainData m_defaultTerrainDataPrefab;
		public TerrainData DefaultTerrainDataPrefab
		{
			get{ return m_defaultTerrainDataPrefab; }
			set{ m_defaultTerrainDataPrefab = value; }
		}

		private Projector m_brushProjector = null;
		public Projector BrushProjector
		{
			get { return m_brushProjector; }
			set
			{
				m_brushProjector = value;
				m_brushProjector.material = (Material)Object.Instantiate(m_brushProjector.material);
				m_brushProjector.ignoreLayers = ~(1 << TERRAIN_LAYER);
			}
		}
		
		private Texture2D m_brushAlphaTexture = null;
		public Texture2D BrushAlphaTexture
		{
			get { return m_brushAlphaTexture; }
			set
			{
				m_brushAlphaTexture = value;
				m_brushProjector.material.mainTexture = m_brushAlphaTexture;
			}
		}

		private int m_selectedSplatPrototype = 0;
		public int SelectedSplatPrototype
		{
			get { return m_selectedSplatPrototype; }
			set { m_selectedSplatPrototype = value; }
		}
		
		private float m_amount = 0.1f;
		public float Amount
		{
			get { return m_amount; }
			set { m_amount = value; }
		}
		
		private float m_size = 0.1f;
		public float Size
		{
			get { return m_size; }
			set
			{
				m_size = value;
				if (m_terrainMgr != null)
				{
					// make the brush size fit the terrain data cell size
					float cellSize = 1f/(float)Mathf.Min(m_terrainMgr.TerrainData.heightmapWidth-1, m_terrainMgr.TerrainData.heightmapHeight-1);
					m_size = Mathf.Floor(m_size/cellSize)*cellSize;
				}
			}
		}
		
		private float m_targetRelativeValue = 0.5f;
		public float TargetRelativeValue
		{
			get { return m_targetRelativeValue; }
			set { m_targetRelativeValue = value; }
		}

		private bool m_isReadingTerrainPaintHeight = false;
		private int m_isTerrainPaintHeightReadInFrame = -1;
		public bool IsReadingTerrainPaintHeight
		{
			get { return m_isReadingTerrainPaintHeight; }
			set
			{
				m_isReadingTerrainPaintHeight = value;
				m_isTerrainPaintHeightReadInFrame = -1;
			}
		}

		private bool m_isDirectedSmooth = false;
		public bool IsDirectedSmooth
		{
			get { return m_isDirectedSmooth; }
			set { m_isDirectedSmooth = value; }
		}

		private float m_directedSmoothAngle = 0f;
		public float DirectedSmoothAngle
		{
			get { return m_directedSmoothAngle; }
			set
			{
				m_directedSmoothAngle = value;
				if (m_brushProjector.transform.childCount > 0)
				{
					m_brushProjector.transform.GetChild(0).transform.localRotation = Quaternion.Euler(Vector3.back * m_directedSmoothAngle);
				}
			}
		}

		private LE_ETerrainEditMode m_editMode = LE_ETerrainEditMode.CHANGE_HEIGHT;
		public LE_ETerrainEditMode EditMode
		{
			get { return m_editMode; }
			set
			{
				m_editMode = value;
				if (m_editMode != LE_ETerrainEditMode.CHANGE_HEIGHT_TO_TARGET_VALUE)
				{
					m_isReadingTerrainPaintHeight = false;
					m_isTerrainPaintHeightReadInFrame = -1;
				}
			}
		}

		private LE_GenCmdTerrain m_genCmdTerrain = null;

		private Vector3 m_lastCursorActiveScreenCoords = -1f*Vector3.one;

		public override LE_EEditMode ActiveEditMode { get{ return LE_EEditMode.TERRAIN; } }

		public override void SetCursorPosition(Vector3 p_cursorScreenCoords)
		{
			m_cursorScreenCoords = p_cursorScreenCoords;
			m_cursorRay = Camera.main.ScreenPointToRay (p_cursorScreenCoords);
			if (m_terrain != null)
			{
				SetIsCursorOverSomething(m_terrain.GetComponent<Collider>().Raycast (m_cursorRay, out m_cursorHitInfo, float.MaxValue));
			}
			else
			{
				SetIsCursorOverSomething(false);
			}
		}

		public override void SetIsCursorAction(bool p_isCursorAction)
		{
			if (m_isTerrainPaintHeightReadInFrame != -1 && !p_isCursorAction && m_isTerrainPaintHeightReadInFrame + 1 < Time.frameCount)
			{
				m_isReadingTerrainPaintHeight = false;
				m_isTerrainPaintHeightReadInFrame = -1;
			}
			if (IsCursorOverSomething && IsInteractable && p_isCursorAction && m_lastCursorActiveScreenCoords != m_cursorScreenCoords)
			{
				m_lastCursorActiveScreenCoords = m_cursorScreenCoords;
				if (m_cursorHitInfo.transform.gameObject.GetComponent<Terrain>() != null)
				{
					if (m_isReadingTerrainPaintHeight)
					{
						Terrain terrain = m_cursorHitInfo.transform.gameObject.GetComponent<Terrain>();
						float heightWorld = terrain.SampleHeight(m_cursorHitInfo.point);
						m_targetRelativeValue = Mathf.Clamp01(heightWorld / terrain.terrainData.size.y);
						m_isTerrainPaintHeightReadInFrame = Time.frameCount;
					}
					else
					{
						if (m_genCmdTerrain == null)
						{
							LE_GenCmdTerrain.Mode cmdMode = m_editMode==LE_ETerrainEditMode.DRAW_TEXTURE ? LE_GenCmdTerrain.Mode.ALPHAMAPS_CMD : LE_GenCmdTerrain.Mode.HEIGHTS_CMD;
							m_genCmdTerrain = new LE_GenCmdTerrain(this, m_terrainMgr, cmdMode);
						}
						switch (m_editMode)
						{
							case LE_ETerrainEditMode.CHANGE_HEIGHT:
								m_genCmdTerrain.ChangeHeight(Mathf.Sign(Amount)*Mathf.Max(0.002f,Amount*Amount) * Time.deltaTime * 2f, BrushAlphaTexture, Size, GetRelativeLocalLocation(m_cursorHitInfo));
								break;
							case LE_ETerrainEditMode.CHANGE_HEIGHT_TO_TARGET_VALUE:
								m_genCmdTerrain.ChangeHeight(Mathf.Max(0.002f,Amount*Amount) * Time.deltaTime * 2f, TargetRelativeValue, BrushAlphaTexture, Size, GetRelativeLocalLocation(m_cursorHitInfo));
								break;
							case LE_ETerrainEditMode.SMOOTH_HEIGHT:
								int neighbourCount = 3 + 2*Mathf.RoundToInt((Mathf.Abs(Amount)*3f));
								m_genCmdTerrain.SmoothHeight(Time.deltaTime * 2f, neighbourCount, BrushAlphaTexture, Size, GetRelativeLocalLocation(m_cursorHitInfo), m_isDirectedSmooth, m_directedSmoothAngle);
								break;
							case LE_ETerrainEditMode.DRAW_TEXTURE:
								m_genCmdTerrain.PaintTexture(SelectedSplatPrototype, Mathf.Abs(Amount) * Time.deltaTime * 8f, TargetRelativeValue, BrushAlphaTexture, Size, GetRelativeLocalLocation(m_cursorHitInfo));
								break;
							default:
								Debug.LogError("LE_GUI3dTerrain: unknown EditMode!");
								break;
						}
					}
				}
			}
			else
			{
				m_lastCursorActiveScreenCoords = -1*Vector3.one;
			}

			if (m_genCmdTerrain != null && m_genCmdTerrain.LastEditedFrame+1 < Time.frameCount)
			{
				UR_ICommand cmd = m_genCmdTerrain.GetCmd();
				if (cmd != null) { UR_CommandMgr.Instance.Add(cmd, true); }
				m_genCmdTerrain = null;
			}
		}

		public void HideCursor()
		{
			BrushProjector.orthographicSize = 0;
			if (BrushProjector.transform.childCount > 0)
			{
				BrushProjector.transform.GetChild(0).gameObject.SetActive(false);
			}
			SetIsCursorOverSomething(false);
		}

		public void ResetToDefaultOrDestroyTerrain()
		{
			if (m_terrain != null)
			{
				if (m_defaultTerrainDataPrefab != null)
				{
					// there is a default terrain -> reset the existing terrain
					TerrainData terrainData = GetDefaultTerrainDataDeepCopy();
					RecycleTerrain(terrainData, false);
					m_terrain.gameObject.layer = TERRAIN_LAYER;
					m_terrain.transform.position = new Vector3(-terrainData.size.x*0.5f, m_terrain.transform.position.y, -terrainData.size.z*0.5f);
				}
				else
				{
					// there is no default terrain -> destroy the existing terrain
					Destroy(m_terrain.gameObject);
					RemoveTerrainManager();
					SetIsCursorOverSomething(false); // hide cursor
				}
			}
		}

		public void RecycleTerrain(TerrainData p_data, bool p_isDefaultTerrainDataApplied)
		{
			if (m_terrain != null)
			{
				if (p_isDefaultTerrainDataApplied && m_defaultTerrainDataPrefab != null)
				{
					ApplyDefaultTerrainData(p_data);
					if (p_data.treePrototypes.Length > 0)
					{
						// this will force an update of the tree y positions
						p_data.SetHeights(0,0, p_data.GetHeights(0, 0, 0, 0));
					}
				}

				m_terrain.enabled = false;

				// destroy old terrain data
				Object.Destroy(m_terrain.terrainData);
				// assign new terrain data
				m_terrain.terrainData = p_data;
				if (m_terrain.GetComponent<TerrainCollider>() != null)
				{
					m_terrain.GetComponent<TerrainCollider>().terrainData = p_data;
				}
				else
				{
					Debug.LogError("LE_GUI3dTerrain: RecycleTerrain: the CustomDefaultTerrain assigned to LE_ConfigTerrain must have a collider!");
				}

				// apply terrain changes
				m_terrain.Flush();
				m_terrain.enabled = true;

				// warning of a possible seldom crash with specific resolution changes
				TT_Terrain9Patch terrain9patcher = m_terrain.GetComponent<TT_Terrain9Patch>();
				if (terrain9patcher != null) { terrain9patcher.CrashCheck(); }

				// recreate terrain manager
				Terrain buffer = m_terrain;
				RemoveTerrainManager();
				SetTerrain(buffer);
			}
			else
			{
				Debug.LogError("LE_GUI3dTerrain: RecycleTerrain: there is not terrain that can be recycled. Call this function only if TerrainInstance is not null!");
			}
		}
		
		public TerrainData GetDefaultTerrainDataDeepCopy()
		{
			if (m_defaultTerrainDataPrefab != null)
			{
				TerrainData copyData = new TerrainData();
				copyData.name = m_defaultTerrainDataPrefab.name + "(DeepCopy)";
				copyData.size = m_defaultTerrainDataPrefab.size; // dummy call, but required (will be overwritten later)
				ApplyDefaultTerrainData(copyData);
				copyData.splatPrototypes = m_defaultTerrainDataPrefab.splatPrototypes;
				copyData.alphamapResolution = m_defaultTerrainDataPrefab.alphamapResolution;
				copyData.heightmapResolution = m_defaultTerrainDataPrefab.heightmapResolution;
				copyData.SetAlphamaps(0, 0, m_defaultTerrainDataPrefab.GetAlphamaps(0,0, m_defaultTerrainDataPrefab.alphamapWidth, m_defaultTerrainDataPrefab.alphamapHeight));
				copyData.SetHeights(0, 0, m_defaultTerrainDataPrefab.GetHeights(0, 0, copyData.heightmapWidth, copyData.heightmapHeight));
				copyData.size = m_defaultTerrainDataPrefab.size;
				return copyData;
			}
			else
			{
				Debug.LogError("LE_GUI3dTerrain: GetDefaultTerrainDataDeepCopy: there is no default terrain assigned! Check that DefaultTerrainDataPrefab is not null before calling this function!");
				return null;
			}
		}

		private void ApplyDefaultTerrainData(TerrainData p_terrainData)
		{
			p_terrainData.hideFlags = m_defaultTerrainDataPrefab.hideFlags;
			p_terrainData.detailPrototypes = m_defaultTerrainDataPrefab.detailPrototypes;
			p_terrainData.treePrototypes = m_defaultTerrainDataPrefab.treePrototypes;
			p_terrainData.treeInstances = m_defaultTerrainDataPrefab.treeInstances;
			p_terrainData.wavingGrassAmount = m_defaultTerrainDataPrefab.wavingGrassAmount;
			p_terrainData.wavingGrassSpeed = m_defaultTerrainDataPrefab.wavingGrassSpeed;
			p_terrainData.wavingGrassStrength = m_defaultTerrainDataPrefab.wavingGrassStrength;
			p_terrainData.wavingGrassTint = m_defaultTerrainDataPrefab.wavingGrassTint;
			p_terrainData.baseMapResolution = m_defaultTerrainDataPrefab.baseMapResolution;
			p_terrainData.SetDetailResolution(m_defaultTerrainDataPrefab.detailResolution, 8); // hardcoded to 8 since variable is not public...
			if (m_defaultTerrainDataPrefab.detailResolution > 0)
			{
				int[] detailLayers = m_defaultTerrainDataPrefab.GetSupportedLayers(0, 0, m_defaultTerrainDataPrefab.detailWidth, m_defaultTerrainDataPrefab.detailHeight);
				for (int i = 0; i < detailLayers.Length; i++)
				{
					p_terrainData.SetDetailLayer(0, 0, detailLayers[i], m_defaultTerrainDataPrefab.GetDetailLayer(0, 0, m_defaultTerrainDataPrefab.detailWidth, m_defaultTerrainDataPrefab.detailHeight, detailLayers[i]));
				}
			}
		}

		private void Start()
		{
			if (m_brushProjector == null)
			{
				Debug.LogError("LE_GUI3dTerrain: m_brushProjector was not initialized!");
			}
			if (m_brushAlphaTexture == null)
			{
				Debug.LogError("LE_GUI3dTerrain: m_brushAlphaTexture was not initialized!");
			}
		}
		
		private void Update()
		{
			UpdateBrushProjector(IsCursorOverSomething, m_cursorHitInfo);
		}
		
		private void UpdateBrushProjector(bool p_isHit, RaycastHit p_hitInfo)
		{
			if (p_isHit && m_terrainMgr != null)
			{
				// make the brush projection fit the terrain data cell positions
				Vector3 terrainSize = m_terrainMgr.TerrainData.size;
				Vector2 relativeLocation = GetRelativeLocalLocation(p_hitInfo);
				BrushProjector.transform.position = new Vector3(
					p_hitInfo.transform.position.x + relativeLocation.y*terrainSize.x,
					p_hitInfo.point.y,
					p_hitInfo.transform.position.z + relativeLocation.x*terrainSize.z);
				// update brush size
				BrushProjector.orthographicSize = m_terrainMgr.TerrainData.size.z * m_size * 0.5f;
				// fit brush projection to terrain aspect ratio
				BrushProjector.aspectRatio = m_terrainMgr.TerrainData.size.x / m_terrainMgr.TerrainData.size.z;
				// activate brush projector
				if (BrushProjector.transform.childCount > 0)
				{
					BrushProjector.transform.GetChild(0).gameObject.SetActive(m_editMode == LE_ETerrainEditMode.SMOOTH_HEIGHT && IsDirectedSmooth);
				}
			}
			else
			{
				BrushProjector.orthographicSize = 0;
				if (BrushProjector.transform.childCount > 0)
				{
					BrushProjector.transform.GetChild(0).gameObject.SetActive(false);
				}
			}
		}
		
		private Vector2 GetRelativeLocalLocation(RaycastHit hit)	
		{
			// calculate relative position
			Terrain terrain = hit.transform.gameObject.GetComponent<Terrain>();
			Vector3 terrainSize = terrain.terrainData.size;
			Vector3 localHitPoint = hit.transform.InverseTransformPoint(hit.point);
			Vector2 relLocalLoc = Vector2.zero;
			relLocalLoc.x = localHitPoint.z / terrainSize.z;
			relLocalLoc.y = localHitPoint.x / terrainSize.x;
			// make the brush position fit the terrain data cell positions
			float cellSizeX = 1f/(float)(terrain.terrainData.heightmapWidth-1);
			float cellSizeY = 1f/(float)(terrain.terrainData.heightmapHeight-1);
			//relLocalLoc.x = Mathf.Round((relLocalLoc.x-0.5f*cellSizeX)/cellSizeX)*cellSizeX+0.5f*cellSizeX;
			//relLocalLoc.y = Mathf.Round((relLocalLoc.y-0.5f*cellSizeY)/cellSizeY)*cellSizeY+0.5f*cellSizeY;
			relLocalLoc.x = Mathf.Round(relLocalLoc.x/cellSizeX)*cellSizeX;
			relLocalLoc.y = Mathf.Round(relLocalLoc.y/cellSizeY)*cellSizeY;
			return relLocalLoc;
		}
	}
}
