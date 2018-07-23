using UnityEngine;
using System.Collections;
using LE_LevelEditor.UI;
using LE_LevelEditor.Logic;
using LE_LevelEditor.Core;

namespace LE_LevelEditor.Commands
{
	public class LE_CmdObjectLink
	{
		private int m_objectInstanceUID = -1;
		public int UID
		{
			get{ return m_objectInstanceUID; }
			set
			{
				if (Obj != null)
				{
					m_objectInstanceUID = value;
					m_objectInstance.UID = m_objectInstanceUID;
				}
				else
				{
					Debug.LogError("LE_CmdObjectLink: could not set UID, because this object was already removed!");
				}
			}
		}

		private LE_Object m_objectInstance = null;
		public LE_Object Obj
		{
			get
			{
				if (m_objectInstance == null && m_objectInstanceUID != -1)
				{
					LE_Object[] objs = Object.FindObjectsOfType<LE_Object>();
					for (int i = 0; i < objs.Length; i++)
					{
						if (objs[i].UID == m_objectInstanceUID)
						{
							m_objectInstance = objs[i];
							break;
						}
					}
				}
				return m_objectInstance;
			}
			set
			{
				m_objectInstance = value;
				if (m_objectInstance != null)
				{
					m_objectInstanceUID = m_objectInstance.UID;
				}
			}
		}

		public LE_CmdObjectLink() {}
		public LE_CmdObjectLink(int p_searchUID)
		{
			m_objectInstanceUID = p_searchUID;
		}
	}
}
