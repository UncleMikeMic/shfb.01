using UnityEngine;
using System.Collections;
using LE_LevelEditor.UI;
using LE_LevelEditor.Events;
using LE_LevelEditor.Core;
using LS_LevelStreaming;
using S_SnapTools;
using MyUtility;
using UndoRedo;
using LE_LevelEditor.Commands;

namespace LE_LevelEditor.Logic
{
	public class LE_LogicObjects : LE_LogicBase
	{
		private LE_GUI3dObject m_GUI3dObject;

		public LE_LogicObjects(LE_GUI3dObject p_GUI3dObject, LE_ObjectMap p_objectMap)
		{
			m_GUI3dObject = p_GUI3dObject;

			// register to events
			LE_GUIInterface.Instance.events.OnObjectEditSpaceBtn += OnObjectEditSpaceBtn;
			LE_GUIInterface.Instance.events.OnObjectEditModeBtn += OnObjectEditModeBtn;
			LE_GUIInterface.Instance.events.OnObjectSelectDraggable += OnObjectSelectDraggable;
			LE_GUIInterface.Instance.events.OnSelectedObjectFocusBtn += OnSelectedObjectFocusBtn;
			LE_GUIInterface.Instance.events.OnSelectedObjectDuplicateBtn += OnSelectedObjectDuplicateBtn;
			LE_GUIInterface.Instance.events.OnSelectedPrefabFindBtn += OnSelectedPrefabFindBtn;
			LE_GUIInterface.Instance.events.OnSelectedObjectDeleteBtn += OnSelectedObjectDeleteBtn;
			LE_GUIInterface.Instance.events.OnSelectedObjectIsSleepOnStartChanged += OnSelectedObjectIsSleepOnStartChanged;
			LE_GUIInterface.Instance.events.OnSelectedObjectColorChanged += OnSelectedObjectColorChanged;
			LE_GUIInterface.Instance.events.OnSelectedObjectVariationIndexChanged += OnSelectedObjectVariationIndexChanged;

			// initialize UI
			if (p_objectMap != null)
			{
				if (p_objectMap.ObjectPrefabs.Length > 0 || p_objectMap.ObjectPrefabResourcePaths.Length > 0 || p_objectMap.SubObjectMaps.Length > 0)
				{
					if (LE_GUIInterface.Instance.delegates.SetObjects != null)
					{
						LE_GUIInterface.Instance.delegates.SetObjects(p_objectMap);
					}
					else
					{
						Debug.LogError("LE_LogicObjects: you have not set the LE_GUIInterface.delegates.SetObjects, but the provided object map is not empty. You have to set this delegate to update your object UI.");
					}
				}
			}
			else
			{
				Debug.LogError("LE_LogicObjects: ROOT_OBJECT_MAP was not initialized! You need to set it in the inspector of LE_LevelEditorMain.");
			}

			m_GUI3dObject.ObjectEditMode = LE_EObjectEditMode.MOVE;

			// generate some warnings if needed
			if (LE_GUIInterface.Instance.delegates.SetIsSelectedObjectFocusBtnInteractable == null)
			{
				Debug.LogWarning("LE_LogicObjects: you have not set the LE_GUIInterface.delegates.SetIsSelectedObjectFocusBtnInteractable delegate! It might confuse players if you have a focus button, which is always clickable, but works only when an object is selected. Set this delegate to disable the focus button when no object is selected.");
			}
			if (LE_GUIInterface.Instance.delegates.SetIsSelectedObjectDuplicateBtnInteractable == null)
			{
				Debug.LogWarning("LE_LogicObjects: you have not set the LE_GUIInterface.delegates.SetIsSelectedObjectDuplicateBtnInteractable delegate! It might confuse players if you have a duplicate button, which is always clickable, but works only when an object can be duplicated. Set this delegate to disable the duplicate button when no object is selected or this object cannot be created (e.g. max. count reached in this level).");
			}
			if (LE_GUIInterface.Instance.delegates.SetIsSelectedObjectDeleteBtnInteractable == null)
			{
				Debug.LogWarning("LE_LogicObjects: you have not set the LE_GUIInterface.delegates.SetIsSelectedObjectDeleteBtnInteractable delegate! It might confuse players if you have a delete button, which is always clickable, but works only when an object is selected. Set this delegate to disable the delete button when no object is selected.");
			}
			if (LE_GUIInterface.Instance.delegates.SetIsSelectedObjectSleepPropertyInteractable == null)
			{
				Debug.LogWarning("LE_LogicObjects: you have not set the LE_GUIInterface.delegates.SetIsSelectedObjectSleepPropertyInteractable delegate! It might confuse players if you have a property menu, which is always editable, but applied only to some objects. Set this delegate to disable the 'Is Sleep On Start' property menu when the selected object does not support it.");
			}
			if (LE_GUIInterface.Instance.delegates.SetSelectedObjectIsSleepOnStartPropertyValue == null)
			{
				Debug.LogWarning("LE_LogicObjects: you have not set the LE_GUIInterface.delegates.SetIsSelectedObjectSleepPropertyValue delegate! The value of this property can change every time when an object is selected. Set this delegate to get updates for this property.");
			}
			if (LE_GUIInterface.Instance.delegates.SetIsSelectedObjectColorPropertyInteractable == null)
			{
				Debug.LogWarning("LE_LogicObjects: you have not set the LE_GUIInterface.delegates.SetIsSelectedObjectColorPropertyInteractable delegate! It might confuse players if you have a property menu, which is always editable, but applied only to some objects. Set this delegate to disable the 'Color' property menu when the selected object does not support it.");
			}
			if (LE_GUIInterface.Instance.delegates.SetSelectedObjectColorPropertyValue == null)
			{
				Debug.LogWarning("LE_LogicObjects: you have not set the LE_GUIInterface.delegates.SetSelectedObjectColorPropertyValue delegate! The value of this property can change every time when an object is selected. Set this delegate to get updates for this property.");
			}
			if (LE_GUIInterface.Instance.delegates.SetSelectedObjectVariationPropertyValue == null)
			{
				Debug.LogWarning("LE_LogicObjects: you have not set the LE_GUIInterface.delegates.SetSelectedObjectVariationPropertyValue delegate! The value of this property can change every time when an object is selected. Set this delegate to get updates for this property.");
			}
		}

