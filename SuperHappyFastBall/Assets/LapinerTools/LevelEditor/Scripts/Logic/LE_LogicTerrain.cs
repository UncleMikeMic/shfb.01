using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LE_LevelEditor.UI;
using LE_LevelEditor.Events;
using LE_LevelEditor.Core;
using UndoRedo;
using LE_LevelEditor.Commands;

namespace LE_LevelEditor.Logic
{
	public class LE_LogicTerrain : LE_LogicBase
	{
		private LE_ConfigTerrain m_confT;
		private LE_GUI3dTerrain m_GUI3dTerrain;

		private TerrainData m_terrainDataToCreate = new TerrainData();
		private bool m_doCreateTerrain = false;
		private bool m_doRebuildTerrainTab = false;
		private int m_selectedTextureIndex = 0;
		private int m_selectedBrushIndex = 0;

		private bool m_isHeightRaise = true;

		public LE_LogicTerrain(LE_ConfigTerrain p_confTerrain, LE_GUI3dTerrain p_GUI3dTerrain)
		{
			m_confT = p_confTerrain;
			m_GUI3dTerrain = p_GUI3dTerrain;
			// init 3d UI
			m_GUI3dTerrain.TERRAIN_LAYER = p_confTerrain.TerrainLayer;
			if (p_confTerrain.BrushProjector != null)
			{
				m_GUI3dTerrain.BrushProjector = p_confTerrain.BrushProjector;
			}
			else
			{
				Debug.LogError("LE_GUILogicTerrain: LE_ConfigTerrain.BrushProjector was not initialized! You need to set it in the inspector.");
			}
			if (p_confTerrain.Brushes.Length > 0)
			{
				m_GUI3dTerrain.BrushAlphaTexture = p_confTerrain.Brushes[0];
			}
			else
			{
				Debug.LogError("LE_GUILogicTerrain: LE_ConfigTerrain.Brushes was not initialized! You need to set it in the inspector.");
			}
			// init further stuff
			CheckParameters();
			InitTerrainValues();
			// register to events
			LE_EventInterface.OnLoadedLevelInEditor += OnLoadedLevelInEditor;
			LE_GUIInterface.Instance.events.OnTerrainWidthChanged += OnTerrainWidthChanged;
			LE_GUIInterface.Instance.events.OnTerrainLengthChanged += OnTerrainLengthChanged;
			LE_GUIInterface.Instance.events.OnTerrainHeightChanged += OnTerrainHeightChanged;
			LE_GUIInterface.Instance.events.OnTerrainBaseTextureChanged += OnTerrainBaseTextureChanged;
			LE_GUIInterface.Instance.events.OnTerrainBrushChanged += OnTerrainBrushChanged;
			LE_GUIInterface.Instance.events.OnTerrainPaintTextureChanged += OnTerrainPaintTextureChanged;
			LE_GUIInterface.Instance.events.OnTerrainPaintTextureAdded += OnTerrainPaintTextureAdded;
			LE_GUIInterface.Instance.events.OnTerrainEditBrushSizeChanged += OnTerrainEditBrushSizeChanged;
			LE_GUIInterface.Instance.events.OnTerrainEditBrushAmountChanged += OnTerrainEditBrushAmountChanged;
			LE_GUIInterface.Instance.events.OnTerrainEditBrushTargetValueChanged += OnTerrainEditBrushTargetValueChanged;
			LE_GUIInterface.Instance.events.OnTerrainEditDirectionChanged += OnTerrainEditDirectionChanged;
			LE_GUIInterface.Instance.events.OnTerrainChangeHeightModeChanged += OnTerrainChangeHeightModeChanged;
			LE_GUIInterface.Instance.events.OnTerrainIsDirectedSmoothChanged += OnTerrainIsDirectedSmoothChanged;
			LE_GUIInterface.Instance.events.OnTerrainCreateBtn += OnTerrainCreateBtn;
			LE_GUIInterface.Instance.events.OnTerrainEditModeBtn += OnTerrainEditModeBtn;
			LE_GUIInterface.Instance.events.OnTerrainReadPaintHeightBtn += OnTerrainReadPaintHeightBtn;
		}

