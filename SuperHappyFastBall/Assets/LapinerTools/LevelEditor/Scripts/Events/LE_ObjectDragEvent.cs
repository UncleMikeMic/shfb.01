using System;
using LE_LevelEditor.Core;

namespace LE_LevelEditor.Events
{
	public class LE_ObjectDragEvent : EventArgs
	{
		private readonly LE_Object m_objectPrefab;
		public LE_Object ObjectPrefab { get{ return m_objectPrefab; } }

		private readonly LE_Object m_objectPreview;
		public LE_Object ObjectPreview { get{ return m_objectPreview; } }

		private bool m_isObjectPlaceable;
		public bool IsObjectPlaceable
		{
			get
			{
				return m_isObjectPlaceable;
			}
			set
			{
				if (!m_isObjectPlaceable && value)
				{
					// if some other listener has set this object to be not placeable at the current location
					// then this was done for a certain reason. If you really want to overwrite this value then
					// you have to hack this code and delete this test.
					UnityEngine.Debug.LogError("LE_ObjectDragEvent: IsObjectPlaceable was 'false' you have tried to " +
						"set it 'true'!");
				}
				else
				{
					m_isObjectPlaceable = value;
				}
			}
		}
		
		private string m_message;
		public string Message
		{
			get{ return m_message; }
			set{ m_message = value; }
		}

		private readonly UnityEngine.RaycastHit m_cursorHitInfo;
		public UnityEngine.RaycastHit CursorHitInfo { get{ return m_cursorHitInfo; } }

		public LE_ObjectDragEvent(
			LE_Object p_objectPrefab,
			LE_Object p_objectPreview,
			bool p_isObjectPlaceable,
			string p_message,
			UnityEngine.RaycastHit p_cursorHitInfo)
		{
			m_objectPrefab = p_objectPrefab;
			m_objectPreview = p_objectPreview;
			m_isObjectPlaceable = p_isObjectPlaceable;
			m_message = p_message;
			m_cursorHitInfo = p_cursorHitInfo;
		}
	}
}