		public override void Destroy ()
		{
			// unregister from events
			if (LE_GUIInterface.Instance != null)
			{
				LE_GUIInterface.Instance.events.OnObjectEditSpaceBtn -= OnObjectEditSpaceBtn;
				LE_GUIInterface.Instance.events.OnObjectEditModeBtn -= OnObjectEditModeBtn;
				LE_GUIInterface.Instance.events.OnObjectSelectDraggable -= OnObjectSelectDraggable;
				LE_GUIInterface.Instance.events.OnSelectedObjectFocusBtn -= OnSelectedObjectFocusBtn;
				LE_GUIInterface.Instance.events.OnSelectedObjectDuplicateBtn -= OnSelectedObjectDuplicateBtn;
				LE_GUIInterface.Instance.events.OnSelectedObjectDeleteBtn -= OnSelectedObjectDeleteBtn;
				LE_GUIInterface.Instance.events.OnSelectedObjectIsSleepOnStartChanged -= OnSelectedObjectIsSleepOnStartChanged;
				LE_GUIInterface.Instance.events.OnSelectedObjectColorChanged -= OnSelectedObjectColorChanged;
				LE_GUIInterface.Instance.events.OnSelectedObjectVariationIndexChanged -= OnSelectedObjectVariationIndexChanged;
			}
		}

