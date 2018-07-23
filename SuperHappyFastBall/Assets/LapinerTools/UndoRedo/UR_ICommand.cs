using UnityEngine;
using System.Collections;

namespace UndoRedo
{
	public interface UR_ICommand
	{
		long GetStoredBytes();
		bool Execute();
		bool Rollback();
		bool CombineWithNext(UR_ICommand p_nextCmd);
	}
}
