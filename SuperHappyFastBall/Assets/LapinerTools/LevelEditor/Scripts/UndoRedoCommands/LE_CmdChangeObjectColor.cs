using UnityEngine;
using System.Collections;
using LE_LevelEditor.UI;
using LE_LevelEditor.Logic;
using LE_LevelEditor.Core;
using UndoRedo;

namespace LE_LevelEditor.Commands
{
	public class LE_CmdChangeObjectColor : LE_CmdBase
	{
		private LE_CmdObjectLink m_object = new LE_CmdObjectLink();
		private Color m_deltaColor;

		public LE_CmdChangeObjectColor(LE_Object p_object, Color p_deltaColor)
		{
			m_object.Obj = p_object;
			m_deltaColor = p_deltaColor;
		}

		public override long GetStoredBytes()
		{
			return 36;
		}
		
		public override bool Execute()
		{
			return base.Execute() && Apply(1f);
		}
		
		public override bool Rollback()
		{
			return base.Rollback() && Apply(-1f);
		}

		public override bool CombineWithNext(UR_ICommand p_nextCmd)
		{
			if (p_nextCmd is LE_CmdChangeObjectColor)
			{
				LE_CmdChangeObjectColor nextCmd = (LE_CmdChangeObjectColor)p_nextCmd;
				if (nextCmd.RealTime - RealTime < 1.25f &&
				    (m_deltaColor.r != 0 || nextCmd.m_deltaColor.r == 0) &&
				    (m_deltaColor.g != 0 || nextCmd.m_deltaColor.g == 0) &&
				    (m_deltaColor.b != 0 || nextCmd.m_deltaColor.b == 0) &&
				    (m_deltaColor.a != 0 || nextCmd.m_deltaColor.a == 0))
				{
					m_deltaColor += nextCmd.m_deltaColor;
					return true;
				}
			}

			return false;
		}

		private bool Apply(float p_direction)
		{
			if (m_object.Obj == null)
			{
				Debug.LogError("LE_CmdChangeColorObject: Execute: could not execute, m_object is null!");
				return false;
			}

			Color targetColor = m_object.Obj.ColorProperty + p_direction * m_deltaColor;
			LE_LogicObjects.ApplyColor(m_object.Obj, targetColor);

			return true;
		}
	}
}
