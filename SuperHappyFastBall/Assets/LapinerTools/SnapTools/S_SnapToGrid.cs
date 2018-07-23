using UnityEngine;
using System.Collections;

namespace S_SnapTools
{
	public class S_SnapToGrid : MonoBehaviour
	{
		public enum ESnapCondition {ON_UPDATE, WHEN_STILL}

		[SerializeField]
		private float m_epsilon = 0.001f;
		public float Epsilon
		{
			get{ return m_epsilon; }
			set{ m_epsilon = value; }
		}

		[SerializeField]
		private float m_maxVelocity_StillMode = 0.001f;
		public float MaxVelocity_StillMode
		{
			get{ return m_maxVelocity_StillMode; }
			set{ m_maxVelocity_StillMode = value; }
		}

		[SerializeField]
		private float m_maxEulerVelocity_StillMode = 0.01f;
		public float MaxEulerVelocity_StillMode
		{
			get{ return m_maxEulerVelocity_StillMode; }
			set{ m_maxEulerVelocity_StillMode = value; }
		}

		[SerializeField]
		private float m_stillTimeBeforSnap_StillMode = 0.2f;
		public float StillTimeBeforSnap_StillMode
		{
			get{ return m_stillTimeBeforSnap_StillMode; }
			set{ m_stillTimeBeforSnap_StillMode = value; }
		}

		[SerializeField]
		private Vector3 m_gridOffset = Vector3.zero;
		public Vector3 GridOffset
		{
			get{ return m_gridOffset; }
			set{ m_gridOffset = value; }
		}

		[SerializeField]
		private Vector3 m_gridCellSize = Vector3.one*10f;
		public Vector3 GridCellSize
		{
			get{ return m_gridCellSize; }
			set{ m_gridCellSize = value; }
		}

		[SerializeField]
		private Vector3 m_rotationOffset = Vector3.zero;
		public Vector3 RotationOffset
		{
			get{ return m_rotationOffset; }
			set{ m_rotationOffset = value; }
		}

		[SerializeField]
		private Vector3 m_rotationStepSize = Vector3.one*22.5f;
		public Vector3 RotationStepSize
		{
			get{ return m_rotationStepSize; }
			set{ m_rotationStepSize = value; }
		}

		[SerializeField]
		private ESnapCondition m_snapCondition = ESnapCondition.ON_UPDATE;
		public ESnapCondition SnapCondition
		{
			get{ return m_snapCondition; }
			set{ m_snapCondition = value; }
		}

		[SerializeField]
		private bool m_isInstantSnap = false;
		public bool IsInstantSnap
		{
			get{ return m_isInstantSnap; }
			set{ m_isInstantSnap = value; }
		}

		[SerializeField]
		private bool m_isSnapAxisX = true;
		public bool IsSnapAxisX
		{
			get{ return m_isSnapAxisX; }
			set{ m_isSnapAxisX = value; }
		}

		[SerializeField]
		private bool m_isSnapAxisY = true;
		public bool IsSnapAxisY
		{
			get{ return m_isSnapAxisY; }
			set{ m_isSnapAxisY = value; }
		}

		[SerializeField]
		private bool m_isSnapAxisZ = true;
		public bool IsSnapAxisZ
		{
			get{ return m_isSnapAxisZ; }
			set{ m_isSnapAxisZ = value; }
		}

		[SerializeField]
		private bool m_isSnapAxisXRotation = true;
		public bool IsSnapAxisXRotation
		{
			get{ return m_isSnapAxisXRotation; }
			set{ m_isSnapAxisXRotation = value; }
		}
		
		[SerializeField]
		private bool m_isSnapAxisYRotation = true;
		public bool IsSnapAxisYRotation
		{
			get{ return m_isSnapAxisYRotation; }
			set{ m_isSnapAxisYRotation = value; }
		}
		
		[SerializeField]
		private bool m_isSnapAxisZRotation = true;
		public bool IsSnapAxisZRotation
		{
			get{ return m_isSnapAxisZRotation; }
			set{ m_isSnapAxisZRotation = value; }
		}

