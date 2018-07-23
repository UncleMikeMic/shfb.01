using UnityEngine;
using System.Collections;

namespace LE_LevelEditor.Core
{
	public class LE_ObjectEditHandle : MonoBehaviour
	{
	#if (UNITY_ANDROID || UNITY_IPHONE || UNITY_WINRT) && !UNITY_EDITOR
		// 1. get mouse position in the first drag frame
		// a touch screen would otherwise still have the mouse position
		// set to the position of the last touch
		// 2. the other frames are needed, because touches are assumed to
		// be static, so that their position is not changed until a threshold
		// is reached. The movement made to reach the threshold would be
		// applied in very few frames -> a little jump would occur. Therefore,
		// the first few frames are skipped
		private const int SKIP_FRAMES = 4;
	#else
		private const int SKIP_FRAMES = 0;
	#endif

		private const float MIN_SCALE_VALUE = 0.05f;

		[SerializeField]
		private LE_EObjectEditSpace m_editSpace = LE_EObjectEditSpace.SELF;
		public LE_EObjectEditSpace EditSpace
		{
			get{ return m_editSpace; }
			set{ m_editSpace = value; }
		}

		[SerializeField]
		private LE_EObjectEditMode m_editMode = LE_EObjectEditMode.MOVE;
		public LE_EObjectEditMode EditMode
		{
			get{ return m_editMode; }
		}

		[SerializeField]
		private Transform m_target = null;
		public Transform Target
		{
			get{ return m_target; }
			set{ m_target = value; }
		}

		private bool m_isTransforming = false;
		private Vector3 m_activeEditAxis = Vector3.zero;
		private LE_ObjectEditHandleCollider m_activeHandle = null;
		private int m_lastDragFrame = -1;
		private Vector3 m_lastCursorPos = -1f*Vector3.one;
		private int m_dragSkipCounter = 0;

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

		public System.Action<LE_EObjectEditMode, Transform> OnBeginTransform;
		public System.Action<LE_EObjectEditMode, Transform> OnTransform;
		public System.Action<LE_EObjectEditMode, Transform> OnEndTransform;

		public bool IsDrag { get{ return Time.frameCount - m_lastDragFrame <= 1; } }

		public void OnMyMouseDrag(LE_ObjectEditHandleCollider p_handleCollider, Vector3 p_axis)
		{
			m_activeHandle = p_handleCollider;
			m_activeEditAxis = p_axis;
			m_lastDragFrame = Time.frameCount;
		}

		public void DisableAxisX()
		{
			DisableAxis(Vector3.right);
		}

		public void DisableAxisY()
		{
			DisableAxis(Vector3.up);
		}

		public void DisableAxisZ()
		{
			DisableAxis(Vector3.forward);
		}

		private void DisableAxis(Vector3 p_axis)
		{
			LE_ObjectEditHandleCollider[] editColliders = GetComponentsInChildren<LE_ObjectEditHandleCollider>();
			for (int i=0; i<editColliders.Length; i++)
			{
				if (editColliders[i].IsPlanarHandle)
				{
					if (Vector3.Dot(editColliders[i].Axis, p_axis) <= 0.0001f)
					{
						Destroy(editColliders[i].gameObject);
					}
				}
				else
				{
					if (editColliders[i].Axis == p_axis)
					{
						Destroy(editColliders[i].gameObject);
					}
					else if (Vector3.Dot(editColliders[i].Axis, p_axis) > 0.0001f)
					{
						Vector3 newAxis = editColliders[i].Axis;
						newAxis.Scale(Vector3.one-p_axis);
						editColliders[i].Axis = newAxis;
					}
				}
			}
		}

		private void Start()
		{
			if (m_target == null)
			{
				enabled = false;
				Debug.LogError("LE_ObjectEditHandle: Target is not set! It must be set via scripts LE_ObjectEditHandle.Target or in the inspector!");
			}
		}