		public override void Destroy ()
		{
			// unregister from events
			LE_EventInterface.OnLoadedLevelInEditor -= OnLoadedLevelInEditor;
			if (LE_GUIInterface.Instance != null)
			{
				LE_GUIInterface.Instance.events.OnTerrainWidthChanged -= OnTerrainWidthChanged;
				LE_GUIInterface.Instance.events.OnTerrainLengthChanged -= OnTerrainLengthChanged;
				LE_GUIInterface.Instance.events.OnTerrainHeightChanged -= OnTerrainHeightChanged;
				LE_GUIInterface.Instance.events.OnTerrainBaseTextureChanged -= OnTerrainBaseTextureChanged;
				LE_GUIInterface.Instance.events.OnTerrainBrushChanged -= OnTerrainBrushChanged;
				LE_GUIInterface.Instance.events.OnTerrainPaintTextureChanged -= OnTerrainPaintTextureChanged;
				LE_GUIInterface.Instance.events.OnTerrainPaintTextureAdded -= OnTerrainPaintTextureAdded;
				LE_GUIInterface.Instance.events.OnTerrainEditBrushSizeChanged -= OnTerrainEditBrushSizeChanged;
				LE_GUIInterface.Instance.events.OnTerrainEditBrushAmountChanged -= OnTerrainEditBrushAmountChanged;
				LE_GUIInterface.Instance.events.OnTerrainEditBrushTargetValueChanged -= OnTerrainEditBrushTargetValueChanged;
				LE_GUIInterface.Instance.events.OnTerrainEditDirectionChanged -= OnTerrainEditDirectionChanged;
				LE_GUIInterface.Instance.events.OnTerrainChangeHeightModeChanged -= OnTerrainChangeHeightModeChanged;
				LE_GUIInterface.Instance.events.OnTerrainIsDirectedSmoothChanged -= OnTerrainIsDirectedSmoothChanged;
				LE_GUIInterface.Instance.events.OnTerrainCreateBtn -= OnTerrainCreateBtn;
				LE_GUIInterface.Instance.events.OnTerrainEditModeBtn -= OnTerrainEditModeBtn;
				LE_GUIInterface.Instance.events.OnTerrainReadPaintHeightBtn -= OnTerrainReadPaintHeightBtn;
			}
		}

		public override void Update()
		{
			// create terrain
			if (m_doCreateTerrain && m_terrainDataToCreate != null)
			{
				m_doCreateTerrain = false;
				UR_CommandMgr.Instance.Execute(new LE_CmdCreateTerrain(this, m_selectedTextureIndex, m_terrainDataToCreate));
				m_terrainDataToCreate = null;
			}

			// update terrain UI
			if (m_doRebuildTerrainTab)
			{
				m_doRebuildTerrainTab = false;
				// update UI values
				InitTerrainValues();
			}
		}

		public void CreateOrRecycleTerrainWithUIUpdate(TerrainData p_terrainData, int p_terrainBaseTextureIndex)
		{
			CreateOrRecycleTerrain(p_terrainData, m_GUI3dTerrain.TERRAIN_LAYER, m_confT, p_terrainBaseTextureIndex);
			m_terrainDataToCreate = null;
			m_doRebuildTerrainTab = true;
			SetTerrainUIMode(LE_GUIInterface.Delegates.ETerrainUIMode.EDIT);
		}

		public void DestroyOrResetTerrainWithUIUpdate()
		{
			DestroyOrResetTerrain();
			m_terrainDataToCreate = new TerrainData();
			m_doRebuildTerrainTab = true;
			SetTerrainUIMode(LE_GUIInterface.Delegates.ETerrainUIMode.CREATE);
		}

		public void AddTerrainTexture(Texture2D p_texture)
		{
			if (m_confT.TerrainTextureConfig != null)
			{
				if (m_GUI3dTerrain != null && m_GUI3dTerrain.TerrainManager != null && m_GUI3dTerrain.TerrainManager.TerrainData != null)
				{
					SplatPrototype[] splatTexturesOld = m_GUI3dTerrain.TerrainManager.TerrainData.splatPrototypes;
					SplatPrototype[] splatTexturesNew = new SplatPrototype[splatTexturesOld.Length+1];
					System.Array.Copy(splatTexturesOld, splatTexturesNew, splatTexturesOld.Length);
					int textureIndex = GetTextureIndex(p_texture);
					if (textureIndex >= 0)
					{
						splatTexturesNew[splatTexturesNew.Length-1] = new SplatPrototype();
						splatTexturesNew[splatTexturesNew.Length-1].texture = m_confT.TerrainTextureConfig.TERRAIN_TEXTURES[textureIndex];
						splatTexturesNew[splatTexturesNew.Length-1].tileSize = m_confT.TerrainTextureConfig.TERRAIN_TEXTURE_SIZES[textureIndex];
						splatTexturesNew[splatTexturesNew.Length-1].tileOffset = m_confT.TerrainTextureConfig.TERRAIN_TEXTURE_OFFSETS[textureIndex];
						m_GUI3dTerrain.TerrainManager.TerrainData.splatPrototypes = splatTexturesNew;
						m_doRebuildTerrainTab = true;
						// notify listeners that the level data was changed
						if (LE_EventInterface.OnChangeLevelData != null)
						{
							LE_EventInterface.OnChangeLevelData(this, new LE_LevelDataChangedEvent(LE_ELevelDataChangeType.TERRAIN_TEXTURES));
						}
					}
					else
					{
						Debug.LogError("LE_LogicTerrain: AddTerrainTexture: could not find given texture in TerrainTextureConfig!");
					}
				}
			}
			else
			{
				Debug.LogError("LE_LogicTerrain: AddTerrainTexture: LE_ConfigTerrain has no TerrainTextureConfig set!");
			}
		}

