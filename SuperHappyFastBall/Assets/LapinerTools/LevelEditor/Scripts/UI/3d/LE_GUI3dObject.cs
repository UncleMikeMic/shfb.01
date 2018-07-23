using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LE_LevelEditor.UI;
using LE_LevelEditor.Events;
using LE_LevelEditor.Core;
using S_SnapTools;
using MyUtility;
using LE_LevelEditor.Logic;
using UndoRedo;
using LE_LevelEditor.Commands;

namespace LE_LevelEditor.UI
{
	public class LE_GUI3dObject : LE_GUI3dBase
	{
		private const float MIN_FOCUS_DISTANCE = 3f;
		private const float SELECTION_MAX_OVERSIZE = 10f;

		private LE_Object m_object = null;
		public LE_Object SelectedPrefab { get{ return m_object; } }
		private string m_objectResourcePath = null;
		private bool m_isObjectPlaceable = false;
		private bool m_isSceneInstanceFound = false;
		public bool IsSceneInstanceFound { get{ return m_isSceneInstanceFound; } }

		private string m_dragMessage = "";

		private LE_Object m_previewInstance = null;
		private LE_Object m_selectedObject = null;
		public LE_Object SelectedObject { get{ return m_selectedObject; } }
		public bool IsSelectedObjectSmartMoved { get{ return m_selectedObject != null && m_selectedObject.EditHandle != null && m_selectedObject.EditHandle.EditMode == LE_EObjectEditMode.SMART && m_selectedObject.EditHandle.IsDrag; } }
		private LE_Object m_cursorActionOnObject = null;
		private bool m_isSelectionPossible = false;
		private bool m_isCursorAction = false;
		private bool m_isCursorActionInThisFrame = false;
		private Vector3 m_lastCursorActionStartPos = -100f * Vector3.one;

		private Dictionary<string, S_SnapToObject> m_snapPointUIDsToSnapPoints = new Dictionary<string, S_SnapToObject>();
		private Dictionary<string, int> m_snapPointUIDsToObjUIDs = new Dictionary<string, int>();
		public Dictionary<string, int> SnapPointUIDsToObjUIDs { get{ return m_snapPointUIDsToObjUIDs; } }
		private int m_snapPointUIDToSnapPointsInvalidatedFrame = -1;
		private bool m_isSnapToObjectActive = true;
		public bool IsSnapToObjectActive { get{ return m_isSnapToObjectActive; } }

		public int TERRAIN_LAYER = 28;
		public System.Action<Vector3, float> OnFocused;

		public override LE_EEditMode ActiveEditMode { get{ return LE_EEditMode.OBJECT; } }

		private LE_EObjectEditSpace m_objectEditSpace = LE_EObjectEditSpace.SELF;
		public LE_EObjectEditSpace ObjectEditSpace
		{
			get{ return m_objectEditSpace; }
			set{ m_objectEditSpace = value; }
		}

		private LE_EObjectEditMode m_objectEditMode = LE_EObjectEditMode.NO_EDIT;
		public LE_EObjectEditMode ObjectEditMode
		{
			get{ return m_objectEditMode; }
			set{ m_objectEditMode = value; }
		}

		private bool m_isKeyComboDuplicate = true;
		public bool IsKeyComboDuplicate
		{
			get{ return m_isKeyComboDuplicate; }
			set{ m_isKeyComboDuplicate = value; }
		}

		private bool m_isKeyComboFocus = true;
		public bool IsKeyComboFocus
		{
			get{ return m_isKeyComboFocus; }
			set{ m_isKeyComboFocus = value; }
		}

		public override void SetCursorPosition(Vector3 p_cursorScreenCoords)
		{
			if (m_object != null && (m_object.SnapType == LE_Object.ESnapType.SNAP_TO_TERRAIN || m_object.SnapType == LE_Object.ESnapType.SNAP_TO_2D_GRID_AND_TERRAIN))
			{
				m_cursorScreenCoords = p_cursorScreenCoords;
				m_cursorRay = Camera.main.ScreenPointToRay (p_cursorScreenCoords);
				SetIsCursorOverSomething(Physics.Raycast(m_cursorRay, out m_cursorHitInfo, float.MaxValue, 1 << TERRAIN_LAYER));
			}
			else
			{
				bool isSelectedObjectSmartMoved = IsSelectedObjectSmartMoved;
				if (isSelectedObjectSmartMoved)
				{
					int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
					Dictionary<GameObject, int> layerBuffer = new Dictionary<GameObject, int>();
					// move all children of the selected object to the ignore raycast layer before raycasting
					MoveToLayer(m_selectedObject.transform, ignoreRaycastLayer, layerBuffer);
					base.SetCursorPosition(p_cursorScreenCoords);
					// move all children of the selected object to the respective layers
					foreach (KeyValuePair<GameObject, int> objectLayer in layerBuffer)
					{
						if (objectLayer.Key != null)
						{
							objectLayer.Key.layer = objectLayer.Value;
						}
					}
				}
				else
				{
					base.SetCursorPosition(p_cursorScreenCoords);
				}
			}
		}

