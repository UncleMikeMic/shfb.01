using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CPG_CameraPerspective;
using LE_LevelEditor.UI;
using LE_LevelEditor.Logic;
using LE_LevelEditor.Events;
using LE_LevelEditor.Core;
using LE_LevelEditor.LEInput;
using UndoRedo;

namespace LE_LevelEditor
{
	public class LE_LevelEditorMain : MonoBehaviour, LE_IInputHandler
	{
		// singleton
		private static LE_LevelEditorMain s_instance = null;
		public static LE_LevelEditorMain Instance
		{
			get
			{
				if (s_instance == null)
				{
					s_instance = FindObjectOfType<LE_LevelEditorMain>();
				}
				return s_instance;
			}
		}

		[SerializeField, Tooltip(
			"Show/hide camera perspective gizmo. Works like the built-in Unity Editor scene view handle: click on the axes " +
			"to change camera's position and direction; click in the middle to toggle between perspective and orthographic camera view.")]
		private bool IS_WITH_CAMERA_PERSPECTIVE_GIZMO = true;

		[SerializeField, Tooltip("Camera perspective gizmo's render layer index. This layer should be excluded from the main camera.")]
		private int CAMERA_PERSPECTIVE_GIZMO_LAYER = 29;

		[SerializeField, Tooltip("Root object map shown in the object editor's tree browser.")]
		private LE_ObjectMap ROOT_OBJECT_MAP = null;

		[SerializeField, Tooltip(
			"Enable to bring focused objects to the center of the visible screen (not hidden by right menu). " +
			"On low resolution devices the right menu could use half of the screen. Without the oblique projection only the " +
			"left half of a big object would be visible, because the right half would be behind the menu.")]
		private bool IS_OBLIQUE_FOCUS_CENTERING = true;

		[SerializeField, Tooltip("If enabled, then the camera will be controlled by the level editor as described in the help popup.")]
		private bool IS_CAMERA_MOVEMENT = true;

		[SerializeField, Tooltip("If enabled, then the terrain editor logic will be initialized and terrain related events handled.")]
		private bool IS_TERRAIN_EDITOR = true;

		[SerializeField, Tooltip("If enabled, then the object editor logic will be initialized and object related events handled.")]
		private bool IS_OBJECT_EDITOR = true;

		[SerializeField, Tooltip("Amount of memory that can be used for the undo/redo history in MB (mega byte).")]
		private int UNDO_REDO_MEMORY_LIMIT = 256;

		[SerializeField, LE_EnumFlagsAttribute("Flag enum containing activated key combinations. For example if DUPLICATE is picked then objects will be cloned when Ctrl+D is typed.")]
		private LE_EKeyCombo ACTIVE_KEY_COMBOS = LE_EKeyCombo.DUPLICATE | LE_EKeyCombo.FOCUS | LE_EKeyCombo.UNDO | LE_EKeyCombo.REDO;

		private LE_EEditMode m_editMode = LE_EEditMode.TERRAIN;
		public LE_EEditMode EditMode { get { return m_editMode; } }

		private List<LE_GUI3dBase> m_GUI3d = new List<LE_GUI3dBase>();
		private LE_GUI3dTerrain m_GUI3dTerrain = null;
		private LE_GUI3dObject m_GUI3dObject = null;
		private List<LE_LogicBase> m_logic = new List<LE_LogicBase>();
		private LE_LogicTerrain m_logicTerrain = null;
		private LE_LogicLevel m_logicLevel = null;
		
		private LE_Input m_input = null;
		private CPG_CameraPerspectiveGizmo m_cameraPerspectiveGizmo = null;

		private Vector3 m_camPivot = Vector3.zero;
		public Vector3 CamPivot
		{
			get{ return m_camPivot; }
			set{ m_camPivot = value; }
		}

		private Camera m_cam = null;
		private Camera Cam
		{
			get
			{
				if (m_cam == null)
				{
					m_cam = Camera.main;
				}
				return m_cam;
			}
		}

		// guestures are sometimes applied when no touches are detected anymore
		// if the GUI was touched this variable is used to filter out taps that
		// would cause cursor activation behind the GUI after all touches are removed
		private int m_lastGUITouchFrame = -100;

		private Rect m_perspectiveGizmoRect;

		private bool m_isFirstUpdateInitialized = false;
		private bool m_isSecondUpdateInitialized = false;
		public bool IsReady { get{ return m_isFirstUpdateInitialized; } } // init in second update are not important

		private System.Action m_executeWhenReady;
		public void ExecuteWhenReady(System.Action p_action) { m_executeWhenReady += p_action; }

