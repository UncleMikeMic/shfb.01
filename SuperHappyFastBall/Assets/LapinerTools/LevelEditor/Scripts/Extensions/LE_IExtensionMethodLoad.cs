using UnityEngine;
using System.Collections;

namespace LE_LevelEditor.Extensions
{
	public interface LE_IExtensionMethodLoad : LE_IExtensionMethodBase
	{
		void Load(object p_sender, System.Action<byte[][]> p_onLoadedCallback, bool p_isReload);
	}
}
