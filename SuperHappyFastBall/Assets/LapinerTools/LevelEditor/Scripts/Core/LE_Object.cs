using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LE_LevelEditor.Events;
using UndoRedo;
using LE_LevelEditor.Commands;

namespace LE_LevelEditor.Core
{
	public class LE_Object : MonoBehaviour 
	{
		public enum ESnapType {SNAP_DISABLED, SNAP_TO_TERRAIN, SNAP_TO_OBJECT, SNAP_TO_3D_GRID, SNAP_TO_2D_GRID_AND_TERRAIN}

		private const float OUTLINE_INV_WIDTH = 300f;

		private static int s_nextFreeUID = 1;
		public static void ReportUsedUID(int p_usedUID)
		{
			s_nextFreeUID = Mathf.Max(s_nextFreeUID, p_usedUID+1);
		}
		
		[SerializeField]
		private string m_iconPath = "";
		/// <summary>
		/// Resource path of the object's icon texture, which will be displayed in the object selection. The texture will be 
		/// generated from Unity's built-in preview and this property will be set automatically later. However, if you want 
		/// to set custom icons you can use this property. There is no direct reference (only a string) to the texture here, 
		/// because there is no need to load the texture outside the level editor (no icon needed ingame).
		/// </summary>
		public string IconPath
		{
			get{ return m_iconPath; }
			set{ m_iconPath = value; }
		}
			
		[SerializeField]
		private bool m_isPlacementRotationByNormal = true;
		/// <summary>
		/// if true the object will be rotated to fit the surface normal when placed. For example when a car is drag and 
		/// dropped on a steep hill the player will probably want it to touch the ground with all four wheels and not hang 
		/// half in the hill. With this property set to true the car will be rotated so that its up vector will be equal to 
		/// the normal of the hill. Applied also if the object is not marked as rotatable.
		/// </summary>
		public bool IsPlacementRotationByNormal
		{
			get{ return m_isPlacementRotationByNormal; }
			set{ m_isPlacementRotationByNormal = value; }
		}

		[SerializeField]
		private bool m_isSmartMove = true;
		/// <summary>
		/// A smart move handle will be displayed when this object is selected in the level editor if this property is true.
		/// </summary>
		public bool IsSmartMove
		{
			get{ return m_isSmartMove; }
			set{ m_isSmartMove = value; }
		}

		[SerializeField]
		private bool m_isMovable = true;
		/// <summary>
		/// Move handle will be displayed when this object is selected in the level editor if this property is true.
		/// </summary>
		public bool IsMovable
		{
			get{ return m_isMovable; }
			set{ m_isMovable = value; }
		}

		[SerializeField]
		private bool m_isMovableOnX = true;
		/// <summary>
		/// Move handle will have x axis if this property is true.
		/// </summary>
		public bool IsMovableOnX
		{
			get{ return m_isMovableOnX; }
			set{ m_isMovableOnX = value; }
		}

		[SerializeField]
		private bool m_isMovableOnY = true;
		/// <summary>
		/// Move handle will have y axis if this property is true.
		/// </summary>
		public bool IsMovableOnY
		{
			get{ return m_isMovableOnY; }
			set{ m_isMovableOnY = value; }
		}

		[SerializeField]
		private bool m_isMovableOnZ = true;
		/// <summary>
		/// Move handle will have z axis if this property is true.
		/// </summary>
		public bool IsMovableOnZ
		{
			get{ return m_isMovableOnZ; }
			set{ m_isMovableOnZ = value; }
		}

		[SerializeField]
		private bool m_isRotatable = true;
		/// <summary>
		/// Rotate handle will be displayed when this object is selected in the level editor if this property is true.
		/// </summary>
		public bool IsRotatable
		{
			get{ return m_isRotatable; }
			set{ m_isRotatable = value; }
		}

		[SerializeField]
		private bool m_isRotatableAroundX = true;
		/// <summary>
		/// Rotate handle will have x axis if this property is true.
		/// </summary>
		public bool IsRotatableAroundX
		{
			get{ return m_isRotatableAroundX; }
			set{ m_isRotatableAroundX = value; }
		}

