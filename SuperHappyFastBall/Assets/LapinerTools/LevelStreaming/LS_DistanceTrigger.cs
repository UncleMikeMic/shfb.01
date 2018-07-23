using UnityEngine;
using System.Collections;

namespace LS_LevelStreaming
{
	public class LS_DistanceTrigger : LS_ITrigger
	{
		private int m_updateFrequency;
		private Transform m_playerOrCameraTransform;
		private Transform m_triggeredObjectTransform;
		private float m_spawnDistance;
		private float m_despawnDistance;

		private bool m_isVisible = false;
		private int m_updateCounter = 0;

		public LS_DistanceTrigger(int p_updateFrequency, Transform p_playerOrCameraTransform, Transform p_triggeredObjectTransform, float p_spawnDistance, float p_despawnDistance)
		{
			m_updateFrequency = p_updateFrequency;
			if (m_updateFrequency > 0)
			{
				m_updateCounter = Random.Range(0, m_updateFrequency);
			}
			m_playerOrCameraTransform = p_playerOrCameraTransform;
			m_triggeredObjectTransform = p_triggeredObjectTransform;
			m_spawnDistance = p_spawnDistance;
			m_despawnDistance = p_despawnDistance;

			if (m_triggeredObjectTransform != null)
			{
				m_isVisible = m_triggeredObjectTransform.gameObject.activeSelf;
			}
		}

		bool LS_ITrigger.IsVisible()
		{
			if (m_updateCounter <= 0)
			{
				m_updateCounter = m_updateFrequency;
				if (m_playerOrCameraTransform != null && m_triggeredObjectTransform != null)
				{
					m_isVisible = m_triggeredObjectTransform.gameObject.activeSelf;
					float distance = (m_playerOrCameraTransform.position - m_triggeredObjectTransform.position).magnitude;
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
			else
			{
				m_updateCounter--;
			}
			return m_isVisible;
		}
	}
}