		public override void SetIsCursorAction (bool p_isCursorAction)
		{
			if (IsInteractable)
			{
				m_isCursorActionInThisFrame |= p_isCursorAction;
			}
		}

		public void SetDraggableObject(LE_Object p_object, string p_objectResourcePath)
		{
			if (m_object != p_object)
			{
				m_isSceneInstanceFound = false;
				m_dragMessage = "";
				SetDragMessageInUI();
				m_object = p_object;
				m_objectResourcePath = p_objectResourcePath;
				m_isObjectPlaceable = IsObjectPlaceable();
			}
		}

		public void SelectObject(LE_Object p_object)
		{
			LE_Object priorSelectedObject = null;
			if (m_selectedObject != null && m_selectedObject != p_object)
			{
				m_selectedObject.EditMode = LE_EObjectEditMode.NO_EDIT;
				m_selectedObject.IsSelected = false;
				priorSelectedObject = m_selectedObject;
			}
			if (LE_LevelEditorMain.Instance != null && LE_LevelEditorMain.Instance.EditMode != LE_EEditMode.OBJECT)
			{
				// selection allowed only in object edit mode
				p_object = null;
			}
			m_selectedObject = p_object;
			if (m_selectedObject != null)
			{
				m_selectedObject.IsSelected = true;
			}
			if (LE_EventInterface.OnObjectSelectedInScene != null)
			{
				LE_EventInterface.OnObjectSelectedInScene(this, new LE_ObjectSelectedEvent(m_selectedObject, priorSelectedObject));
			}
		}

		public void Focus()
		{
			if (m_selectedObject != null)
			{
				// calculate objects size
				Renderer[] renderers = m_selectedObject.GetComponentsInChildren<Renderer>();
				Vector3 center = Vector3.zero;
				Vector3 size = Vector3.zero;
				float counter = 0;
				foreach (Renderer r in renderers)
				{
					if (r.GetComponentInParent<LE_ObjectEditHandle>() == null)
					{
						center += r.bounds.center;
						size += r.bounds.size;
						counter++;
					}
				}
				if (counter != 0)
				{
					center *= 1f/counter;
					size *= 1f/counter;
				}
				else
				{
					center = m_selectedObject.transform.position;
					size = Vector3.one*MIN_FOCUS_DISTANCE;
				}

				Vector3 dir = Camera.main.transform.position - center;
				float distance = Mathf.Max(size.x, size.y, size.z)*3f;
				Camera.main.transform.position = center + dir.normalized*distance;
				if (Camera.main.transform.position.y < center.y)
				{
					Vector3 pos = Camera.main.transform.position;
					pos.y = center.y;
					Camera.main.transform.position = pos;
				}
				Camera.main.transform.LookAt(center, Vector3.up);
				if (OnFocused != null)
				{
					OnFocused(center, distance);
				}
			}
		}

		public void SelectNFocusPrefabInstanceInScene()
		{
			bool isInstanceOfPrefabSelected = m_selectedObject != null && m_object != null && m_selectedObject.name == m_objectResourcePath;
			LE_Object firstInstanceOfPrefab = null;
			// find instances of the selected prefab
			LE_Object[] objects = Object.FindObjectsOfType<LE_Object>();
			for (int i = 0; i < objects.Length; i++)
			{
				// check if this is an instance of the selected prefab
				if (objects[i].name == m_objectResourcePath)
				{
					if (isInstanceOfPrefabSelected)
					{
						// save the first instance (it will be selected if no later instance is found)
						if (firstInstanceOfPrefab == null) { firstInstanceOfPrefab = objects[i]; }
						// an instance of the selected prefab was already selected -> try to find it and slip all instances before this instance
						if (m_selectedObject == objects[i])
						{
							isInstanceOfPrefabSelected = false; // found the selected instance -> use the next hit or the first found instance
						}
					}
					else
					{
						// there was no instance of the selected prefab selected -> select and focus on this instance
						SelectObject(objects[i]);
						Focus();
						return;
					}
				}
			}
			// select the first instance if no better match found
			if (firstInstanceOfPrefab != null)
			{
				SelectObject(firstInstanceOfPrefab);
				Focus();
			}
		}

