using UnityEngine;
using System.Collections;
using LE_LevelEditor.LEInput;

namespace LE_LevelEditor.UI
{
	public abstract class LE_GUI3dBase : MonoBehaviour, LE_IInputHandler
	{
		protected Ray m_cursorRay = new Ray();
		protected RaycastHit m_cursorHitInfo;

		private bool m_isCursorOverSomething = false;
		public bool IsCursorOverSomething
		{
			get
			{
				CheckCursor();
				return m_isCursorOverSomething;
			}
		}
		public void SetIsCursorOverSomething(bool p_isHit) { m_isCursorOverSomething = p_isHit; }

		private int m_inactiveFrame = -1;
		public bool IsInteractable
		{
			get
			{
				// add one more frame to compensate script execution order
				// the script that sets IsInteractable could be executed after
				// the script that reads IsInteractable
				return m_inactiveFrame+1 < Time.frameCount;
			}
			set
			{
				if (value)
				{
					m_inactiveFrame = -1;
				}
				else
				{
					m_inactiveFrame = Time.frameCount;
				}
			}
		}

		protected Vector3 m_cursorScreenCoords = -1f*Vector3.zero;
		public Vector3 CursorScreenCoords
		{
			get { return m_cursorScreenCoords; }
		}

		public abstract LE_EEditMode ActiveEditMode { get; }

		public virtual void SetCursorPosition(Vector3 p_cursorScreenCoords)
		{
			m_cursorScreenCoords = p_cursorScreenCoords;
			m_cursorRay = Camera.main.ScreenPointToRay(p_cursorScreenCoords);
			m_isCursorOverSomething = Physics.Raycast(m_cursorRay, out m_cursorHitInfo);
		}

		public virtual void SetIsCursorAction(bool p_isCursorAction)
		{
		}
		
		public virtual void MoveCamera(Vector3 p_fromScreenCoords, Vector3 p_toScreenCoords)
		{
		}
		
		public virtual void RotateCamera(Vector3 p_fromScreenCoords, Vector3 p_toScreenCoords)
		{
		}
		
		public virtual void RotateCameraAroundPivot(Vector3 p_fromScreenCoords, Vector3 p_toScreenCoords)
		{
		}

		private void CheckCursor()
		{
			if (m_isCursorOverSomething && (m_cursorHitInfo.collider == null || m_cursorHitInfo.transform == null))
			{
				m_isCursorOverSomething = false;
			}
		}
	}
}
