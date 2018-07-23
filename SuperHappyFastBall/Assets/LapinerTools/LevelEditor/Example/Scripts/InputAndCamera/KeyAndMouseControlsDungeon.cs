using UnityEngine;
using System.Collections;

namespace LE_LevelEditor.Example
{
	public class KeyAndMouseControlsDungeon : MonoBehaviour
	{
		[SerializeField]
		private float SPEED = 5f;
		[SerializeField]
		private float ROT_SPEED = 1f;
		[SerializeField]
		private Transform CAMERA_PIVOT;
		
		private CharacterController m_character;
		private Vector3 m_lastMousePos;
		private int m_lastTouchFrame = 0;
		
		void Start ()
		{
			m_character = GetComponent<CharacterController>();
			m_lastMousePos = Input.mousePosition;
		}
		
		void Update ()
		{
			// Disable in combination with touch screen
			if (Input.touchCount!=0)
			{
				m_lastTouchFrame = Time.frameCount;
			}
			
			// Movement
			Vector3 movement = Vector3.zero;
			if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) { movement += Vector3.forward; }
			if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) { movement += Vector3.left; }
			if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) { movement += Vector3.back; }
			if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) { movement += Vector3.right; }
			if (Input.GetKey(KeyCode.Space)) { movement += Vector3.up; }
			movement.Normalize();
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			{
				movement *= 5f;
			}
			if (movement != Vector3.zero)
			{
				// Actually move the character
				m_character.Move( Camera.main.transform.TransformDirection(movement*SPEED*Time.deltaTime));
			}
			
			// Look
			if (Input.mousePosition != m_lastMousePos)
			{
				if (m_lastTouchFrame + 10 < Time.frameCount && Input.GetMouseButton(1))
				{
					Vector3 euler = Vector3.zero;
					euler.x = (0.5f - (Input.mousePosition.y / (float)Screen.height)) * 65f+90f;
					euler.y = (Input.mousePosition.x / (float)Screen.width) * 600f * ROT_SPEED;
					CAMERA_PIVOT.rotation = Quaternion.Euler(euler);
				}
				m_lastMousePos = Input.mousePosition;
			}
		}
	}
}