		public void RemoveTerrainTexture(Texture2D p_texture)
		{
			if (m_confT.TerrainTextureConfig != null)
			{
				if (m_GUI3dTerrain != null && m_GUI3dTerrain.TerrainManager != null && m_GUI3dTerrain.TerrainManager.TerrainData != null)
				{
					SplatPrototype[] splatTexturesOld = m_GUI3dTerrain.TerrainManager.TerrainData.splatPrototypes;
					if (splatTexturesOld.Length > 0)
					{
						bool isTexFound = false;
						SplatPrototype[] splatTexturesNew = new SplatPrototype[splatTexturesOld.Length-1];
						for (int i = 0; i < splatTexturesOld.Length; i++)
						{
							if (!isTexFound && splatTexturesOld[i] != null && splatTexturesOld[i].texture == p_texture)
							{
								isTexFound = true;
							}
							else
							{
								splatTexturesNew[isTexFound ? i-1 : i] = splatTexturesOld[i];
							}
						}
						if (isTexFound)
						{
							m_GUI3dTerrain.TerrainManager.TerrainData.splatPrototypes = splatTexturesNew;
							m_doRebuildTerrainTab = true;
							// notify listeners that the level data was changed
							if (LE_EventInterface.OnChangeLevelData != null)
							{
								LE_EventInterface.OnChangeLevelData(this, new LE_LevelDataChangedEvent(LE_ELevelDataChangeType.TERRAIN_TEXTURES));
							}
						}
						else
						{
							Debug.LogError("LE_LogicTerrain: RemoveTerrainTexture: given texture is not a splat texture of the terrain!");
						}
					}
					else
					{
						Debug.LogError("LE_LogicTerrain: RemoveTerrainTexture: terrain has no splat textures set!");
					}
				}
			}
			else
			{
				Debug.LogError("LE_LogicTerrain: RemoveTerrainTexture: LE_ConfigTerrain has no TerrainTextureConfig set!");
			}
		}

// STATIC LOGIC -------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Create a terrain from its terrain data. If the level has a terrain editor, then an existing terrain will be recycled.
		/// </summary>
		public static GameObject CreateOrRecycleTerrain(TerrainData p_terrainData, int p_terrainLayer)
		{
			// try to find an existing terrain
			LE_GUI3dTerrain gui3dTerrain = Object.FindObjectOfType<LE_GUI3dTerrain>();
			Terrain terrain = gui3dTerrain!=null ? gui3dTerrain.TerrainInstance : null;
			GameObject terrainGO;
			if (terrain != null)
			{
				// an editable terrain was already existent -> recycle it
				terrainGO = terrain.gameObject;
				gui3dTerrain.RecycleTerrain(p_terrainData, true);
			}
			else
			{
				// there is no editable terrain in this level -> create new terrain GO
				terrainGO = Terrain.CreateTerrainGameObject(p_terrainData);
				// assign terrain for further editing
				if (gui3dTerrain != null)
				{
					gui3dTerrain.SetTerrain(terrainGO.GetComponent<Terrain>());
				}
			}
			terrainGO.layer = p_terrainLayer;
			terrainGO.transform.position = new Vector3(-p_terrainData.size.x*0.5f, terrainGO.transform.position.y, -p_terrainData.size.z*0.5f);
			
			return terrainGO;
		}

		/// <summary>
		/// This will destroy the terrain instance used by the level editor. This function works only if the terrain editor exists in the scene.
		/// </summary>
		public static void DestroyOrResetTerrain()
		{
			// try to find an existing terrain
			LE_GUI3dTerrain gui3dTerrain = Object.FindObjectOfType<LE_GUI3dTerrain>();
			if (gui3dTerrain != null)
			{
				// if an editable terrain is existent, then it must be reset or destroyed
				gui3dTerrain.ResetToDefaultOrDestroyTerrain();
			}
		}

