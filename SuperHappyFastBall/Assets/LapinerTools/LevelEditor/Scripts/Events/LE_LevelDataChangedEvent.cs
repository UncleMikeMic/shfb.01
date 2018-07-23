using System;

namespace LE_LevelEditor.Events
{
	public class LE_LevelDataChangedEvent : EventArgs
	{
		private readonly LE_ELevelDataChangeType m_changeType;
		public LE_ELevelDataChangeType ChangeType { get{ return m_changeType; } }
		
		public LE_LevelDataChangedEvent(LE_ELevelDataChangeType p_changeType)
		{
			m_changeType = p_changeType;
		}
	}
}