		public override void Update()
		{
			// update selected object buttons
			bool isObjectSelected = m_GUI3dObject.SelectedObject != null;
			bool isObjectDuplicatable = isObjectSelected && m_GUI3dObject.IsObjectPlaceable(m_GUI3dObject.SelectedObject, m_GUI3dObject.SelectedObject.name);
			if (LE_GUIInterface.Instance.delegates.SetIsSelectedObjectFocusBtnInteractable != null)
			{
				LE_GUIInterface.Instance.delegates.SetIsSelectedObjectFocusBtnInteractable(isObjectSelected);
			}
			if (LE_GUIInterface.Instance.delegates.SetIsSelectedObjectDuplicateBtnInteractable != null)
			{
				LE_GUIInterface.Instance.delegates.SetIsSelectedObjectDuplicateBtnInteractable(isObjectDuplicatable);
			}
			if (LE_GUIInterface.Instance.delegates.SetIsSelectedObjectDeleteBtnInteractable != null)
			{
				LE_GUIInterface.Instance.delegates.SetIsSelectedObjectDeleteBtnInteractable(isObjectSelected);
			}
			if (LE_GUIInterface.Instance.delegates.SetIsSelectedPrefabFindBtnInteractable != null)
			{
				LE_GUIInterface.Instance.delegates.SetIsSelectedPrefabFindBtnInteractable(m_GUI3dObject.IsSceneInstanceFound);
			}

			// update selected object Is Sleep On Start property
			bool isSleepingStartPropertyInteractable = isObjectSelected && m_GUI3dObject.SelectedObject.IsRigidbodySleepingStartEditable && m_GUI3dObject.SelectedObject.IsWithRigidbodies;
			if (LE_GUIInterface.Instance.delegates.SetIsSelectedObjectSleepPropertyInteractable != null)
			{
				LE_GUIInterface.Instance.delegates.SetIsSelectedObjectSleepPropertyInteractable(isSleepingStartPropertyInteractable);
			}
			if (isSleepingStartPropertyInteractable && LE_GUIInterface.Instance.delegates.SetSelectedObjectIsSleepOnStartPropertyValue != null)
			{
				LE_GUIInterface.Instance.delegates.SetSelectedObjectIsSleepOnStartPropertyValue(m_GUI3dObject.SelectedObject.IsRigidbodySleepingStart);
			}
			// update selected object Color property
			bool isColorPropertyInteractable = isObjectSelected && m_GUI3dObject.SelectedObject.IsWithColorProperty;
			if (LE_GUIInterface.Instance.delegates.SetIsSelectedObjectColorPropertyInteractable != null)
			{
				LE_GUIInterface.Instance.delegates.SetIsSelectedObjectColorPropertyInteractable(isColorPropertyInteractable);
			}
			if (isColorPropertyInteractable && LE_GUIInterface.Instance.delegates.SetSelectedObjectColorPropertyValue != null)
			{
				LE_GUIInterface.Instance.delegates.SetSelectedObjectColorPropertyValue(m_GUI3dObject.SelectedObject.ColorProperty);
			}
			// update selected object Variations property
			if (LE_GUIInterface.Instance.delegates.SetSelectedObjectVariationPropertyValue != null)
			{
				if (isObjectSelected)
				{
					LE_GUIInterface.Instance.delegates.SetSelectedObjectVariationPropertyValue(m_GUI3dObject.SelectedObject.VariationsDefaultIndex, m_GUI3dObject.SelectedObject.Variations);
				}
				else
				{
					LE_GUIInterface.Instance.delegates.SetSelectedObjectVariationPropertyValue(0, null);
				}
			}
		}

// STATIC LOGIC -------------------------------------------------------------------------------------------------------------------

		public static void OnNewObjectSnapped(LE_GUI3dObject p_gui3d, LE_Object p_newObject, S_SnapToObjectEventArgs p_args)
		{
			if (p_newObject != null)
			{
				LE_Object sourceObj = p_args.Source.GetComponentInParent<LE_Object>();
				LE_Object destinationObj = p_args.NewInstance.GetComponent<LE_Object>();
				// mark source snap point as used
				p_gui3d.MarkSnapPointAsUsed(sourceObj, destinationObj, p_args.Source);
				// mark destination snap point as used
				if (destinationObj.RootSnapPointIndex != -1)
				{
					p_gui3d.MarkSnapPointAsUsed(destinationObj, sourceObj, destinationObj.RootSnapPointIndex);
				}
				// setup new object
				OnNewObjectPlaced(p_gui3d, p_newObject);
			}
		}

