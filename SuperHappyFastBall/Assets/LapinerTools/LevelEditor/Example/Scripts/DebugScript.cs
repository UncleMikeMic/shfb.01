#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_6_1 || UNITY_4_6_2 || UNITY_4_6_3 || UNITY_4_6_4 || UNITY_4_6_5 || UNITY_4_6_6 || UNITY_4_6_7 || UNITY_4_6_8 || UNITY_4_6_9
#define UNITY_VERSION_4
#endif

using UnityEngine;
using System.Collections;

#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

// warning generation for Unity 5
[System.Obsolete("You seem to compile with a very early version of Unity 5. Unfortunately, Unity 5.0.0 has a bug in its terrain engine (http://issuetracker.unity3d.com/issues/raycast-when-terrain-is-changed-but-not-saved-raycasthit-dot-point-returns-incorrect-value). The minimal required Unity 5 version for the terrain editor to work is Unity 5.0.0p3 (not 5.0.0f4). Get the patched Unity version here: http://unity3d.com/unity/qa/patch-releases/5.0.0p3")]
public class Unity5_Terrain: System.Attribute{}

// warning generation for Windows
[System.Obsolete("In Unity 4.6.4 and 5.0.0 Windows Store App support is still buggy. On devices with an attachable keyboard(mouse) like the Surface tablets from Microsoft the flag 'Input.simulateMouseWithTouches' must be disabled otherwise all sliders and draggable UI stops working after a while when used with touchscreen. Therefore, you must make sure to set 'Input.simulateMouseWithTouches=false;' in your project!!!")]
public class Win_Hacks2: System.Attribute{}

namespace LE_LevelEditor.Example
{
	public class DebugScript : MonoBehaviour
	{
		private string m_debugString = "";
		private int m_linesCount = 0;
		private float m_timeFPS = 0;
		private int m_counterFPS = 0;
		private int m_fps = 0;

		private Rect m_qualityUpBtnRect;
		private Rect m_qualityLabelRect;
		private Rect m_qualityDownBtnRect;

#if UNITY_METRO
		[Win_Hacks2]
#endif
#if UNITY_5_0_0
		[Unity5_Terrain]
#endif
		void Awake()
		{
#if UNITY_METRO
			Input.simulateMouseWithTouches = false;
#endif

#if UNITY_VERSION_4
			Application.RegisterLogCallback(HandleLog);
#else

			Application.logMessageReceived += HandleLog;
#endif

#if UNITY_5_0_0
			Debug.LogError("Remove this log if you use Unity 5.0.0p3 or higher. Unfortunately, Unity 5.0.0 has a bug in its terrain engine. The minimal required Unity 5 version for the terrain editor to work is 5.0.0p3 (not 5.0.0f4). See compiler warning 'Unity5_Terrain' for more information.");
#endif

#if UNITY_5_3_OR_NEWER
			if (SceneManager.GetActiveScene().name != "LE_ExampleGame")
#else
			if (Application.loadedLevelName != "LE_ExampleGame")
#endif
			{
				m_qualityUpBtnRect = new Rect(0, Screen.height-108, 67, 35);
				m_qualityLabelRect = new Rect(0, Screen.height-73, 67, 38);
				m_qualityDownBtnRect = new Rect(0, Screen.height-35, 67, 35);
			}
			else
			{
				m_qualityUpBtnRect = new Rect(Screen.width-67, 0, 67, 35);
				m_qualityLabelRect = new Rect(Screen.width-67, 35, 67, 38);
				m_qualityDownBtnRect = new Rect(Screen.width-67, 73, 67, 35);
			}
		}
		
		void Update()
		{
			m_timeFPS += Time.deltaTime;
			m_counterFPS++;
			if (m_timeFPS >= 1f)
			{
				m_timeFPS -= 1f;
				m_fps = m_counterFPS;
				m_counterFPS = 0;
			}
		}

#if !UNITY_VERSION_4
		void OnDestroy()
		{
			Application.logMessageReceived -= HandleLog;
		}
#endif
		
		void OnGUI()
		{
			GUI.depth = -9999;
			GUILayout.Label("                 FPS: " + m_fps + "\tMRLE v1.38 + Save/Load v2.3");
			GUILayout.Label(m_debugString);

#if UNITY_EDITOR || UNITY_WEBPLAYER || UNITY_WEBGL || UNITY_STANDALONE || (UNITY_METRO && !UNITY_WP_8_1)
#if UNITY_5_3_OR_NEWER
			if (SceneManager.GetActiveScene().name == "LE_ExampleDungeonEditorGame" || SceneManager.GetActiveScene().name == "LE_ExampleEditorFirstPerson")
#else
			if (Application.loadedLevelName == "LE_ExampleDungeonEditorGame" || Application.loadedLevelName == "LE_ExampleEditorFirstPerson")
#endif
			{
				GUI.Label(new Rect(100, Screen.height-20, 250, 20), "<b>Look around with RIGHT MOUSE button</b>");
			}
#endif

			GUI.Box(m_qualityLabelRect, "Quality\n" + QualitySettings.names[QualitySettings.GetQualityLevel()]);
			if (GUI.Button(m_qualityUpBtnRect, "Increase"))
			{
				QualitySettings.IncreaseLevel();
			}
			if (GUI.Button(m_qualityDownBtnRect, "Decrease"))
			{
				QualitySettings.DecreaseLevel();
			}
		}
		
		void HandleLog (string logString, string stackTrace, LogType type)
		{
			string nextLine = "["+type+"] " + logString + "\n";

			if (logString.StartsWith("LE_LevelEditorMain: Inspector property")) { return; }
			if (m_debugString.StartsWith(nextLine)) { return; }

			m_debugString += nextLine;
			m_linesCount++;
			if (m_linesCount > Screen.height / 20f)
			{
				m_debugString = m_debugString.Substring(m_debugString.IndexOf("\n")+1);
			}
		}
	}
}
