using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace LE_LevelEditor.LEInput
{
	public class LE_InputDeviceMouse : LE_InputDeviceBase
	{
		private const float MOUSE_WHEEL_SENSITIVITY = 200f;
		
		private Vector3 m_mouseLookStart = Vector3.zero;
		private Vector3 m_lastMousePosition = Vector3.zero;
		private float m_lastTouchTime = -100f;
		
		public LE_InputDeviceMouse(LE_IInputHandler p_inputHandler)
			: base(p_inputHandler)
		{
			m_lastMousePosition = Input.mousePosition;
		}
		
		public override void Update ()
		{
			bool isAlt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr);

			// no mouse if touch screen is in use
			if (Input.touchCount != 0)
			{
				// if device can handle touches and mouse then prefer touch before mouse
				m_lastTouchTime = Time.realtimeSinceStartup;
				m_lastMousePosition = Input.mousePosition;
				return;
			}
			if (Time.realtimeSinceStartup - m_lastTouchTime <= 1f)
			{
				// wait for some time before reactivating mouse input again
				m_lastMousePosition = Input.mousePosition;
				return;
			}

			// cursor activation
			if (m_lastMousePosition != Input.mousePosition)
			{
				m_inputHandler.SetCursorPosition(Input.mousePosition);
			}
			m_inputHandler.SetIsCursorAction(Input.GetMouseButton(0) && !isAlt);

			// camera direction
			if (Input.GetMouseButton(1) || (isAlt && Input.GetMouseButton(0)))
			{
				if (m_mouseLookStart.sqrMagnitude == 0)
				{
					m_mouseLookStart = Input.mousePosition;
				}
				else if ((m_mouseLookStart - Input.mousePosition).magnitude > 0.0001f)
				{
					if (isAlt)
					{
						m_inputHandler.RotateCameraAroundPivot(Input.mousePosition, m_mouseLookStart);
					}
					else
					{
						m_inputHandler.RotateCamera(Input.mousePosition, m_mouseLookStart);
					}
					m_mouseLookStart = Input.mousePosition;
				}
			}
			else
			{
				m_mouseLookStart = Vector3.zero;
			}

			// zoom (only if mouse is on screen and not over UI)
			if (EventSystem.current != null && !EventSystem.current.IsPointerOverGameObject())
			{
				float mouseWheel = Input.GetAxis("Mouse ScrollWheel");
				if (mouseWheel != 0f)
				{
					m_inputHandler.MoveCamera(Vector3.zero, Vector3.forward*mouseWheel*MOUSE_WHEEL_SENSITIVITY);
				}
			}
		}

		public override void Destroy ()
		{
		}
	}
}
