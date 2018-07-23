using UnityEngine;
using System.Collections;
using System.IO;
using MyUtility;
using LapinerTools.uMyGUI;
using LE_LevelEditor.Extensions;

namespace LE_LevelEditor.Example
{
	/// <summary>
	/// This class contains the save/load example logic
	/// </summary>
	public static class ExampleGame_LoadSave
	{
		private const string LEVEL_FILE_NAME = "level.txt";

		// set load and save callbacks in the LE_ExtensionInterface (these callbacks can be overwritten by MRLE extensions bought in the Asset Store)
		public static void Init()
		{
			LE_ExtensionInterface.Load.SetDelegate(1, (object p_sender, System.Action<byte[][]> p_onLoadedCallback, bool p_isReload)=>
			{
				// show a loading message
				if (uMyGUI_PopupManager.Instance != null)
				{
					uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_LOADING);
				}
				// try load level
				((MonoBehaviour)p_sender).StartCoroutine(ExampleGame_LoadSave.LoadRoutineByFileName(LEVEL_FILE_NAME, p_onLoadedCallback));
			});
			LE_ExtensionInterface.Save.SetDelegate(1, (object p_sender, byte[] p_levelData, byte[] p_levelMeta, int p_removedDuplicatesCount)=>
			{
				// save to file
				string popupText = ExampleGame_LoadSave.SaveByFileName(LEVEL_FILE_NAME, p_levelData, p_levelMeta);
				if (uMyGUI_PopupManager.Instance != null)
				{
					if (p_removedDuplicatesCount > 0)
					{
						popupText += "\n'" + p_removedDuplicatesCount + "' duplicate object(s) removed before saving\n(duplicate = same: object, position, rotation, scale).";
					}
					((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT)).SetText("Level Saved", popupText).ShowButton("ok");	
				}
			});
		}

		/// <summary>
		/// Save data to a text file or to the clipboard in the WebPlayer.
		/// The level's p_relativeFileName is relative to Application.persistentDataPath.
		/// </summary>
		public static string SaveByFileName(string p_relativeFileName, byte[] p_data, byte[] p_meta)
		{
#if (UNITY_WEBPLAYER || UNITY_WEBGL) && !UNITY_EDITOR
			string dataAsString = System.Convert.ToBase64String(p_data) + "#" + System.Convert.ToBase64String(p_meta);
			string levelSizeMessage = "Level size: '" + ((float)(p_data.Length + p_meta.Length) / 1048576f).ToString("0.00") + "' MB\n";
			// save to clipboard
#if UNITY_5_2 || UNITY_5_3_OR_NEWER
			GUIUtility.systemCopyBuffer = dataAsString;
#else
			TextEditor te = new TextEditor();
			te.content = new GUIContent(dataAsString);
			te.SelectAll();
			te.Copy();
#endif
			return "Level copied to clipboard. " + levelSizeMessage +
				"The load button will retrieve it from clipboard.\n" +
				"To store the level locally paste it into a text file.";
#else
			return SaveByFilePath(Path.Combine(Application.persistentDataPath, p_relativeFileName), p_data, p_meta);
#endif
		}

#if (!UNITY_WEBPLAYER && !UNITY_WEBGL) || UNITY_EDITOR
		/// <summary>
		/// Save data to a text file or to the clipboard in the WebPlayer
		/// </summary>
		public static string SaveByFilePath(string p_filePath, byte[] p_data, byte[] p_meta)
		{
			// YOU SHOULD ZIP THE LEVEL DATA! YOU CAN SAVE UP TO 95% DATA VOLUME, AVARAGE SEEMS TO BE AROUND 75% REDUCTION
			string dataAsString = System.Convert.ToBase64String(p_data) + "#" + System.Convert.ToBase64String(p_meta);
			string levelSizeMessage = "Level size: '" + ((float)(p_data.Length + p_meta.Length) / 1048576f).ToString("0.00") + "' MB\n";

			UtilityPlatformIO.SaveToFile(p_filePath, dataAsString);
			return "Level saved to '"+p_filePath+"'.\n" +
				levelSizeMessage +
				"The load button will load from this file.";
		}
#endif

		/// <summary>
		/// Load data from a text file or from the clipboard in the WebPlayer.
		/// The level's p_relativeFileName is relative to Application.persistentDataPath.
		/// </summary>
		public static IEnumerator LoadRoutineByFileName(string p_relativeFileName, System.Action<byte[][]> p_onLoaded)
		{
#if (UNITY_WEBPLAYER || UNITY_WEBGL) && !UNITY_EDITOR
			// Disk IO is not available in Web Player
			// YOU SHOULD GET YOUR DATA FROM SOMEWHERE ELSE HERE (e.g. from server)
#if UNITY_5_2 || UNITY_5_3_OR_NEWER
			string savedLevel = GUIUtility.systemCopyBuffer;
#else
			TextEditor te = new TextEditor();
			te.content.text = "";
			te.Paste();
			string savedLevel = te.content.text;
#endif
			yield return new WaitForEndOfFrame(); // needed only to allow IEnumerator as return type
			LoadFromStr(savedLevel, p_onLoaded);
#else
			return LoadRoutineByFilePath(Path.Combine(Application.persistentDataPath, p_relativeFileName), p_onLoaded);
#endif
		}

#if (!UNITY_WEBPLAYER && !UNITY_WEBGL) || UNITY_EDITOR
		/// <summary>
		/// Load data from a text file
		/// </summary>
		public static IEnumerator LoadRoutineByFilePath(string p_filePath, System.Action<byte[][]> p_onLoaded)
		{
			// load level data with the WWW class (WWW is supported on all platforms for disk IO)
			WWW www = new WWW(UtilityPlatformIO.FixFilePath(p_filePath));
			yield return www;
			string savedLevel;
			if (string.IsNullOrEmpty(www.error))
			{
				savedLevel = www.text;
			}
			else
			{
				Debug.LogError("ExampleGame_LoadSave: LoadRoutine: could load file '" + www.url + "'! Error:\n" + www.error);
				savedLevel = null;
			}

			LoadFromStr(savedLevel, p_onLoaded);
		}
#endif

		/// <summary>
		/// Load data from a text file or from the clipboard in the WebPlayer.
		/// </summary>
		public static void LoadFromStr(string p_savedLevelStr, System.Action<byte[][]> p_onLoaded)
		{
			if (!string.IsNullOrEmpty(p_savedLevelStr))
			{
				string[] dataAsStringArray = p_savedLevelStr.Split('#');
				byte[][] loadedByteArrays;
				try
				{
					loadedByteArrays = new byte[][]
					{
						System.Convert.FromBase64String(dataAsStringArray[0]),
						System.Convert.FromBase64String(dataAsStringArray[1])
					};
				}
				catch (System.Exception p_ex)
				{
					Debug.LogError("ExampleGame_LoadSave: LoadRoutine: unknown format! Error: " + p_ex.Message);
					loadedByteArrays = null;
				}
				p_onLoaded(loadedByteArrays);
			}
			else
			{
				p_onLoaded(null);
			}
		}

		public static bool IsLevelFileFound(string p_fileName)
		{
#if (UNITY_WEBPLAYER || UNITY_WEBGL) && !UNITY_EDITOR
			return false;
#elif UNITY_WINRT
			return UnityEngine.Windows.File.Exists(Application.persistentDataPath + "/" + p_fileName);
#else
			return File.Exists(Application.persistentDataPath + "/" + p_fileName);
#endif
		}
	}
}
