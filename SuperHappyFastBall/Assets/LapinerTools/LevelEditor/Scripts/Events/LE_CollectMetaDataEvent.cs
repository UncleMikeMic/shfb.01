using System;
using System.Collections.Generic;

namespace LE_LevelEditor.Events
{
	public class LE_CollectMetaDataEvent : EventArgs
	{
		private readonly Dictionary<string, string> m_levelMetaData = new Dictionary<string, string>();
		public Dictionary<string, string> LevelMetaData { get{ return m_levelMetaData; } }

		public LE_CollectMetaDataEvent()
		{
		}

		public KeyValuePair<string,string>[] GetCollectedMetaData()
		{
			KeyValuePair<string, string>[] result = new KeyValuePair<string, string>[m_levelMetaData.Count];
			int index = 0;
			foreach (KeyValuePair<string, string> item in m_levelMetaData)
			{
				result[index] = item;
				index++;		
			}
			return result;
		}
	}
}
