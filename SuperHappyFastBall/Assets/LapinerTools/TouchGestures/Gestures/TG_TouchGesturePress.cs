using UnityEngine;
using System.Collections;

namespace TG_TouchGesture
{
	public class TG_TouchGesturePress : TG_TouchGestureBase
	{
		private int m_fingerCount;
		private float m_minTouchTimeBeforeDetection;

		private bool m_isTapDown = false;
		private float m_tapStartTime = 0;
		private Vector2 m_initialCenterPosition = Vector2.zero;
		private Vector2 m_lastCenterPosition = Vector2.zero;
		private bool m_isWaitingForNoTap = false;
		
		public TG_TouchGesturePress(TG_ETouchGestureType p_type, int p_fingerCount, float p_minTouchTimeBeforeDetection)
			: base(p_type)
		{
			m_fingerCount = p_fingerCount;
			m_minTouchTimeBeforeDetection = p_minTouchTimeBeforeDetection;
		}
		
		public override TG_TouchGestureEventArgs Update ()
		{
			if (m_isTapDown)
			{
	#if UNITY_EDITOR
				if (m_fingerCount == 1 && Input.GetMouseButton(0))
				{
					// simulated press
					if (Time.realtimeSinceStartup - m_tapStartTime > m_minTouchTimeBeforeDetection)
					{
						m_isWaitingForNoTap = false;
						Vector2 currCenter = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
						Vector2 delta = currCenter - m_lastCenterPosition;
						m_lastCenterPosition = currCenter;
						return new TG_TouchGestureEventArgs(Type, m_initialCenterPosition, currCenter, delta);
					}
				} else
	#endif
				if (Input.touchCount != m_fingerCount)
				{
					// no tap possible if there are less or more fingers than m_fingerCount
					m_isWaitingForNoTap = true;
					m_isTapDown = false;
				}
				else if (Time.realtimeSinceStartup - m_tapStartTime > m_minTouchTimeBeforeDetection)
				{
					// press
					m_isWaitingForNoTap = false;
					Vector2 currCenter = GetTouchesCenterPosition();
					Vector2 delta = currCenter - m_lastCenterPosition;
					m_lastCenterPosition = currCenter;
					return new TG_TouchGestureEventArgs(Type, m_initialCenterPosition, currCenter, delta);
				}
			}
	#if UNITY_EDITOR
			else if (Input.GetMouseButton(0) && m_fingerCount == 1)
			{
				// simulate touch with mouse
				if (!m_isWaitingForNoTap)
				{
					m_isTapDown = true;
					m_tapStartTime = Time.realtimeSinceStartup;
					m_lastCenterPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
					m_initialCenterPosition = new Vector2(m_lastCenterPosition.x, m_lastCenterPosition.y);

				}
			}
	#endif
			else if (Input.touchCount == m_fingerCount)
			{
				// m_fingerCount finger on the screen -> possible press
				if (!m_isWaitingForNoTap)
				{
					m_isTapDown = true;
					m_tapStartTime = Time.realtimeSinceStartup;
					m_lastCenterPosition = GetTouchesCenterPosition();
					m_initialCenterPosition = new Vector2(m_lastCenterPosition.x, m_lastCenterPosition.y);
				}
			}
			else
			{
				// more or less than m_fingerCount finger -> no press
				m_isTapDown = false;
				// if more fingers than needed first wait for no tap this way 2 fingers are not recognized when 3 finger gesture is ending
				if (Input.touchCount > m_fingerCount)
				{
					m_isWaitingForNoTap = true;
				}
			}
			if (m_isWaitingForNoTap && Input.touchCount == 0)
			{
				m_isWaitingForNoTap = false;
			}
			return null;
		}
	}
}