		private void LateUpdate()
		{
			// destroy handle when target is destroyed or set to null
			if (m_target == null)
			{
				Destroy(gameObject);
				return;
			}

			// move handle to targets position
			transform.position = m_target.position;

			// apply transformation space
			if (!m_isTransforming)
			{
				if (m_editMode == LE_EObjectEditMode.SCALE || m_editSpace == LE_EObjectEditSpace.SELF)
				{
					if (transform.rotation != m_target.rotation)
					{
						transform.rotation = m_target.rotation; // local move, rotate or scale
					}
				}
				else if (transform.rotation != Quaternion.identity)
				{
					transform.rotation = Quaternion.identity; // global move or rotate (global scale does not exist)
				}
			}

			if (m_lastDragFrame < Time.frameCount)
			{
				m_activeHandle = null;
				m_activeEditAxis = Vector3.zero;
				m_dragSkipCounter = 0;
				if (m_isTransforming)
				{
					m_isTransforming = false;
					if (OnEndTransform != null)
					{
						OnEndTransform(m_editMode, m_target);
					}
				}
			}

			if (m_activeEditAxis.sqrMagnitude > 0.01)
			{
				if (!m_isTransforming)
				{
					m_isTransforming = true;
					if (OnBeginTransform != null)
					{
						OnBeginTransform(m_editMode, m_target);
					}
				}

				switch (m_editMode)
				{
					case LE_EObjectEditMode.MOVE: Move(); break;
					case LE_EObjectEditMode.ROTATE: Rotate(); break;
					case LE_EObjectEditMode.SCALE: Scale(); break;
					case LE_EObjectEditMode.SMART: Smart(); break;
				}
				if (m_editMode != LE_EObjectEditMode.NO_EDIT && OnTransform != null)
				{
					OnTransform(m_editMode, m_target);
				}
			}
			m_lastCursorPos = GetCursorPosition();
		}

		private void OnDestroy()
		{
			OnBeginTransform = null;
			OnTransform = null;
			OnEndTransform = null;
		}

		private void Move()
		{
			float editDelta = GetEditDelta();
			if (m_dragSkipCounter == SKIP_FRAMES)
			{
				if (m_activeHandle != null && m_activeHandle.IsPlanarHandle)
				{
					// planar handle
					Vector3 activeAxisBK = m_activeEditAxis;
					// move on X
					if (activeAxisBK.z != 0 || activeAxisBK.y != 0)
					{
						m_activeEditAxis = Vector3.right;
						editDelta = GetEditDelta();
						m_target.position += transform.right*editDelta;
					}
					// move on Y
					if (activeAxisBK.z != 0 || activeAxisBK.x != 0)
					{
						m_activeEditAxis = Vector3.up;
						editDelta = GetEditDelta();
						m_target.position += transform.up*editDelta;
					}
					// move on Z
					if (activeAxisBK.y != 0 || activeAxisBK.x != 0)
					{
						m_activeEditAxis = Vector3.forward;
						editDelta = GetEditDelta();
						m_target.position += transform.forward*editDelta;
					}
					transform.position = m_target.position;
				}
				else
				{
					// axis handle
					Vector3 worldAxis = transform.TransformDirection(m_activeEditAxis);
					m_target.position += worldAxis*editDelta;
					transform.position = m_target.position;
				}
				m_target.SendMessage("SolveCollisionAndDeactivateRigidbody");
			}
			else if (Mathf.Abs(editDelta) > 0.0005f)
			{
				// skip first frame (on mobile jumps possible)
				m_dragSkipCounter++;
			}
		}

		private void Scale()
		{
			float editDelta = GetEditDelta();
			if (m_dragSkipCounter == SKIP_FRAMES)
			{
				m_target.localScale += m_activeEditAxis*editDelta;
				m_target.localScale = Vector3.Max(Vector3.one*MIN_SCALE_VALUE, m_target.localScale);
				Vector3 updatedScale = transform.localScale;
				updatedScale.x += m_activeEditAxis.x*editDelta * transform.localScale.x / m_target.localScale.x;
				updatedScale.y += m_activeEditAxis.y*editDelta * transform.localScale.y / m_target.localScale.y;
				updatedScale.z += m_activeEditAxis.z*editDelta * transform.localScale.z / m_target.localScale.z;
				updatedScale = Vector3.Max(Vector3.one*MIN_SCALE_VALUE, updatedScale);
				transform.localScale = updatedScale;
				m_target.SendMessage("SolveCollisionAndDeactivateRigidbody");
			}
			else if (Mathf.Abs(editDelta) > 0.0005f)
			{
				// skip first frame (on mobile jumps possible)
				m_dragSkipCounter++;
			}
		}

		private float GetEditDelta()
		{
			Camera cam = Cam;
			if (cam != null)
			{
				Vector3 editAxisInScreenCoords = cam.WorldToScreenPoint(transform.position + transform.TransformDirection(m_activeEditAxis)) - cam.WorldToScreenPoint(transform.position);
				// with bigger z value the angle between camera and move handle is bigger and object needs to move faster
				editAxisInScreenCoords.z *= editAxisInScreenCoords.z; // x*x
				editAxisInScreenCoords.z *= editAxisInScreenCoords.z; // x*x*x*x
				float zFactor = 1f / (1-Mathf.Clamp(editAxisInScreenCoords.z, 0f, 0.8f));
				editAxisInScreenCoords.z = 0;
				editAxisInScreenCoords.Normalize();
				if (cam.orthographic)
				{
					return GetEditDeltaOrthogonal(editAxisInScreenCoords, zFactor, cam);
				}
				else
				{
					float distToCam = (cam.transform.position - transform.position).magnitude;
					return GetEditDeltaPerspective(editAxisInScreenCoords, zFactor*distToCam, cam);
				}
			}
			else
			{
				return 0f;
			}
		}

