using UnityEngine;
using System.Collections;
using System.IO;
using LE_LevelEditor.Core;
using LE_LevelEditor.Events;
using LapinerTools.uMyGUI;
using LE_LevelEditor.Extensions;

#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace LE_LevelEditor.Example
{
	public class ExampleGame_Editor : MonoBehaviour
	{
		// if this static variable is set to true then the saved level will be loaded
		// if the player has just tested his level he will want the level to be loaded when he comes back to the editor
		public static bool m_isComingBackFromGame = false;

		// indicated the frame number (Time.frameCount) of the latest save to file action
		// this way the latest save frame can be compared with the latest change frame, which
		// allows to determine if there are changes made that are not saved yet
		private int m_lastSaveFrame = -100;

		// start level if there are no unsaved changes and the level is playable, otherwise show an error message (this method is called by the UI button)
		public void OnPlayButtonClick()
		{
			if (m_lastSaveFrame > LE_LevelEditorMain.Instance.LastChangeFrame)
			{
				string errorMessage;
				if (IsLevelPlayable(out errorMessage))
				{
					uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_LOADING);
					StartCoroutine(LatePlay("LE_ExampleGame"));
					
				}
				else
				{
					((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT)).SetText("Game logic objects missing!", errorMessage).ShowButton("ok");
				}
			}
			else
			{
				((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT)).SetText(
					"Unsaved Changes!",
					"There are unsaved changes. You need to save the level before you can play it!").ShowButton("ok");
			}
		}
		
		// open the documentation of the Multiplatform Runtime Level Editor (linked to a button in left menu)
		public void OnDocumentationBtn()
		{
			Application.OpenURL("http://www.freebord-game.com/index.php/multiplatform-runtime-level-editor/documentation");
		}

		// open the documentation of the Multiplatform Runtime Level Editor (linked to a button in left menu)
		public void OnFullLevelEditorExampleBtn()
		{
			uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_LOADING);
			StartCoroutine(LatePlay("LE_ExampleEditor"));
		}

		// open the documentation of the Multiplatform Runtime Level Editor (linked to a button in left menu)
		public void OnDungeonGameExampleBtn()
		{
			uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_LOADING);
			StartCoroutine(LatePlay("LE_ExampleDungeonEditorGame"));
		}

		// open the documentation of the Multiplatform Runtime Level Editor (linked to a button in left menu)
		public void OnPureTerrainEditorExampleBtn()
		{
			uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_LOADING);
			StartCoroutine(LatePlay("LE_ExampleEditorTerrainOnly"));
		}

		// close the application (linked to a button in left menu)
		public void OnExitBtn()
		{
	#if UNITY_WEBPLAYER || UNITY_WEBGL
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT))
				.SetText("Exit", "Simply close this browser window to close the application ;)").ShowButton("ok");
	#else
			Application.Quit();
	#endif
		}

		// load level if player is coming back from a test session
		// register for events
		private void Start()
		{
			ExampleGame_LoadSave.Init();

			// when the game was started from the editor and the user is coming back to the editor from his game session
			// he probably will want the level that he has played to be loaded into the editor
			if (m_isComingBackFromGame)
			{
				// reload level
				LE_ExtensionInterface.Load.Delegate(this, (byte[][] p_levelData)=>
				{
					if (LE_LevelEditorMain.Instance.IsReady)
					{
						StartCoroutine(LateLoad(LE_LevelEditorMain.Instance.GetLoadEvent(), p_levelData));
					}
					else
					{
						LE_LevelEditorMain.Instance.ExecuteWhenReady(()=>StartCoroutine(LateLoad(LE_LevelEditorMain.Instance.GetLoadEvent(), p_levelData)));
					}
				}, true);
			}
			m_isComingBackFromGame = false;
			// set up the event handling (link the buttons in the editor to functions in this script)
			LE_EventInterface.OnSave += OnSave;
			LE_EventInterface.OnLoad += OnLoad;
		}

	#if UNITY_WP8 || UNITY_WP_8_1
		// back button implementation is required for windows phone certification
		private void Update()
		{
			if (Input.GetKey(KeyCode.Escape))
			{
				Application.Quit();
			}
		}
	#endif

		// remove all references to this instance
		private void OnDestroy()
		{
			// always remove references to this script when it is destroyed otherwise memory leaks can be possible
			LE_EventInterface.OnSave -= OnSave;
			LE_EventInterface.OnLoad -= OnLoad;
			LE_EventInterface.UnregisterAll();
		}

		// save the current level to a file and note the latest save action frame
		private void OnSave(object p_sender, LE_SaveEvent p_args)
		{
			m_lastSaveFrame = Time.frameCount;
			LE_ExtensionInterface.Save.Delegate(this, p_args.SavedLevelData, p_args.SavedLevelMeta, p_args.RemovedDuplicatesCount);
		}

		// try to load the saved level file and show a loading dialog or an error message
		private void OnLoad(object p_sender, LE_LoadEvent p_args)
		{
			// try load level
			LE_ExtensionInterface.Load.Delegate(this, (byte[][] p_levelData)=>
			{
				if (p_levelData != null && p_levelData.Length == 2 && p_levelData[0] != null && p_levelData[1] != null)
				{
					// load the level from file if the file exists
					StartCoroutine(LateLoad(p_args, p_levelData));
				}
				else
				{
					uMyGUI_PopupManager.Instance.HidePopup(uMyGUI_PopupManager.POPUP_LOADING);
					// show an error message if there is no saved level
					((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT)).SetText(
						"No saved level found!",
						"You first need to save a level!\nNo saved level was found!\nIn the webplayer the level must be in the clipboard!").ShowButton("ok");
				}
			}, false);
		}

		// load level delayed by a few frames so that the loading message is rendered first
		private IEnumerator LateLoad(LE_LoadEvent p_args, byte[][] p_data)
		{
			// wait a few frames until the loading diolog is rendered and start loading the level
			yield return new WaitForSeconds(0.3f);
			// close loading popup (the rest of the loading mechanics will happen in this frame)
			uMyGUI_PopupManager.Instance.HidePopup(uMyGUI_PopupManager.POPUP_LOADING);

			// while the level is stored in a different file on all platforms except the webplayer, the webplayer can load a level created with the 
			// full level editor example or the dungeon game example from the clipboard. The code below will identify such loading operations
			bool isLevelNotSupported = false;
#if UNITY_5_3_OR_NEWER
			if (SceneManager.GetActiveScene().name == "LE_ExampleEditorTerrainOnly")
#else
			if (Application.loadedLevelName == "LE_ExampleEditorTerrainOnly")
#endif
			{
				LE_TerrainTextureConfig confTerrain = FindObjectOfType<LE_ConfigTerrain>().TerrainTextureConfig;
				LE_SaveLoadDataPeek peekLevelData = LE_SaveLoad.PeekLevelDataFromByteArray(p_data[0], confTerrain.TERRAIN_TEXTURES, confTerrain.TERRAIN_TEXTURE_SIZES, confTerrain.TERRAIN_TEXTURE_OFFSETS);
				isLevelNotSupported =
					// the terrain only level has no create terrain UI, because it has a default terrain, therefore all saved levels must have a terrain
					peekLevelData.TerrainDataPreview == null || 
					// in the terrain only example the terrain must have the width and length of 500 to fit into the level
					peekLevelData.TerrainDataPreview.size.x != 500 || peekLevelData.TerrainDataPreview.size.z != 500 ||
					// additionally, crashes are possible if a 9 patch terrain is used with different heightmap resolutions (see 'TT_Terrain9Patch.CrashCheck')
				    peekLevelData.TerrainDataPreview.heightmapResolution != 257 ||
				    // besides, there is no object editing supported in the terrain only example
					peekLevelData.LevelObjectsCount > 0;
				Destroy(peekLevelData.TerrainDataPreview);
				if (isLevelNotSupported)
				{
					((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT)).SetText(
						"Loading Failed", "It seems that you have a level saved in your clipboard that comes from the full " +
						"level editor example or from the dungeon game example. However, this is a terrain only editor example that cannot load such levels...").ShowButton("ok");
				}
			}

			if (!isLevelNotSupported)
			{
				// load level
				p_args.LoadLevelDataFromBytesCallback(p_data[0]);
				p_args.LoadLevelMetaFromBytesCallback(p_data[1]);
				// wait until the old level is really destroyed
				yield return new WaitForEndOfFrame();
				// look at player
				GameObject player = GameObject.Find("Objects/PlayerStartPosition");
				if (player != null)
				{
					Camera.main.transform.LookAt(player.transform.position, Vector3.up);
				}
				// just loaded a level -> it has no changes
				m_lastSaveFrame = Time.frameCount+1; // plus one because LE_LevelEditorMain.LastChangeFrame is set to this frame
			}
		}

		// again delay level loading so that the loading message is rendered first
		private IEnumerator LatePlay(string p_levelName)
		{
			yield return new WaitForSeconds(0.3f);
#if UNITY_5_3_OR_NEWER
			SceneManager.LoadScene(p_levelName);
#else
			Application.LoadLevel(p_levelName);
#endif
		}

		// check if the level is playable, this is the case when the player start position is defined
		private bool IsLevelPlayable(out string o_errorMessage)
		{
			// check if player position is defined
			GameObject goPlayerStart = GameObject.Find("Objects/PlayerStartPosition");
			if (goPlayerStart == null)
			{
				o_errorMessage = "You must define the\n<b>player start position</b>\nfor this level!\n" +
					"1. Go to Objects->GameLogic in the right window.\n" +
					"2. Drag and drop the player capsule into the level.";
				return false;
			}
			o_errorMessage = "";
			return true;
		}
	}
}
