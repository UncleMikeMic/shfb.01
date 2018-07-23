using UnityEngine;
using System.Collections;

namespace LE_LevelEditor.Example
{
	public class WheelUpdater : MonoBehaviour
	{
		private void Start()
		{
			// preview instances will have no rigidbody attached -> WheelCollider must also be removed
			if (transform.root.name == "LE_GUI3dObject Preview Instance")
			{
				DestroyImmediate(GetComponent<WheelCollider>());
				Destroy(this);
			}
			else
			{
				Init();
			}
		}
#if !(UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6)
		private WheelCollider m_wheel = null;
		private Transform m_wheelVisuals = null;

		private void Init()
		{
			m_wheel = GetComponent<WheelCollider>();
			m_wheelVisuals = transform.childCount>0 ? transform.GetChild(0) : null;

			// hardcoded values for Unity 5 (prefab is saved in Unity 4)
			m_wheel.mass = 10f;
			JointSpring spring = m_wheel.suspensionSpring;
			spring.spring = 3000f;
			spring.damper = 450f;
			m_wheel.suspensionSpring = spring;
			WheelFrictionCurve fwdFriction = m_wheel.forwardFriction;
			fwdFriction.asymptoteSlip = 0.001f;
			fwdFriction.extremumSlip = 0.001f;
			fwdFriction.asymptoteValue = 0.001f;
			fwdFriction.extremumValue = 0.001f;
			m_wheel.center = m_wheel.suspensionDistance * Vector3.up;
		}

		private void Update()
		{
			if (m_wheel != null && m_wheelVisuals != null)
			{
				Vector3 position;
				Quaternion rotation;
				m_wheel.GetWorldPose(out position, out rotation);
				m_wheelVisuals.position = position;
				m_wheelVisuals.rotation = rotation;
			}
		}
#else
		private void Init()
		{
		}
#endif
	}
}
