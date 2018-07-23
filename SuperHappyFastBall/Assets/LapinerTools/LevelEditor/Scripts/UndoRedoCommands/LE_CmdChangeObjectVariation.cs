using UnityEngine;
using System.Collections;
using LE_LevelEditor.UI;
using LE_LevelEditor.Logic;
using LE_LevelEditor.Core;
using UndoRedo;

namespace LE_LevelEditor.Commands
{
	public class LE_CmdChangeObjectVariation : LE_CmdBase
	{
		private LE_CmdObjectLink m_object = new LE_CmdObjectLink();
		private int m_variationIndex;
		private int m_variationIndexBefore = -10;

		public LE_CmdChangeObjectVariation(LE_Object p_object, int p_variationIndex)
		{
			m_object.Obj = p_object;
			m_variationIndex = p_variationIndex;
		}

		public override long GetStoredBytes()
		{
			return 28;
		}
		
		public override bool Execute()
		{
			return base.Execute() && Apply(m_variationIndex);
		}
		
		public override bool Rollback()
		{
			return base.Rollback() && Apply(m_variationIndexBefore);
		}

		private bool Apply(int p_variationIndex)
		{
			if (m_object.Obj == null)
			{
				Debug.LogError("LE_CmdChangeObjectVariation: Execute: could not execute, m_object is null!");
				return false;
			}

			if (m_variationIndexBefore == -10)
			{
				m_variationIndexBefore = m_object.Obj.VariationsDefaultIndex;
			}

			LE_LogicObjects.ApplyVariation(m_object.Obj, p_variationIndex);

			return true;
		}
	}
}
