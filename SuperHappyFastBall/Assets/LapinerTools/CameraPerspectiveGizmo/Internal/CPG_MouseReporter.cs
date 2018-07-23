using UnityEngine;
using System.Collections;
using MyUtility;

namespace CPG_CameraPerspective.Internal
{
	public class CPG_MouseReporter : MonoBehaviour
	{
		[SerializeField]
		private CPG_CameraPerspectiveGizmo m_parent;
		[SerializeField]
		private CPG_CameraPerspectiveGizmo.EButtonTypes m_type;

		private void Start()
		{
			UtilityClickTouchDetector clickTouch = gameObject.AddComponent<UtilityClickTouchDetector>();
			clickTouch.CameraInstance = m_parent.OwnCamera;
			clickTouch.m_onClick += OnClicked;
		}

		private void OnClicked()
		{
			if (m_parent != null)
			{
				m_parent.ReportClick(m_type);
			}
		}
	}
}
