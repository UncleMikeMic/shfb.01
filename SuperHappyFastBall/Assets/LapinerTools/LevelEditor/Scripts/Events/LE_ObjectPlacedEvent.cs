using System;
using LE_LevelEditor.Core;

namespace LE_LevelEditor.Events
{
	public class LE_ObjectPlacedEvent : EventArgs
	{
		private readonly LE_Object m_object;
		public LE_Object Object { get{ return m_object; } }

		public LE_ObjectPlacedEvent(LE_Object p_object)
		{
			m_object = p_object;
		}
	}
}