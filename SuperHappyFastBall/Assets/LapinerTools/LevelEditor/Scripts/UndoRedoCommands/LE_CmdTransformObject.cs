using UnityEngine;
using System.Collections;
using LE_LevelEditor.UI;
using LE_LevelEditor.Logic;
using LE_LevelEditor.Core;

namespace LE_LevelEditor.Commands
{
	public class LE_CmdTransformObject : LE_CmdBase
	{
		private LE_CmdObjectLink m_object = new LE_CmdObjectLink();
		private Vector3 m_deltaPos;
		private Quaternion m_deltaRot;
		private Vector3 m_deltaLocalScale;

		public LE_CmdTransformObject(LE_Object p_object, Vector3 p_deltaPos, Quaternion p_deltaRot, Vector3 p_deltaLocalScale)
		{
			m_object.Obj = p_object;
			m_deltaPos = p_deltaPos;
			m_deltaRot = p_deltaRot;
			m_deltaLocalScale = p_deltaLocalScale;
			m_isExecuted = true;
		}

		public override long GetStoredBytes()
		{
			return 60;
		}
		
		public override bool Execute()
		{
			return base.Execute() && Apply(1f);
		}
		
		public override bool Rollback()
		{
			return base.Rollback() && Apply(-1f);
		}

		private bool Apply(float p_direction)
		{
			if (m_object.Obj == null)
			{
				Debug.LogError("LE_CmdTransformObject: Execute: could not execute, m_object is null!");
				return false;
			}

			Transform transform = m_object.Obj.transform;

			transform.position += p_direction * m_deltaPos;
			transform.rotation *= p_direction > 0 ? m_deltaRot : Quaternion.Inverse(m_deltaRot);
			transform.localScale += p_direction * m_deltaLocalScale;

			return true;
		}
	}
}
