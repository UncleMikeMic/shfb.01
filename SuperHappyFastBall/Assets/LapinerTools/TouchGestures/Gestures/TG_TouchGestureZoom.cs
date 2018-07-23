using UnityEngine;
using System.Collections;

namespace TG_TouchGesture
{
	public class TG_TouchGestureZoom : TG_TouchGestureBase
	{
		private const float MIN_RELATIVE_ZOOM_THRESHOLD = 0.1f;
		
		private float m_lastZoomDist = -1;
		private bool m_isZooming = false;

		public TG_TouchGestureZoom()
			: base(TG_ETouchGestureType.ZOOM)
		{
		}
		
		public override TG_TouchGestureEventArgs Update ()
		{
			if (Input.touchCount == 2) // two fingers for zooming
			{
				Touch[] touches = Input.touches;
				float currZoomDist = (touches[1].position - touches[0].position).magnitude;
				if (m_lastZoomDist == -1)
				{
					m_lastZoomDist = currZoomDist;
				}
				else
				{
					float screenSize = Mathf.Min(Screen.width, Screen.height);
					if (Mathf.Abs(currZoomDist - m_lastZoomDist) / screenSize > MIN_RELATIVE_ZOOM_THRESHOLD || m_isZooming)
					{
						if (!m_isZooming)
						{
							m_isZooming = true;
							m_lastZoomDist = currZoomDist;
						}
						else
						{
							float zoomValue = (currZoomDist - m_lastZoomDist) / screenSize;
							m_lastZoomDist = currZoomDist;
							return new TG_TouchGestureEventArgs(Type, GetTouchesCenterPosition(), Vector2.one*zoomValue);
						}
					}
				}
			}
			else
			{
				m_lastZoomDist = -1;
				m_isZooming = false;
			}
			return null;
		}
	}
}