		public void Delete()
		{
			UR_CommandMgr.Instance.Execute(new LE_CmdDeleteObject(this, m_selectedObject));
		}

		public void RemoveSelection()
		{
			SelectObject(null);
		}

		public void CloneObject()
		{
			string objectResourcePath = m_selectedObject.name;
			if (m_selectedObject != null && IsObjectPlaceable(m_selectedObject, objectResourcePath))
			{
				// remove selection
				m_selectedObject.EditMode = LE_EObjectEditMode.NO_EDIT;
				m_selectedObject.IsSelected = false;
				// selection state would be applied at the end of the frame, but we need it right now
				m_selectedObject.ApplySelectionState();
				// clone
				UR_CommandMgr.Instance.Execute(new LE_CmdCloneObject(this, m_selectedObject.UID, m_selectedObject.transform, objectResourcePath));
			}
		}

		public bool IsObjectPlaceable(LE_Object p_object, string p_resourcePath)
		{
			// check if there is an instance count limit
			m_isSceneInstanceFound = false;
			if (p_object.MaxInstancesInLevel != 0)
			{
				int count = 0;
				LE_Object[] objects = Object.FindObjectsOfType<LE_Object>();
				for (int i = 0; i < objects.Length; i++)
				{
					if (objects[i].name == p_resourcePath)
					{
						m_isSceneInstanceFound = true;
						count++;
						if (p_object.MaxInstancesInLevel <= count)
						{
							return false; // the limit (MaxInstancesInLevel) is already reached
						}
					}
				}
			}
			else
			{
				m_isSceneInstanceFound = GameObject.Find(m_objectResourcePath);
			}
			return true;
		}

		public void UpdateIsObjectPlaceable()
		{
			m_isObjectPlaceable = IsObjectPlaceable();
		}

		public void SetSnapPointUIDsToObjUIDs(Dictionary<string, int> p_snapPointUIDsToObjUIDs)
		{
			foreach (KeyValuePair<string, int> reference in p_snapPointUIDsToObjUIDs)
			{
				// save the reference
				if (!m_snapPointUIDsToObjUIDs.ContainsKey(reference.Key))
				{
					m_snapPointUIDsToObjUIDs.Add(reference.Key, reference.Value);
				}
				else
				{
					Debug.LogError("LE_GUI3dObject: SetSnapPointUIDsToObjUIDs: snap point with UID '"+reference.Key+"' is already snapped to object with UID '"+m_snapPointUIDsToObjUIDs[reference.Key]+"'! This call tried to snap it to object with UID '"+reference.Value+"'");
				}
			}
		}

		public void SetSnapPointUIDsToObjUIDsAndApplyChanges(Dictionary<string, int> p_snapPointUIDsToObjUIDs)
		{
			SetSnapPointUIDsToObjUIDs(p_snapPointUIDsToObjUIDs);
			foreach (string snapPointUID in p_snapPointUIDsToObjUIDs.Keys)
			{
				S_SnapToObject snapPoint;
				if (m_snapPointUIDsToSnapPoints.TryGetValue(snapPointUID, out snapPoint))
				{
					if (snapPoint.SnapCounter == 0)
					{
						snapPoint.IncSnapCounter();
					}
					else
					{
						Debug.LogWarning("LE_GUI3dObject: SetSnapPointUIDsToObjUIDsAndApplyChanges: snap point with UID '"+snapPointUID+"' already was snapped!");
					}
				}
			}
		}

		public void AddSnapPoint(string p_snapPointUID, S_SnapToObject p_snapInstance)
		{
			if (!m_snapPointUIDsToSnapPoints.ContainsKey(p_snapPointUID))
			{
				m_snapPointUIDsToSnapPoints.Add(p_snapPointUID, p_snapInstance);
			}
			else
			{
				Debug.LogError("LE_GUI3dObject: AddSnapPoint: a snap point instance with the UID '" + p_snapPointUID + "' already exists!");
			}
		}

		public void LoadSnapCounter(string snapPointUID, S_SnapToObject snapInstance)
		{
			if (m_snapPointUIDsToObjUIDs.ContainsKey(snapPointUID))
			{
				snapInstance.IncSnapCounter();
			}
		}

