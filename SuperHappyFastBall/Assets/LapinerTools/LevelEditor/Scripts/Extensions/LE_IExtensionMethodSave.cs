using UnityEngine;
using System.Collections;

namespace LE_LevelEditor.Extensions
{
	public interface LE_IExtensionMethodSave : LE_IExtensionMethodBase
	{
		void Save(object p_sender, byte[] p_levelData, byte[] p_levelMeta, int p_removedDuplicatesCount);
	}
}
