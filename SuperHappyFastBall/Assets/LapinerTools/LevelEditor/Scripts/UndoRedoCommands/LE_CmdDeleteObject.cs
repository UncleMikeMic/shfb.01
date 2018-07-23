using UnityEngine;
using System.Collections;
using LE_LevelEditor.UI;
using LE_LevelEditor.Logic;
using LE_LevelEditor.Core;
using MyUtility;
using System.Collections.Generic;

namespace LE_LevelEditor.Commands
{
	public class LE_CmdDeleteObject : LE_CmdBase
	{
		private LE_GUI3dObject m_gui3d;
		private LE_CmdObjectLink m_objectInstance = new LE_CmdObjectLink();
		private LE_Object m_prefab;
		private string m_objectResourcePath;

		private Vector3 m_position;
		private Quaternion m_rotation;
		private Vector3 m_scale;
		private bool m_isRigidbodySleepingStart;
		private Color m_color;
		private int m_variationIndex;
		private int m_UID;
		private Dictionary<string, int> m_snapPointLinks = new Dictionary<string, int>();
		
		public LE_CmdDeleteObject(LE_GUI3dObject p_gui3d, LE_Object p_selectedObject)
		{
			m_gui3d = p_gui3d;
			m_objectInstance.Obj = p_selectedObject;
			m_prefab = Resources.Load<LE_Object>(p_selectedObject.name);
			m_objectResourcePath = p_selectedObject.name;
			m_UID = p_selectedObject.UID;

			if (m_prefab == null)
			{
				Debug.LogWarning("LE_CmdDeleteObject: '" + p_selectedObject.name + "' is not a valid resource path! Command will fail on execution!");
			}
		}

		public override long GetStoredBytes()
		{
			return 200 + m_objectResourcePath.Length;
		}
		
		public override bool Execute()
		{
			if (!base.Execute()) { return false; }
			
			if (m_objectInstance.Obj == null)
			{
				Debug.LogError("LE_CmdDeleteObject: Execute: could not execute, m_objectInstance is null!");
				return false;
			}

			if (m_gui3d == null)
			{
				Debug.LogError("LE_CmdDeleteObject: Execute: could not execute, m_gui3d is null!");
				return false;
			}

			// save objects state for rollback
			m_position = m_objectInstance.Obj.transform.position;
			m_rotation = m_objectInstance.Obj.transform.rotation;
			m_scale = m_objectInstance.Obj.transform.localScale;
			m_isRigidbodySleepingStart = m_objectInstance.Obj.IsRigidbodySleepingStart;
			m_color = m_objectInstance.Obj.ColorProperty;
			m_variationIndex = m_objectInstance.Obj.VariationsDefaultIndex;
			m_snapPointLinks.Clear();
			List<KeyValuePair<string, int>> snapPointLinks = m_gui3d.GetSnapPointsToReactivate(m_UID, m_objectInstance.Obj.ObjectSnapPoints.Length);
			foreach (KeyValuePair<string, int> link in snapPointLinks)
			{
				m_snapPointLinks.Add(link.Key, link.Value);
			}
			// delete object
			LE_LogicObjects.DeleteObject(m_gui3d, m_objectInstance.Obj);
			return true;
		}
		
		public override bool Rollback()
		{
			if (!base.Rollback()) { return false; }

			if (m_prefab == null)
			{
				Debug.LogError("LE_CmdDeleteObject: Rollback: could not rollback, m_prefab is null!");
				return false;
			}

			if (m_gui3d == null)
			{
				Debug.LogError("LE_CmdDeleteObject: Rollback: could not rollback, m_gui3d is null!");
				return false;
			}

			m_objectInstance.Obj = LE_LogicObjects.InstantiateObject(m_prefab, m_position, m_rotation, m_scale, m_objectResourcePath);
			m_objectInstance.UID = m_UID;
			m_objectInstance.Obj.IsRigidbodySleepingStart = m_isRigidbodySleepingStart;
			m_objectInstance.Obj.DeactivateRigidbody(); // no collision solving needed
			LE_LogicObjects.ApplyColor(m_objectInstance.Obj, m_color);
			LE_LogicObjects.ApplyVariation(m_objectInstance.Obj, m_variationIndex);
			// load snap points that were connected before deleting
			m_gui3d.SetSnapPointUIDsToObjUIDsAndApplyChanges(m_snapPointLinks);
			LE_LogicObjects.AddSnappingScripts(m_gui3d, m_objectInstance.Obj);
			LE_LogicObjects.SelectNewObjectAndNotifyListeners(m_gui3d, m_objectInstance.Obj);

			return true;
		}
	}
}