		[SerializeField]
		private bool m_isRotatableAroundY = true;
		/// <summary>
		/// Rotate handle will have y axis if this property is true.
		/// </summary>
		public bool IsRotatableAroundY
		{
			get{ return m_isRotatableAroundY; }
			set{ m_isRotatableAroundY = value; }
		}

		[SerializeField]
		private bool m_isRotatableAroundZ = true;
		/// <summary>
		/// Rotate handle will have z axis if this property is true.
		/// </summary>
		public bool IsRotatableAroundZ
		{
			get{ return m_isRotatableAroundZ; }
			set{ m_isRotatableAroundZ = value; }
		}

		[SerializeField]
		private float m_rotationRndEulerX = -1f;
		/// <summary>
		/// If this value is greater than 0 then the euler rotation (0°-360°) on the X axis of this object will be randomized when the object is placed. 
		/// The euler rotation will be increased or decreased by a value between 0 and +- half of the value.
		/// For example, trees can use this value to make a forest look more varied.
		/// </summary>
		public float RotationRndEulerX
		{
			get{ return m_rotationRndEulerX; }
			set{ m_rotationRndEulerX = value; }
		}

		[SerializeField]
		private float m_rotationRndEulerY = -1f;
		/// <summary>
		/// If this value is greater than 0 then the euler rotation (0°-360°) on the Y axis of this object will be randomized when the object is placed. 
		/// The euler rotation will be increased or decreased by a value between 0 and +- half of the value.
		/// For example, trees can use this value to make a forest look more varied.
		/// </summary>
		public float RotationRndEulerY
		{
			get{ return m_rotationRndEulerY; }
			set{ m_rotationRndEulerY = value; }
		}

		[SerializeField]
		private float m_rotationRndEulerZ = -1f;
		/// <summary>
		/// If this value is greater than 0 then the euler rotation (0°-360°) on the Z axis of this object will be randomized when the object is placed. 
		/// The euler rotation will be increased or decreased by a value between 0 and +- half of the value.
		/// For example, trees can use this value to make a forest look more varied.
		/// </summary>
		public float RotationRndEulerZ
		{
			get{ return m_rotationRndEulerZ; }
			set{ m_rotationRndEulerZ = value; }
		}

		[SerializeField]
		private bool m_isScaleable = true;
		/// <summary>
		/// Scale handle will be displayed when this object is selected in the level editor if this property is true.
		/// </summary>
		public bool IsScaleable
		{
			get{ return m_isScaleable; }
			set{ m_isScaleable = value; }
		}

		[SerializeField]
		private bool m_isScaleableOnX = true;
		/// <summary>
		/// Scale handle will have x axis if this property is true.
		/// </summary>
		public bool IsScaleableOnX
		{
			get{ return m_isScaleableOnX; }
			set{ m_isScaleableOnX = value; }
		}

		[SerializeField]
		private bool m_isScaleableOnY = true;
		/// <summary>
		/// Scale handle will have y axis if this property is true.
		/// </summary>
		public bool IsScaleableOnY
		{
			get{ return m_isScaleableOnY; }
			set{ m_isScaleableOnY = value; }
		}

		[SerializeField]
		private bool m_isScaleableOnZ = true;
		/// <summary>
		/// Scale handle will have z axis if this property is true.
		/// </summary>
		public bool IsScaleableOnZ
		{
			get{ return m_isScaleableOnZ; }
			set{ m_isScaleableOnZ = value; }
		}

		[SerializeField]
		private bool m_isUniformScale = false;
		/// <summary>
		/// If this property is true then the scale handle will have only one axis. 
		/// Scaling this axis will scale the object on all scaleable axes at same rate.
		/// </summary>
		public bool IsUniformScale
		{
			get{ return m_isUniformScale; }
			set{ m_isUniformScale = value; }
		}

		[SerializeField]
		private float m_uniformScaleRnd = -1f;
		/// <summary>
		/// If this value is greater than 0 then the scale of this object will be randomized when the object is placed. 
		/// The random scale change is always uniform, but applied only on scaleable axes. The scale will be increased or 
		/// decreased by a value between 0 and +- half of the value. For example, trees can use this value to make a forest 
		/// look more varied.
		/// </summary>
		public float UniformScaleRnd
		{
			get{ return m_uniformScaleRnd; }
			set{ m_uniformScaleRnd = value; }
		}

