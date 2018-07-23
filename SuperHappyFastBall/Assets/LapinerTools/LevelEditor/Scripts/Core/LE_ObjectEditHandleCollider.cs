using UnityEngine;
using System.Collections;

namespace LE_LevelEditor.Core
{
	public class LE_ObjectEditHandleCollider : MonoBehaviour
	{
		[SerializeField]
		private LE_ObjectEditHandle m_parent;
		[SerializeField]
		private Vector3 m_axis;
		public Vector3 Axis
		{
			get{ return m_axis; }
			set{ m_axis = value; }
		}
		[SerializeField]
		private bool m_isRotationHandle = false;
		[SerializeField]
		private Vector3 m_rotationHandleDir;
		[SerializeField]
		private bool m_isPlanarHandle = false;
		public bool IsPlanarHandle
		{
			get{ return m_isPlanarHandle; }
			set{ m_isPlanarHandle = value; }
		}

		private Renderer m_renderer;
		private Collider[] m_colliders;
		private Color m_originalColor;
		private bool m_isDrag = false;
		private float m_lastCamDist = -1;
		private Vector3 m_lastParentScale = Vector3.one;

		private bool m_isMouseDown = false;
		private bool m_isMouseDownOnMe = false;

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

		private void Start()
		{
			m_colliders = GetComponentsInChildren<Collider>();
			m_renderer = GetComponent<Renderer>();
			m_originalColor = m_renderer.material.color;
		}

		private void Update()
		{
			bool isAlt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr);
			bool isMouseDownThisFrame = !isAlt && (Input.GetMouseButton(0) && Input.touchCount < 2) || Input.touchCount == 1;
			if (!m_isMouseDown && !isMouseDownThisFrame)
			{
				return;
			}
			if (m_isMouseDownOnMe && !isMouseDownThisFrame)
			{
				OnMyMouseUp();
			}
			else if (m_isMouseDownOnMe && isMouseDownThisFrame)
			{
				OnMyMouseDrag();
			}
			else
			{
				// raycast only in the Ignore Raycast layer, this way handles cannot be covered by other
				// objects and other objects are not snapped to handles while they are places
				Camera cam = Cam;
				if (m_colliders != null && cam != null)
				{
					RaycastHit hit;
					for (int i = 0; i < m_colliders.Length; i++)
					{
						Collider collider = m_colliders[i];
						if (collider != null &&
						    !m_isMouseDown &&
						    isMouseDownThisFrame &&
						    !m_parent.IsDrag &&
						    (collider.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity) || // mouse
						 	(Input.touchCount == 1 && collider.Raycast(cam.ScreenPointToRay(Input.GetTouch(0).position), out hit, Mathf.Infinity)))) // touch
						{
							m_isMouseDownOnMe = true;
							OnMyMouseDown();
							break;
						}
					}
				}
			}
			m_isMouseDown = isMouseDownThisFrame;
			if (!m_isMouseDown)
			{
				m_isMouseDownOnMe = false;
			}
		}

		private void LateUpdate()
		{
			Camera cam = Cam;
			if (cam != null)
			{
				Vector3 dirToCam = m_parent.transform.InverseTransformPoint(cam.transform.position);

				// direct planar handles to camera
				if (!m_parent.IsDrag && m_isPlanarHandle)
				{
					if (m_axis.x != 0) { transform.localRotation = Quaternion.Euler(GetPlannerEulerRotationOnXAxis(dirToCam)); }
					else if (m_axis.y != 0) { transform.localRotation = Quaternion.Euler(GetPlannerEulerRotationOnYAxis(dirToCam)); }
					else if (m_axis.z != 0) { transform.localRotation = Quaternion.Euler(GetPlannerEulerRotationOnZAxis(dirToCam)); }
				}

				// direct rotation half circle handle to camera
				if (!m_isDrag && m_isRotationHandle)
				{
					Vector3 relCamDir = dirToCam - Vector3.Dot(m_axis, dirToCam)*m_axis;
					relCamDir.Normalize();
					float sin = Vector3.Dot(m_rotationHandleDir, relCamDir);
					Vector3 angleAxis = Vector3.Cross(m_rotationHandleDir, relCamDir);
					angleAxis.Scale(m_axis);
					float angle = Mathf.Rad2Deg * Mathf.Asin(angleAxis.magnitude);
					angleAxis.Normalize();
					if (sin < 0)
					{
						angle = Mathf.Abs(-180+angle);
					}
					angleAxis *= angle;
					transform.localRotation = Quaternion.Euler(angleAxis);
				}

				// correct handle scaling
				float camDist = (cam.transform.position - transform.position).magnitude;
				if (!m_isDrag &&
				    (cam.orthographic ||
					Mathf.Abs(camDist - m_lastCamDist) > 0.0001 ||
					m_lastParentScale != m_parent.transform.localScale))
				{
					float camScaleFactor;
					if (cam.orthographic)
					{
						camScaleFactor =cam.orthographicSize / 50f;
					}
					else
					{
						camScaleFactor = camDist / 75f;
					}
					transform.localScale = new Vector3(
						camScaleFactor / m_parent.transform.localScale.x,
						camScaleFactor / m_parent.transform.localScale.y,
						camScaleFactor / m_parent.transform.localScale.z);
					m_lastCamDist = camDist;
					m_lastParentScale = m_parent.transform.localScale;
				}
			}
		}

		private Vector3 GetPlannerEulerRotationOnXAxis(Vector3 p_dirToCam)
		{
			Vector3 euler = Vector3.zero;
			if (p_dirToCam.y > 0)
			{
				if (p_dirToCam.z > 0)
				{
					euler.x = 0;
				}
				else
				{
					euler.x = -90f;
				}
			}
			else
			{
				if (p_dirToCam.z > 0)
				{
					euler.x = 90f;
				}
				else
				{
					euler.x = 180f;
				}
			}
			return euler;
		}

		private Vector3 GetPlannerEulerRotationOnYAxis(Vector3 p_dirToCam)
		{
			Vector3 euler = Vector3.zero;
			if (p_dirToCam.z > 0)
			{
				if (p_dirToCam.x > 0)
				{
					euler.y = 0;
				}
				else
				{
					euler.y = -90f;
				}
			}
			else
			{
				if (p_dirToCam.x > 0)
				{
					euler.y = 90f;
				}
				else
				{
					euler.y = 180f;
				}
			}
			return euler;
		}

		private Vector3 GetPlannerEulerRotationOnZAxis(Vector3 p_dirToCam)
		{
			Vector3 euler = Vector3.zero;
			if (p_dirToCam.x > 0)
			{
				if (p_dirToCam.y > 0)
				{
					euler.z = 0;
				}
				else
				{
					euler.z = -90f;
				}
			}
			else
			{
				if (p_dirToCam.y > 0)
				{
					euler.z = 90f;
				}
				else
				{
					euler.z = 180f;
				}
			}
			return euler;
		}

		private void OnMyMouseDown()
		{
			m_parent.OnMyMouseDrag(this, m_axis);
			m_renderer.material.color = Color.yellow;
			m_isDrag = true;
		}

		private void OnMyMouseDrag()
		{
			m_parent.OnMyMouseDrag(this, m_axis);
			m_isDrag = true;
		}

		private void OnMyMouseUp()
		{
			m_renderer.material.color = m_originalColor;
			m_parent.transform.localScale = Vector3.one;
			m_isDrag = false;
		}
	}
}
