using System;

namespace LE_LevelEditor.Events
{
	public class LE_LoadEvent : EventArgs
	{
		public delegate void LoadLevelDataFromBytes(byte[] p_savedLevelData);
		public delegate void LoadLevelMetaFromBytes(byte[] p_savedLevelMeta);

		private LoadLevelDataFromBytes m_callbackData;
		/// <summary>
		/// Call this callback when you have selected the level that you want to load. Will load level data.
		/// </summary>
		public LoadLevelDataFromBytes LoadLevelDataFromBytesCallback { get{ return m_callbackData; } }

		private LoadLevelMetaFromBytes m_callbackMeta;
		/// <summary>
		/// Call this callback when you have selected the level that you want to load. Will load level meta data.
		/// </summary>
		public LoadLevelMetaFromBytes LoadLevelMetaFromBytesCallback { get{ return m_callbackMeta; } }

		public LE_LoadEvent(LoadLevelDataFromBytes p_loadLevelDataFromBytesCallback, LoadLevelMetaFromBytes p_loadLevelMetaFromBytesCallback)
		{
			m_callbackData = p_loadLevelDataFromBytesCallback;
			m_callbackMeta = p_loadLevelMetaFromBytesCallback;
		}
	}
}
