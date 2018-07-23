using UnityEngine;
using System.Collections;

namespace MyUtility
{
	public class UtilityClickTouchDetector : MonoBehaviour
	{
		private enum ECursorState { DOWN_FRAME, UP_FRAME, HELD }

		public System.Action m_onClick;

		private Collider m_collider;
		public Collider ColliderInstance
		{
			get{ return m_collider; }
			set{ m_collider = value; }
		}

		private Camera m_camera;
		public Camera CameraInstance
		{
			get{ return m_camera; }
			set{ m_camera = value; }
		}

		private bool m_isMouseDown = false;
		private bool m_isHeldDown = false;

		private void Start()
		{
			if (m_collider == null)
			{
				m_collider = GetComponent<Collider>();
				if (m_collider == null)
				{
					Debug.LogError("UtilityClickTouchDetector: could not find a collider!");
					enabled = false;
				}
			}

			if (m_camera == null)
			{
				m_camera = Camera.main;
				if (m_camera == null)
				{
					Debug.LogError("UtilityClickTouchDetector: could not find a camera!");
					enabled = false;
				}
			}
		}
		
		private void Update()
		{
			if (m_collider != null && m_camera != null)
			{
				// check mouse
				bool isMouseDownNow = Input.GetMouseButton(0);
				if (isMouseDownNow || m_isMouseDown)
				{
					ECursorState mouseState = isMouseDownNow?(m_isMouseDown?ECursorState.HELD:ECursorState.DOWN_FRAME):ECursorState.UP_FRAME;
					RaycastCursor(Input.mousePosition, mouseState);
					m_isMouseDown = isMouseDownNow;
				}
				// check touch
				Touch[] touches = Input.touches;
				for (int i = 0; i < touches.Length; i++)
				{
					ECursorState touchState;
					switch (touches[i].phase)
					{
						case TouchPhase.Began:
							touchState = ECursorState.DOWN_FRAME;
							break;
						case TouchPhase.Ended:
							touchState = ECursorState.UP_FRAME;
							break;
						case TouchPhase.Moved:
						case TouchPhase.Stationary:
							touchState = ECursorState.HELD;
							break;
						case TouchPhase.Canceled:
						default:
							continue;
					}
					RaycastCursor(touches[i].position, touchState);
				}
			}
			else if (m_collider == null)
			{
				Debug.LogError("UtilityClickTouchDetector: Update: lost reference to collider!");
				enabled = false;
			}
			else
			{
				Debug.LogError("UtilityClickTouchDetector: Update: lost reference to camera!");
				enabled = false;
			}
		}

		private void OnDestroy()
		{
			m_onClick = null;
		}

		private void RaycastCursor(Vector3 p_screenPos, ECursorState p_state)
		{
			RaycastHit hit;
			if (m_collider.Raycast(m_camera.ScreenPointToRay(p_screenPos), out hit, float.MaxValue))
			{
				if (m_isHeldDown)
				{
					switch (p_state)
					{
						case ECursorState.DOWN_FRAME:
							// was held already and klicked down again (maybe multiple input modules or multitouch)
							m_isHeldDown = true;
							break;
						case ECursorState.UP_FRAME:
							// was held down and is released in this frame -> valid click
							m_isHeldDown = false;
							if (m_onClick != null) { m_onClick(); }
							break;
	//					case ECursorState.HELD:
	//					default:
	//						// was held down and is still held down -> nothing changed
	//						break;
					}
				}
				else
				{
					switch (p_state)
					{
						case ECursorState.DOWN_FRAME:
							// was up and has been pressed down in this frame -> valid down
							m_isHeldDown = true;
							break;
	//					case ECursorState.UP_FRAME:
	//						// was up and cursor was released over it -> not valid up -> nothing changes
	//						break;
	//					case ECursorState.HELD:
	//					default:
	//						// was up and cursor is held and moved over it -> not valid down -> nothing changes
	//						break;
					}
				}
			}
			else
			{
				if (m_isHeldDown)
				{
					switch (p_state)
					{
						case ECursorState.DOWN_FRAME:
							// was held down and cursor was put down over another object (maybe multiple input modules or multitouch) -> invalidates click
						case ECursorState.UP_FRAME:
							// was held down and cursor was released over another object -> invalidates click
							m_isHeldDown = false;
							break;
	//					case ECursorState.HELD:
	//						// was held down and cursor is still down but moved away -> nothing changes
	//					default:
	//						break;
					}
				}
	//			else
	//			{
	//				switch (p_state)
	//				{
	//					case ECursorState.DOWN_FRAME:
	//						// was up and cursor was pressed down over another object -> nothing changes
	//						break;
	//					case ECursorState.UP_FRAME:
	//						// was up and cursor was clicked over another object -> nothing changes
	//						break;
	//					case ECursorState.HELD:
	//						// was up and cursor was moved while down over another object -> nothing changes
	//					default:
	//						break;
	//				}
	//			}
			}
		}
	}
}
