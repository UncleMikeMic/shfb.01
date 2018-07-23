using UnityEngine;
using System.Collections;

namespace TG_TouchGesture
{
	public abstract class TG_TouchGestureBase
	{
		private TG_ETouchGestureType m_type;
		public TG_ETouchGestureType Type { get { return m_type; } }
		
		public TG_TouchGestureBase(TG_ETouchGestureType p_type)
		{
			m_type = p_type;
		}
		
		public abstract TG_TouchGestureEventArgs Update();

		protected Vector2 GetTouchesCenterPosition()
		{
			Vector2 center = Vector2.zero;
			Touch[] touches = Input.touches;
			for (int i = 0; i < Input.touchCount; i++)
			{
				center += touches[i].position;
			}
			return center / (float)Input.touchCount;
		}

		protected bool IsMovedTouch()
		{
			Touch[] touches = Input.touches;
			for (int i = 0; i < Input.touchCount; i++)
			{
				if (touches[i].phase == TouchPhase.Moved)
				{
					return true;
				}
			}
			return false;
		}
	}
}
