using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MyUtility;

namespace S_SnapTools
{
	public class S_SnapToObjectPreview : MonoBehaviour
	{
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
		
		private SphereCollider m_collider;
		private bool m_isDrawUI = false;
		private float m_relativeScreenSize = 0.2f;
		private System.Action m_onClick = null;
		
		// ui
		private Material m_uiMaterialLine = null;
		private Material m_uiMaterialFill = null;
		private Mesh m_uiMesh = null;
		private Vector3[] m_uiVerts = new Vector3[30];
		// circles
		//private Vector3[] m_uiVerts = new Vector3[5]; // squares
		private bool m_isUiMeshIndicesSet = false;

		private float m_worldRadius = 1f;
		public float WorldRadius { get{ return m_worldRadius; } }

		public void Init(bool p_isDrawUI, float p_relativeScreenSize, System.Action p_onClick, Material p_uiMaterialLine, Material p_uiMaterialFill)
		{
			m_isDrawUI = p_isDrawUI;
			m_relativeScreenSize = p_relativeScreenSize;
			m_onClick = p_onClick;
			m_uiMaterialLine = p_uiMaterialLine;
			m_uiMaterialFill = p_uiMaterialFill;
			if (m_isDrawUI && (m_uiMaterialLine == null || m_uiMaterialFill == null))
			{
				Debug.LogError("S_SnapToObjectPreview: Init: IsDrawUI is set to 'true', but no UImaterial was passed!");
			}
		}

		private void Start()
		{
			Renderer[] renderers = GetComponentsInChildren<Renderer>();
			Vector3 center = Vector3.zero;
			foreach (Renderer r in renderers)
			{
				center += r.bounds.center;
			}
			center *= 1f/(float)renderers.Length;
			m_collider = gameObject.AddComponent<SphereCollider>();
			m_collider.center = transform.InverseTransformPoint(center);
			if (m_isDrawUI)
			{
				CreateUIObject();
			}
			// add mouse and touch click behaviour (don't use 'On Mouse Up As Button', since it will not work with "Input.simulateMouseWithTouches = false;" for touch)
			if (m_onClick != null)
			{
				UtilityClickTouchDetector clickTouch = gameObject.AddComponent<UtilityClickTouchDetector>();
				clickTouch.ColliderInstance = m_collider;
				clickTouch.m_onClick += m_onClick;
			}
			else
			{
				Debug.LogError("S_SnapToObjectPreview: Start: you have to call 'Init' before 'Start' and pass a valid p_onClick parameter!");
			}
		}

		private void Update()
		{
			if (Cam != null)
			{
				Vector3 left = Cam.ScreenToWorldPoint(Cam.WorldToScreenPoint(transform.position) + (float)Screen.height*Vector3.left*m_relativeScreenSize);
				m_worldRadius = (transform.position - left).magnitude;
				m_collider.radius = (1f/transform.lossyScale.magnitude)*m_worldRadius;

				UpdateUIVertices();
			}
		}

		private void OnDestroy()
		{
			if (m_uiMesh != null)
			{
				Destroy(m_uiMesh);
			}
		}

		private void CreateUIObject()
		{
			GameObject uiGO = new GameObject("S_SnapToObjectPreview UI");
			uiGO.transform.parent = transform;
			uiGO.transform.localPosition = Vector3.zero;
			uiGO.transform.localRotation = Quaternion.identity;
			uiGO.transform.localScale = Vector3.one;
			m_uiMesh = new Mesh();
			uiGO.AddComponent<MeshFilter>().mesh = m_uiMesh;
			MeshRenderer renderer = uiGO.AddComponent<MeshRenderer>();
			renderer.sharedMaterials = new Material[]{m_uiMaterialFill, m_uiMaterialLine};
		}

		private void UpdateUIVertices()
		{
			if (m_uiMesh != null && m_collider != null && Cam != null )
			{
				float scale = transform.lossyScale.magnitude;
				float sideLength = 1.5f*scale;

				// circles
				Vector3 objCenter = transform.TransformPoint(m_collider.center);
				Vector3 uiRoot = objCenter + (objCenter-Cam.transform.position).normalized*scale;
				float angleStep = Mathf.Deg2Rad*(360f/(float)m_uiVerts.Length);
				for (int i = 0; i < m_uiVerts.Length; i++)
				{
					float angle = (float)i*angleStep;
					m_uiVerts[i] = transform.InverseTransformPoint(uiRoot + Cam.transform.TransformDirection(new Vector3(Mathf.Sin(angle),  Mathf.Cos(angle), 0f)) * sideLength);
				}
				m_uiMesh.vertices = m_uiVerts;
				if (!m_isUiMeshIndicesSet)
				{
					m_isUiMeshIndicesSet = true;
					m_uiMesh.subMeshCount = 2;
					List<int> fillIndices = new List<int>();
					List<int> outlineIndices = new List<int>();
					for (int i = 0; i < m_uiVerts.Length; i++)
					{
						if (i+2 < m_uiVerts.Length)
						{
							fillIndices.Add(0);
							fillIndices.Add(i+1);
							fillIndices.Add(i+2);
						}
						outlineIndices.Add(i);
					}
					outlineIndices.Add(0);
					m_uiMesh.SetIndices(fillIndices.ToArray(), MeshTopology.Triangles, 0);
					m_uiMesh.SetIndices(outlineIndices.ToArray(), MeshTopology.LineStrip, 1);
				}

				// squares
				/*m_uiVerts[0] = transform.InverseTransformPoint(transform.position + Cam.transform.TransformDirection(new Vector3(-1f, -1f, 0f)) * sideLength);
				m_uiVerts[1] = transform.InverseTransformPoint(transform.position + Cam.transform.TransformDirection(new Vector3(-1f,  1f, 0f)) * sideLength);
				m_uiVerts[2] = transform.InverseTransformPoint(transform.position + Cam.transform.TransformDirection(new Vector3( 1f,  1f, 0f)) * sideLength);
				m_uiVerts[3] = transform.InverseTransformPoint(transform.position + Cam.transform.TransformDirection(new Vector3( 1f, -1f, 0f)) * sideLength);
				m_uiVerts[4] = transform.InverseTransformPoint(transform.parent.position);
				m_uiMesh.vertices = m_uiVerts;
				if (!m_isUiMeshIndicesSet)
				{
					m_isUiMeshIndicesSet = true;
					m_uiMesh.subMeshCount = 3;
					m_uiMesh.SetIndices(new int[]{0, 4, 1, 4, 2, 4, 3}, MeshTopology.LineStrip, 2);
					m_uiMesh.SetIndices(new int[]{0, 1, 2, 2, 3, 0}, MeshTopology.Triangles, 1);
					m_uiMesh.SetIndices(new int[]{0, 1, 2, 3, 0}, MeshTopology.LineStrip, 0);

				}*/

				m_uiMesh.RecalculateBounds();
			}
		}
	}
}
