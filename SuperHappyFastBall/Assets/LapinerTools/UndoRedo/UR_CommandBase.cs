using UnityEngine;
using System.Collections;

namespace UndoRedo
{
	public abstract class UR_CommandBase : UR_ICommand
	{
		protected bool m_isExecuted = false;

		public abstract long GetStoredBytes();
		public abstract bool CombineWithNext(UR_ICommand p_nextCmd);

		public virtual bool Execute()
		{
			if (m_isExecuted)
			{
				Debug.LogError("UR_CommandBase: called 'Execute', but this command was already executed!");
				return false;
			}

			m_isExecuted = true;
			return true;
		}

		public virtual bool Rollback()
		{
			if (!m_isExecuted)
			{
				Debug.LogError("UR_CommandBase: called 'Rollback', but this command was not yet rolled back!");
				return false;
			}

			m_isExecuted = false;
			return true;
		}
	}
}
