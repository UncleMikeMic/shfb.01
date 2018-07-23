using UnityEngine;
using System.Collections;
using LE_LevelEditor.UI;
using LE_LevelEditor.Logic;
using LE_LevelEditor.Core;
using UndoRedo;

namespace LE_LevelEditor.Commands
{
	public class LE_CmdChangeObjectIsSleepingStart : LE_CmdBase
	{
		private LE_CmdObjectLink m_object = new LE_CmdObjectLink();
		private bool m_isSleepingStart;

		public LE_CmdChangeObjectIsSleepingStart(LE_Object p_object, bool p_isSleepingStart)
		{
			m_object.Obj = p_object;
			m_isSleepingStart = p_isSleepingStart;
		}

		public override long GetStoredBytes()
		{
			return 24;
		}
		
		public override bool Execute()
		{
			return base.Execute() && Apply(m_isSleepingStart);
		}
		
		public override bool Rollback()
		{
			return base.Rollback() && Apply(!m_isSleepingStart);
		}

		private bool Apply(bool p_isSleepingStart)
		{
			if (m_object.Obj == null)
			{
				Debug.LogError("LE_CmdChangeObjectIsSleepingStart: Execute: could not execute, m_object is null!");
				return false;
			}

			m_object.Obj.IsRigidbodySleepingStart = p_isSleepingStart;

			return true;
		}
	}
}
