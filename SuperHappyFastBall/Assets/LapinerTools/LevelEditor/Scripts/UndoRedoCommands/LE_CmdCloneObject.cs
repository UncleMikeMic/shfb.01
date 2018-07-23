using UnityEngine;
using System.Collections;
using LE_LevelEditor.UI;
using LE_LevelEditor.Logic;
using LE_LevelEditor.Core;

namespace LE_LevelEditor.Commands
{
	public class LE_CmdCloneObject : LE_CmdPlaceObject
	{
		private LE_CmdObjectLink m_instanceToClone;

		public LE_CmdCloneObject(LE_GUI3dObject p_gui3d, int p_instanceToCloneUID, Transform p_copyTransform, string p_objectResourcePath)
			: base(p_gui3d, null, p_copyTransform, p_objectResourcePath)
		{
			m_instanceToClone = new LE_CmdObjectLink(p_instanceToCloneUID);
			m_isDestroyClonedScripts = true;
		}

		public override long GetStoredBytes()
		{
			return base.GetStoredBytes() + 15;
		}
		
		public override bool Execute()
		{
			// find the object that needs to be cloned and set it as prefab
			m_prefab = m_instanceToClone.Obj;

			if (m_prefab == null)
			{
				Debug.LogError("LE_CmdCloneObject: Execute: could not execute, object that needed to be cloned with UID '"+m_instanceToClone.UID+"' was not found!");
				return false;
			}

			return base.Execute();
		}
	}
}