		public static void DeleteObject(LE_GUI3dObject p_gui3d, LE_Object p_selectedObject)
		{
			if (p_selectedObject != null)
			{
				// if this object was snapped to any other object then reactivate the snap point to which this object was attached
				p_gui3d.ReactivateSnapPoints(p_selectedObject.UID, p_selectedObject.ObjectSnapPoints.Length);
				// destroy game object
				GameObject.Destroy(p_selectedObject.gameObject);
				// some script could search this kind of objects -> mark as deleted
				p_selectedObject.name = "deleted";
				// IsObjectPlaceable could have changed
				p_gui3d.UpdateIsObjectPlaceable();
				// notify listeners that the level data was changed
				if (LE_EventInterface.OnChangeLevelData != null)
				{
					LE_EventInterface.OnChangeLevelData(p_selectedObject.gameObject, new LE_LevelDataChangedEvent(LE_ELevelDataChangeType.OBJECT_DELETE));
				}
			}
		}

		public static LE_Object PlaceObject(LE_GUI3dObject p_gui3d, LE_Object p_prefab, Vector3 p_position, Quaternion p_rotation, Vector3 p_scale, string p_objectResourcePath, bool p_isDestroyClonedScripts)
		{
			return PlaceObject(p_gui3d, p_prefab, p_position, p_rotation, p_scale, p_objectResourcePath, p_isDestroyClonedScripts, -1);
		}

		public static LE_Object PlaceObject(LE_GUI3dObject p_gui3d, LE_Object p_prefab, Vector3 p_position, Quaternion p_rotation, Vector3 p_scale, string p_objectResourcePath, bool p_isDestroyClonedScripts, int p_customUID)
		{
			LE_Object instance = InstantiateObject(p_prefab, p_position, p_rotation, p_scale, p_objectResourcePath);
			if (p_customUID > 0)
			{
				instance.UID = p_customUID;
			}
			if (p_isDestroyClonedScripts)
			{
				// remove cloned LE_ObjectEditHandle
				LE_ObjectEditHandle handle = instance.GetComponentInChildren<LE_ObjectEditHandle>();
				if (handle != null) { GameObject.Destroy(handle.gameObject); }
				// remove cloned S_SnapToWorld
				S_SnapToWorld worldSnap = instance.GetComponent<S_SnapToWorld>();
				if (worldSnap != null) { GameObject.Destroy(worldSnap); }
				// remove cloned S_SnapToGrid
				S_SnapToGrid gridSnap = instance.GetComponent<S_SnapToGrid>();
				if (gridSnap != null) { GameObject.Destroy(gridSnap); }
				// remove cloned S_SnapToObject
				S_SnapToObject[] objectSnapArray = instance.GetComponentsInChildren<S_SnapToObject>(true);
				for (int i=0; i<objectSnapArray.Length; i++)
				{
					LE_ObjectSnapPoint.DestroySnapSystem(objectSnapArray[i]);
				}
				// remove cloned UtilityOnDestroyHandler
				UtilityOnDestroyHandler destroyHandler = instance.GetComponent<UtilityOnDestroyHandler>();
				if (destroyHandler != null) { destroyHandler.DestroyWithoutHandling(); }
				
			}
			OnNewObjectPlaced(p_gui3d, instance);
			return instance;
		}

		public static LE_Object InstantiateObject(LE_Object p_prefab, Vector3 p_position, Quaternion p_rotation, Vector3 p_scale, string p_objectResourcePath)
		{
			LE_Object instance = (LE_Object)GameObject.Instantiate(p_prefab);
			instance.name = p_objectResourcePath;
			instance.transform.position = p_position;
			instance.transform.rotation = p_rotation;
			instance.transform.localScale = p_scale;
			return instance;
		}

		public static void OnNewObjectPlaced(LE_GUI3dObject p_gui3d, LE_Object p_newInstance)
		{
			ApplyRandomity(p_newInstance);
			ApplyColor(p_newInstance, p_newInstance.ColorProperty);
			ApplyVariation(p_newInstance, p_newInstance.VariationsDefaultIndex);
			p_newInstance.SolveCollisionAndDeactivateRigidbody(); // solve placement of rigidbodies
			AddSnappingScripts(p_gui3d, p_newInstance);
			SelectNewObjectAndNotifyListeners(p_gui3d, p_newInstance);
		}

