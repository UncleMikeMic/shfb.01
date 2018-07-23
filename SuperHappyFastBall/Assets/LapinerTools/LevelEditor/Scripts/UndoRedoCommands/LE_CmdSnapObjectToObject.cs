using UnityEngine;
using System.Collections;
using LE_LevelEditor.UI;
using LE_LevelEditor.Logic;
using LE_LevelEditor.Core;
using S_SnapTools;

namespace LE_LevelEditor.Commands
{
	public class LE_CmdSnapObjectToObject : LE_CmdBase
	{
		private LE_GUI3dObject m_gui3d;
		private LE_CmdObjectLink m_sourceObj;
		private LE_CmdObjectLink m_snappedObj = new LE_CmdObjectLink();
		private int m_sourceSnapPointIndex;
		private S_SnapToObjectPrefab m_snapPrefab;

		public LE_CmdSnapObjectToObject(LE_GUI3dObject p_gui3d, int p_sourceObjUID, int p_sourceSnapPointIndex, S_SnapToObjectPrefab p_snapPrefab)
		{
			m_gui3d = p_gui3d;
			m_sourceObj = new LE_CmdObjectLink(p_sourceObjUID);
			m_sourceSnapPointIndex = p_sourceSnapPointIndex;
			m_snapPrefab = p_snapPrefab;
		}

		public override long GetStoredBytes()
		{
			return 30;
		}
		
		public override bool Execute()
		{
			if (!base.Execute()) { return false; }
			
			if (m_snapPrefab == null)
			{
				Debug.LogError("LE_CmdSnapObjectToObject: Execute: could not execute, m_snapPrefab is null!");
				return false;
			}

			if (m_sourceObj.Obj == null)
			{
				Debug.LogError("LE_CmdSnapObjectToObject: Execute: could not execute, m_sourceObj is null!");
				return false;
			}

			if (m_sourceSnapPointIndex >= m_sourceObj.Obj.ObjectSnapPoints.Length)
			{
				Debug.LogError("LE_CmdSnapObjectToObject: Execute: could not execute, m_sourceSnapPointIndex('"+m_sourceSnapPointIndex+"') is not a valid[0,"+(m_sourceObj.Obj.ObjectSnapPoints.Length-1)+"] snap point index!");
				return false;
			}

			S_SnapToObject snapSys = m_sourceObj.Obj.ObjectSnapPoints[m_sourceSnapPointIndex].SnapSystemInstance;
			if (snapSys == null)
			{
				Debug.LogError("LE_CmdSnapObjectToObject: Execute: could not execute, m_sourceSnapPointIndex('"+m_sourceSnapPointIndex+"') leads to a null SnapSystemInstance!");
				return false;
			}

			snapSys.OnAfterObjectSnapped += OnAfterObjectSnapped;
			snapSys.PlacePrefab(m_snapPrefab);
			snapSys.OnAfterObjectSnapped -= OnAfterObjectSnapped;

			return true;
		}
		
		public override bool Rollback()
		{
			if (!base.Rollback()) { return false; }

			if (m_snappedObj.Obj == null)
			{
				Debug.LogError("LE_CmdSnapObjectToObject: Rollback: could not rollback, m_snappedObj is null!");
				return false;
			}

			if (m_gui3d == null)
			{
				Debug.LogError("LE_CmdSnapObjectToObject: Rollback: could not rollback, m_gui3d is null!");
				return false;
			}

			LE_LogicObjects.DeleteObject(m_gui3d, m_snappedObj.Obj);
			return true;
		}

		private void OnAfterObjectSnapped(object p_sender, S_SnapToObjectEventArgs p_args)
		{
			if (m_gui3d == null)
			{
				Debug.LogError("LE_CmdSnapObjectToObject: OnAfterObjectSnapped: m_gui3d is null!");
				return;
			}

			LE_Object newObj = p_args.NewInstance.GetComponent<LE_Object>();
			if (newObj != null)
			{
				if (m_snappedObj.UID > 0)
				{
					newObj.UID = m_snappedObj.UID; // reuse snap object UID of the first creation
				}
				m_snappedObj.Obj = newObj;
				LE_LogicObjects.OnNewObjectSnapped(m_gui3d, newObj, p_args);
			}
			else
			{
				Debug.LogError("LE_CmdSnapObjectToObject: OnAfterObjectSnapped: LE_Object is not attached to the root of the new object! This object will not be saved if it has no LE_Object attached!");
			}
		}
	}
}