		[SerializeField]
		private bool m_isRigidbodySleepingStart = true;
		/// <summary>
		/// If true and a rigidbody is attached to this object or its children then all attached rigidbodies will go into 
		/// the sleep state (Rigidbody.Sleep()) when the level is loaded. This will increase performance in the first few 
		/// frames if you level has many rigidbodies. For example if you have a pile of crates you don't want them to fall 
		/// down before the player touches them, therefore you will want them to sleep. However, if you have a huge snowball 
		/// that has to roll behind the player you will probably not want to send it to sleep.
		/// </summary>
		public bool IsRigidbodySleepingStart
		{
			get{ return m_isRigidbodySleepingStart; }
			set{ m_isRigidbodySleepingStart = value; }
		}

		[SerializeField]
		private bool m_isRigidbodySleepingStartEditable = true;
		/// <summary>
		/// When this object is selected in the editor then a property is presented to the end user that allows to change 
		/// 'IsRigidbodySleepingStart' if this property is true.
		/// </summary>
		public bool IsRigidbodySleepingStartEditable
		{
			get{ return m_isRigidbodySleepingStartEditable; }
			set{ m_isRigidbodySleepingStartEditable = value; }
		}

		[SerializeField]
		private int m_maxInstancesInLevel = 0;
		/// <summary>
		/// If value is not 0 then the number of instances of this object in a level will be limited to the value of this 
		/// property. For example if some of your objects are very detailed then you will want to limit the maximal count, 
		/// because of performance reasons. Another example is the start position of the player, since it should exist only 
		/// once. No more objects of this type can be drag&dropped or cloned into the level once the limit is reached.
		/// </summary>
		public int MaxInstancesInLevel
		{
			get{ return m_maxInstancesInLevel; }
			set{ m_maxInstancesInLevel = value; }
		}

		[SerializeField]
		private bool m_isWithColorProperty = false;
		/// <summary>
		/// A color picker will be displayed in the level editor if this value is set to true. Additionally, a default color 
		/// can be specified in the 'Color' property. Modifies the '_Color' property of all materials in all renderers 
		/// including those in children.
		/// </summary>
		public bool IsWithColorProperty
		{
			get{ return m_isWithColorProperty; }
			set{ m_isWithColorProperty = value; }
		}

		[SerializeField]
		private ESnapType m_snapType = ESnapType.SNAP_DISABLED;
		/// <summary>
		/// Choose among the available snap types: SNAP_DISABLED, SNAP_TO_TERRAIN, SNAP_TO_OBJECT, SNAP_TO_3D_GRID, 
		/// SNAP_TO_2D_GRID_AND_TERRAIN
		/// </summary>
		public ESnapType SnapType
		{
			get{ return m_snapType; }
			set{ m_snapType = value; }
		}

		[SerializeField]
		private int m_rootSnapPointIndex = -1;
		/// <summary>
		/// If set to a value different from '-1' then the snap point at given index will be deactivated when this object 
		/// is created through being snapped to another object.
		/// </summary>
		public int RootSnapPointIndex
		{
			get{ return m_rootSnapPointIndex; }
			set{ m_rootSnapPointIndex = value; }
		}

		[SerializeField]
		private LE_ObjectSnapPoint[] m_objectSnapPoints = new LE_ObjectSnapPoint[0];
		/// <summary>
		/// This array contains definitions of snap points for the 'Snap To Object' feature. A snap point defines the root 
		/// location of the snap point and the objects which can be snapped to this point. The local transformation in the 
		/// space of the point can be set for each snap object individually.
		/// </summary>
		public LE_ObjectSnapPoint[] ObjectSnapPoints
		{
			get{ return m_objectSnapPoints; }
			set{ m_objectSnapPoints = value; }
		}

		[SerializeField]
		private bool m_isDrawSnapToObjectUI = true;
		/// <summary>
		/// If set to 'true' then the built in UI for the snap object selection will be drawn. It uses the 'SnapToObjectUI' material from the resource folder.
		/// </summary>
		public bool IsDrawSnapToObjectUI
		{
			get{ return m_isDrawSnapToObjectUI; }
			set{ m_isDrawSnapToObjectUI = value; }
		}