		/// <summary>
		/// Create a terrain from its terrain data. If the level has a terrain editor, then an existing terrain will be recycled.
		/// Apply defaults from terrain config and use a custom base texture.
		/// </summary>
		private static void CreateOrRecycleTerrain(TerrainData p_terrainData, int p_terrainLayer, LE_ConfigTerrain p_confT, int p_terrainBaseTextureIndex)
		{
			if (p_terrainData == null)
			{
				Debug.LogError("LE_LogicTerrain: CreateOrRecycleTerrain: passed terrain data was null!");
				return;
			}
			
			Vector3 size = p_terrainData.size;
			float maxSize = Mathf.Max(size.x, size.z);
			// set heightmap resolution depending on selected terrain size
			int heightmapResolution = p_confT.HeightmapResolutions[0];
			for (int i = 1; i < p_confT.HeightmapResolutionSizes.Length; i++)
			{
				if (maxSize >= p_confT.HeightmapResolutionSizes[i-1])
				{
					heightmapResolution = p_confT.HeightmapResolutions[i];
				}
				else
				{
					break;
				}
			}
			p_terrainData.heightmapResolution = heightmapResolution;
			// set alphamap resolution depending on selected terrain size
			int alphamapResolution = p_confT.AlphamapResolutions[0];
			for (int i = 1; i < p_confT.AlphamapResolutionSizes.Length; i++)
			{
				if (maxSize >= p_confT.AlphamapResolutionSizes[i-1])
				{
					alphamapResolution = p_confT.AlphamapResolutions[i];
				}
				else
				{
					break;
				}
			}
			p_terrainData.alphamapResolution = alphamapResolution;
			// restore size since it is changed when resolution is set
			p_terrainData.size = size;
			// set base texture in terrain data
			SplatPrototype[] baseTextureSet = new SplatPrototype[1];
			baseTextureSet[0] = new SplatPrototype();
			baseTextureSet[0].texture = p_confT.TerrainTextureConfig.TERRAIN_TEXTURES[p_terrainBaseTextureIndex];
			baseTextureSet[0].tileSize = p_confT.TerrainTextureConfig.TERRAIN_TEXTURE_SIZES[p_terrainBaseTextureIndex];
			baseTextureSet[0].tileOffset = p_confT.TerrainTextureConfig.TERRAIN_TEXTURE_OFFSETS[p_terrainBaseTextureIndex];
			p_terrainData.splatPrototypes = baseTextureSet;
			// create terrain GO
			GameObject terrainGO = CreateOrRecycleTerrain(p_terrainData, p_terrainLayer);
			
			// notify listeners that a terrain was created
			if (LE_EventInterface.OnTerrainCreated != null)
			{
				LE_EventInterface.OnTerrainCreated(terrainGO, new LE_TerrainCreatedEvent(terrainGO));
			}
		}

		private static void SetTerrainUIMode(LE_GUIInterface.Delegates.ETerrainUIMode p_uiMode)
		{
			if (LE_GUIInterface.Instance.delegates.SetTerrainUIMode != null)
			{
				LE_GUIInterface.Instance.delegates.SetTerrainUIMode(p_uiMode);
			}
			else
			{
				Debug.LogWarning("LE_LogicTerrain: SetTerrainUIMode: you have not set the LE_GUIInterface.delegates.SetTerrainUIMode delegate. You need to set it for example if you want to disable the create UI after terrain creation!");
			}
		}

// EVENT HANDLERS -----------------------------------------------------------------------------------------------------------------

		private void OnLoadedLevelInEditor(object p_obj, System.EventArgs p_args)
		{
			LE_GUIInterface.Delegates.ETerrainUIMode terrainUIMode = GetTerrainUIMode();
			if (terrainUIMode == LE_GUIInterface.Delegates.ETerrainUIMode.CREATE)
			{
				m_terrainDataToCreate = new TerrainData();
			}
			else
			{
				m_terrainDataToCreate = null;
			}
			if (LE_GUIInterface.Instance.delegates.SetTerrainUIMode != null)
			{
				LE_GUIInterface.Instance.delegates.SetTerrainUIMode(terrainUIMode);
			}
			else
			{
				Debug.LogWarning("LE_LogicTerrain: you have not set the LE_GUIInterface.delegates.SetTerrainUIMode delegate. Set it to show the create UI if the loaded level has no terrain and the edit UI if the loaded level has a terrain!");
			}
			m_doRebuildTerrainTab = true;
		}

		private void OnTerrainWidthChanged(object p_obj, LE_GUIInterface.EventHandlers.StringEventArgs p_args)
		{
			int inputValue;
			if (int.TryParse(p_args.Value, out inputValue))
			{
				inputValue = Mathf.Max(32, inputValue);
				if (m_terrainDataToCreate != null)
				{
					m_terrainDataToCreate.size = new Vector3(inputValue, m_terrainDataToCreate.size.y, m_terrainDataToCreate.size.z);
				}
				else
				{
					m_GUI3dTerrain.TerrainManager.TerrainData.size = new Vector3(inputValue, m_GUI3dTerrain.TerrainManager.TerrainData.size.y, m_GUI3dTerrain.TerrainManager.TerrainData.size.z);
				}
			}
		}

		private void OnTerrainLengthChanged(object p_obj, LE_GUIInterface.EventHandlers.StringEventArgs p_args)
		{
			int inputValue;
			if (int.TryParse(p_args.Value, out inputValue))
			{
				inputValue = Mathf.Max(32, inputValue);
				if (m_terrainDataToCreate != null)
				{
					m_terrainDataToCreate.size = new Vector3(m_terrainDataToCreate.size.x, m_terrainDataToCreate.size.y, inputValue);
				}
				else
				{
					m_GUI3dTerrain.TerrainManager.TerrainData.size = new Vector3(m_GUI3dTerrain.TerrainManager.TerrainData.size.x, m_GUI3dTerrain.TerrainManager.TerrainData.size.y, inputValue);
				}
			}
		}

