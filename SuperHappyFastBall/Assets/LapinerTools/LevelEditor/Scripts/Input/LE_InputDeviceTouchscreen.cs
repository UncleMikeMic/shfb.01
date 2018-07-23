using UnityEngine;
using System.Collections;
using TG_TouchGesture;

namespace LE_LevelEditor.LEInput
{
	public class LE_InputDeviceTouchscreen : LE_InputDeviceBase
	{
		private const float ZOOM_SENSITIVITY = 1500f;

		private int m_lastCursorActivationFrame = -100;

		public LE_InputDeviceTouchscreen(LE_IInputHandler p_inputHandler)
			: base(p_inputHandler)
		{
			TG_TouchGestures.Instance.EnableGesture(TG_ETouchGestureType.PRESS_1_FINGER);
			TG_TouchGestures.Instance.EnableGesture(TG_ETouchGestureType.PRESS_2_FINGER);
			TG_TouchGestures.Instance.EnableGesture(TG_ETouchGestureType.PRESS_3_FINGER);
			TG_TouchGestures.Instance.EnableGesture(TG_ETouchGestureType.ZOOM);
			TG_TouchGestures.Instance.OnGestureDetected += OnTouchGestureDetected;
		}
		
		public override void Update ()
		{
			// reset cursor action state if no cursor activating gestures were detected
			// in this frame it might happen that gestures will be detected in this frame
			// later in this case the cursor action state will be overwritten again
			if (m_lastCursorActivationFrame != Time.frameCount)
			{
				m_inputHandler.SetIsCursorAction(false);
			}
		}

		public override void Destroy ()
		{
			if (TG_TouchGestures.IsInstanceSet)
			{
				TG_TouchGestures.Instance.OnGestureDetected -= OnTouchGestureDetected;
			}
		}
		
		private void OnTouchGestureDetected(object p_object, TG_TouchGestureEventArgs p_args)
		{
			switch (p_args.Type)
			{
				// cursor activation
				case TG_ETouchGestureType.PRESS_1_FINGER:
				{
					Vector2 touch = p_args.Position;
					m_inputHandler.SetCursorPosition(new Vector3(touch.x, touch.y, 0f));
					m_inputHandler.SetIsCursorAction(true);
					m_lastCursorActivationFrame = Time.frameCount;
					break;
				}
				// camera direction
				case TG_ETouchGestureType.PRESS_2_FINGER:
				{
					Vector2 touch = p_args.Position;
					Vector2 delta = p_args.Delta;
					m_inputHandler.RotateCamera(touch, touch+delta);
					break;
				}
				// camera movement
				case TG_ETouchGestureType.PRESS_3_FINGER:
				{
					Vector2 touch = p_args.Position;
					Vector2 delta = p_args.Delta;
					m_inputHandler.MoveCamera(touch, touch-delta);
					break;
				}
				// zoom
				case TG_ETouchGestureType.ZOOM:
				{
					float zoomValue = p_args.Delta.x;
					m_inputHandler.MoveCamera(Vector3.zero, Vector3.forward*zoomValue*ZOOM_SENSITIVITY);
					break;
				}
			}
		}
	}
}
