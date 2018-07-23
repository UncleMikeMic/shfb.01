using System;
using LE_LevelEditor.Core;

namespace LE_LevelEditor.Events
{
	public class LE_ObjectSelectedEvent : EventArgs
	{
		private readonly LE_Object m_selectedObject;
		public LE_Object SelectedObject { get{ return m_selectedObject; } }

		private readonly LE_Object m_priorSelectedObject;
		public LE_Object PriorSelectedObject { get{ return m_priorSelectedObject; } }
		
		public LE_ObjectSelectedEvent(LE_Object p_selectedObject, LE_Object p_priorSelectedObject)
		{
			m_selectedObject = p_selectedObject;
			m_priorSelectedObject = p_priorSelectedObject;
		}
	}
}