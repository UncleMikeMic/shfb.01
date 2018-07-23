using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace LE_LevelEditor.UI
{
	public class LE_HelpSelector : MonoBehaviour
	{
		[SerializeField]
		private Toggle MOUSE_AND_KEYBOARD_TOGGLE;
		[SerializeField]
		private Toggle TOUCHSCREEN_TOGGLE;

		private void Start()
		{
			if (MOUSE_AND_KEYBOARD_TOGGLE != null && TOUCHSCREEN_TOGGLE != null)
			{
#if UNITY_ANDROID || UNITY_IPHONE || UNITY_WINRT
				TOUCHSCREEN_TOGGLE.isOn = true;
#else
				MOUSE_AND_KEYBOARD_TOGGLE.isOn = true;
#endif
			}
			else
			{
				Debug.LogError("LE_HelpSelector: MOUSE_AND_KEYBOARD_TOGGLE or TOUCHSCREEN_TOGGLE were not set in inspector!");
			}
		}
	}
}