		public List<KeyValuePair<string, int>> GetSnapPointsToReactivate(int p_deletedObjectUID, int p_deletedObjectSnapPointCount)
		{
			List<KeyValuePair<string, int>> reactivatedSnapPointUIDsToObjectUIDs = new List<KeyValuePair<string, int>>();
			// find all connections from the object that is going to be deleted to other objects
			for (int i = 0; i < p_deletedObjectSnapPointCount; i++)
			{
				string snapPointUID = LE_LogicObjects.GetSnapPointUID(p_deletedObjectUID, i);
				if (m_snapPointUIDsToObjUIDs.ContainsKey(snapPointUID))
				{
					reactivatedSnapPointUIDsToObjectUIDs.Add(new KeyValuePair<string, int>(snapPointUID, m_snapPointUIDsToObjUIDs[snapPointUID]));
				}
			}
			// find all connections from other objects to the object that is going to be deleted
			foreach (KeyValuePair<string, int> reference in m_snapPointUIDsToObjUIDs)
			{
				if (reference.Value == p_deletedObjectUID)
				{
					reactivatedSnapPointUIDsToObjectUIDs.Add(new KeyValuePair<string, int>(reference.Key, p_deletedObjectUID));
				}
			}
			return reactivatedSnapPointUIDsToObjectUIDs;
		}

		public void ReactivateSnapPoints(int p_deletedObjectUID, int p_deletedObjectSnapPointCount)
		{
			List<KeyValuePair<string, int>> reactivatedSnapPointUIDsToObjectUIDs = GetSnapPointsToReactivate(p_deletedObjectUID, p_deletedObjectSnapPointCount);
			foreach (KeyValuePair<string, int> reference in reactivatedSnapPointUIDsToObjectUIDs)
			{
				// reactivate snap point
				if (m_snapPointUIDsToSnapPoints.ContainsKey(reference.Key)) // the source object might have been deleted already
				{
					m_snapPointUIDsToSnapPoints[reference.Key].DecSnapCounter();
				}
				// delete meta information about the now active snap point
				m_snapPointUIDsToObjUIDs.Remove(reference.Key);
			}
			// force update of snap point UID to snap point array
			m_snapPointUIDToSnapPointsInvalidatedFrame = Time.frameCount;
		}

		public void MarkSnapPointAsUsed(LE_Object p_sourceObj, LE_Object p_destinationObj, S_SnapToObject p_snapScript)
		{
			if (p_sourceObj != null)
			{
				int snapPointIndex = GetSnapPointIndex(p_sourceObj, p_snapScript);
				MarkSnapPointAsUsed(p_sourceObj, p_destinationObj, snapPointIndex);
			}
			else
			{
				Debug.LogError("LE_GUI3dObject: MarkSnapPointAsUsed: could not find LE_Object of snap source!");
			}
		}
		
		public void MarkSnapPointAsUsed(LE_Object p_sourceObj, LE_Object p_destinationObj, int snapPointIndex)
		{
			if (p_sourceObj != null)
			{
				if (snapPointIndex != -1)
				{
					string snapPointUID = p_sourceObj.UID+"_"+snapPointIndex;
					if (!m_snapPointUIDsToObjUIDs.ContainsKey(snapPointUID))
					{
						m_snapPointUIDsToObjUIDs.Add(snapPointUID, p_destinationObj.UID);
					}
					else
					{
						Debug.LogError("LE_GUI3dObject: MarkSnapPointAsUsed: duplicate snapping on snapPointUID("+snapPointUID+") new (ignored) objID(" + p_destinationObj.UID + ") and old objID(" + m_snapPointUIDsToObjUIDs[snapPointUID] + ")");
					}
				}
				else
				{
					Debug.LogError("LE_GUI3dObject: MarkSnapPointAsUsed: could not find LE_ObjectSnapPoint of snap source!");
				}
			}
			else
			{
				Debug.LogError("LE_GUI3dObject: MarkSnapPointAsUsed: could not find LE_Object of snap source!");
			}
		}

		public void ClearLevelData()
		{
			m_snapPointUIDsToSnapPoints.Clear();
			m_snapPointUIDsToObjUIDs.Clear();
			m_isSnapToObjectActive = true;
		}

		private void Start()
		{
			if (LE_GUIInterface.Instance.delegates.IsObjectDragged == null)
			{
				Debug.LogError("LE_GUI3dObject: you must provide the LE_GUIInterface.delegates.IsObjectDragged to make the object drag&drop working!");
			}
			if (LE_GUIInterface.Instance.delegates.SetDraggableObjectMessage == null)
			{
				Debug.LogWarning("LE_GUI3dObject: LE_GUIInterface.delegates.SetDraggableObjectMessage is not set. Set it if you want to visualize the 'max. # reached!' message or messages generated via LE_ObjectDragEvent.");
			}
			if (LE_GUIInterface.Instance.delegates.SetDraggableObjectState == null)
			{
				Debug.LogWarning("LE_GUI3dObject: LE_GUIInterface.delegates.SetDraggableObjectState is not set. Set it if you want to hide your UI when a 3d preview is shown or show different UI if the object is not placeable.");
			}
			
			S_SnapToObject.OnGlobalBeforeObjectSnapped += OnBeforeObjectSnapped;
			S_SnapToObject.OnGlobalPreviewObjectInstantiated += OnPreviewObjectInstantiated;
		}

