using UnityEngine;
using System.Collections;
using LE_LevelEditor.UI;
using LE_LevelEditor.Logic;
using LE_LevelEditor.Core;

namespace LE_LevelEditor.Commands
{
	public class LE_CmdPlaceObject : LE_CmdBase
	{
		protected LE_GUI3dObject m_gui3d;
		protected LE_Object m_prefab;
		protected Vector3 m_position;
		protected Quaternion m_rotation;
		protected Vector3 m_scale;
		protected string m_objectResourcePath;
		protected bool m_isDestroyClonedScripts = false;

		protected LE_CmdObjectLink m_objectInstance = new LE_CmdObjectLink();

		public LE_CmdPlaceObject(LE_GUI3dObject p_gui3d, LE_Object p_prefab, Transform p_copyTransform, string p_objectResourcePath)
		{
			m_gui3d = p_gui3d;
			m_prefab = p_prefab;
			m_position = p_copyTransform.position;
			m_rotation = p_copyTransform.rotation;
			m_scale = p_copyTransform.localScale;
			m_objectResourcePath = p_objectResourcePath;
		}

		public override long GetStoredBytes()
		{
			return 70 + m_objectResourcePath.Length;
		}
		
		public override bool Execute()
		{
			if (!base.Execute()) { return false; }
			
			if (m_prefab == null)
			{
				Debug.LogError("LE_CmdPlaceObject: Execute: could not execute, m_prefab is null!");
				return false;
			}

			if (m_gui3d == null)
			{
				Debug.LogError("LE_CmdPlaceObject: Execute: could not execute, m_gui3d is null!");
				return false;
			}

			int priorUID = m_objectInstance.UID;
			m_objectInstance.Obj = LE_LogicObjects.PlaceObject(m_gui3d, m_prefab, m_position, m_rotation, m_scale, m_objectResourcePath, m_isDestroyClonedScripts, priorUID);
			if (priorUID == -1)
			{
				// save randomity result in the first run
				m_position = m_objectInstance.Obj.transform.position;
				m_rotation = m_objectInstance.Obj.transform.rotation;
				m_scale = m_objectInstance.Obj.transform.localScale;
			}
			else
			{
				// restore randomity result of the first run
				m_objectInstance.Obj.transform.position = m_position;
				m_objectInstance.Obj.transform.rotation = m_rotation;
				m_objectInstance.Obj.transform.localScale = m_scale;
			}
			return true;
		}
		
		public override bool Rollback()
		{
			if (!base.Rollback()) { return false; }

			if (m_objectInstance.Obj == null)
			{
				Debug.LogError("LE_CmdPlaceObject: Rollback: could not rollback, m_objectInstance is null!");
				return false;
			}

			if (m_gui3d == null)
			{
				Debug.LogError("LE_CmdPlaceObject: Rollback: could not rollback, m_gui3d is null!");
				return false;
			}

			LE_LogicObjects.DeleteObject(m_gui3d, m_objectInstance.Obj);
			return true;
		}
	}
}
