using UnityEngine;
using System.Collections;

namespace LE_LevelEditor.LEInput
{
	public class LE_Input
	{
		private LE_InputDeviceBase[] m_inputDevices;
		
		public LE_Input(LE_IInputHandler p_inputHandler)
		{
			m_inputDevices = new LE_InputDeviceBase[3];
			m_inputDevices[0] = new LE_InputDeviceKeyboard(p_inputHandler);
			m_inputDevices[1] = new LE_InputDeviceMouse(p_inputHandler);
			m_inputDevices[2] = new LE_InputDeviceTouchscreen(p_inputHandler);
		}
		
		public void Update()
		{
			for (int i=0; i<m_inputDevices.Length; i++)
			{
				m_inputDevices[i].Update();
			}
		}

		public void Destroy()
		{
			for (int i=0; i<m_inputDevices.Length; i++)
			{
				m_inputDevices[i].Destroy();
			}
		}
	}
}