		private void OnDestroy()
		{
			S_SnapToObject.OnGlobalBeforeObjectSnapped -= OnBeforeObjectSnapped;
			S_SnapToObject.OnGlobalPreviewObjectInstantiated -= OnPreviewObjectInstantiated;
		}

		private void OnPreviewObjectInstantiated(object p_sender, S_SnapToObjectEventArgs p_args)
		{
			LE_Object leObj = p_args.NewInstance.GetComponent<LE_Object>();
			if (leObj != null)
			{
				Destroy(leObj);
			}
		}

		private void OnBeforeObjectSnapped(object p_sender, S_SnapToObjectBeforePlacementEventArgs p_args)
		{
			p_args.IsDelayedPlacePrefab = true;
			LE_Object sourceObj = p_args.Source.GetComponentInParent<LE_Object>();
			int snapPointIndex = GetSnapPointIndex(sourceObj, p_args.Source);
			if (snapPointIndex >= 0) // error was written to console already
			{
				UR_CommandMgr.Instance.Execute(new LE_CmdSnapObjectToObject(this, sourceObj.UID, snapPointIndex, p_args.SnapPrefab));
			}
		}

		private int GetSnapPointIndex(LE_Object p_obj, S_SnapToObject p_snapScript)
		{
			if (p_obj != null)
			{
				for (int i = 0; i < p_obj.ObjectSnapPoints.Length; i++)
				{
					if (p_obj.ObjectSnapPoints[i].SnapSystemInstance == p_snapScript)
					{
						return i;
					}
				}
				Debug.LogError("LE_GUI3dObject: GetSnapPointIndex: p_snapScript is not a SnapSystemInstance of p_obj!");
				return -1;
			}
			else
			{
				Debug.LogError("LE_GUI3dObject: GetSnapPointIndex: p_obj is null!");
				return -1;
			}
		}

		private bool IsObjectPlaceable()
		{
			if (m_object != null)
			{
				if (!IsObjectPlaceable(m_object, m_objectResourcePath))
				{
					m_dragMessage = "max. # reached!";
					SetDragMessageInUI();
					return false;
				}
				else
				{
					m_dragMessage = "";
					SetDragMessageInUI();
					return true;
				}
			}
			else
			{
				m_isSceneInstanceFound = false;
			}
			return false;
		}

		private void Update()
		{
			UpdateObjectSelection();
			UpdateNewObjectDragAndDrop();
			UpdateSmartMove();
			UpdateSnapToObjectInstances();

			// check single shortcut keys
			if (m_isKeyComboFocus && Input.GetKeyDown(KeyCode.F))
			{
				Focus();
			}

			// reset variables
			m_isCursorActionInThisFrame = false;
		}

#if UNITY_EDITOR || !(UNITY_ANDROID || UNITY_IOS || UNITY_IPHONE || UNITY_WP8 || UNITY_WP_8 || UNITY_WP_8_1)
		private void OnGUI()
		{
			// check shortcut key combinations
			if (m_isKeyComboDuplicate)
			{
				Event e = Event.current;
				if (e.type == EventType.KeyUp && e.control && e.keyCode == KeyCode.D)
				{
					CloneObject();
				}
			}
		}
#endif
		
