using UnityEngine;
using System.Collections;
using System;

namespace TG_TouchGesture
{
	public class TG_TouchGestureEventArgs : EventArgs
	{
		private TG_ETouchGestureType m_type;
		public TG_ETouchGestureType Type { get { return m_type; } }

		private Vector2 m_initialPosition;
		public Vector2 InitialPosition { get { return m_initialPosition; } }

		private Vector2 m_position;
		public Vector2 Position { get { return m_position; } }

		private Vector2 m_delta;
		public Vector2 Delta { get { return m_delta; } }
		
		// e.g. tap
		public TG_TouchGestureEventArgs(TG_ETouchGestureType p_type, Vector2 p_position)
			: this(p_type, p_position, Vector2.zero)
		{
		}

		// e.g. zoom
		public TG_TouchGestureEventArgs(TG_ETouchGestureType p_type, Vector2 p_position, Vector2 p_delta)
			: this(p_type, p_position, p_position, p_delta)
		{
		}

		// e.g. press
		public TG_TouchGestureEventArgs(TG_ETouchGestureType p_type, Vector2 p_initialPosition, Vector2 p_position, Vector2 p_delta)
		{
			m_type = p_type;
			m_initialPosition = p_initialPosition;
			m_position = p_position;
			m_delta = p_delta;
		}
	}
}