		private void OnTerrainHeightChanged(object p_obj, LE_GUIInterface.EventHandlers.StringEventArgs p_args)
		{
			int inputValue;
			if (int.TryParse(p_args.Value, out inputValue))
			{
				inputValue = Mathf.Max(32, inputValue);
				if (m_terrainDataToCreate != null)
				{
					m_terrainDataToCreate.size = new Vector3(m_terrainDataToCreate.size.x, inputValue, m_terrainDataToCreate.size.z);
				}
				else
				{
					m_GUI3dTerrain.TerrainManager.TerrainData.size = new Vector3(m_GUI3dTerrain.TerrainManager.TerrainData.size.x, inputValue, m_GUI3dTerrain.TerrainManager.TerrainData.size.z);
				}
			}
		}

		private void OnTerrainBaseTextureChanged(object p_obj, LE_GUIInterface.EventHandlers.IntEventArgs p_args)
		{
			m_selectedTextureIndex = p_args.Value;
			if (m_terrainDataToCreate == null)
			{
				Debug.LogWarning("LE_LogicTerrain: OnTerrainBaseTextureChanged: terrain base texture cannot be changed after the terrain was created! If a terrain already exists then you have changed the terrain paint texture with this call.");
			}
		}

		private void OnTerrainBrushChanged(object p_obj, LE_GUIInterface.EventHandlers.IntEventArgs p_args)
		{
			// select brush
			m_selectedBrushIndex = p_args.Value;
			m_GUI3dTerrain.BrushAlphaTexture = m_confT.Brushes[m_selectedBrushIndex];
			// show image of the brush if possible
			m_GUI3dTerrain.SetCursorPosition(new Vector3(Screen.width/2,Screen.height/2,0));
		}

		private void OnTerrainPaintTextureChanged(object p_obj, LE_GUIInterface.EventHandlers.IntEventArgs p_args)
		{
			m_selectedTextureIndex = p_args.Value;
			m_GUI3dTerrain.SelectedSplatPrototype = m_selectedTextureIndex;
		}

		private void OnTerrainPaintTextureAdded(object p_obj, LE_GUIInterface.EventHandlers.TextureEventArgs p_args)
		{
			UR_CommandMgr.Instance.Execute(new LE_CmdAddTerrainTexture(this, p_args.Value));
		}

		private void OnTerrainEditBrushSizeChanged(object p_obj, LE_GUIInterface.EventHandlers.FloatEventArgs p_args)
		{
			float newValue = p_args.Value;
			newValue *= newValue; // size increases exponentially (=> slow)
			newValue = Mathf.Max(0.002f, newValue); // size should be never zero
			m_GUI3dTerrain.Size = newValue;
			// show size of the brush if possible
			m_GUI3dTerrain.SetCursorPosition(new Vector3(Screen.width/2,Screen.height/2,0));
		}

		private void OnTerrainEditBrushAmountChanged(object p_obj, LE_GUIInterface.EventHandlers.FloatEventArgs p_args)
		{
			float newValue = p_args.Value;
			newValue = Mathf.Max(0.002f, newValue); // amount should be never zero
			m_GUI3dTerrain.Amount = m_isHeightRaise ? newValue : -newValue;
		}

		private void OnTerrainEditBrushTargetValueChanged(object p_obj, LE_GUIInterface.EventHandlers.FloatEventArgs p_args)
		{
			m_GUI3dTerrain.TargetRelativeValue = p_args.Value;
		}

		private void OnTerrainEditDirectionChanged(object p_obj, LE_GUIInterface.EventHandlers.FloatEventArgs p_args)
		{
			// set directed smooth angle (only 22.5° steps are working effective)
			m_GUI3dTerrain.DirectedSmoothAngle = Mathf.Round(p_args.Value * 360f/22.5f)*22.5f;
			// show direction if possible
			m_GUI3dTerrain.SetCursorPosition(new Vector3(Screen.width/2,Screen.height/2,0));
		}

		private void OnTerrainChangeHeightModeChanged(object p_obj, LE_GUIInterface.EventHandlers.TerrainChangeHeightModeEventArgs p_args)
		{
			if (p_args.ChangeHeightMode == LE_GUIInterface.EventHandlers.ETerrainChangeHeightMode.RAISE)
			{
				m_isHeightRaise = true;
				m_GUI3dTerrain.Amount = Mathf.Abs(m_GUI3dTerrain.Amount);
			}
			else
			{
				m_isHeightRaise = false;
				m_GUI3dTerrain.Amount = -Mathf.Abs(m_GUI3dTerrain.Amount);
			}
		}

		private void OnTerrainIsDirectedSmoothChanged(object p_obj, LE_GUIInterface.EventHandlers.BoolEventArgs p_args)
		{
			// set directed smooth flag
			m_GUI3dTerrain.IsDirectedSmooth = p_args.Value;
			// show direction if possible
			m_GUI3dTerrain.SetCursorPosition(new Vector3(Screen.width/2,Screen.height/2,0));
		}

		private void OnTerrainCreateBtn(object p_obj, System.EventArgs p_args)
		{
			m_doCreateTerrain = true;
			m_doRebuildTerrainTab = true;
		}

		private void OnTerrainEditModeBtn(object p_obj, LE_GUIInterface.EventHandlers.TerrainEditModeEventArgs p_args)
		{
			m_GUI3dTerrain.EditMode = p_args.EditMode;
		}