		private void UpdateObjectSelection()
		{
			bool isCursorHit = IsCursorOverSomething;
			// object selection when no new object is dragged
			if (m_previewInstance == null && !IsObjectDraggedInUI())
			{
				// mouse if over UI (or not interactable) and something is clicked -> no selection possible
				if (!IsInteractable)
				{
					m_isCursorAction = isCursorHit;
					m_isSelectionPossible = false;
				}
				// mouse down in this frame -> cursor was not active and was activated in this frame
				else if (!m_isCursorAction && m_isCursorActionInThisFrame)
				{
					if (isCursorHit)
					{
						// cursor is activated on an object
						m_cursorActionOnObject = m_cursorHitInfo.collider.GetComponentInParent<LE_Object>();
					}
					else
					{
						// cursor did not hit anything
						m_cursorActionOnObject = null;
					}
					m_lastCursorActionStartPos = m_cursorScreenCoords;
					m_isSelectionPossible = true;
					m_isCursorAction = true;
				}
				// mouse press -> cursor was and is active
				else if (m_isCursorAction && m_isCursorActionInThisFrame)
				{
					// cursor has to stay over something
					if ((!isCursorHit && m_cursorActionOnObject != null) ||
					    // cursor has to stay on the same object for selection
					    (isCursorHit && m_cursorActionOnObject != m_cursorHitInfo.collider.GetComponentInParent<LE_Object>()) ||
					    // cursor has to stay at the same position for selection
					    (m_lastCursorActionStartPos - m_cursorScreenCoords).magnitude > Screen.height * 0.025)
					{
						m_isSelectionPossible = false;
						m_cursorActionOnObject = null;
					}
				}
				// mouse up in this frame -> cursor was active, but is not active in this frame
				else if (m_isCursorAction && !m_isCursorActionInThisFrame)
				{
					// selection if cursor action stopped on the same object as it was started
					if (m_isSelectionPossible)
					{
						// select something if clicked in empty space
						if (m_cursorActionOnObject == null)
						{
							m_cursorActionOnObject = TrySelectByOversizedBoundingVolume();
						}
						SelectObject(m_cursorActionOnObject);
					}
					m_isSelectionPossible = false;
					m_cursorActionOnObject = null;
					m_isCursorAction = false;
				}
				else
				{
					m_cursorActionOnObject = null;
					m_isCursorAction = false;
				}
			}
			
			// update selected object edit mode and space
			if (m_selectedObject != null)
			{
				m_selectedObject.EditSpace = m_objectEditSpace;
				m_selectedObject.EditMode = m_objectEditMode;
			}
		}

		private void UpdateNewObjectDragAndDrop()
		{
			// drag and drop
			if (m_object != null && m_objectResourcePath != null)
			{
				// reset drag icon text it could have been changed with
				// custom is placeable text
				SetDragMessageInUI();
				// hide preview if cursor is over 2d GUI
				if (!IsInteractable)
				{
					if (m_previewInstance != null)
					{
						Destroy(m_previewInstance.gameObject);
					}
				}
				// check if the icon is being dragged and the cursor is over something
				else if (IsCursorOverSomething)
				{
					// make a preview of the dragged object
					if (IsObjectDraggedInUI())
					{
						// object can be dragged
						if (OnObjectDrag() &&
						    // object is not snapped to terrain OR
							((m_object.SnapType != LE_Object.ESnapType.SNAP_TO_TERRAIN && m_object.SnapType != LE_Object.ESnapType.SNAP_TO_2D_GRID_AND_TERRAIN) ||
							 // hit point is on terrain
						 	 (m_cursorHitInfo.collider != null && m_cursorHitInfo.collider.gameObject.layer == TERRAIN_LAYER)))
						{
							// instatiate the 3d representation
							if (m_previewInstance == null)
							{
								m_previewInstance = (LE_Object)Instantiate(m_object);
								m_previewInstance.name =  "LE_GUI3dObject Preview Instance";
								MoveToLayer(m_previewInstance.transform, LayerMask.NameToLayer("Ignore Raycast"));
								// destroy all rigidbodies
								Rigidbody[] rigidbodies = m_previewInstance.GetComponentsInChildren<Rigidbody>();
								for (int i = 0; i < rigidbodies.Length; i++)
								{
									Destroy(rigidbodies[i]);
								}
								// add grid snapping to preview if needed
								if (m_previewInstance.SnapType == LE_Object.ESnapType.SNAP_TO_3D_GRID || m_previewInstance.SnapType == LE_Object.ESnapType.SNAP_TO_2D_GRID_AND_TERRAIN)
								{
									LE_LogicObjects.AddGridSnapping(this, m_previewInstance, true);
								}
							}
							SmartMove(m_previewInstance);
						}
					}
					// place object if cursor was released while the object was over something
					else if (m_isObjectPlaceable)
					{
						PlaceObject();
					}
					else
					{
						if (m_previewInstance != null)
						{
							Destroy(m_previewInstance.gameObject);
						}
					}
				}
				else if (m_previewInstance != null)
				{
					Destroy(m_previewInstance.gameObject);
				}
				if (LE_GUIInterface.Instance.delegates.SetDraggableObjectState != null)
				{
					// icon hiding and coloring
					if (!IsInteractable || !IsObjectDraggedInUI())
					{
						// keep icon color if icon is over 2d GUI
						LE_GUIInterface.Instance.delegates.SetDraggableObjectState(LE_GUIInterface.Delegates.EDraggedObjectState.NONE);
					}
					else if (m_previewInstance != null)
					{
						// hide icon if a 3d preview is drawn
						LE_GUIInterface.Instance.delegates.SetDraggableObjectState(LE_GUIInterface.Delegates.EDraggedObjectState.IN_3D_PREVIEW);
					}
					else
					{
						// show icon in red since it cannot be placed here
						LE_GUIInterface.Instance.delegates.SetDraggableObjectState(LE_GUIInterface.Delegates.EDraggedObjectState.NOT_PLACEABLE);
					}
				}
			}
		}