		public static void SelectNewObjectAndNotifyListeners(LE_GUI3dObject p_gui3d, LE_Object p_newInstance)
		{
			// select new object
			p_gui3d.SelectObject(p_newInstance);
			// check if more objects of this kind can be placed
			p_gui3d.UpdateIsObjectPlaceable();
			// notify listeners that the level data was changed
			if (LE_EventInterface.OnChangeLevelData != null)
			{
				LE_EventInterface.OnChangeLevelData(p_newInstance, new LE_LevelDataChangedEvent(LE_ELevelDataChangeType.OBJECT_PLACE));
			}
			// notify listeners that an object has been placed
			if (LE_EventInterface.OnObjectPlaced != null)
			{
				LE_EventInterface.OnObjectPlaced(p_gui3d, new LE_ObjectPlacedEvent(p_newInstance));
			}
		}

		public static void ApplyRandomity(LE_Object p_newInstance)
		{
			// apply rotation randomity
			Vector3 rndRotation = new Vector3(
				Mathf.Clamp(p_newInstance.RotationRndEulerX*0.5f, 0f, 360f),
				Mathf.Clamp(p_newInstance.RotationRndEulerY*0.5f, 0f, 360f),
				Mathf.Clamp(p_newInstance.RotationRndEulerZ*0.5f, 0f, 360f));
			if (rndRotation.sqrMagnitude > 0.0001f)
			{
				p_newInstance.transform.localEulerAngles += Vector3.Scale(rndRotation, Random.insideUnitSphere);
			}
			// apply scale randomity
			if (p_newInstance.UniformScaleRnd > 0)
			{
				Vector3 axes = new Vector3(
					p_newInstance.IsScaleableOnX?1f:0f,
					p_newInstance.IsScaleableOnY?1f:0f,
					p_newInstance.IsScaleableOnZ?1f:0f);
				p_newInstance.transform.localScale += axes * p_newInstance.UniformScaleRnd * (Random.value - 0.5f);
			}
		}

		public static void ApplyColor(LE_Object p_instance, Color p_color)
		{
			if (p_instance.IsWithColorProperty)
			{
				p_instance.ColorProperty = p_color;
			}
		}

		public static void ApplyVariation(LE_Object p_instance, int p_variationIndex)
		{
			if (p_instance.Variations.Length == 1)
			{
				p_instance.Variations[0].Apply(p_instance);
				ApplyColor(p_instance, p_instance.ColorProperty); // apply color to the variation
			}
			else if (p_instance.Variations.Length > 1)
			{
				if (p_variationIndex < 0 || p_variationIndex >= p_instance.Variations.Length)
				{
					Debug.LogError("LE_LogicObjects: ApplyVariation: p_variationIndex '"+p_variationIndex+"' is out of bounds [0,"+(p_instance.Variations.Length-1)+"]");
					return;
				}
				p_instance.VariationsDefaultIndex = p_variationIndex;
				p_instance.Variations[p_variationIndex].Apply(p_instance);
				ApplyColor(p_instance, p_instance.ColorProperty); // apply color to the variation
			}
		}

		public static void AddSnappingScripts(LE_GUI3dObject p_gui3d, LE_Object p_newObject)
		{
			if (p_newObject != null)
			{
				// handle snapping
				if (p_newObject.SnapType == LE_Object.ESnapType.SNAP_TO_OBJECT) // object snapping
				{
					AddObjectSnapping(p_gui3d, p_newObject);
				}
				else if (p_newObject.SnapType == LE_Object.ESnapType.SNAP_TO_TERRAIN) // terrain snapping
				{
					AddTerrainSnapping(p_gui3d, p_newObject);
				}
				else
				{
					if (p_newObject.SnapType == LE_Object.ESnapType.SNAP_TO_3D_GRID || p_newObject.SnapType == LE_Object.ESnapType.SNAP_TO_2D_GRID_AND_TERRAIN)
					{
						// add grid snapping
						AddGridSnapping(p_gui3d, p_newObject, false);
					}
				}
			}
			else
			{
				Debug.LogError("LE_GUI3dObject: AddSnappingScripts: passed object is null!");
			}
		}

