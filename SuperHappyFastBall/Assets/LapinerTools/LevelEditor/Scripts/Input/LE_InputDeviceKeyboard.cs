using UnityEngine;
using System.Collections;

namespace LE_LevelEditor.LEInput
{
	public class LE_InputDeviceKeyboard : LE_InputDeviceBase
	{
		private const float MOVE_SPEED = 6f;
		private const float MOVE_SPEED_SHIFT_FACTOR = 5f;

		public LE_InputDeviceKeyboard(LE_IInputHandler p_inputHandler)
			: base(p_inputHandler)
		{
		}
		
		public override void Update ()
		{
			Vector3 moveDirection = Vector3.zero;
			if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
			{
				moveDirection += Vector3.left;
			}
			if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
			{
				moveDirection += Vector3.right;
			}
			if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
			{
				moveDirection += Vector3.back;
			}
			if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
			{
				moveDirection += Vector3.forward;
			}
			if (Input.GetKey(KeyCode.Q))
			{
				moveDirection += Vector3.down;
			}
			if (Input.GetKey(KeyCode.E))
			{
				moveDirection += Vector3.up;
			}
			moveDirection *= MOVE_SPEED;
			if (moveDirection.sqrMagnitude != 0)
			{
				if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				{
					moveDirection *= MOVE_SPEED_SHIFT_FACTOR;
				}
				m_inputHandler.MoveCamera(Vector3.zero, moveDirection);
			}
		}
		
		public override void Destroy ()
		{
		}
	}
}