		private void OnTerrainReadPaintHeightBtn (object sender, System.EventArgs e)
		{
			// enable terrain paint height read UI
			if (LE_GUIInterface.Instance.delegates.SetTerrainIsReadingPaintHeight != null)
			{
				LE_GUIInterface.Instance.delegates.SetTerrainIsReadingPaintHeight(true);
			}
			else
			{
				Debug.LogWarning("LE_LogicTerrain: OnTerrainReadPaintHeightBtn: you have not set the LE_GUIInterface.delegates.SetTerrainIsReadingPaintHeight delegate. Set it if you want to give visual feedback to the user showing if the terrain paint height is currently being read!");
			}
			m_GUI3dTerrain.IsReadingTerrainPaintHeight = true;
			m_GUI3dTerrain.StartCoroutine(ReadPaintHeightRoutine());
		}

// LOGIC --------------------------------------------------------------------------------------------------------------------------

		private void CheckParameters()
		{
			if (string.IsNullOrEmpty(LayerMask.LayerToName(m_confT.TerrainLayer)))
			{
				Debug.LogWarning("LE_GUILogicTerrain: Inspector property 'LE_ConfigTerrain.TerrainLayer' is set to '"+m_confT.TerrainLayer+"', but this layer has no name in 'Tags & Layers' set. Please set the name of this layer to 'Terrain' or change the value of the 'LE_ConfigTerrain.TerrainLayer' property! To open the 'Tags & Layers' manager select any game object, in the inspector click on the layer drop down at top right then click on 'Add Layer...'.");
			}
			else if (LayerMask.LayerToName(m_confT.TerrainLayer) != "Terrain")
			{
				Debug.LogWarning("LE_GUILogicTerrain: Inspector property 'LE_ConfigTerrain.TerrainLayer' is set to '"+m_confT.TerrainLayer+"', but the name of this layer is '" + LayerMask.LayerToName(m_confT.TerrainLayer) + "'. Is this intended? If not please set the name of this layer to 'Terrain' or change the value of the 'LE_ConfigTerrain.TerrainLayer' property! To open the 'Tags & Layers' manager select any game object, in the inspector click on the layer drop down at top right then click on 'Add Layer...'.");
			}
			if (m_confT.HeightmapResolutions == null || m_confT.HeightmapResolutions.Length == 0)
			{
				Debug.LogError("LE_GUILogicTerrain: LE_ConfigTerrain: TerrainHeightmapResolutions is null or empty! Add supported heightmap resolutions e.g. {65, 129, 257, 513} etc.");
			}
			else if (m_confT.HeightmapResolutionSizes == null || m_confT.HeightmapResolutionSizes.Length == 0)
			{
				Debug.LogError("LE_GUILogicTerrain: LE_ConfigTerrain: TerrainHeightmapResolutionSizes is null or empty! Add the terrain size thresholds that define which of the supported heightmap resolutions is used e.g. TerrainHeightmapResolutions = {65, 129, 257, 513} and TerrainHeightmapResolutionSizes = {125, 250, 500, 9999} etc.");
			}
			else if (m_confT.HeightmapResolutions.Length != m_confT.HeightmapResolutionSizes.Length)
			{
				Debug.LogError("LE_GUILogicTerrain: LE_ConfigTerrain: TerrainHeightmapResolutions and TerrainHeightmapResolutionSizes must have the same length!");
			}
			if (m_confT.AlphamapResolutions == null || m_confT.AlphamapResolutions.Length == 0)
			{
				Debug.LogError("LE_GUILogicTerrain: LE_ConfigTerrain: TerrainAlphamapResolutions is null or empty! Add supported alphamap resolutions e.g. {64, 128, 256, 512} etc.");
			}
			else if (m_confT.AlphamapResolutionSizes == null || m_confT.AlphamapResolutionSizes.Length == 0)
			{
				Debug.LogError("LE_GUILogicTerrain: LE_ConfigTerrain: TerrainAlphamapResolutionSizes is null or empty! Add the terrain size thresholds that define which of the supported alphamap resolutions is used e.g. TerrainAlphamapResolutions = {64, 128, 256, 512} and TerrainAlphamapResolutionSizes = {125, 250, 500, 9999} etc.");
			}
			else if (m_confT.AlphamapResolutions.Length != m_confT.AlphamapResolutionSizes.Length)
			{
				Debug.LogError("LE_GUILogicTerrain: LE_ConfigTerrain: TerrainAlphamapResolutions and TerrainAlphamapResolutionSizes must have the same length!");
			}
		}

