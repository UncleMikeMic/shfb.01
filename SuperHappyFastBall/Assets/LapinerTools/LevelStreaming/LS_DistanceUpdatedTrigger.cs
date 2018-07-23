using UnityEngine;
using System.Collections;

namespace LS_LevelStreaming
{
	public class LS_DistanceUpdatedTrigger : LS_ITriggerByUpdatedPosition
	{
		private int m_updateFrequency;
		private Transform m_playerOrCameraTransform;
		private Vector3 m_triggeredObjectPosition;
		private float m_spawnDistance;
		private float m_despawnDistance;
		private bool m_isAlwaysVisibleWithOrthoCam = false;

		private bool m_isVisible = false;
		private int m_updateCounter = 0;

		public LS_DistanceUpdatedTrigger(int p_updateFrequency, Transform p_playerOrCameraTransform, float p_spawnDistance, float p_despawnDistance, bool p_isAlwaysVisibleWithOrthoCam)
			: this(p_updateFrequency, p_playerOrCameraTransform, p_spawnDistance, p_despawnDistance)
		{
			m_isAlwaysVisibleWithOrthoCam = p_isAlwaysVisibleWithOrthoCam;
		}

		public LS_DistanceUpdatedTrigger(int p_updateFrequency, Transform p_playerOrCameraTransform, float p_spawnDistance, float p_despawnDistance)
		{
			m_updateFrequency = p_updateFrequency;
			if (m_updateFrequency > 0)
			{
				m_updateCounter = Random.Range(0, m_updateFrequency);
			}
			m_playerOrCameraTransform = p_playerOrCameraTransform;
			m_spawnDistance = p_spawnDistance;
			m_despawnDistance = p_despawnDistance;

			m_isVisible = false;
		}

		void LS_ITriggerByUpdatedPosition.Update(Vector3 p_newPosition)
		{
			m_triggeredObjectPosition = p_newPosition;
		}

		bool LS_ITrigger.IsVisible()
		{
			if (m_updateCounter <= 0)
			{
				m_updateCounter = m_updateFrequency;
				if (m_playerOrCameraTransform != null)
				{
					bool isOrthoCam = false;
					if (m_isAlwaysVisibleWithOrthoCam)
					{
						Camera cam = m_playerOrCameraTransform.GetComponent<Camera>();
						if (cam != null && cam.orthographic)
						{
							isOrthoCam = true;
						}
					}
					if (isOrthoCam)
					{
						m_isVisible = true;
					}
					else
					{
						float distance = (m_playerOrCameraTransform.position - m_triggeredObjectPosition).magnitude;
						if (m_spawnDistance >= distance)
						{
							m_isVisible = true;
						}
						else if (m_despawnDistance <= distance)
						{
							m_isVisible = false;
						}
					}
				}
			}
			else
			{
				m_updateCounter--;
			}
			return m_isVisible;
		}
	}
}
