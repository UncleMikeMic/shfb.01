using UnityEngine;
using System.Collections;
using System.IO;
using LE_LevelEditor.UI;
using LE_LevelEditor.Events;
using LapinerTools.uMyGUI;

#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace LE_LevelEditor.Example
{
	public class ExampleGame_DungeonGame : MonoBehaviour
	{
		[SerializeField]
		private string LEVEL_FILE_NAME = "level_dungeon.txt";
		[SerializeField]
		private CharacterController PLAYER = null;
		
		private bool m_isInit = false;

		// open the documentation of the Multiplatform Runtime Level Editor (linked to a button in left menu)
		public void OnDocumentationBtn()
		{
			Application.OpenURL("http://www.freebord-game.com/index.php/multiplatform-runtime-level-editor/documentation");
		}

		// open the documentation of the Multiplatform Runtime Level Editor (linked to a button in left menu)
		public void OnFullLevelEditorExampleBtn()
		{
			uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_LOADING);
			StartCoroutine(LatePlay());
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

			if (PLAYER != null) { PLAYER.enabled = false; }
			// show a loading message
			uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_LOADING);
			// try load dungeon level from file
			if (ExampleGame_LoadSave.IsLevelFileFound(LEVEL_FILE_NAME))
			{
				// load level from file
				StartCoroutine(ExampleGame_LoadSave.LoadRoutineByFileName(LEVEL_FILE_NAME, (byte[][] p_levelData)=>
				{
					LE_LevelEditorMain.Instance.ExecuteWhenReady(()=>StartCoroutine(LateLoad(LE_LevelEditorMain.Instance.GetLoadEvent(), p_levelData)));
				}));
			}
			else
			{
				// load default dungeon level if no file is saved
				string[] dataAsStringArray = ExampleGame_DungeonGame_Level.LEVEL.Split('#');
				byte[][] loadedByteArrays = new byte[][]
				{
					System.Convert.FromBase64String(dataAsStringArray[0]),
					System.Convert.FromBase64String(dataAsStringArray[1])
				};
				LE_LevelEditorMain.Instance.ExecuteWhenReady(()=>StartCoroutine(LateLoad(LE_LevelEditorMain.Instance.GetLoadEvent(), loadedByteArrays)));
			}

			// set up the event handling (link the buttons in the editor to functions in this script)
			LE_EventInterface.OnSave += OnSave;
			LE_EventInterface.OnLoad += OnLoad;
		}

		// remove all references to this instance
		private void OnDestroy()
		{
			// always remove references to this script when it is destroyed otherwise memory leaks can be possible
			LE_EventInterface.OnSave -= OnSave;
			LE_EventInterface.OnLoad -= OnLoad;
			LE_EventInterface.UnregisterAll();
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

		private void LateUpdate()
		{
			if (!m_isInit)
			{
				// activate object mode
				LE_GUIInterface.Instance.OnEditModeBtn(1);
				m_isInit = true;
			}
		}

		// save the current level to a file and note the latest save action frame
		private void OnSave(object p_sender, LE_SaveEvent p_args)
		{
			Save(p_args);
		}
		
		// try to load the saved level file and show a loading dialog or an error message
		private void OnLoad (object p_sender, LE_LoadEvent p_args)
		{
			// show a loading message
			uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_LOADING);
			// try load level
			StartCoroutine(ExampleGame_LoadSave.LoadRoutineByFileName(LEVEL_FILE_NAME, (byte[][] p_levelData)=>
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
			}));
		}

		// again delay level loading so that the loading message is rendered first
		private IEnumerator LatePlay()
		{
			yield return new WaitForSeconds(0.3f);
#if UNITY_5_3_OR_NEWER
			SceneManager.LoadScene("LE_ExampleEditor");
#else
			Application.LoadLevel("LE_ExampleEditor");
#endif
		}

		// load level delayed by a few frames so that the loading message is rendered first
		private IEnumerator LateLoad(LE_LoadEvent p_args, byte[][] p_data)
		{
			// wait a few frames until the loading diolog is rendered and start loading the level
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			
			// close loading popup (the rest of the loading mechanics will happen in this frame)
			uMyGUI_PopupManager.Instance.HidePopup(uMyGUI_PopupManager.POPUP_LOADING);
			if (PLAYER != null) { PLAYER.enabled = true; }

			// safe load level
			p_args.LoadLevelDataFromBytesCallback(p_data[0]);
			p_args.LoadLevelMetaFromBytesCallback(p_data[1]);
		}

		// save the level and show the info dialog
		public void Save(LE_SaveEvent p_args)
		{
			// save to file
			string popupText = ExampleGame_LoadSave.SaveByFileName(LEVEL_FILE_NAME, p_args.SavedLevelData, p_args.SavedLevelMeta);
			if (p_args.RemovedDuplicatesCount > 0)
			{
				popupText += "\n'" + p_args.RemovedDuplicatesCount + "' duplicate object(s) removed before saving\n(duplicate = same: object, position, rotation, scale).";
			}
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT)).SetText("Level Saved", popupText).ShowButton("ok");
		}
	}
}
