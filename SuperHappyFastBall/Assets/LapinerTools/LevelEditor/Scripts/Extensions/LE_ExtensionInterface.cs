using UnityEngine;
using System.Collections;

namespace LE_LevelEditor.Extensions
{
	public static partial class LE_ExtensionInterface
	{
		public delegate void LoadDelegate(object p_sender, System.Action<byte[][]> p_onLoadedCallback, bool p_isReload);
		public delegate void SaveDelegate(object p_sender, byte[] p_levelData, byte[] p_levelMeta, int p_removedDuplicatesCount);

		private static LE_ExtensionDelegate<LoadDelegate> m_load = null;
		public static LE_ExtensionDelegate<LoadDelegate> Load
		{
			get
			{
				if (m_load == null) { m_load = new LE_ExtensionDelegate<LoadDelegate>(); }
				return m_load;
			}
		}

		private static LE_ExtensionDelegate<SaveDelegate> m_save = null;
		public static LE_ExtensionDelegate<SaveDelegate> Save
		{
			get
			{
				if (m_save == null) { m_save = new LE_ExtensionDelegate<SaveDelegate>(); }
				return m_save;
			}
		}
	}
}
