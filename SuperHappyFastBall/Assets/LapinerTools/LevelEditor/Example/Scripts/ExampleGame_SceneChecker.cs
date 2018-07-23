using UnityEngine;
using System.Collections;

#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace LE_LevelEditor.Example
{
	public class ExampleGame_SceneChecker : MonoBehaviour
	{
		private string m_errorMsg = null;

		private void Start ()
		{
#if UNITY_5_3_OR_NEWER
			if (SceneManager.sceneCountInBuildSettings < 4)
#else
			if (Application.levelCount < 4)
#endif
			{
				m_errorMsg =
						"<b>There is one more thing you have to do...\n" +
						"Please add\n" +
						"<color=red>" +
						"LE_ExampleEditor\n" +
						"LE_ExampleGame\n" +
						"LE_ExampleDungeonEditorGame\n" +
						"LE_ExampleEditorTerrainOnly" +
						"</color>\n" +
						"scenes to your Build Settings.</b>\n" +
						"<color=red>To add a level to the build settings use the menu File->Build Settings...</color>\n";
#if UNITY_5_3_OR_NEWER
				if (SceneManager.GetActiveScene().name != "LE_ExampleEditor")
#else
				if (Application.loadedLevelName != "LE_ExampleEditor")
#endif
				{
					m_errorMsg += "Without adding the example scenes to the Build Settings you will not be able to load the game scene when the 'Play' button is pressed!";
				}
				else
				{
					m_errorMsg += "Without adding the example scenes to the Build Settings you will not be able to load the editor scene when the 'Back To Editor' button is pressed!";
				}
			}
			else
			{
				Destroy(this);
			}
		}

		private void OnGUI()
		{
			if (!string.IsNullOrEmpty(m_errorMsg))
			{
				GUI.depth = -10000;
				Rect boxRect = new Rect(Screen.width*0.1f, Screen.height*0.05f, Screen.width*0.8f, Screen.height*0.8f);
				GUI.Box(boxRect, "");
				GUI.Box(boxRect, "");
				GUI.Box(boxRect, "");
				GUI.Box(boxRect, "");
				Rect labelRect = new Rect(boxRect);
				labelRect.xMin += 35f;
				labelRect.xMax -= 35f;
				labelRect.yMin += 35f;
				labelRect.yMax -= 35f;
				GUI.Label(labelRect, m_errorMsg);
				Rect btnRect = new Rect(boxRect);
				btnRect.yMin = btnRect.yMax;
				btnRect.height = Screen.height*0.1f;
				if (GUI.Button(btnRect, "Close"))
				{
					Destroy(this);
				}
			}
		}
	}
}
