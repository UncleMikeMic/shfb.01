using UnityEngine;
using System.Collections;

namespace LS_LevelStreaming
{
	public class LS_ManagedObjectActivateDeactivate : LS_ManagedObjectBase
	{
		protected GameObject m_go =  null;

		public LS_ManagedObjectActivateDeactivate(GameObject p_go, LS_ITrigger p_trigger)
			: base(p_trigger)
		{
			m_go = p_go;
			if (m_go != null)
			{
				m_isVisible = m_go.activeSelf;
			}
		}

		protected override void Hide ()
		{
			if (m_go != null)
			{
				m_go.SetActive(false);
			}
		}

		protected override void Show ()
		{
			if (m_go != null)
			{
				m_go.SetActive(true);
			}
		}
	}
}