		private void UpdateSmartMove()
		{
			if (IsCursorOverSomething && // first check if cursor if over something
			    IsSelectedObjectSmartMoved && // check if the currently selected object is being smart moved
			    // object is not snapped to terrain OR
			    ((m_selectedObject.SnapType != LE_Object.ESnapType.SNAP_TO_TERRAIN && m_selectedObject.SnapType != LE_Object.ESnapType.SNAP_TO_2D_GRID_AND_TERRAIN) ||
				// hit point is on terrain
				(m_cursorHitInfo.collider != null && m_cursorHitInfo.collider.gameObject.layer == TERRAIN_LAYER)))
			{
				SmartMove(m_selectedObject);
			}
		}

		private void SmartMove(LE_Object p_obj)
		{
			// draw the 3d object at the place where it would be placed
			p_obj.transform.position = m_cursorHitInfo.point;
			// if object is snapped to grid apply a small offset (needed for example when stacking grid cubes)
			if (p_obj.SnapType == LE_Object.ESnapType.SNAP_TO_3D_GRID || p_obj.SnapType == LE_Object.ESnapType.SNAP_TO_2D_GRID_AND_TERRAIN)
			{
				p_obj.transform.position += m_cursorHitInfo.normal*0.005f;
			}
			// if placement option IsPlacementRotationByNormal is true rotate the object accordingly
			if (p_obj.IsPlacementRotationByNormal)
			{
				p_obj.transform.up = m_cursorHitInfo.normal;
			}
		}

		private void UpdateSnapToObjectInstances()
		{
			bool isChangeToActive = !m_isSnapToObjectActive && IsInteractable;
			if (isChangeToActive || (m_isSnapToObjectActive && !IsInteractable) || m_snapPointUIDToSnapPointsInvalidatedFrame == Time.frameCount-1)
			{
				m_isSnapToObjectActive = isChangeToActive;
				List<string> nullReferencingUIDs = new List<string>();
				Dictionary<string, S_SnapToObject>.KeyCollection snapPointUIDs = m_snapPointUIDsToSnapPoints.Keys;
				foreach (string snapPointUID in snapPointUIDs)
				{
					if (m_snapPointUIDsToSnapPoints[snapPointUID] != null)
					{
						if (!isChangeToActive || m_snapPointUIDsToSnapPoints[snapPointUID].SnapCounter == 0)
						{
							m_snapPointUIDsToSnapPoints[snapPointUID].gameObject.SetActive(isChangeToActive);
						}
					}
					else
					{
						nullReferencingUIDs.Add(snapPointUID);
					}
				}
				foreach (string snapPointUID in nullReferencingUIDs)
				{
					m_snapPointUIDsToSnapPoints.Remove(snapPointUID);
				}
			}
		}

		/// <summary>
		/// Sometimes the camera is too far away from small objects, so that it is hard
		/// to select the desired object. Before removing the selection by a click into free
		/// space, the oversized bounding volumes of objects are checked. Objects near the
		/// actual click are selected if the cursor hits their bounding box
		/// </summary>
		private LE_Object TrySelectByOversizedBoundingVolume()
		{
			List<LE_Object> foundObjects = new List<LE_Object>();
			List<float> foundObjectSizes = new List<float>();
			LE_Object[] levelObjects = FindObjectsOfType<LE_Object>();
			for (int i = 0; i < levelObjects.Length; i++)
			{
				Vector3 min, max;
				if (GetMinMaxBounds(levelObjects[i], out min, out max))
				{
					float overSize = 1f;
					float dist = (levelObjects[i].transform.position - Camera.main.transform.position).magnitude;
					Vector3 size = new Vector3();
					size.x = Mathf.Abs(max.x-min.x);
					size.y = Mathf.Abs(max.y-min.y);
					size.z = Mathf.Abs(max.z-min.z);
					float sizeFloat = size.magnitude;
					Vector3 maxSize = Vector3.one*SELECTION_MAX_OVERSIZE+size;
					if (dist > 50)
					{
						overSize = 2.25f;
						size = Vector3.Max(size, Vector3.one*1.5f);
					}
					else if (dist > 25)
					{
						overSize = 1.85f;
						size = Vector3.Max(size, Vector3.one);
					}
					else if (dist > 15)
					{
						overSize = 1.5f;
						size = Vector3.Max(size, Vector3.one*0.5f);
					}
					size = Vector3.Min(size*overSize,maxSize);
					Bounds boundsOversized = new Bounds((max+min)*0.5f, size);
					if (boundsOversized.IntersectRay(m_cursorRay))
					{
						foundObjects.Add(levelObjects[i]);
						foundObjectSizes.Add(sizeFloat);
					}
				}
			}
			if (foundObjects.Count > 0)
			{
				LE_Object bestObject = foundObjects[0];
				float bestSize = foundObjectSizes[0];
				// select the smallest object if there are multiple selections possible
				for (int i = 1; i < foundObjects.Count; i++)
				{
					if (foundObjectSizes[i] < bestSize)
					{
						bestObject = foundObjects[i];
						bestSize = foundObjectSizes[i];
					}
				}
				// select the object closest to cursor ray amonth those with similar size
				float bestCursorDistance = 9999999;
				for (int i = 0; i < foundObjects.Count; i++)
				{
					if (foundObjectSizes[i] <= bestSize*1.25f)
					{
						float distToCursor = Vector3.Cross(m_cursorRay.direction, foundObjects[i].transform.position - m_cursorRay.origin).magnitude;
						if (bestCursorDistance > distToCursor)
						{
							bestCursorDistance = distToCursor;
							bestObject = foundObjects[i];
						}
					}
				}
				return bestObject;
			}
			return null;
		}

