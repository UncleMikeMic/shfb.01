using UnityEngine;
using System.Collections;

namespace S_SnapTools
{
	public class S_SnapToObjectBeforePlacementEventArgs : System.EventArgs
	{
		private readonly S_SnapToObject m_source;
		public S_SnapToObject Source { get{ return m_source; } }

		private readonly S_SnapToObjectPrefab m_snapPrefab;
		public S_SnapToObjectPrefab SnapPrefab { get{ return m_snapPrefab; } }

		private bool m_isDelayedPlacePrefab = false;
		public bool IsDelayedPlacePrefab
		{
			get{ return m_isDelayedPlacePrefab; }
			set{ m_isDelayedPlacePrefab = value; }
		}

		public S_SnapToObjectBeforePlacementEventArgs(S_SnapToObject p_source, S_SnapToObjectPrefab p_snapPrefab)
		{
			m_source = p_source;
			m_snapPrefab = p_snapPrefab;
		}
	}
}
