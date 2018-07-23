using UnityEngine;
using System.Collections;

namespace CPG_CameraPerspective
{
	/// <summary>
	/// Add a nice camera perspective gizmo similar to the gizmo in the Scene View of the Unity Editor to your game. Click on the axes to change camera's position and direction. Click on the cube in the middle to toggle between perspective and orthographic camera view.
	/// </summary>
	public class CPG_CameraPerspectiveGizmo : MonoBehaviour
	{
		/// <summary>
		/// Add a camera view control with six axis (+X,-X,+Y,-Y,+Z,-Z) and a view mode toggle (perspective, orthographic) with this single function call.
		/// </summary>
		/// <param name="p_targetCamera">Camera which is managed by the gizmo.</param>
		/// <param name="p_layerToUse">This layer will be used to render the gizmo. The camera perspective gizmo uses its own layer. The gizmo object is moved to this layer and rendered by its own camera. Disable the rendering of the gizmo layer in the inspector of your main camera.</param>
		public static CPG_CameraPerspectiveGizmo Create(Camera p_targetCamera, int p_layerToUse)
		{
			if (p_targetCamera != null)
			{
				Object resource = Resources.Load("CPG_CameraPerspectiveGizmo");
				if (resource != null)
				{
					GameObject go = (GameObject)Instantiate(resource);
					CPG_CameraPerspectiveGizmo cpgCam = go.GetComponent<CPG_CameraPerspectiveGizmo>();
					cpgCam.TargetCamera = p_targetCamera;
					cpgCam.LayerToUse = p_layerToUse;
					return cpgCam;
				}
				else
				{
					Debug.LogError("CPG_CameraPerspectiveGizmo: Create: prefab 'CPG_CameraPerspectiveGizmo' not found!");
				}
			}
			else
			{
				Debug.LogError("CPG_CameraPerspectiveGizmo: Create: p_targetCamera is null!");
			}
			return null;
		}

		public enum EButtonTypes { TOGGLE_ORTHO, X_POS, X_NEG, Y_POS, Y_NEG, Z_POS, Z_NEG }

		[SerializeField]
		private Vector2 m_relativeScreenPos = Vector2.one*0.85f;
		/// <summary>
		/// Center in relative screen coordinates [0-1]
		/// </summary>
		public Vector2 RelativeScreenPos
		{
			get{ return m_relativeScreenPos; }
			set{ m_relativeScreenPos = value; }
		}

		[SerializeField]
		private float m_relativeScreenSize = 0.1f;
		/// <summary>
		/// Size in relative screen coordinates [0-1]
		/// </summary>
		public float RelativeScreenSize
		{
			get{ return m_relativeScreenSize; }
			set{ m_relativeScreenSize = value; }
		}

		[SerializeField]
		private int m_layerToUse = 31;
		/// <summary>
		/// This layer will be used to render the gizmo (hide it in main camera)
		/// </summary>
		public int LayerToUse
		{
			get{ return m_layerToUse; }
			set{ m_layerToUse = value; }
		}

		[SerializeField]
		private Vector3 m_pivot = Vector3.zero;
		/// <summary>
		/// This point will be focused when one of the axes is clicked. The camera should always look at this point to avoid camera jumps. Update it if you move the camera in your own code
		/// </summary>
		public Vector3 Pivot
		{
			get{ return m_pivot; }
			set{ m_pivot = value; }
		}

		[SerializeField]
		private float m_minDistToPivot = 15;
		/// <summary>
		/// Camera will have at least this distance to the pivot point after the view axis is changed 
		/// </summary>
		public float MinDistToPivot
		{
			get{ return m_minDistToPivot; }
			set{ m_minDistToPivot = value; }
		}

		[SerializeField]
		private float m_orthoOffset = 0;
		/// <summary>
		/// This offset will be applied when the camera mode is changed to orthographic. You might need it to avoid objects being clipped because of the near plane
		/// </summary>
		public float OrthoOffset
		{
			get{ return m_orthoOffset; }
			set{ m_orthoOffset = value; }
		}

		private Camera m_targetCamera = null;
		/// <summary>
		/// Camera which is managed by the gizmo. 
		/// </summary>
		public Camera TargetCamera
		{
			get
			{
				if (m_targetCamera == null)
				{
					m_targetCamera = Camera.main;
				}
				return m_targetCamera;
			}
			set{ m_targetCamera = value; }
		}

		private Camera m_ownCamera;
		public Camera OwnCamera
		{
			get
			{
				if (m_ownCamera == null)
				{
					m_ownCamera = GetComponentInChildren<Camera>();
				}
				return m_ownCamera;
			}
		}

		public System.EventHandler m_onBeforeSwitchToOrthographic;
		public System.EventHandler m_onAfterSwitchToOrthographic;
		public System.EventHandler m_onBeforeSwitchToPerspective;
		public System.EventHandler m_onAfterSwitchToPerspective;

