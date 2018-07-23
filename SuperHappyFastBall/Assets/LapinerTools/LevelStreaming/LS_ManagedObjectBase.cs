using UnityEngine;
using System.Collections;

namespace LS_LevelStreaming
{
	public abstract class LS_ManagedObjectBase
	{
		private static int s_nextFreeID = 1;

		public System.EventHandler m_onShow;
		public System.EventHandler m_onHide;

		protected readonly int m_id;
		public int ID { get{ return m_id; } }

		protected readonly LS_ITrigger m_trigger;
		public LS_ITrigger Trigger { get{ return m_trigger; } }

		protected bool m_isVisible = false;
		public bool IsVisible { get{ return m_isVisible; } }

		public LS_ManagedObjectBase(LS_ITrigger p_trigger)
		{
			m_id = s_nextFreeID++;
			m_trigger = p_trigger;
		}

		public virtual void Update()
		{
			bool triggerIsVisible = m_trigger.IsVisible();
			if (m_isVisible && !triggerIsVisible)
			{
				ForceHide();
			}
			else if (!m_isVisible && triggerIsVisible)
			{
				ForceShow();
			}
		}

		public void ForceShow()
		{
			if (!m_isVisible)
			{
				m_isVisible = true;
				Show();
				if (m_onShow != null) { m_onShow(this, System.EventArgs.Empty); }
			}
		}

		public void ForceHide()
		{
			if (m_isVisible)
			{
				m_isVisible = false;
				Hide();
				if (m_onHide != null) { m_onHide(this, System.EventArgs.Empty); }
			}
		}

		protected abstract void Show();
		protected abstract void Hide();
	}
}