		[SerializeField]
		private Vector3 m_snapGridOffset = Vector3.zero;
		/// <summary>
		/// Defines the offset of the snap grid for SNAP_TO_3D_GRID and SNAP_TO_2D_GRID_AND_TERRAIN
		/// </summary>
		public Vector3 SnapGridOffset
		{
			get{ return m_snapGridOffset; }
			set{ m_snapGridOffset = value; }
		}

		[SerializeField]
		private Vector3 m_snapGridCellSize = Vector3.one*10f;
		/// <summary>
		/// Defines the cell size of the snap grid for SNAP_TO_3D_GRID and SNAP_TO_2D_GRID_AND_TERRAIN
		/// </summary>
		public Vector3 SnapGridCellSize
		{
			get{ return m_snapGridCellSize; }
			set{ m_snapGridCellSize = value; }
		}

		[SerializeField]
		private bool m_isLevelStreaming = false;
		/// <summary>
		/// If true then this object will be instantiated only if it is closer to the 'Main Camera' than the given 
		/// instantiate distance. This way a level can be loaded stepwise. Additionally, performance is improved by 
		/// destroying this object when it is further away from the camera than the given destroy distance.
		/// </summary>
		public bool IsLevelStreaming
		{
			get{ return m_isLevelStreaming; }
			set{ m_isLevelStreaming = value; }
		}

		[SerializeField]
		private float m_levelStreamingInstantiateDistance = 100f;
		/// <summary>
		/// This object will be instantiated if it is closer to the camera than the given distance. This value is used ingame.
		/// </summary>
		public float LevelStreamingInstantiateDistance
		{
			get{ return m_levelStreamingInstantiateDistance; }
			set{ m_levelStreamingInstantiateDistance = value; }
		}

		[SerializeField]
		private float m_levelStreamingDestroyDistance = 100f;
		/// <summary>
		/// This object will be destroyed if it is further away from the camera than the given distance. This value is used ingame.
		/// </summary>
		public float LevelStreamingDestroyDistance
		{
			get{ return m_levelStreamingDestroyDistance; }
			set{ m_levelStreamingDestroyDistance = value; }
		}

		[SerializeField]
		private float m_levelStreamingInstantiateDistanceInEditor = 200f;
		/// <summary>
		/// This object will be instantiated if it is closer to the camera than the given distance. This value is used in the editor.
		/// </summary>
		public float LevelStreamingInstantiateDistanceInEditor
		{
			get{ return m_levelStreamingInstantiateDistanceInEditor; }
			set{ m_levelStreamingInstantiateDistanceInEditor = value; }
		}
		
		[SerializeField]
		private float m_levelStreamingDestroyDistanceInEditor = 200f;
		/// <summary>
		/// This object will be destroyed if it is further away from the camera than the given distance. This value is used in the editor.
		/// </summary>
		public float LevelStreamingDestroyDistanceInEditor
		{
			get{ return m_levelStreamingDestroyDistanceInEditor; }
			set{ m_levelStreamingDestroyDistanceInEditor = value; }
		}

		[SerializeField]
		private int m_levelStreamingUpdateFrequency = 15;
		/// <summary>
		/// Defines the number of skipped frames between streamed object state updates. A low value should be used for fast 
		/// games. However a low value (e.g. '0') will have performance implications if the number of streamed objects is 
		/// very high (>300). A slow game can have a higher value. The value of '15' (with 30 FPS in average) means that 
		/// streamed objects will be checked twice in a second which is fast enough for slow games.
		/// </summary>
		public int LevelStreamingUpdateFrequency
		{
			get{ return m_levelStreamingUpdateFrequency; }
			set{ m_levelStreamingUpdateFrequency = value; }
		}

		[SerializeField]
		private bool m_isTransformationCachedOnDestroyWhenLevelStreaming = true;
		/// <summary>
		/// If this property is true then position, rotation and scale are cached when spawned objects are destroyed because 
		/// they are too far away. The object will be instantiated with the transformation, which it had when it was 
		/// destroyed, when it is close enough again. 
		/// </summary>
		public bool IsTransformationCachedOnDestroyWhenLevelStreaming
		{
			get{ return m_isTransformationCachedOnDestroyWhenLevelStreaming; }
			set{ m_isTransformationCachedOnDestroyWhenLevelStreaming = value; }
		}