		private void Rotate()
		{
			float editDelta = GetEditDeltaRotation();
			if (m_dragSkipCounter == SKIP_FRAMES)
			{
				m_target.Rotate(m_activeEditAxis*editDelta, m_editSpace == LE_EObjectEditSpace.SELF ? Space.Self : Space.World);
				if (m_editSpace == LE_EObjectEditSpace.WORLD)
				{
					transform.Rotate(m_activeEditAxis*editDelta, Space.World);
				}
				else
				{
					transform.Rotate(m_target.TransformDirection(m_activeEditAxis)*editDelta, Space.World);
				}
				m_target.SendMessage("SolveCollisionAndDeactivateRigidbody");
			}
			else if (Mathf.Abs(editDelta) > 0.0005f)
			{
				// skip first frame (on mobile jumps possible)
				m_dragSkipCounter++;
			}
		}
		
		private float GetEditDeltaRotation()
		{
			Camera cam = Cam;
			if (cam != null)
			{
				Vector3 worldDir = transform.TransformDirection(m_activeEditAxis);
				Vector3 pivotScreenPos = cam.WorldToScreenPoint(transform.position);
				pivotScreenPos.z = 0;
				Vector3 screenDirection;
				float axisCameraAngle = Vector3.Dot(worldDir, cam.transform.forward);
				if (Mathf.Abs(axisCameraAngle) > 0.5f)
				{
					float sign = Mathf.Sign(Vector3.Dot(cam.transform.forward, worldDir));
					if (sign == 0) { sign = 1; }
					Vector3 mouseDelta = (GetCursorPosition() - pivotScreenPos).normalized;
					screenDirection = new Vector3(-Mathf.Sign(mouseDelta.y),Mathf.Sign(mouseDelta.x),0f);
					screenDirection *= sign;
				}
				else
				{
					screenDirection = Vector3.Cross(cam.transform.forward, worldDir);
					screenDirection = cam.transform.InverseTransformDirection(screenDirection);
					screenDirection.z = 0;
				}
				screenDirection.Normalize();

				if (cam.orthographic)
				{
					return GetEditDeltaOrthogonal(screenDirection, 360f/cam.orthographicSize, cam);
				}
				else
				{
					return GetEditDeltaPerspective(screenDirection, 360f, cam);
				}
			}
			else
			{
				return 0f;
			}
		}

		private float GetEditDeltaPerspective(Vector3 p_screenAxis, float p_multiplier, Camera p_cam)
		{
			float mouseDeltaInAxisDir = Vector3.Dot(p_screenAxis, GetCursorPosition() - m_lastCursorPos);
			Ray rayAfter = p_cam.ScreenPointToRay(m_lastCursorPos+mouseDeltaInAxisDir*p_screenAxis);
			Ray rayBefore = p_cam.ScreenPointToRay(m_lastCursorPos);
			Vector3 rayDirAfter = rayAfter.direction;
			Vector3 rayDirBefore = rayBefore.direction;
			return (rayDirAfter-rayDirBefore).magnitude*Mathf.Sign(mouseDeltaInAxisDir)*p_multiplier;
		}

		private float GetEditDeltaOrthogonal(Vector3 p_screenAxis, float p_multiplier, Camera p_cam)
		{
			float mouseDeltaInAxisDir = Vector3.Dot(p_screenAxis, GetCursorPosition() - m_lastCursorPos);
			Vector3 pointAfter = p_cam.ScreenToWorldPoint(m_lastCursorPos+mouseDeltaInAxisDir*p_screenAxis);
			Vector3 pointBefore = p_cam.ScreenToWorldPoint(m_lastCursorPos);
			return (pointAfter-pointBefore).magnitude*Mathf.Sign(mouseDeltaInAxisDir)*p_multiplier;
		}

		private void Smart()
		{
			// the smart drag is handled by the LE_GUI3dObject class
			m_target.SendMessage("SolveCollisionAndDeactivateRigidbody");
		}

		private Vector3 GetCursorPosition()
		{
			if (Input.touchCount > 0)
			{
				return Input.GetTouch(0).position;
			}
			else
			{
				return Input.mousePosition;
			}
		}
	}
}
