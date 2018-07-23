using UnityEngine;
using System.Collections;

namespace LS_LevelStreaming
{
	public interface LS_ITrigger
	{
		bool IsVisible();
	}

	public interface LS_ITriggerByUpdatedPosition : LS_ITrigger
	{
		void Update(Vector3 p_newPosition);
	}
}
