using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using LapinerTools.uMyGUI;
using LE_LevelEditor.Core;

namespace LE_LevelEditor.UI
{
	public class LE_GUIInterface_uGUIimplBase : MonoBehaviour
	{
		protected RectTransform m_transform;

		protected virtual void Start()
		{
			if (LE_GUIInterface.Instance != null)
			{
				// register delegates
				LE_GUIInterface.Instance.delegates.IsCursorOverUI += IsCursorOverUI;
			}
			else
			{
				Debug.LogError("LE_GUIInterface_uGUIimplBase: Start: could not find LE_GUIInterface!");
			}

			m_transform = (RectTransform)transform;
			if (m_transform == null)
			{
				Debug.LogError("LE_GUIInterface_uGUIimplBase: this script must be attached to a RectTransform!");
			}
		}

		protected virtual bool IsCursorOverUI ()
		{
			// check touchscreen input
			Touch[] touches = Input.touches;
			for (int t = 0; t < touches.Length; t++)
			{
				if (EventSystem.current.IsPointerOverGameObject(touches[t].fingerId) ||
				    // input at the edge of the screen is not allowed (3d UI could have a 1 pixel offset to the edge of the screen)
				    IsAtScreensEdge(touches[t].position))
				{
					return true;
				}
			}




			// check mouse input
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IPHONE || UNITY_WP8 || UNITY_WP_8_1)
			// don't care about mouse on these platforms, since it is simulated anyway
			return false;
#else
			// check mouse if no touch detected
			return EventSystem.current.IsPointerOverGameObject()  ||
				// input at the edge of the screen is not allowed (3d UI could have a 1 pixel offset to the edge of the screen)
				IsAtScreensEdge(Input.mousePosition);
#endif
		}

		private static bool IsAtScreensEdge(Vector2 p_pos)
		{
			const float edgeSize = 4f;
			return p_pos.x < edgeSize || Screen.width < p_pos.x + edgeSize || p_pos.y < edgeSize || Screen.height < p_pos.y + edgeSize;
		}
	}
}