		[SerializeField]
		private LE_ObjectVariationMaterial[] m_variationsMaterial = new LE_ObjectVariationMaterial[0];
		[SerializeField]
		private LE_ObjectVariationActivateDeactivate[] m_variationsActivateDeactivate = new LE_ObjectVariationActivateDeactivate[0];
		/// <summary>
		/// The player will be able to choose between these variations if he selects an instance of this level object in the scene (similar to color property).
		/// The Multiplatform Runtime Level Editor comes with two built-in variation classes.
		/// The 'material variation' will replace all materials of the selected renderers in this level objects.
		/// For example, different textures could be used for the materials of a car: one with colors only, one with stripes and one with damage hints.
		/// The 'activate/deactivate sub object variation' will activate/deactivate sub objects of this level object.
		/// For example, different meshes could be used for a car: one mesh for the normal car, additional meshes for tunning and a different mesh for a destroyed car.
		/// Furthermore, the variation system is generic and allows you to create custom variations. Take a look at the LE_ObjectVariationBase class and its derived classes.
		/// If you want to implement further variations and you need assistance, please contact me in the forum:
		/// http://forum.unity3d.com/threads/multiplatform-runtime-level-editor-any-one-interested.250920/
		/// </summary>
		public LE_ObjectVariationBase[] Variations
		{
			get
			{
				if (m_variationsMaterial.Length > 0)
				{
					return m_variationsMaterial;
				}
				else
				{
					return m_variationsActivateDeactivate;
				}
			}
		}
		public LE_ObjectVariationMaterial[] VariationsMaterial
		{
			get{ return m_variationsMaterial; }
			set{ m_variationsMaterial = value; }
		}
		public LE_ObjectVariationActivateDeactivate[] VariationsActivateDeactivate
		{
			get{ return m_variationsActivateDeactivate; }
			set{ m_variationsActivateDeactivate = value; }
		}

		[SerializeField]
		private int m_variationsDefaultIndex = 0;
		/// <summary>
		/// When a new object is placed in the scene, then the variation from the Variations property with the index VariationsDefaultIndex will be selected.
		/// This property is ignored if no variations or exactly one variation were provided for the Variations property.
		/// </summary>
		public int VariationsDefaultIndex
		{
			get{ return m_variationsDefaultIndex; }
			set{ m_variationsDefaultIndex = value; }
		}

		// ------------------------------------------------------------------------------------------
		// BEGIN: members needed for editor object editing
		// ------------------------------------------------------------------------------------------
		private int m_UID = s_nextFreeUID++;
		public int UID
		{
			get{ return m_UID; }
			set{ m_UID = value; }
		}

		private Material m_outlineMaterial = null;
		private LE_SubMeshCombiner[] m_subMeshCombiners = null;
		private bool m_isSelectedChanged = false;
		private bool m_isSelected = false;
		public bool IsSelected
		{
			get { return m_isSelected; }
			set
			{
				if (m_isSelected != value)
				{
					m_isSelected = value;
					m_isSelectedChanged = true;
				}
			}
		}

		private LE_EObjectEditSpace m_editSpace = LE_EObjectEditSpace.SELF;
		public LE_EObjectEditSpace EditSpace
		{
			set{ m_editSpace = value; }
			get{ return m_editSpace; }
		}

		private LE_EObjectEditMode m_editMode = LE_EObjectEditMode.NO_EDIT;
		public LE_EObjectEditMode EditMode
		{
			set{ m_editMode = value; }
			get{ return m_editMode; }
		}

		[SerializeField]
		private Color m_color = Color.white;
		/// <summary>
		/// Default color. Will be applied only if 'Is With Color Property' is set to true. Modifies the '_Color' property 
		/// of all materials in all renderers including those in children. 'Is With Color Property' is ignored and color
		/// is always applied if property is set on runtime.
		/// </summary>
		public Color ColorProperty
		{
			get
			{
				return m_color;
			}
			set
			{
				m_color = value;
	#if UNITY_EDITOR
				if (Application.isPlaying && !UnityEditor.EditorUtility.IsPersistent(this))
	#endif
				{
					Renderer[] renderers = GetComponentsInChildren<Renderer>();
					for (int i = 0; i < renderers.Length; i++)
					{
						Renderer r = renderers[i];
						if (r.transform.parent == null || !r.transform.parent.name.StartsWith("ObjectEditHandle"))
						{
							Material[] materials = r.materials;
							for (int j = 0; j < materials.Length; j++)
							{
								Material m = materials[j];
								if (m != null &&
								    (m_outlineMaterial == null || m_outlineMaterial.shader.name != m.shader.name))
								{
									if (m.HasProperty("_Color"))
									{
										m.SetColor("_Color", m_color);
									}
								}
								else
								{
									Destroy(m);
									materials[j] = m_outlineMaterial;
								}
							}
							r.sharedMaterials = materials;
						}
					}
				}
			}
		}