		private void InitTerrainValues()
		{
			// init width, length and height and pass it to UI
			int width, length, height;
			if (m_terrainDataToCreate != null)
			{
				width = m_confT.InitialWidth;
				length = m_confT.InitialLength;
				height = m_confT.InitialHeight;
				m_terrainDataToCreate.size = new Vector3(width, height, length);
			}
			else
			{
				width = (int)m_GUI3dTerrain.TerrainManager.TerrainData.size.x;
				length = (int)m_GUI3dTerrain.TerrainManager.TerrainData.size.z;
				height = (int)m_GUI3dTerrain.TerrainManager.TerrainData.size.y;
			}
			string errorMessage = "LE_GUILogicTerrain: InitTerrainValues: you have not provided {0} delegate. If you show the terrain size values then the real terrain size might be different than shown in your UI!";
			if (LE_GUIInterface.Instance.delegates.SetTerrainWidth != null) { LE_GUIInterface.Instance.delegates.SetTerrainWidth(width); }
			else { Debug.LogWarning(string.Format(errorMessage, "LE_GUIInterface.delegates.SetTerrainWidth")); }

			if (LE_GUIInterface.Instance.delegates.SetTerrainLength != null) { LE_GUIInterface.Instance.delegates.SetTerrainLength(length); }
			else { Debug.LogWarning(string.Format(errorMessage, "LE_GUIInterface.delegates.SetTerrainLength")); }

			if (LE_GUIInterface.Instance.delegates.SetTerrainHeight != null) { LE_GUIInterface.Instance.delegates.SetTerrainHeight(height); }
			else { Debug.LogWarning(string.Format(errorMessage, "LE_GUIInterface.delegates.SetTerrainHeight")); }

			// init terrain base textures and pass them to UI
			m_selectedTextureIndex = 0;
			if (m_confT.IsBaseTextureSelection)
			{
				if (LE_GUIInterface.Instance.delegates.SetTerrainBaseTextures != null)
				{
					LE_GUIInterface.Instance.delegates.SetTerrainBaseTextures(m_confT.TerrainTextureConfig.TERRAIN_TEXTURES, m_selectedTextureIndex);
				}
				else
				{
					Debug.LogError("LE_GUILogicTerrain: InitTerrainValues: you have not provided the LE_GUIInterface.delegates.SetTerrainBaseTextures delegate. If you want to use LE_ConfigTerrain.IsBaseTextureSelection==true, then you must set this delegate to show and update the base texture selection in your UI!");
				}
			}

			// init terrain brush textures and pass them to UI
			m_selectedBrushIndex = 0;
			if (m_confT.Brushes.Length > 0)
			{
				m_GUI3dTerrain.BrushAlphaTexture = m_confT.Brushes[0];
			}
			if (LE_GUIInterface.Instance.delegates.SetTerrainBrushes != null)
			{
				LE_GUIInterface.Instance.delegates.SetTerrainBrushes(m_confT.Brushes, m_selectedBrushIndex);
			}
			else
			{
				Debug.LogError("LE_GUILogicTerrain: InitTerrainValues: you have not provided the LE_GUIInterface.delegates.SetTerrainBrushes delegate. You must set this delegate to show and update the brush texture selection in your UI!");
			}

			// init terrain brush size slider value
			if (LE_GUIInterface.Instance.delegates.SetTerrainEditBrushSize != null)
			{
				LE_GUIInterface.Instance.delegates.SetTerrainEditBrushSize(Mathf.Sqrt(m_GUI3dTerrain.Size));
			}
			else
			{
				Debug.LogWarning("LE_GUILogicTerrain: InitTerrainValues: you have not provided the LE_GUIInterface.delegates.SetTerrainEditBrushSize delegate. If you have a brush size slider, then its value might be different from the value used by the level editor.");
			}

			// init terrain brush amount slider value
			if (LE_GUIInterface.Instance.delegates.SetTerrainEditBrushAmount != null)
			{
				LE_GUIInterface.Instance.delegates.SetTerrainEditBrushAmount(Mathf.Abs(m_GUI3dTerrain.Amount));
			}
			else
			{
				Debug.LogWarning("LE_GUILogicTerrain: InitTerrainValues: you have not provided the LE_GUIInterface.delegates.SetTerrainEditBrushAmount delegate. If you have a brush amount slider, then its value might be different from the value used by the level editor.");
			}

			// init terrain brush target value slider value
			if (LE_GUIInterface.Instance.delegates.SetTerrainEditBrushTargetValue != null)
			{
				LE_GUIInterface.Instance.delegates.SetTerrainEditBrushTargetValue(m_GUI3dTerrain.TargetRelativeValue);
			}
			else
			{
				Debug.LogWarning("LE_GUILogicTerrain: InitTerrainValues: you have not provided the LE_GUIInterface.delegates.SetTerrainEditBrushTargetValue delegate. If you have a brush target value slider, then its value might be different from the value used by the level editor.");
			}

			// update currently used paint textures of the terrain
			Texture2D[] textures = GetTerrainPaintTextures();
			bool isTextureLimitReached = textures.Length >= m_confT.TerrainTextureConfig.TERRAIN_TEXTURES.Length || textures.Length >= m_confT.MaxTextureCount;
			m_selectedTextureIndex = Mathf.Max(0, textures.Length-1);
			m_GUI3dTerrain.SelectedSplatPrototype = m_selectedTextureIndex;
			if (LE_GUIInterface.Instance.delegates.SetTerrainPaintTextures != null)
			{
				LE_GUIInterface.Instance.delegates.SetTerrainPaintTextures(textures, GetUnusedTerrainTextures(), m_selectedTextureIndex, !isTextureLimitReached);
			}
			else
			{
				Debug.LogWarning("LE_LogicTerrain: InitTerrainValues: you have not set the LE_GUIInterface.delegates.SetTerrainPaintTextures delegate. You need to set it to visualize the available paint textures and allow the user to select one of them!");
			}
		}

