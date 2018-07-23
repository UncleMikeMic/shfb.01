using UnityEngine;
using System.Collections;

namespace LE_LevelEditor.LEInput
{
	public abstract class LE_InputDeviceBase
	{
		protected LE_IInputHandler m_inputHandler;
		
		public LE_InputDeviceBase(LE_IInputHandler p_inputHandler)
		{
			m_inputHandler = p_inputHandler;
		}
		
		public abstract void Update();
		public abstract void Destroy();
	}
}
