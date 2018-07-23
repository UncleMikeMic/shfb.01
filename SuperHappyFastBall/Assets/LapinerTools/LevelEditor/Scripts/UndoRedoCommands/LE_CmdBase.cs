using UnityEngine;
using System.Collections;
using UndoRedo;

namespace LE_LevelEditor.Commands
{
	public abstract class LE_CmdBase : UR_CommandBase
	{
		protected float m_time;
		public float RealTime { get{ return m_time; } }

		protected int m_frame;
		public int Frame { get{ return m_frame; } }

		public LE_CmdBase()
		{
			m_time = Time.realtimeSinceStartup;
			m_frame = Time.frameCount;
		}
		
		public override bool CombineWithNext(UR_ICommand p_nextCmd)
		{
			return false;
		}
	}
}