		private int m_lastChangeFrame = -1;
		public int LastChangeFrame { get{ return m_lastChangeFrame; } }

		/// <summary>
		/// Call to load a level into the level editor. Use the callbacks in the returned event args to start loading from byte arrays.
		/// Use if loading is needed without using the load button. EXECUTE ONLY IF 'IsReady' RETURNS TRUE or inside 'ExecuteWhenReady' callbacks. Learn more:
		/// http://www.freebord-game.com/index.php/multiplatform-runtime-level-editor/documentation/load
		/// </summary>
		public LE_LoadEvent GetLoadEvent()
		{
			if (m_logicLevel != null)
			{
				return m_logicLevel.GetLoadEvent();
			}
			else
			{
				Debug.LogError("LE_LevelEditorMain: GetLoadEvent: you have called this function before m_logicLevel was initialized! Check if editor is initialized with 'IsReady' or use the 'ExecuteWhenReady' function.");
				return null;
			}
		}

		public static bool SetObliqueFocusProjectionMatrix(bool p_isObliqueProjectionEnabled)
		{
			if (LE_GUIInterface.Instance.delegates.GetObliqueCameraPerspectiveRightPixelOffset != null)
			{
				if (p_isObliqueProjectionEnabled)
				{
					Camera.main.ResetProjectionMatrix();
					Matrix4x4 mat  = Camera.main.projectionMatrix;
					mat[0, 2] = LE_GUIInterface.Instance.delegates.GetObliqueCameraPerspectiveRightPixelOffset() / (float)Screen.width;
					Camera.main.projectionMatrix = mat;
				}
				else
				{
					Camera.main.ResetProjectionMatrix();
				}
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool IsUndoable { get{ return UR_CommandMgr.Instance.IsUndoable; } }
		public void Undo()
		{
			UR_CommandMgr.Instance.Undo();
		}

		public bool IsRedoable { get{ return UR_CommandMgr.Instance.IsRedoable; } }
		public void Redo()
		{
			UR_CommandMgr.Instance.Redo();
		}

		void LE_IInputHandler.SetCursorPosition(Vector3 p_cursorScreenCoords)
		{
			for (int i = 0; i < m_GUI3d.Count; i++)
			{
				if (m_GUI3d[i].ActiveEditMode == m_editMode && m_GUI3d[i].IsInteractable)
				{
					m_GUI3d[i].SetCursorPosition(p_cursorScreenCoords);
				}
			}
		}
		
		void LE_IInputHandler.SetIsCursorAction(bool p_isCursorAction)
		{
			for (int i = 0; i < m_GUI3d.Count; i++)
			{
				if (m_GUI3d[i].ActiveEditMode == m_editMode)
				{
					m_GUI3d[i].SetIsCursorAction(p_isCursorAction);
				}
			}
		}
		
		void LE_IInputHandler.MoveCamera(Vector3 p_fromScreenCoords, Vector3 p_toScreenCoords)
		{
			if (!IS_CAMERA_MOVEMENT) { return; }

			Camera cam = Cam;
			if (cam != null)
			{
				float camDist = EstimateDistanceToLevel(EEstimateDistanceMode.AVERAGE);

				Vector3 camMove;
				Vector3 dir = p_toScreenCoords - p_fromScreenCoords;
				dir = cam.transform.TransformDirection(dir);
				if (cam.orthographic)
				{
					float forwardDir = Vector3.Dot(dir, cam.transform.forward);
					dir -= cam.transform.forward*forwardDir;
					cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - forwardDir * (cam.orthographicSize / Screen.width), 5, 1000);
					camMove = dir * (cam.orthographicSize / Screen.width);
				}
				else
				{
					camMove = dir * (camDist / Screen.width);
				}
				cam.transform.position += camMove;
			}
		}
		
		void LE_IInputHandler.RotateCamera(Vector3 p_fromScreenCoords, Vector3 p_toScreenCoords)
		{
			if (!IS_CAMERA_MOVEMENT) { return; }

			// create two rays starting from the given screen coordinates
			Camera cam = Cam;
			if (cam != null)
			{
				Ray fromRay = cam.ScreenPointToRay(p_fromScreenCoords);
				Ray toRay = cam.ScreenPointToRay(p_toScreenCoords);
				if (cam.orthographic)
				{
					fromRay.direction = (fromRay.origin+fromRay.direction*100f - cam.transform.position).normalized;
					toRay.direction = (toRay.origin+toRay.direction*100f - cam.transform.position).normalized;
				}
				float angle = Vector3.Angle(fromRay.direction, toRay.direction);
				Vector3 axis = Vector3.Cross(fromRay.direction, toRay.direction).normalized;
				// slow down rotation if camera looks straight down or up
				float factor = Mathf.Abs(cam.transform.forward.y);
				const float MAX_FACTOR = 0.9f;
				if (factor > MAX_FACTOR)
				{
					factor = (1f-factor)/(1f-MAX_FACTOR);
					factor = factor*factor;
					float upDownRelAmount = Vector3.Dot(axis, cam.transform.right);
					axis -= (1f-factor)*(axis-upDownRelAmount*cam.transform.right);
				}
				// rotate the camera by the angle of the created rays
				cam.transform.Rotate(axis, -angle, Space.World);
				// keep the camera up direction
				if (cam.transform.up.y > 0)
				{
					cam.transform.LookAt(cam.transform.position + cam.transform.forward, Vector3.up);
				}
				else
				{
					cam.transform.LookAt(cam.transform.position + cam.transform.forward, Vector3.down);
				}
			}
		}

		void LE_IInputHandler.RotateCameraAroundPivot(Vector3 p_fromScreenCoords, Vector3 p_toScreenCoords)
		{
			if (!IS_CAMERA_MOVEMENT) { return; }

			Camera cam = Cam;
			if (cam != null)
			{
				const float LOOK_AROUND_ROTATION_SPEED = 300f;
				Vector2 relativeScreenDist = new Vector2(p_toScreenCoords.x, p_toScreenCoords.y) - new Vector2(p_fromScreenCoords.x, p_fromScreenCoords.y);
				relativeScreenDist.x /= (float)Screen.width;
				relativeScreenDist.y /= (float)Screen.height;
				cam.transform.RotateAround(m_camPivot, cam.transform.right, relativeScreenDist.y*LOOK_AROUND_ROTATION_SPEED);
				cam.transform.RotateAround(m_camPivot, cam.transform.up, -relativeScreenDist.x*LOOK_AROUND_ROTATION_SPEED);
				if (cam.transform.up.y > 0)
				{
					cam.transform.LookAt(m_camPivot, Vector3.up);
				}
				else
				{
					cam.transform.LookAt(m_camPivot, Vector3.down);
				}
			}
		}

		private void Start ()
		{
			// check if there is an instance of the LE_GUIInterface, which is required for the editor to work
			if (LE_GUIInterface.Instance == null)
			{
				Debug.LogError("LE_LevelEditorMain: a LE_GUIInterface object must be added to the scene!");
			}

			// search or create a default(will be not functional!) config
			LE_ConfigTerrain LEConfTerrain = GetComponentInChildren<LE_ConfigTerrain>();
			if (LEConfTerrain == null)
			{
				Debug.LogError("LE_LevelEditorMain: a LE_ConfigTerrain component must be added to the game object with the LE_LevelEditorMain script!");
				LEConfTerrain = gameObject.AddComponent<LE_ConfigTerrain>();
			}
			// check if everything is setup right
			if (LEConfTerrain.TerrainTextureConfig == null)
			{
				Debug.LogError("LE_LevelEditorMain: TerrainTextureConfig was not initialized! You need to set it in the inspector of LE_ConfigTerrain. Provide an empty terrain texture config if you do not want to use the terrain editor.");
			}
			if (!IS_TERRAIN_EDITOR)
			{
				if (LEConfTerrain.Brushes.Length > 0)
				{
					Debug.LogWarning("LE_LevelEditorMain: IS_TERRAIN_EDITOR is set to 'false', but you have provided a non empty array for LE_ConfigTerrain.Brushes! This is performance waist and will increase loading time! Remove brushes or reenable terrain editing.");
				}
				if (LEConfTerrain.BrushProjector != null)
				{
					Debug.LogWarning("LE_LevelEditorMain: IS_TERRAIN_EDITOR is set to 'false', but you have provided a value for LE_ConfigTerrain.BrushProjector! This is performance waist and will increase loading time! Remove brush projector from scene or reenable terrain editing.");
				}
			}

			Camera cam = Cam;
			if (cam == null)
			{
				Debug.LogError("LE_LevelEditorMain: Start: could not find main camera!");
			}

			// initialize command manager (undo/redo)
			InitializeCommandManager();

			// initialize object 3d GUI
			if (IS_OBJECT_EDITOR)
			{
				m_GUI3dObject = gameObject.AddComponent<LE_GUI3dObject>();
				m_GUI3dObject.TERRAIN_LAYER = LEConfTerrain.TerrainLayer;
				m_GUI3dObject.OnFocused += OnGUI3dObjectFocused;
				m_GUI3dObject.IsKeyComboFocus = (ACTIVE_KEY_COMBOS & LE_EKeyCombo.FOCUS) != 0;
				m_GUI3dObject.IsKeyComboDuplicate = (ACTIVE_KEY_COMBOS & LE_EKeyCombo.DUPLICATE) != 0;
				m_GUI3d.Add(m_GUI3dObject);
			}

			// initialize input
			m_input = new LE_Input(this);

			// set pivot point
			m_camPivot = cam.transform.position + cam.transform.forward * 100f;

			// to monitor the last change frame number register to the OnChangeLevelData event
			LE_EventInterface.OnChangeLevelData += OnChangeLevelData;

			// register to UI events
			LE_GUIInterface.Instance.events.OnEditModeBtn += OnEditModeBtn;
			LE_GUIInterface.Instance.events.OnUndoBtn += OnUndoBtn;
			LE_GUIInterface.Instance.events.OnRedoBtn += OnRedoBtn;
		}

		// some of the content in this method must be called in the first update and cannot be initialized in the start function
		private void Initialize_InFirstUpdate()
		{
			LE_ConfigLevel LEConfLevel = GetComponentInChildren<LE_ConfigLevel>();
			if (LEConfLevel == null)
			{
				Debug.LogError("LE_LevelEditorMain: a LE_ConfigLevel component must be added to the game object with the LE_LevelEditorMain script!");
				LEConfLevel = gameObject.AddComponent<LE_ConfigLevel>();
			}

			Camera cam = Cam;
			LE_ConfigTerrain LEConfTerrain = GetComponentInChildren<LE_ConfigTerrain>();

			// check further parameters that have been set in Start of other scripts
			if (LE_GUIInterface.Instance.delegates.IsCursorOverUI == null)
			{
				Debug.LogError("LE_LevelEditorMain: you have not setup LE_GUIInterface.delegates.IsCursorOverUI. Terrain might be edited behind UI while the user interacts with the UI, same is true for object placement!");
			}

			// initialize terrain logic
			if (IS_TERRAIN_EDITOR)
			{
				// init 3d UI
				m_GUI3dTerrain = gameObject.AddComponent<LE_GUI3dTerrain>();
				m_GUI3d.Add(m_GUI3dTerrain);

				// init default terrain
				InitializeDefaultTerrain(LEConfTerrain);

				// init logic
				m_logicTerrain = new LE_LogicTerrain(LEConfTerrain, m_GUI3dTerrain);
				m_logic.Add(m_logicTerrain);
			}
			else if (LEConfTerrain.CustomDefaultTerrain != null)
			{
				Debug.LogWarning("LE_LevelEditorMain: IS_TERRAIN_EDITOR is set to 'false', but you have provided a value for LE_ConfigTerrain.CustomDefaultTerrain! The value will be ignored!");
			}

			// initialize level logic
			m_logicLevel = new LE_LogicLevel(
				LEConfLevel,
				m_GUI3dTerrain,
				m_GUI3dObject,
				IS_OBLIQUE_FOCUS_CENTERING,
				LEConfTerrain.TerrainTextureConfig.TERRAIN_TEXTURES,
				LEConfTerrain.TerrainTextureConfig.TERRAIN_TEXTURE_SIZES,
				LEConfTerrain.TerrainTextureConfig.TERRAIN_TEXTURE_OFFSETS);
			m_logic.Add(m_logicLevel);

			// initialize object logic
			if (IS_OBJECT_EDITOR)
			{
				m_logic.Add(new LE_LogicObjects(m_GUI3dObject, ROOT_OBJECT_MAP));
			}

			// initialize camera gizmo
			if (cam != null && IS_WITH_CAMERA_PERSPECTIVE_GIZMO)
			{
				if (LE_GUIInterface.Instance.delegates.GetCameraPerspectiveGizmoRightPixelOffset != null)
				{
					// calculate the screen rect of the camera perspective gizmo (used later for mouse over UI calculation)
					const float relativeSize = 0.2f;
					float rectW = relativeSize*(float)Screen.width/cam.aspect;
					float rectH = relativeSize*(float)Screen.height;
					m_perspectiveGizmoRect = new Rect(Screen.width-LE_GUIInterface.Instance.delegates.GetCameraPerspectiveGizmoRightPixelOffset()-rectW, 0f, rectW, rectH);
					if (string.IsNullOrEmpty(LayerMask.LayerToName(CAMERA_PERSPECTIVE_GIZMO_LAYER)))
					{
						Debug.LogWarning("LE_LevelEditorMain: Inspector property 'CAMERA_PERSPECTIVE_GIZMO_LAYER' is set to '"+CAMERA_PERSPECTIVE_GIZMO_LAYER+"', but this layer has no name in 'Tags & Layers' set. Please set the name of this layer to 'CameraPerspectiveGizmo' or change the value of the 'CAMERA_PERSPECTIVE_GIZMO_LAYER' property! To open the 'Tags & Layers' manager select any game object, in the inspector click on the layer drop down at top right then click on 'Add Layer...'.");
					}
					else if (LayerMask.LayerToName(CAMERA_PERSPECTIVE_GIZMO_LAYER) != "CameraPerspectiveGizmo")
					{
						Debug.LogWarning("LE_LevelEditorMain: Inspector property 'CAMERA_PERSPECTIVE_GIZMO_LAYER' is set to '"+CAMERA_PERSPECTIVE_GIZMO_LAYER+"', but the name of this layer is '" + LayerMask.LayerToName(CAMERA_PERSPECTIVE_GIZMO_LAYER) + "'. Is this intended? If not please set the name of this layer to 'CameraPerspectiveGizmo' or change the value of the 'CAMERA_PERSPECTIVE_GIZMO_LAYER' property! To open the 'Tags & Layers' manager select any game object, in the inspector click on the layer drop down at top right then click on 'Add Layer...'.");
					}
					// create and setup the camera perspective gizmo
					m_cameraPerspectiveGizmo = CPG_CameraPerspectiveGizmo.Create(cam, CAMERA_PERSPECTIVE_GIZMO_LAYER);
					if (m_cameraPerspectiveGizmo != null)
					{
						// move the camera perspective gizmo far away (it will not be rendered anyway, but this is needed to prevent collision)
						m_cameraPerspectiveGizmo.transform.position = Vector3.down*-10000f; 
						// set ortho offset of the gizmo
						m_cameraPerspectiveGizmo.OrthoOffset = cam.farClipPlane*0.2f;
						// make sure that the main camera will not render the gizmo
						cam.cullingMask = cam.cullingMask & ~(1 << CAMERA_PERSPECTIVE_GIZMO_LAYER);
						// set position of the gizmo
						m_cameraPerspectiveGizmo.RelativeScreenSize = relativeSize;
						float rightMenuOffset = LE_GUIInterface.Instance.delegates.GetCameraPerspectiveGizmoRightPixelOffset() / (float)Screen.width;
						m_cameraPerspectiveGizmo.RelativeScreenPos = new Vector2(1f-relativeSize*0.5f/cam.aspect-rightMenuOffset, 1f-relativeSize*0.5f); 
						// register for events of the camera perspective gizmo
						m_cameraPerspectiveGizmo.m_onBeforeSwitchToOrthographic += OnCameraPerspectiveSwitchToOrthographic;
						m_cameraPerspectiveGizmo.m_onAfterSwitchToPerspective += OnCameraPerspectiveSwitchToPerspective;
					}

				}
				else
				{
					Debug.LogError("LE_LevelEditorMain: LE_GUIInterface.delegates.GetCameraPerspectiveGizmoRightPixelOffset was not set from the UI scripts! The camera perspective gizmo cannot be placed to the right screen position! To set this delegate use 'LE_GUIInterface.Instance.delegates.GetCameraPerspectiveGizmoRightPixelOffset = ...' in one of the Start functions of your scripts!");
				}
			}

			// apply an oblique camera projection to bring focused objects to the middle of the visible screen
			// instead of being in the middle of the rendered screen, which means close to the right menu UI
			if (IS_OBLIQUE_FOCUS_CENTERING && !SetObliqueFocusProjectionMatrix(true))
			{
				Debug.LogError("LE_LevelEditorMain: IS_OBLIQUE_FOCUS_CENTERING is true, but you have not provided the LE_GUIInterface.delegates.GetObliqueCameraPerspectiveRightPixelOffset delegate! Provide it to bring focused objects in the center of the visible (not covered by UI) screen area!");
			}
		}

		private void Initialize_InSecondUpdate()
		{
			// update obliqie focus centering (apsect ration could have changed (editor maximize on play) or screen rotation + comply with ortho settings)
			if (IS_OBLIQUE_FOCUS_CENTERING)
			{
				Camera cam = Cam;
				if (cam != null)
				{
					SetObliqueFocusProjectionMatrix(!cam.orthographic);
				}
			}
		}

		private void InitializeDefaultTerrain(LE_ConfigTerrain p_LEConfTerrain)
		{
			// initialize custom default terrain
			if (p_LEConfTerrain.CustomDefaultTerrain != null && p_LEConfTerrain.CustomDefaultTerrain.terrainData != null)
			{
				// save a reference to the default data prefab
				m_GUI3dTerrain.DefaultTerrainDataPrefab = p_LEConfTerrain.CustomDefaultTerrain.terrainData;
				// clone the terrain data so that the asset is not broken when testing in Unity Editor
				p_LEConfTerrain.CustomDefaultTerrain.enabled = false;
				p_LEConfTerrain.CustomDefaultTerrain.terrainData = m_GUI3dTerrain.GetDefaultTerrainDataDeepCopy();
				if (p_LEConfTerrain.CustomDefaultTerrain.GetComponent<TerrainCollider>() != null)
				{
					p_LEConfTerrain.CustomDefaultTerrain.GetComponent<TerrainCollider>().terrainData = p_LEConfTerrain.CustomDefaultTerrain.terrainData;
				}
				else
				{
					Debug.LogError("LE_LevelEditorMain: the CustomDefaultTerrain assigned to LE_ConfigTerrain must have a collider!");
				}
				p_LEConfTerrain.CustomDefaultTerrain.Flush();
				p_LEConfTerrain.CustomDefaultTerrain.enabled = true;
				// access the custom predefined terrain data
				// and wrap it with a terrain manager, which is then assigned to the GUI3dTerrain instance
				m_GUI3dTerrain.SetTerrain(p_LEConfTerrain.CustomDefaultTerrain);
				// your terrain must be in the LE_ConfigTerrain.TerrainLayer layer, you can set this in the game object,
				// but it is included here so that you cannot forget it
				p_LEConfTerrain.CustomDefaultTerrain.gameObject.layer = p_LEConfTerrain.TerrainLayer;
				// just to be on the safe side call this event and notify listeners that the level data was changed
				if (LE_EventInterface.OnChangeLevelData != null)
				{
					LE_EventInterface.OnChangeLevelData(p_LEConfTerrain.CustomDefaultTerrain.gameObject, new LE_LevelDataChangedEvent(LE_ELevelDataChangeType.TERRAIN_LOADED_DEFAULT));
				}
				// a terrain does exist -> activate edit terrain UI
				if (LE_GUIInterface.Instance.delegates.SetTerrainUIMode != null)
				{
					LE_GUIInterface.Instance.delegates.SetTerrainUIMode(LE_GUIInterface.Delegates.ETerrainUIMode.EDIT);
				}
				else
				{
					Debug.LogWarning("LE_LevelEditorMain: you have not set the LE_GUIInterface.delegates.SetTerrainUIMode delegate. You need to set it for example if you want to disable the create UI if the default Unity terrain is set!");
				}
			}
			else
			{
				// there is no terrain -> activate create terrain UI
				if (LE_GUIInterface.Instance.delegates.SetTerrainUIMode != null)
				{
					LE_GUIInterface.Instance.delegates.SetTerrainUIMode(LE_GUIInterface.Delegates.ETerrainUIMode.CREATE);
				}
				else
				{
					Debug.LogWarning("LE_LevelEditorMain: you have not set the LE_GUIInterface.delegates.SetTerrainUIMode delegate. You need to set it for example if you want to show the create UI if there is no default Unity terrain set!");
				}
			}
		}

		private void InitializeCommandManager()
		{
			UR_CommandMgr.Instance.IsDestroyedOnSceneLoad = true;
			UR_CommandMgr.Instance.StoredBytesLimit = UNDO_REDO_MEMORY_LIMIT*1024*1024;
			// to reset the undo/redo history a registration to the OnLoadedLevelInEditor event is made
			LE_EventInterface.OnLoadedLevelInEditor += OnLoadedLevelInEditor;
		}
		
		private void Update()
		{
			// initialization
			if (m_isFirstUpdateInitialized && !m_isSecondUpdateInitialized)
			{
				m_isSecondUpdateInitialized = true;
				Initialize_InSecondUpdate();
			}
			if (!m_isFirstUpdateInitialized)
			{
				m_isFirstUpdateInitialized = true;
				Initialize_InFirstUpdate();
			}

			bool isMouseOverGUI = IsMouseOverGUI();
			// update interactable states of 3d GUI
			for (int i = 0; i < m_GUI3d.Count; i++)
			{
				m_GUI3d[i].IsInteractable = !isMouseOverGUI && m_GUI3d[i].ActiveEditMode == m_editMode;
			}
			// update logic
			for (int i = 0; i < m_logic.Count; i++)
			{
				m_logic[i].Update();
			}
			// update input
			m_input.Update();
			// update key combos
			UpdateKeyCombos();
			// update camera pivot point
			UpdateCameraPivotPoint();
			if (m_cameraPerspectiveGizmo != null)
			{
				m_cameraPerspectiveGizmo.Pivot = m_camPivot;
			}

			// execute actions that wait for the editor to get ready
			if (m_executeWhenReady != null && IsReady)
			{
				m_executeWhenReady();
				m_executeWhenReady = null;
			}
		}

		private bool IsMouseOverGUI()
		{
			// check if mouse is over UI
			bool isMouseOverDelegate = LE_GUIInterface.Instance.delegates.IsCursorOverUI != null && LE_GUIInterface.Instance.delegates.IsCursorOverUI();
			// check if camera perspective gizmo is clicked
			bool isMouseOverGizmo = false;
			if (IS_WITH_CAMERA_PERSPECTIVE_GIZMO && !isMouseOverDelegate)
			{
				Vector3 mousePos = Input.mousePosition;
				mousePos.y = Mathf.Clamp(Screen.height - mousePos.y, 0, Screen.height);
				mousePos.x = Mathf.Clamp(mousePos.x, 0, Screen.width);
#if UNITY_ANDROID || UNITY_IPHONE || UNITY_WP8 || UNITY_WP_8_1
				isMouseOverGizmo = Input.GetMouseButton(0) && m_perspectiveGizmoRect.Contains(mousePos);
#else
				isMouseOverGizmo = m_perspectiveGizmoRect.Contains(mousePos);
#endif
				if (!isMouseOverGizmo)
				{
					Touch[] touches = Input.touches;
					for (int t = 0; t < Input.touchCount; t++)
					{
						Vector2 touch = touches[t].position;
						Vector3 pos = new Vector3(touch.x, Mathf.Clamp(Screen.height - touch.y, 0, Screen.height), 0f);
						if (m_perspectiveGizmoRect.Contains(pos))
						{
							isMouseOverGizmo = true;
							break;
						}
					}
				}
			}
			if (isMouseOverDelegate || isMouseOverGizmo)
			{
				m_lastGUITouchFrame = Time.frameCount;
			}
			return isMouseOverDelegate || isMouseOverGizmo || Time.frameCount - m_lastGUITouchFrame <= 1; // two frames after touch/mouse over GUI
		}

		private void OnGUI3dObjectFocused (Vector3 p_focusOn, float p_distance)
		{
			m_camPivot = p_focusOn;
			Camera cam = Cam;
			if (cam != null && cam.orthographic && m_cameraPerspectiveGizmo != null)
			{
				cam.transform.position = m_camPivot - cam.transform.forward*Mathf.Abs(m_cameraPerspectiveGizmo.OrthoOffset + p_distance);
				cam.orthographicSize = p_distance;
			}
		}

		private void OnDestroy()
		{
			// to monitor the last change frame number a registration to the OnChangeLevelData event was made
			LE_EventInterface.OnChangeLevelData -= OnChangeLevelData;

			// to reset the undo/redo history a registration to the OnLoadedLevelInEditor event was made
			LE_EventInterface.OnLoadedLevelInEditor -= OnLoadedLevelInEditor;

			// unregister UI events
			if (LE_GUIInterface.Instance != null)
			{
				LE_GUIInterface.Instance.events.OnEditModeBtn -= OnEditModeBtn;
				LE_GUIInterface.Instance.events.OnUndoBtn -= OnUndoBtn;
				LE_GUIInterface.Instance.events.OnRedoBtn -= OnRedoBtn;
			}

			// unregister callbacks from 3d GUI
			for (int i = 0; i < m_GUI3d.Count; i++)
			{
				if (m_GUI3d[i] is LE_GUI3dObject)
				{
					((LE_GUI3dObject)m_GUI3d[i]).OnFocused = null;
				}
			}

			// unregister external callbacks
			m_executeWhenReady = null;

			// remove input
			if (m_input != null)
			{
				m_input.Destroy();
			}

			// destroy logic
			for (int i = 0; i < m_logic.Count; i++)
			{
				m_logic[i].Destroy();
			}
		}

		private void OnChangeLevelData(object p_sender, System.EventArgs p_args)
		{
			m_lastChangeFrame = Time.frameCount;
		}
		
		private void OnLoadedLevelInEditor(object p_sender, System.EventArgs p_args)
		{
			UR_CommandMgr.Instance.Reset();
		}

		private void OnEditModeBtn(object p_sender, LE_GUIInterface.EventHandlers.EditModeEventArgs p_args)
		{
			m_editMode = p_args.EditMode;
			if (m_editMode != LE_EEditMode.TERRAIN && m_GUI3dTerrain != null)
			{
				m_GUI3dTerrain.HideCursor();
			}
			if (m_editMode != LE_EEditMode.OBJECT && m_GUI3dObject != null)
			{
				m_GUI3dObject.RemoveSelection();
			}
		}

		private void OnUndoBtn(object p_sender, System.EventArgs p_args)
		{
			Undo();
		}

		private void OnRedoBtn(object p_sender, System.EventArgs p_args)
		{
			Redo();
		}

		private void OnCameraPerspectiveSwitchToOrthographic(object p_sender, System.EventArgs p_args)
		{
			// remove oblique frustrum matrix modification, because it will not work in orthographic mode
			if (IS_OBLIQUE_FOCUS_CENTERING)
			{
				SetObliqueFocusProjectionMatrix(false);
			}

			// try to set the orthographic size to a reasonable value
			if (m_cameraPerspectiveGizmo != null)
			{
				float camDist = EstimateDistanceToLevel(EEstimateDistanceMode.NEAREST);
				Camera cam = Cam;
				if (cam != null)
				{
					cam.orthographicSize = Mathf.Clamp(camDist, 5f, 60f);
				}
			}
		}

		private void OnCameraPerspectiveSwitchToPerspective(object p_sender, System.EventArgs p_args)
		{
			// add oblique frustrum if needed
			if (IS_OBLIQUE_FOCUS_CENTERING)
			{
				SetObliqueFocusProjectionMatrix(true);
			}
		}

		private void UpdateKeyCombos()
		{
			bool isUndoKeyComboActive = (ACTIVE_KEY_COMBOS & LE_EKeyCombo.UNDO) != 0;
			bool isRedoKeyComboActive = (ACTIVE_KEY_COMBOS & LE_EKeyCombo.REDO) != 0;

			if (isUndoKeyComboActive || isRedoKeyComboActive)
			{
#if UNITY_EDITOR
				bool isCtrl = true;
#else
				bool isCtrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftApple) || Input.GetKey(KeyCode.RightApple);
#endif
				bool isShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

				if (isUndoKeyComboActive && isCtrl && (!isShift && Input.GetKeyUp(KeyCode.Z)))
				{
					Undo();
				}

				if (isRedoKeyComboActive && isCtrl && (Input.GetKeyUp(KeyCode.Y) || (isShift && Input.GetKeyUp(KeyCode.Z))))
				{
					Redo();
				}
			}
		}

