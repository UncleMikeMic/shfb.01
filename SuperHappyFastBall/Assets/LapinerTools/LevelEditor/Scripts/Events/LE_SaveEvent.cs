using System;

namespace LE_LevelEditor.Events
{
	public class LE_SaveEvent : EventArgs
	{
		private readonly byte[] m_savedLevelData;
		public byte[] SavedLevelData { get{ return m_savedLevelData; } }

		private readonly byte[] m_savedLevelMeta;
		public byte[] SavedLevelMeta { get{ return m_savedLevelMeta; } }

		private readonly int m_removedDuplicatesCount;
		public int RemovedDuplicatesCount { get{ return m_removedDuplicatesCount; } }

		public LE_SaveEvent(byte[] p_savedLevelData, byte[] p_savedLevelMeta, int p_removedDuplicatesCount)
		{
			m_savedLevelData = p_savedLevelData;
			m_savedLevelMeta = p_savedLevelMeta;
			m_removedDuplicatesCount = p_removedDuplicatesCount;
		}
	}
}