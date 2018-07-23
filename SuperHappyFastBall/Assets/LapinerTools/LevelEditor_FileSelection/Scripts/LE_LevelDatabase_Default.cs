#if (!UNITY_WEBPLAYER && !UNITY_WEBGL) || UNITY_EDITOR
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using LapinerTools.uMyGUI;
using MyUtility;

using ILevelDatabase = LE_LevelEditor.Extensions.LE_ExtensionInterface.FileSelectionExtensionLoader.ILevelDatabase;
using LevelFile = LE_LevelEditor.Extensions.LE_ExtensionInterface.FileSelectionExtensionLoader.LevelFile;

namespace LE_LevelEditor.Extensions
{
	public class LE_LevelDatabase_Default : ILevelDatabase
	{
		private const string LEVEL_FILE_NAMES_PATH = "_savedLevelFileNames.txt";

		public IEnumerator GetLevelListAsync(System.Action<LevelFile[]> p_onLoaded)
		{
			// show loading popup
			if (uMyGUI_PopupManager.Instance != null)
			{
				uMyGUI_PopupManager.Instance.ShowPopup(LE_FileSelectionHelpers.POPUP_LOADING);
			}
			
			// load file with the level file names
			WWW www = new WWW(UtilityPlatformIO.FixFilePath(Path.Combine(Application.persistentDataPath, LEVEL_FILE_NAMES_PATH)));
			yield return www;
			string savedLevelFileNamesText;
			if (string.IsNullOrEmpty(www.error))
			{
				savedLevelFileNamesText = www.text;
			}
			else
			{
				savedLevelFileNamesText = "";
			}
			
			// hide loading popup
			if (uMyGUI_PopupManager.Instance != null)
			{
				uMyGUI_PopupManager.Instance.HidePopup(LE_FileSelectionHelpers.POPUP_LOADING);
			}
			
			// callback with the loaded level names
			string[] savedLevelFileNames = savedLevelFileNamesText.Split(new string[]{"\r\n", "\n"}, System.StringSplitOptions.RemoveEmptyEntries);
			if (p_onLoaded != null)
			{
				LevelFile[] levelFiles = new LevelFile[savedLevelFileNames.Length];
				for (int i = 0; i < levelFiles.Length; i++)
				{
					levelFiles[i] = new LevelFile()
					{
						Name = savedLevelFileNames[i],
						PathData = Path.Combine(Application.persistentDataPath, savedLevelFileNames[i] + ".txt"),
						PathIcon = Path.Combine(Application.persistentDataPath, savedLevelFileNames[i] + ".png")
					};
				}
				p_onLoaded(levelFiles);
			}
		}

		public void SaveFile(string p_levelName, byte[] p_levelData, byte[] p_levelMeta, int p_removedDuplicatesCount, LevelFile[] p_levelFiles, System.Action<string> p_onSuccess, System.Action p_onFail)
		{
			// check if this is a new level file name
			bool isExistingLevel = false;
			string[] levelNames = LevelFile.GetLevelNames(p_levelFiles);
			for (int i = 0; i < levelNames.Length; i++)
			{
				if (levelNames[i] == p_levelName)
				{
					isExistingLevel = true;
					break;
				}
			}

			string levelDataFilePath = Path.Combine(Application.persistentDataPath, p_levelName + ".txt");
			string levelIconFilePath = Path.Combine(Application.persistentDataPath, p_levelName + ".png");
			if (!isExistingLevel)
			{
				// add new level file name to the list
				string[] updatedLevelNames = new string[levelNames.Length+1];
				System.Array.Copy(levelNames, updatedLevelNames, levelNames.Length);
				updatedLevelNames[updatedLevelNames.Length-1] = p_levelName;
				System.Array.Sort(updatedLevelNames);
				string levelFileNamesText = "";
				for (int i = 0; i < updatedLevelNames.Length; i++)
				{
					levelFileNamesText += updatedLevelNames[i] + "\n";
				}
				string levelListFilePath = Path.Combine(Application.persistentDataPath, LEVEL_FILE_NAMES_PATH);
				UtilityPlatformIO.SaveToFile(levelListFilePath, levelFileNamesText);
				// save level
				LE_FileSelectionHelpers.SaveLevel(levelDataFilePath, levelIconFilePath, p_levelData, p_levelMeta, p_removedDuplicatesCount);
				if (p_onSuccess != null) { p_onSuccess(levelDataFilePath); }
			}
			else
			{
				// show confirm popup
				((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(LE_FileSelectionHelpers.POPUP_TEXT)).SetText("Save Level", "The level '"+p_levelName+"' already exists, do you want to overwrite it?")
					.ShowButton("no", ()=>
					{
						// go back
						if (p_onFail != null) { p_onFail(); }
					})
					.ShowButton("yes", ()=>
					{
						// save level
						LE_FileSelectionHelpers.SaveLevel(levelDataFilePath, levelIconFilePath, p_levelData, p_levelMeta, p_removedDuplicatesCount);
						if (p_onSuccess != null) { p_onSuccess(levelDataFilePath); }
					});
			}
		}

		public IEnumerator Delete(LevelFile p_levelFile, System.Action<bool> p_onResult)
		{
			return GetLevelListAsync((LevelFile[] p_levels)=>
			{
				// show confirm popup
				((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(LE_FileSelectionHelpers.POPUP_TEXT)).SetText("Delete Level", "Do you really want to delete '"+p_levelFile.Name+"'?")
					.ShowButton("no", ()=>
					{
						if (p_onResult != null) { p_onResult(false); }
					})
					.ShowButton("yes", ()=>
					{
						// remove level file name from the list
						List<string> updatedLevelNames = new List<string>();
						for (int i = 0; i < p_levels.Length; i++)
						{
							if (p_levels[i].Name != p_levelFile.Name)
							{
								updatedLevelNames.Add(p_levels[i].Name);
							}
						}
						string levelFileNamesText = "";
						for (int i = 0; i < updatedLevelNames.Count; i++)
						{
							levelFileNamesText += updatedLevelNames[i] + "\n";
						}
						string levelListFilePath = Path.Combine(Application.persistentDataPath, LEVEL_FILE_NAMES_PATH);
						UtilityPlatformIO.SaveToFile(levelListFilePath, levelFileNamesText);
						// delete level files
						if (!string.IsNullOrEmpty(p_levelFile.PathData)) { File.Delete(p_levelFile.PathData); }
						if (!string.IsNullOrEmpty(p_levelFile.PathIcon)) {File.Delete(p_levelFile.PathIcon); }
						if (p_onResult != null) { p_onResult(true); }
					});
			});
		}
	}
}
#endif