		public static void AddObjectSnapping(LE_GUI3dObject p_gui3d, LE_Object p_newObject)
		{
			for (int j = 0; j < p_newObject.ObjectSnapPoints.Length; j++)
			{
				if (p_newObject.ObjectSnapPoints[j] != null)
				{
					Material matLine = null;
					Material matFill = null;
					if (p_newObject.IsDrawSnapToObjectUI)
					{
						matLine = (Material)Resources.Load("SnapToObjectUIMaterial_Line");
						matFill = (Material)Resources.Load("SnapToObjectUIMaterial_Fill");
					}
					S_SnapToObject snapInstance = p_newObject.ObjectSnapPoints[j].InstatiateSnapSystem((GameObject)GameObject.Instantiate(Resources.Load("ObjectSnapButtonVisuals")), p_newObject.IsDrawSnapToObjectUI, matLine, matFill);
					if (snapInstance != null)
					{
						string snapPointUID = GetSnapPointUID(p_newObject.UID, j);
						p_gui3d.AddSnapPoint(snapPointUID, snapInstance);
						// deactivate if snap UI is hidden right now
						if (!p_gui3d.IsSnapToObjectActive)
						{
							snapInstance.gameObject.SetActive(false);
						}
						// restore the already snapped object states
						p_gui3d.LoadSnapCounter(snapPointUID, snapInstance);
					}
				}
				else
				{
					Debug.LogError("LE_GUI3dObject: AddObjectSnapping: object '" + p_newObject.name + "' has a nullpointer in the ObjectSnapPoints array at index '" + j + "'!");
				}
			}
		}

		public static string GetSnapPointUID(int p_objectUID, int p_snapPointIndex)
		{
			return p_objectUID + "_" + p_snapPointIndex;
		}
		
		public static void AddTerrainSnapping(LE_GUI3dObject p_gui3d, LE_Object p_newObject)
		{
			// add and configurate world snapper
			S_SnapToWorld worldSnap = p_newObject.gameObject.AddComponent<S_SnapToWorld>();
			worldSnap.SnapToLayers = 1 << p_gui3d.TERRAIN_LAYER;
			worldSnap.SnapFrameRate = -1;
			worldSnap.IsRotationSnap = p_newObject.IsPlacementRotationByNormal;
			// snap after create
			worldSnap.DoSnap();
			// update snapper every time the terrain is changed or the object is moved
			System.EventHandler<LE_LevelDataChangedEvent> snapUpdateFunct = (object p_object, LE_LevelDataChangedEvent p_args)=>
			{
				if (p_args.ChangeType == LE_ELevelDataChangeType.TERRAIN_HEIGHTS ||
				    (p_object is LE_ObjectEditHandle &&
				 	(((LE_ObjectEditHandle)p_object).Target == p_newObject.transform || ((LE_ObjectEditHandle)p_object).transform.IsChildOf(p_newObject.transform))))
				{
					worldSnap.DoSnap();
				}
			};
			LE_EventInterface.OnChangeLevelData += snapUpdateFunct;
			p_newObject.gameObject.AddComponent<UtilityOnDestroyHandler>().m_onDestroy = ()=>
			{
				LE_EventInterface.OnChangeLevelData -= snapUpdateFunct;
			};
		}
		
		public static void AddGridSnapping(LE_GUI3dObject p_gui3d, LE_Object p_newObject, bool p_isPreview)
		{
			S_SnapToGrid gridSnap = p_newObject.gameObject.AddComponent<S_SnapToGrid>();
			gridSnap.GridOffset = p_newObject.SnapGridOffset;
			gridSnap.GridCellSize = p_newObject.SnapGridCellSize;
			gridSnap.SnapCondition = p_isPreview ? S_SnapToGrid.ESnapCondition.ON_UPDATE : S_SnapToGrid.ESnapCondition.WHEN_STILL;
			gridSnap.IsInstantSnap = p_isPreview;
			gridSnap.IsSnapAxisXRotation = gridSnap.IsSnapAxisYRotation = gridSnap.IsSnapAxisZRotation = false; // under construction
			if (p_newObject.SnapType == LE_Object.ESnapType.SNAP_TO_2D_GRID_AND_TERRAIN)
			{
				// disable y axis snapping and activate terrain snapping
				gridSnap.IsSnapAxisY = false;
				if (!p_isPreview)
				{
					AddTerrainSnapping(p_gui3d, p_newObject);
				}
			}
		}

// EVENT HANDLERS -----------------------------------------------------------------------------------------------------------------

