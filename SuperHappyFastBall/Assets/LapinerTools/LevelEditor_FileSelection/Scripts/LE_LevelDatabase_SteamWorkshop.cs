#if (!UNITY_WEBPLAYER && !UNITY_WEBGL && !UNITY_METRO) || UNITY_EDITOR
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using LapinerTools.uMyGUI;

using ILevelDatabase = LE_LevelEditor.Extensions.LE_ExtensionInterface.FileSelectionExtensionLoader.ILevelDatabase;
using LevelFile = LE_LevelEditor.Extensions.LE_ExtensionInterface.FileSelectionExtensionLoader.LevelFile;

namespace LE_LevelEditor.Extensions
{
	public class LE_LevelDatabase_SteamWorkshop : ILevelDatabase
	{
		private string[] m_levelDirs;

		public LE_LevelDatabase_SteamWorkshop()
		{
			m_levelDirs = new string[]{ Application.persistentDataPath };
		}
		public LE_LevelDatabase_SteamWorkshop(string[] p_searchLevelInDirectories)
		{
			m_levelDirs = p_searchLevelInDirectories;
		}


		public IEnumerator GetLevelListAsync(System.Action<LevelFile[]> p_onLoaded)
		{
			// scan folders for levels
			List<LevelFile> foundLevels = new List<LevelFile>();

			// search levels in the given folders
			foreach (string levelDir in m_levelDirs)
			{
				string[] subDirectories;
				try
				{
					subDirectories = Directory.GetDirectories(levelDir);
				}
				catch (DirectoryNotFoundException)
				{
					Debug.LogWarning("LE_LevelDatabase_SteamWorkshop: level directory '"+levelDir+"' not found!");
					subDirectories = new string[0];
				}
				foreach (string dir in subDirectories)
				{
					string[] files = Directory.GetFiles(dir);
					// this is a level if it contains a .txt file with the same name as the directory itself
					foreach (string levelFile in files)
					{
						if ((dir + ".txt").EndsWith(Path.GetFileName(levelFile)))
						{
							string levelName = Path.GetFileNameWithoutExtension(levelFile);
							string levelPath = Path.Combine(dir, levelName);
							foundLevels.Add(new LevelFile()
							{
								Name = levelName,
								PathData = levelPath + ".txt",
								PathIcon = levelPath + ".png"
							});
							break;
						}
					}
				}
			}
			if (p_onLoaded != null) { p_onLoaded(foundLevels.ToArray()); }

			yield return null;
		}
		
		public void SaveFile(string p_levelName, byte[] p_levelData, byte[] p_levelMeta, int p_removedDuplicatesCount, LevelFile[] p_levelFiles, System.Action<string> p_onSuccess, System.Action p_onFail)
		{
			// check if this is a new level file name
			bool isExistingLevel = false;
			string[] levelNames = System.Array.ConvertAll(p_levelFiles, levelFile => levelFile.Name);
			for (int i = 0; i < levelNames.Length; i++)
			{
				if (levelNames[i] == p_levelName)
				{
					isExistingLevel = true;
					break;
				}
			}

			string levelDataFilePath = Path.Combine(Application.persistentDataPath, Path.Combine(p_levelName, p_levelName + ".txt"));
			string levelIconFilePath = Path.Combine(Application.persistentDataPath, Path.Combine(p_levelName, p_levelName + ".png"));
			if (!isExistingLevel)
			{
				// create directory
				Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, p_levelName));
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
			// show confirm popup
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(LE_FileSelectionHelpers.POPUP_TEXT)).SetText("Delete Level", "Do you really want to delete '"+p_levelFile.Name+"'?")
				.ShowButton("no", ()=>
				{
					if (p_onResult != null) { p_onResult(false); }
				})
				.ShowButton("yes", ()=>
				{
#if !UNITY_WEBPLAYER && !UNITY_WEBGL
					Directory.Delete(Path.GetDirectoryName(p_levelFile.PathData), true);
					if (p_onResult != null) { p_onResult(true); }
#endif
				});
			yield return null;
		}
	}
}
#endif
