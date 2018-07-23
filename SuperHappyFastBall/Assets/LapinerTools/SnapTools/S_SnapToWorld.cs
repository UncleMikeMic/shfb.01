using UnityEngine;
using System.Collections;

namespace S_SnapTools
{
	public class S_SnapToWorld : MonoBehaviour
	{
		[SerializeField]
		private LayerMask m_snapToLayers = 1;
		public LayerMask SnapToLayers
		{
			get{ return m_snapToLayers; }
			set{ m_snapToLayers = value; }
		}

		[SerializeField]
		private Vector3 m_snapDirection = Vector3.down;
		public Vector3 SnapDirection
		{
			get{ return m_snapDirection; }
			set{ m_snapDirection = value; }
		}
		
		[SerializeField]
		/// <summary>
		/// Use -1 to disable frame based snapping, otherwise object will be snapped after the given frame count repeatedly.
		/// </summary>
		private int m_snapFrameRate = 5;
		/// <summary>
		/// Use -1 to disable frame based snapping, otherwise object will be snapped after the given frame count repeatedly.
		/// </summary>
		public int SnapFrameRate
		{
			get{ return m_snapFrameRate; }
			set{ m_snapFrameRate = value; }
		}

		[SerializeField]
		private bool m_isRotationSnap = true;
		public bool IsRotationSnap
		{
			get{ return m_isRotationSnap; }
			set{ m_isRotationSnap = value; }
		}
		
		[SerializeField]
		private float m_maxSnapDistanceAgainstDirection = 1000f;
		public float MaxSnapDistanceAgainstDirection
		{
			get{ return m_maxSnapDistanceAgainstDirection; }
			set{ m_maxSnapDistanceAgainstDirection = value; }
		}

		private int m_snapCounter = -1;

		public void DoSnap()
		{
			Vector3 rayOrigin = transform.position - m_snapDirection*m_maxSnapDistanceAgainstDirection;
			if (((1<<gameObject.layer) & m_snapToLayers.value) != 0)
			{
				// raycast could hit self -> search all possible collision points and select one to snap to
				RaycastHit[] hits = Physics.RaycastAll(rayOrigin, m_snapDirection, Mathf.Infinity, m_snapToLayers.value);
				RaycastHit? bestHit = null;
				float minDist = Mathf.Infinity;
				for (int i=0; i<hits.Length; i++)
				{
					if (!hits[i].transform.IsChildOf(transform))
					{
						if (minDist > hits[i].distance)
						{
							minDist = hits[i].distance;
							bestHit = hits[i];
						}
					}
				}
				if (bestHit != null)
				{
					SnapToHit(bestHit.Value);
				}
			}
			else
			{
				// raycast cannot hit self -> search for the first collision point to snap to
				RaycastHit hit;
				if (Physics.Raycast(rayOrigin, m_snapDirection, out hit, Mathf.Infinity, m_snapToLayers.value))
				{
					SnapToHit(hit);
				}
			}
		}

		private void SnapToHit(RaycastHit p_hit)
		{
			transform.position = p_hit.point;
			if (m_isRotationSnap)
			{
				Vector3 oldForward = transform.forward;
				transform.up = p_hit.normal;
				oldForward -= transform.up*Vector3.Dot(transform.up, oldForward);
				oldForward.Normalize();
				if (Vector3.Dot(Vector3.Cross(transform.forward, oldForward), p_hit.normal) > 0)
				{
					float angle = -Vector3.Angle(transform.forward, oldForward);
					if (!float.IsNaN(angle))
					{
						transform.Rotate(Vector3.down, angle);
					}
				}
				else
				{
					float angle = -Vector3.Angle(transform.forward, oldForward);
					if (!float.IsNaN(angle))
					{
						transform.Rotate(Vector3.up, angle);
					}
				}
			}
		}
		
		private void Update ()
		{
			if (m_snapFrameRate != -1)
			{
				if (m_snapCounter <= 0)
				{
					m_snapCounter = m_snapFrameRate;
					DoSnap();
				}
				else
				{
					m_snapCounter--;
				}
			}
		}

		private static void MoveToLayer(Transform p_root, int p_layer)
		{
			p_root.gameObject.layer = p_layer;
			foreach(Transform child in p_root)
			{
				MoveToLayer(child, p_layer);
			}
		}
	}
}