		private float m_stillTimePos = 0f;
		private float m_stillTimeRot = 0f;
		private Vector3 m_lastPosition = Vector3.zero;
		private Vector3 m_lastEuler = Vector3.zero;

		public void DoSnap()
		{
			DoSnapPosition();
			DoSnapRotation();
		}

		public void DoSnapPosition()
		{
			Vector3 targetPos;
			if (GetSnapTarget(transform.position, m_gridOffset, m_gridCellSize, m_isSnapAxisX, m_isSnapAxisY, m_isSnapAxisZ, out targetPos))
			{
				if (m_isInstantSnap)
				{
					transform.position = targetPos;
				}
				else
				{
					transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
				}
				m_lastPosition = transform.position;
			}
		}

		public void DoSnapRotation()
		{
			Vector3 targetEuler;
			if (GetSnapTarget(transform.localEulerAngles, m_rotationOffset, m_rotationStepSize, m_isSnapAxisXRotation, m_isSnapAxisYRotation, m_isSnapAxisZRotation, out targetEuler))
			{
				if (m_isInstantSnap)
				{
					transform.localEulerAngles = targetEuler;
				}
				else
				{
					transform.localEulerAngles = Vector3.Slerp(transform.localEulerAngles, targetEuler, 0.5f);
				}
				m_lastEuler = transform.localEulerAngles;
			}
		}

		private void Start()
		{
			if (m_gridCellSize.x == 0 || m_gridCellSize.y == 0 || m_gridCellSize.z == 0)
			{
				m_gridCellSize += Vector3.one*0.1f;
				Debug.LogError("S_SnapToGrid: Start: All components of the GridCellSize vector must be unequal 0!");
			}
		}

		private void LateUpdate()
		{
			if (m_snapCondition == ESnapCondition.ON_UPDATE)
			{
				DoSnap();
			}
			else
			{
				// position snap
				if (IsStill(transform.position, m_maxVelocity_StillMode, ref m_lastPosition, ref m_stillTimePos))
				{
					DoSnapPosition();
				}
				// rotation snap
				if (IsStill(transform.localEulerAngles, m_maxEulerVelocity_StillMode, ref m_lastEuler, ref m_stillTimeRot))
				{
					DoSnapRotation();
				}
			}
		}

		private bool IsStill(Vector3 p_currentValue, float p_maxV, ref Vector3 r_lastValue, ref float r_stillTime)
		{
			if ((r_lastValue - p_currentValue).magnitude / Time.deltaTime < p_maxV)
			{
				r_stillTime += Time.deltaTime;
			}
			else
			{
				r_stillTime = 0f;
			}
			r_lastValue = p_currentValue;
			return r_stillTime >= m_stillTimeBeforSnap_StillMode;
		}

		/// <summary>
		/// Returns true if p_value is further apart than m_epsilon from the closest snap value. o_snapTarget contains the closest snap value.
		/// </summary>
		private bool GetSnapTarget(Vector3 p_value, Vector3 p_offset, Vector3 p_stepSize, bool p_isSnapAxisX, bool p_isSnapAxisY, bool p_isSnapAxisZ, out Vector3 o_snapTarget)
		{
			Vector3 currentPosition = p_value - p_offset;
			float indexX = currentPosition.x / p_stepSize.x;
			float indexY = currentPosition.y / p_stepSize.y;
			float indexZ = currentPosition.z / p_stepSize.z;
			if ((p_isSnapAxisX && indexX - Mathf.Floor(indexX) > m_epsilon) ||
			    (p_isSnapAxisY && indexY - Mathf.Floor(indexY) > m_epsilon) ||
			    (p_isSnapAxisZ && indexZ - Mathf.Floor(indexZ) > m_epsilon))
			{
				if (p_isSnapAxisX)
				{
					indexX = Mathf.Round(indexX);
				}
				if (p_isSnapAxisY)
				{
					indexY = Mathf.Round(indexY);
				}
				if (p_isSnapAxisZ)
				{
					indexZ = Mathf.Round(indexZ);
				}
				o_snapTarget = p_offset + Vector3.Scale(new Vector3(indexX, indexY, indexZ), p_stepSize);
				return true;
			}
			o_snapTarget = p_value;
			return false;
		}
	}
}
