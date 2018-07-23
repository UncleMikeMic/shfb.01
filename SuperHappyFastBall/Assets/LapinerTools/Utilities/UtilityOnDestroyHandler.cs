using UnityEngine;
using System.Collections;

namespace MyUtility
{
	public class UtilityOnDestroyHandler : MonoBehaviour
	{
		private bool m_isHandlingDisabled = false;

		public System.Action m_onDestroy = null;

		public void DestroyWithoutHandling ()
		{
			m_isHandlingDisabled = true;
			m_onDestroy = null;
			Destroy(this);
		}

		// Update is called once per frame
		private void OnDestroy ()
		{
			if (m_onDestroy != null)
			{
				m_onDestroy();
			}
			else if (!m_isHandlingDisabled)
			{
				Debug.LogError("UtilityOnDestroyHandler: OnDestroy: destroy handler was not set!");
			}
		}
	}
}