		private bool m_isWithRigidbodies = false;
		private bool m_isRigidbodySearchDone = false;
		public bool IsWithRigidbodies
		{
			get
			{
				if (!m_isRigidbodySearchDone)
				{
					m_isRigidbodySearchDone = true;
					Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
					m_isWithRigidbodies = rigidbodies.Length > 0;
				}
				return m_isWithRigidbodies;
			}
		}

		private LE_ObjectEditHandle m_editHandle = null;
		public LE_ObjectEditHandle EditHandle { get{ return m_editHandle; } }

		private Collision m_latestCollision = null;
		private int m_solveCollisionCounter = -1;
		private bool m_isSolvingCollisions = false;

		private Rigidbody m_rigidbody = null;

		private Vector3 m_beginTransformPos;
		private Quaternion m_beginTransformRot;
		private Vector3 m_beginTransformLocalScale;
		// ------------------------------------------------------------------------------------------
		// END: members needed for editor object editing
		// ------------------------------------------------------------------------------------------

		private void Awake()
		{
			m_rigidbody = GetComponent<Rigidbody>();
		}

		public void SolveCollisionAndDeactivateRigidbody()
		{
			if (m_rigidbody != null)
			{
				m_isSolvingCollisions = true;
				m_solveCollisionCounter = 8;
				m_rigidbody.isKinematic = false;
			}
		}

		public void DeactivateRigidbody()
		{
			if (m_rigidbody != null)
			{
				m_rigidbody.isKinematic = true;
			}
		}

		public void ApplySelectionState()
		{
			// add or hide outline
			if (m_subMeshCombiners != null)
			{
				for (int i = 0; i < m_subMeshCombiners.Length; i++)
				{
					if (m_isSelected)
					{
						m_subMeshCombiners[i].ShowCombinedSubMesh();
					}
					else
					{
						m_subMeshCombiners[i].HideCombinedSubMesh();
					}
				}
			}
		}

		private void OnCollisionEnter(Collision p_collisionInfo)
		{
			m_latestCollision = p_collisionInfo;
		}

		private void OnCollisionStay(Collision p_collisionInfo)
		{
			m_latestCollision = p_collisionInfo;
		}

		private void FixedUpdate()
		{
			// solve collision
			if (m_isSolvingCollisions && m_rigidbody != null)
			{
				if (m_solveCollisionCounter >= 0)
				{
					m_solveCollisionCounter--;
					if (m_latestCollision != null && m_latestCollision.contacts.Length > 0)
					{
						m_rigidbody.velocity = Vector3.zero;
						m_rigidbody.angularVelocity = Vector3.zero;
						transform.position += m_latestCollision.contacts[0].normal*Time.deltaTime;
						m_latestCollision = null;
					}
				}
				else if (!m_rigidbody.isKinematic)
				{
					DeactivateRigidbody();
				}
			}
		}