		private void OnObjectEditSpaceBtn(object p_obj, LE_GUIInterface.EventHandlers.ObjectEditSpaceEventArgs p_args)
		{
			m_GUI3dObject.ObjectEditSpace = p_args.EditSpace;
		}

		private void OnObjectEditModeBtn(object p_obj, LE_GUIInterface.EventHandlers.ObjectEditModeEventArgs p_args)
		{
			m_GUI3dObject.ObjectEditMode = p_args.EditMode;
		}

		private void OnObjectSelectDraggable(object p_obj, LE_GUIInterface.EventHandlers.ObjectSelectDraggableEventArgs p_args)
		{
			m_GUI3dObject.SetDraggableObject(p_args.ObjPrefab, p_args.ResourcePath);
		}

		private void OnSelectedObjectFocusBtn(object p_obj, System.EventArgs p_args)
		{
			if (m_GUI3dObject.SelectedObject != null)
			{
				m_GUI3dObject.Focus();
			}
			else
			{
				Debug.LogError("LE_LogicObject: OnSelectedObjectFocusBtn: you have triggered the focus button behaviour, but there is no object selected! You should listen to the LE_GUIInterface.delegates.SetIsSelectedObjectFocusBtnInteractable and change the button's state accordingly. This will prevent users from getting irritated by not working buttons.");
			}
		}

		private void OnSelectedObjectDuplicateBtn(object p_obj, System.EventArgs p_args)
		{
			if (m_GUI3dObject.SelectedObject != null)
			{
				m_GUI3dObject.CloneObject();
			}
			else
			{
				Debug.LogError("LE_LogicObject: OnSelectedObjectDuplicateBtn: you have triggered the duplicate button behaviour, but there is no object selected! You should listen to the LE_GUIInterface.delegates.SetIsSelectedObjectDuplicateBtnInteractable and change the button's state accordingly. This will prevent users from getting irritated by not working buttons.");
			}
		}


		private void OnSelectedPrefabFindBtn(object p_obj, System.EventArgs p_args)
		{
			if (m_GUI3dObject.SelectedPrefab != null)
			{
				m_GUI3dObject.SelectNFocusPrefabInstanceInScene();
			}
			else
			{
				Debug.LogError("LE_LogicObject: OnSelectedPrefabFindBtn: you have triggered the find button behaviour, but there is no prefab selected! You should listen to the LE_GUIInterface.delegates.SetIsSelectedPrefabFindBtnInteractable and change the button's state accordingly. This will prevent users from getting irritated by not working buttons.");
			}
		}

		private void OnSelectedObjectDeleteBtn(object p_obj, System.EventArgs p_args)
		{
			if (m_GUI3dObject.SelectedObject != null)
			{
				if (LE_GUIInterface.Instance.delegates.ShowPopupConfirmDeleteObject != null)
				{
					ConfirmDelete();
				}
				else
				{
					Debug.LogError("LE_LogicObject: OnSelectedObjectDeleteBtn: you have triggered the delete button behaviour, but the LE_GUIInterface.delegates.ShowPopupConfirmDeleteObject delegate is not set! Set this delegate to make the delete button work!");
				}
			}
			else
			{
				Debug.LogError("LE_LogicObject: OnSelectedObjectDeleteBtn: you have triggered the delete button behaviour, but there is no object selected! You should listen to the LE_GUIInterface.delegates.SetIsSelectedObjectDeleteBtnInteractable and change the button's state accordingly. This will prevent users from getting irritated by not working buttons.");
			}
		}