		private LE_GUIInterface.Delegates.ETerrainUIMode GetTerrainUIMode()
		{
			if (m_GUI3dTerrain.TerrainManager != null)
			{
				return LE_GUIInterface.Delegates.ETerrainUIMode.EDIT;
			}
			else
			{
				return LE_GUIInterface.Delegates.ETerrainUIMode.CREATE;
			}
		}

		private Texture2D[] GetTerrainPaintTextures()
		{
			if (m_GUI3dTerrain.TerrainManager != null && m_GUI3dTerrain.TerrainManager.TerrainData != null)
			{
				SplatPrototype[] splatTextures = m_GUI3dTerrain.TerrainManager.TerrainData.splatPrototypes;
				Texture2D[] usedTextures = new Texture2D[splatTextures.Length];
				for (int i = 0; i < splatTextures.Length; i++)
				{
					usedTextures[i] = splatTextures[i].texture;
				}
				return usedTextures;
			}
			else
			{
				return new Texture2D[0];
			}
		}

		private int GetTextureIndex(Texture2D p_texture)
		{
			for (int i = 0; i < m_confT.TerrainTextureConfig.TERRAIN_TEXTURES.Length; i++)
			{
				if (p_texture == m_confT.TerrainTextureConfig.TERRAIN_TEXTURES[i])
				{
					return i;
				}
			}
			return -1;
		}

		private IEnumerator ReadPaintHeightRoutine()
		{
			WaitForEndOfFrame wait = new WaitForEndOfFrame();
			// read terrain paint height
			while(m_GUI3dTerrain.IsReadingTerrainPaintHeight)
			{
				// update terrain paint height
				if (LE_GUIInterface.Instance.delegates.SetTerrainEditBrushTargetValue != null)
				{
					LE_GUIInterface.Instance.delegates.SetTerrainEditBrushTargetValue(m_GUI3dTerrain.TargetRelativeValue);
				}
				yield return wait;
			}
			// disable terrain paint height read UI
			if (LE_GUIInterface.Instance.delegates.SetTerrainIsReadingPaintHeight != null)
			{
				LE_GUIInterface.Instance.delegates.SetTerrainIsReadingPaintHeight(false);
			}
			else
			{
				Debug.LogWarning("LE_LogicTerrain: OnTerrainReadPaintHeightBtn: you have not set the LE_GUIInterface.delegates.SetTerrainIsReadingPaintHeight delegate. Set it if you want to give visual feedback to the user showing if the terrain paint height is currently being read!");
			}
			// set final terrain paint height
			if (LE_GUIInterface.Instance.delegates.SetTerrainEditBrushTargetValue != null)
			{
				LE_GUIInterface.Instance.delegates.SetTerrainEditBrushTargetValue(m_GUI3dTerrain.TargetRelativeValue);
			}
			else
			{
				Debug.LogWarning("LE_GUILogicTerrain: ReadPaintHeightRoutine: you have not provided the LE_GUIInterface.delegates.SetTerrainEditBrushTargetValue delegate. If you have a brush target value slider, then its value is now different from the value used by the level editor, because the terrain paint height was read.");
			}
		}

		private Texture2D[] GetUnusedTerrainTextures()
		{
			if (m_confT.TerrainTextureConfig != null)
			{
				if (m_GUI3dTerrain != null && m_GUI3dTerrain.TerrainManager != null && m_GUI3dTerrain.TerrainManager.TerrainData != null)
				{
					Texture2D[] configTextures = m_confT.TerrainTextureConfig.TERRAIN_TEXTURES;
					SplatPrototype[] usedSplatTextures = m_GUI3dTerrain.TerrainManager.TerrainData.splatPrototypes;
					List<Texture2D> unusedTexturesList = new List<Texture2D>();
					for (int i = 0; i < configTextures.Length; i++)
					{
						bool isUsed = false;
						for (int j = 0; j < usedSplatTextures.Length; j++)
						{
							if (usedSplatTextures[j].texture == configTextures[i])
							{
								isUsed = true;
								break;
							}
						}
						if (!isUsed)
						{
							unusedTexturesList.Add(configTextures[i]);
						}
					}
					return unusedTexturesList.ToArray();
				}
				else
				{
					return m_confT.TerrainTextureConfig.TERRAIN_TEXTURES;
				}
			}
			else
			{
				Debug.LogError("LE_LogicTerrain: GetUnusedTerrainTextures: LE_ConfigTerrain has no TerrainTextureConfig set!");
				return new Texture2D[0];
			}
		}
	}
}