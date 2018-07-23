using UnityEngine;
using System.Collections;

using LE_LevelEditor.Core;
using LE_LevelEditor.Extensions;

using LapinerTools.uMyGUI;

#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace LE_LevelEditor.Example
{
	public class ExampleGame_Game : MonoBehaviour
	{
		[SerializeField]
		private int TERRAIN_LAYER = 28;
		[SerializeField]
		private LE_TerrainTextureConfig TERRAIN_TEXTURE_CONFIG = null;
		[SerializeField]
		private GameObject PLAYER = null;

		private void Start ()
		{
			ExampleGame_LoadSave.Init();

			// load level
			LE_ExtensionInterface.Load.Delegate(this, (byte[][] p_levelData)=>
			{
				if (p_levelData != null && p_levelData.Length > 0 && p_levelData[0] != null)
				{
					// load level data (we do not need p_levelData[1], since it contains only meta data for example the level icon)
					// however, you might want to load it as well when you add other information to it for example the level time
					LE_SaveLoadData level = LE_SaveLoad.LoadLevelDataFromByteArray(
						p_levelData[0],
						TERRAIN_LAYER,
						TERRAIN_TEXTURE_CONFIG.TERRAIN_TEXTURES,
						TERRAIN_TEXTURE_CONFIG.TERRAIN_TEXTURE_SIZES,
						TERRAIN_TEXTURE_CONFIG.TERRAIN_TEXTURE_OFFSETS);
					// call this function to destroy level editing scripts and improve performance
					LE_SaveLoad.DisableLevelEditing(level);
				}
				else
				{
					Debug.LogError("ExampleGame_Game: No saved level found!");
				}

				// hide loading popup shown after calling LE_ExtensionInterface.Load
				uMyGUI_PopupManager.Instance.HidePopup(uMyGUI_PopupManager.POPUP_LOADING);

				// find player start position
				/*GameObject goPlayerStart = GameObject.Find("Objects/PlayerStartPosition");
				if (goPlayerStart != null)
				{
					PLAYER.transform.position = goPlayerStart.transform.position + goPlayerStart.transform.up;
					Destroy(goPlayerStart); // not needed any more
				}
				else
				{
					Debug.LogError("ExampleGame_Game: could not find a PlayerStartPosition GameObject!");
				}*/
			}, true);
		}

#if UNITY_WP8 || UNITY_WP_8_1
		// back button implementation is required for windows phone certification
		private void Update()
		{
			if (Input.GetKey(KeyCode.Escape))
			{
				// tell the editor to reload the level
				ExampleGame_Editor.m_isComingBackFromGame = true;
				m_goBackToEditor = true;
			}
		}
#endif
		
		private void OnGUI()
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(400);
			if (GUILayout.Button("Back to Editor", GUILayout.Height(40)))
			{
				StartCoroutine(LateLoadEditor());
			}
			GUILayout.EndHorizontal();
		}

		private IEnumerator LateLoadEditor()
		{
			// hide UI
			enabled = false;
			// show loading popup
			uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_LOADING);
			// wait for the popup animation to finish
			yield return new WaitForSeconds(1f);
			// tell the editor to reload the level
			ExampleGame_Editor.m_isComingBackFromGame = true;
			// load editor scene
#if UNITY_5_3_OR_NEWER
			SceneManager.LoadScene("LE_ExampleEditor");
#else
			Application.LoadLevel("LE_ExampleEditor");
#endif
		}
	}
}