		private void UpdateCameraPivotPoint()
		{
			Camera cam = Cam;
			if (cam != null)
			{
				Vector3 camToPivot = m_camPivot - cam.transform.position;
				float pivotDirection = Vector3.Dot(cam.transform.forward, camToPivot);
				if (pivotDirection < 1f)
				{
					float corrector = pivotDirection - 1.5f;
					m_camPivot = cam.transform.position - cam.transform.forward*corrector;
					camToPivot = m_camPivot - cam.transform.position;
					pivotDirection = Vector3.Dot(cam.transform.forward, camToPivot);
				}
				float pivotOffset = Vector3.Cross(cam.transform.forward, camToPivot).magnitude;
				if (pivotOffset > 1f)
				{
					m_camPivot = cam.transform.position + cam.transform.forward*pivotDirection;
				}
			}
		}

		private enum EEstimateDistanceMode { AVERAGE, NEAREST }
		private float EstimateDistanceToLevel(EEstimateDistanceMode p_mode)
		{
			float camDist = 300f;
			Camera cam = Cam;
			if (cam != null)
			{
				// raycast four rays to find out the approximate distance between camera and level
				float hitCount = 0;
				Vector3 hitValue = Vector3.zero;
				float nearestHit = float.MaxValue;
				for (float x = 0; x < 2; x++)
				{
					for (float y = 0; y < 2; y++)
					{
						Ray ray = cam.ScreenPointToRay(new Vector3(
							cam.rect.width * Screen.width*(0.25f+0.5f*x),
							cam.rect.height * Screen.height*(0.25f+0.5f*y), 0f));
						RaycastHit hit;
						if (Physics.Raycast(ray, out hit))
						{
							hitCount++;
							if (p_mode == EEstimateDistanceMode.AVERAGE)
							{
								hitValue += hit.point;
							}
							else
							{
								float distToCam = (hit.point - cam.transform.position).magnitude;
								if (nearestHit > distToCam)
								{
									nearestHit  = distToCam;
									hitValue = hit.point;
								}
							}
						}
					}
				}
				if (hitCount > 0)
				{
					if (p_mode == EEstimateDistanceMode.AVERAGE)
					{
						hitValue /= hitCount;
					}
					camDist = (hitValue - cam.transform.position).magnitude;
				}
			}
			return camDist;
		}
	}
}