		/// <summary>
		/// Internal public function you don't need it.
		/// </summary>
		public void ReportClick(EButtonTypes p_type)
		{
			switch (p_type)
			{
				case EButtonTypes.TOGGLE_ORTHO: ToggleOrtho(); break;
				case EButtonTypes.X_POS: ToogleAxis(Vector3.right); break;
				case EButtonTypes.X_NEG: ToogleAxis(Vector3.left); break;
				case EButtonTypes.Y_POS: ToogleAxis(Vector3.up); break;
				case EButtonTypes.Y_NEG: ToogleAxis(Vector3.down); break;
				case EButtonTypes.Z_POS: ToogleAxis(Vector3.forward); break;
				case EButtonTypes.Z_NEG: ToogleAxis(Vector3.back); break;
			}
		}

		private void ToggleOrtho()
		{
			Camera cam = TargetCamera;
			if (cam != null)
			{
				// events before
				if (cam.orthographic && m_onBeforeSwitchToPerspective != null)
				{
					m_onBeforeSwitchToPerspective(this, System.EventArgs.Empty);
				}
				if (!cam.orthographic && m_onBeforeSwitchToOrthographic != null)
				{
					m_onBeforeSwitchToOrthographic(this, System.EventArgs.Empty);
				}
				// offset
				if (cam.orthographic)
				{
					cam.transform.position += cam.transform.forward*m_orthoOffset;
				}
				else
				{
					cam.transform.position -= cam.transform.forward*m_orthoOffset;
				}
				// switch
				cam.orthographic = !cam.orthographic;
				// events after
				if (cam.orthographic && m_onAfterSwitchToOrthographic != null)
				{
					m_onAfterSwitchToOrthographic(this, System.EventArgs.Empty);
				}
				if (!cam.orthographic && m_onAfterSwitchToPerspective != null)
				{
					m_onAfterSwitchToPerspective(this, System.EventArgs.Empty);
				}
			}
		}

		private void ToogleAxis(Vector3 p_axis)
		{
			Camera cam = TargetCamera;
			if (cam != null)
			{
				float distanceBeforeAxisSwitch = (cam.transform.position-Pivot).magnitude;
				if (distanceBeforeAxisSwitch == 0) { distanceBeforeAxisSwitch = 1f; }
				float minDist = m_minDistToPivot;
				if (cam.orthographic)
				{
					minDist += m_orthoOffset;
				}
				cam.transform.position = Pivot+Mathf.Max(minDist, distanceBeforeAxisSwitch)*p_axis;
				cam.transform.LookAt(m_pivot);
			}
		}

		private void Start()
		{
			MoveToLayer(transform, m_layerToUse);
			m_ownCamera = GetComponentInChildren<Camera>();
			if (m_ownCamera != null)
			{
				m_ownCamera.aspect = 1f;
				Camera cam = TargetCamera;
				if (cam != null && cam.pixelRect.xMax - cam.pixelRect.xMin > 1 && cam.pixelRect.yMax - cam.pixelRect.yMin > 1)
				{
					Rect camRect = cam.rect;
					float width = (m_relativeScreenSize/cam.aspect)*camRect.width;
					float height = m_relativeScreenSize*camRect.height;
					Vector2 xy = new Vector2(camRect.x, camRect.y) + m_relativeScreenPos - new Vector2(1f-camRect.xMax, 1f-camRect.yMax) - 0.5f*new Vector2(width, height);
					m_ownCamera.rect = new Rect(xy.x, xy.y, width, height);
				}
				else
				{
					Debug.LogError("CPG_CameraPerspectiveGizmo: Start: could not find any camera with a non empty view rect! Will destroy self now!");
					Destroy(gameObject);
				}
				m_ownCamera.cullingMask = 1 << m_layerToUse;
			}
			else
			{
				Debug.LogError("CPG_CameraPerspectiveGizmo: Start: CPG_CameraPerspectiveGizmo prefab seems to be broken it needs a camera in its children!");
			}
		}

		private void Update()
		{
			Camera cam = TargetCamera;
			if (cam != null && m_ownCamera != null)
			{
				m_ownCamera.orthographic = cam.orthographic;
				m_ownCamera.transform.localPosition = -5.2f*cam.transform.forward;
				m_ownCamera.transform.rotation = cam.transform.rotation;
			}
			else
			{
				Debug.LogWarning("CPG_CameraPerspectiveGizmo: Update: lost reference to camera ! Will destroy self now!");
				Destroy(gameObject);
			}
		}

		private void OnDestroy()
		{
			m_onBeforeSwitchToOrthographic = null;
			m_onBeforeSwitchToPerspective = null;
			m_onAfterSwitchToOrthographic = null;
			m_onAfterSwitchToPerspective = null;
		}

		private void MoveToLayer(Transform root, int layer)
		{
			root.gameObject.layer = layer;
			foreach(Transform child in root)
			{
				MoveToLayer(child, layer);
			}
		}
	}
}