		private void Update()
		{
			// update outline width in depending on camera distance
			if (m_outlineMaterial != null)
			{
				Camera cam = Camera.main;
				if (cam != null)
				{
					if (cam.orthographic)
					{
						m_outlineMaterial.SetFloat("_Outline", 1.5f / OUTLINE_INV_WIDTH);
						m_outlineMaterial.SetFloat("_IsOrthogonal", 1f);
					}
					else
					{
						m_outlineMaterial.SetFloat("_Outline", (cam.transform.position-transform.position).magnitude / OUTLINE_INV_WIDTH);
						m_outlineMaterial.SetFloat("_IsOrthogonal", 0f);
					}
				}
			}
			// remove edit handle if it is not needed any more
			if (m_editMode == LE_EObjectEditMode.NO_EDIT)
			{
				if (m_editHandle != null)
				{
					Destroy(m_editHandle.gameObject);
				}
			}
			// create edit handle if needed
			else if (m_editHandle == null || m_editHandle.EditMode != m_editMode)
			{
				if (m_editHandle != null)
				{
					Destroy(m_editHandle.gameObject);
				}
				// check if this edit mode is suported
				if ((m_editMode == LE_EObjectEditMode.SMART && m_isSmartMove) ||
					(m_editMode == LE_EObjectEditMode.MOVE && m_isMovable && (m_isMovableOnX || m_isMovableOnY || m_isMovableOnZ)) ||
				    (m_editMode == LE_EObjectEditMode.ROTATE && m_isRotatable && (m_isRotatableAroundX || m_isRotatableAroundY || m_isRotatableAroundZ)) ||
				    (m_editMode == LE_EObjectEditMode.SCALE && m_isScaleable && (m_isScaleableOnX || m_isScaleableOnY || m_isScaleableOnZ)))
				{
					// create edit handle
					string handlePostfix = m_editMode.ToString();
					if (m_editMode == LE_EObjectEditMode.SCALE && m_isUniformScale)
					{
						handlePostfix += "_UNIFORM";
					}
					GameObject editHandleGO = (GameObject)Instantiate(Resources.Load("ObjectEditHandle" + handlePostfix), transform.position, transform.rotation);
					m_editHandle = editHandleGO.GetComponent<LE_ObjectEditHandle>();
					m_editHandle.Target = transform;
					// notify listeners that the level data was changed
					m_editHandle.OnBeginTransform += OnBeginTransform;
					m_editHandle.OnEndTransform += OnEndTransform;
					m_editHandle.OnTransform += OnTransform;
					switch (m_editMode)
					{
						case LE_EObjectEditMode.MOVE:
							if (!m_isMovableOnX) { m_editHandle.DisableAxisX(); }
							if (!m_isMovableOnY) { m_editHandle.DisableAxisY(); }
							if (!m_isMovableOnZ) { m_editHandle.DisableAxisZ(); }
							break;
						case LE_EObjectEditMode.ROTATE:
							if (!m_isRotatableAroundX) { m_editHandle.DisableAxisX(); }
							if (!m_isRotatableAroundY) { m_editHandle.DisableAxisY(); }
							if (!m_isRotatableAroundZ) { m_editHandle.DisableAxisZ(); }
							break;
						case LE_EObjectEditMode.SCALE:
							if (!m_isScaleableOnX) { m_editHandle.DisableAxisX(); }
							if (!m_isScaleableOnY) { m_editHandle.DisableAxisY(); }
							if (!m_isScaleableOnZ) { m_editHandle.DisableAxisZ(); }
							break;
					}
				}
			}
			// set handle edit space
			if (m_editHandle != null)
			{
				m_editHandle.EditSpace = m_editSpace;
			}
		}

