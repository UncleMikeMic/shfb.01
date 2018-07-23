using UnityEngine;
using System.Collections;

namespace S_SnapTools
{
	public class S_SnapToObjectEventArgs : System.EventArgs
	{
		private readonly S_SnapToObject m_source;
		public S_SnapToObject Source { get{ return m_source; } }

		private readonly GameObject m_newInstance;
		public GameObject NewInstance { get{ return m_newInstance; } }

		public S_SnapToObjectEventArgs(S_SnapToObject p_source, GameObject p_newInstance)
		{
			m_source = p_source;
			m_newInstance = p_newInstance;
		}
	}
}