		private void OnSelectedObjectIsSleepOnStartChanged(object p_obj, LE_GUIInterface.EventHandlers.BoolEventArgs p_args)
		{
			if (m_GUI3dObject.SelectedObject != null && m_GUI3dObject.SelectedObject.IsRigidbodySleepingStartEditable)
			{
				bool isChanged = m_GUI3dObject.SelectedObject.IsRigidbodySleepingStart != p_args.Value;
				if (isChanged)
				{
					UR_CommandMgr.Instance.Execute(new LE_CmdChangeObjectIsSleepingStart(m_GUI3dObject.SelectedObject, p_args.Value));
					// notify listeners that the level data was changed
					if (LE_EventInterface.OnChangeLevelData != null)
					{
						LE_EventInterface.OnChangeLevelData(m_GUI3dObject.SelectedObject, new LE_LevelDataChangedEvent(LE_ELevelDataChangeType.OBJECT_RIGIDBODY_SLEEPING_START));
					}
				}
			}
			else
			{
				Debug.LogError("LE_LogicObject: OnSelectedObjectIsSleepOnStartChanged was called, but the selected object does not allow to change this property (or nothing is selected)! You should listen to the LE_GUIInterface.delegates.SetIsSelectedObjectSleepPropertyInteractable and change the UI's state accordingly. This will prevent users from getting irritated by not working buttons.");
			}
		}

		private void OnSelectedObjectColorChanged(object p_obj, LE_GUIInterface.EventHandlers.ColorEventArgs p_args)
		{
			if (m_GUI3dObject.SelectedObject != null)
			{
				bool isChanged = m_GUI3dObject.SelectedObject.ColorProperty != p_args.Value;
				if (isChanged)
				{
					UR_CommandMgr.Instance.Execute(new LE_CmdChangeObjectColor(m_GUI3dObject.SelectedObject, p_args.Value - m_GUI3dObject.SelectedObject.ColorProperty));
					// notify listeners that the level data was changed
					if (LE_EventInterface.OnChangeLevelData != null)
					{
						LE_EventInterface.OnChangeLevelData(m_GUI3dObject.SelectedObject, new LE_LevelDataChangedEvent(LE_ELevelDataChangeType.OBJECT_COLOR));
					}
				}
			}
			else
			{
				Debug.LogError("LE_LogicObject: OnSelectedObjectColorChanged was called, but the selected object does not allow to change this property (or nothing is selected)! You should listen to the LE_GUIInterface.delegates.SetIsSelectedObjectColorPropertyInteractable and change the UI's state accordingly. This will prevent users from getting irritated by not working buttons.");
			}
		}

		private void OnSelectedObjectVariationIndexChanged(object p_obj, LE_GUIInterface.EventHandlers.IntEventArgs p_args)
		{
			if (m_GUI3dObject.SelectedObject != null)
			{
				bool isChanged = m_GUI3dObject.SelectedObject.VariationsDefaultIndex != p_args.Value;
				if (isChanged)
				{
					UR_CommandMgr.Instance.Execute(new LE_CmdChangeObjectVariation(m_GUI3dObject.SelectedObject, p_args.Value));
					// notify listeners that the level data was changed
					if (LE_EventInterface.OnChangeLevelData != null)
					{
						LE_EventInterface.OnChangeLevelData(m_GUI3dObject.SelectedObject, new LE_LevelDataChangedEvent(LE_ELevelDataChangeType.OBJECT_VARIATION));
					}
				}
			}
			else
			{
				Debug.LogError("LE_LogicObject: OnSelectedObjectVariationIndexChanged was called, but the selected object does not allow to change this property (or nothing is selected)! You should listen to the LE_GUIInterface.delegates.SetSelectedObjectVariationPropertyValue and change the UI's state accordingly. This will prevent users from getting irritated by not working buttons.");
			}
		}

// LOGIC --------------------------------------------------------------------------------------------------------------------------

		private void ConfirmDelete()
		{
			// save current object edit mode and deactivate it
			LE_EObjectEditMode editModeBK = m_GUI3dObject.ObjectEditMode;
			m_GUI3dObject.ObjectEditMode = LE_EObjectEditMode.NO_EDIT;
			// show confirm delete popup
			LE_GUIInterface.Instance.delegates.ShowPopupConfirmDeleteObject((bool p_isDeleteConfirmed)=>
			{
				// delte object if confirmed
				if (p_isDeleteConfirmed)
				{
					m_GUI3dObject.Delete();
				}
				// restore the saved edit mode
				m_GUI3dObject.ObjectEditMode = editModeBK;
			});
		}
	}
}