		private bool GetMinMaxBounds(LE_Object p_object, out Vector3 o_min, out Vector3 o_max)
		{
			o_min = Vector3.one * 9999999;
			o_max = -Vector3.one * 9999999;
			Collider[] colliders = p_object.GetComponentsInChildren<Collider>();
			if (colliders.Length > 0)
			{
				bool isColliderFound = false;
				for (int j = 0; j < colliders.Length; j++)
				{
					if (colliders[j].GetComponent<LE_ObjectEditHandleCollider>() == null)
					{
						isColliderFound = true;
						Bounds bounds = colliders[j].bounds;
						o_min = Vector3.Min(o_min, bounds.min);
						o_max = Vector3.Max(o_max, bounds.max);
					}
				}
				return isColliderFound;
			}
			return false;
		}

		private void PlaceObject()
		{
			if (m_previewInstance != null)
			{
				// destroy preview (call first to disable drag/drop in case of later errors)
				Destroy(m_previewInstance.gameObject);
				// instantiate a new object (since the preview instance was changed e.g. layers)
				UR_CommandMgr.Instance.Execute(new LE_CmdPlaceObject(this, m_object, m_previewInstance.transform, m_objectResourcePath));
			}
		}

		private void MoveToLayer(Transform root, int layer, Dictionary<GameObject, int> layerBuffer = null)
		{
			if (layerBuffer != null)
			{
				layerBuffer.Add(root.gameObject, root.gameObject.layer);
			}
			root.gameObject.layer = layer;
			foreach(Transform child in root)
			{
				MoveToLayer(child, layer, layerBuffer);
			}
		}

		private bool OnObjectDrag()
		{
			// notify listeners that an object is dragged over something
			// this also means: ask listeners if the object is placeable
			bool isPlaceable = m_isObjectPlaceable;
			if (LE_EventInterface.OnObjectDragged != null)
			{
				LE_ObjectDragEvent dragEventArgs = new LE_ObjectDragEvent(
					m_object,
					m_previewInstance,
					m_isObjectPlaceable,
					m_dragMessage,
					m_cursorHitInfo);
				LE_EventInterface.OnObjectDragged(this, dragEventArgs);
				isPlaceable = dragEventArgs.IsObjectPlaceable;
				SetDragMessageInUI(dragEventArgs.Message);
			}
			// hide preview if 3d GUI is disabled for any other reason
			if (!isPlaceable)
			{
				if (m_previewInstance != null)
				{
					Destroy(m_previewInstance.gameObject);
				}
			}
			return isPlaceable;
		}

		private void SetDragMessageInUI()
		{
			SetDragMessageInUI(m_dragMessage);
		}

		private void SetDragMessageInUI(string p_message)
		{
			if (LE_GUIInterface.Instance.delegates.SetDraggableObjectMessage != null)
			{
				LE_GUIInterface.Instance.delegates.SetDraggableObjectMessage(m_dragMessage);
			}
		}

		private bool IsObjectDraggedInUI()
		{
			if (LE_GUIInterface.Instance.delegates.IsObjectDragged != null)
			{
				return LE_GUIInterface.Instance.delegates.IsObjectDragged();
			}
			else
			{
				return false;
			}
		}
	}
}