		private void LateUpdate()
		{
			if (m_isSelectedChanged)
			{
				m_isSelectedChanged = false;

				// instantiate outline material when needed
				if (m_outlineMaterial == null)
				{
					m_outlineMaterial = new Material(Shader.Find("Hidden/LE_SelectionOutlineShader"));
					m_outlineMaterial.color = Color.green;
				}
				// instantiate sub mesh combiners when needed
				if (m_subMeshCombiners == null)
				{
					MeshFilter[] meshFiltersRaw = GetComponentsInChildren<MeshFilter>();
					SkinnedMeshRenderer[] smrsRaw = GetComponentsInChildren<SkinnedMeshRenderer>();
					if (meshFiltersRaw.Length == 0 && smrsRaw.Length == 0)
					{
						Debug.LogError("LE_Object: could not add selection outline, because neither a MeshFilter nor a SkinnedMeshRenderer was found!");
						return;
					}
					// remove mesh filter of the edit handles and not readable meshes
					List<MeshFilter> meshFilters = new List<MeshFilter>();
					for (int i = 0; i < meshFiltersRaw.Length; i++)
					{
						if (meshFiltersRaw[i].GetComponentInParent<LE_ObjectEditHandle>() == null) // don't select edit handles
						{
							if (meshFiltersRaw[i].sharedMesh.isReadable) // check if mesh is readable
							{
								meshFilters.Add(meshFiltersRaw[i]);
							}
							else
							{
								Debug.LogError("LE_Object: all meshes of an LE_Object must be readable. Please enable the 'Read/Write Enabled' option in the mesh importer settings of '" + meshFiltersRaw[i].name + "'!");
							}
						}
					}
					// remove not readable meshes
					List<SkinnedMeshRenderer> smrs = new List<SkinnedMeshRenderer>();
					for (int i = 0; i < smrsRaw.Length; i++)
					{
						if (smrsRaw[i].sharedMesh.isReadable)
						{
							smrs.Add(smrsRaw[i]);
						}
						else
						{
							Debug.LogError("LE_Object: all meshes of an LE_Object must be readable. Please enable the 'Read/Write Enabled' option in the mesh importer settings of '" + smrsRaw[i].name + "'!");
						}
					}
					// instantiate the filtered meshes to prevent project assets from being changed
					for (int i = 0; i < meshFilters.Count; i++)
					{
						if (meshFilters[i].sharedMesh != null)
						{
							string oldName = meshFilters[i].sharedMesh.name;
							meshFilters[i].sharedMesh = (Mesh)Instantiate(meshFilters[i].sharedMesh);
							meshFilters[i].sharedMesh.name = oldName;
						}
						else
						{
							Debug.LogError("LE_Object: missing mesh in object '" + name + "' in mesh filter of '" + meshFilters[i].name + "'");
						}
					}
					// instantiate the skinned meshes to prevent project assets from being changed
					for (int i = 0; i < smrs.Count; i++)
					{
						if (smrs[i].sharedMesh != null)
						{
							string oldName = smrs[i].sharedMesh.name;
							smrs[i].sharedMesh = (Mesh)Instantiate(smrs[i].sharedMesh);
							smrs[i].sharedMesh.name = oldName;
						}
						else
						{
							Debug.LogError("LE_Object: missing mesh in object '" + name + "' in skinned mesh renderer of '" + smrs[i].name + "'");
						}
					}
					// instantiate sub mesh combiners
					m_subMeshCombiners = new LE_SubMeshCombiner[meshFilters.Count + smrs.Count];
					for (int i = 0; i < m_subMeshCombiners.Length; i++)
					{
						m_subMeshCombiners[i] = new LE_SubMeshCombiner(m_outlineMaterial,
						                                               i<meshFilters.Count ? meshFilters[i].sharedMesh : smrs[i-meshFilters.Count].sharedMesh,
						                                               i<meshFilters.Count ? (Renderer)meshFilters[i].GetComponent<MeshRenderer>() : (Renderer)smrs[i-meshFilters.Count]);
					}
				}
				ApplySelectionState();
			}
		}

		private void OnDestroy()
		{
			if (m_outlineMaterial != null)
			{
				Destroy(m_outlineMaterial);
			}
		}

		private void OnTransform(LE_EObjectEditMode p_editMode, Transform p_transform)
		{
			if (LE_EventInterface.OnChangeLevelData != null)
			{
				LE_EventInterface.OnChangeLevelData(m_editHandle, new LE_LevelDataChangedEvent(LE_ELevelDataChangeType.OBJECT_TRANSFORM));
			}
		}

		private void OnBeginTransform(LE_EObjectEditMode p_editMode, Transform p_transform)
		{
			if (transform != null) // if object is deleted, but this event handler is still mapped
			{
				m_beginTransformPos = transform.position;
				m_beginTransformRot = transform.rotation;
				m_beginTransformLocalScale = transform.localScale;
			}
		}

		private void OnEndTransform(LE_EObjectEditMode p_editMode, Transform p_transform)
		{
			if (transform != null) // if object is deleted, but this event handler is still mapped
			{
				Vector3 deltaPos = transform.position - m_beginTransformPos;
				Quaternion deltaRot = Quaternion.Inverse(m_beginTransformRot) * transform.rotation;
				Vector3 deltaLocalScale = transform.localScale - m_beginTransformLocalScale;
				UR_CommandMgr.Instance.Add(new LE_CmdTransformObject(this, deltaPos, deltaRot, deltaLocalScale), true);
			}
		}
	}
}