#if (!UNITY_WEBPLAYER && !UNITY_WEBGL) || UNITY_EDITOR
using UnityEngine;
using System.IO;
using System.Collections;
using LapinerTools.uMyGUI;
using MyUtility;

using LE_LevelEditor.UI;
using LE_LevelEditor.Core;
using LE_LevelEditor.Example;

namespace LE_LevelEditor.Extensions
{
	/// <summary>
	/// This class will inject delegate definitions for Load and Save
	/// </summary>
	public static partial class LE_ExtensionInterface
	{
		private static FileSelectionExtensionLoader s_fileSelectionExtensionLoader = new FileSelectionExtensionLoader();
		public static FileSelectionExtensionLoader FileSelectionInstance { get{ return s_fileSelectionExtensionLoader; } }

		public class FileSelectionExtensionLoader
		{
			public interface ILevelDatabase
			{
				IEnumerator GetLevelListAsync(System.Action<LevelFile[]> p_onLoaded);
				void SaveFile(string p_levelName, byte[] p_levelData, byte[] p_levelMeta, int p_removedDuplicatesCount, LevelFile[] p_levelFiles, System.Action<string> p_onSuccess, System.Action p_onFail);
				IEnumerator Delete(LevelFile p_levelFile, System.Action<bool> p_onResult);
			}
			public struct LevelFile
			{
				public string Name { get; set; }
				public string PathData { get; set; }
				public string PathIcon { get; set; }

				public static string[] GetLevelNames(LevelFile[] p_levelFiles)
				{
					string[] levelNames = new string[p_levelFiles.Length];
					for (int i = 0; i < p_levelFiles.Length; i++)
					{
						levelNames[i] = p_levelFiles[i].Name;
					}
					return levelNames;
				}

				public static string[] GetLevelIconPaths(LevelFile[] p_levelFiles)
				{
					string[] iconPaths = new string[p_levelFiles.Length];
					for (int i = 0; i < p_levelFiles.Length; i++)
					{
						iconPaths[i] = p_levelFiles[i].PathIcon;
					}
					return iconPaths;
				}
			}

			private string m_reloadLevelName = null;
			public string ReloadLevelName
			{
				get{ return m_reloadLevelName; }
				set{ m_reloadLevelName = value; }
			}

			public ILevelDatabase LevelDB { get; set; }

			public FileSelectionExtensionLoader()
			{
				// set default database (will be used by this extension to load and save files)
				LevelDB = new LE_LevelDatabase_Default();

				// set delegate callbacks (will be used by the MRLE and examples)
				Load.SetDelegate(10, SelectAndLoadFile);
				Save.SetDelegate(10, SelectAndSaveFile);
			}

			private void SelectAndLoadFile(object p_sender, System.Action<byte[][]> p_onLoadedCallback, bool p_isReload)
			{
				if (p_isReload)
				{
					if (!string.IsNullOrEmpty(m_reloadLevelName))
					{
						LoadFile(p_sender, m_reloadLevelName, p_onLoadedCallback);
					}
					else
					{
						Debug.LogError("FileSelectionExtensionLoader: SelectAndLoadFile: no level was loaded yet, but 'p_isReload' was 'true'!");
					}
					return;
				}

				// load file with the level file names
				((MonoBehaviour)p_sender).StartCoroutine(LevelDB.GetLevelListAsync((LevelFile[] p_levelFiles)=>
				{
					if (uMyGUI_PopupManager.Instance != null)
					{
						// show file selection UI
						string[] levelNames = LevelFile.GetLevelNames(p_levelFiles);
						string[] levelIconPaths = LevelFile.GetLevelIconPaths(p_levelFiles);
						((LE_PopupFileSelection)uMyGUI_PopupManager.Instance.ShowPopup(LE_FileSelectionHelpers.POPUP_NAME))
							.SetFiles(levelNames, levelIconPaths, (int p_selectedLevelIndex)=>
							{
								LoadFile(p_sender, p_levelFiles[p_selectedLevelIndex].PathData, p_onLoadedCallback);
							}, null)
							.SetText("Load Level", "Which level do you want to load?")
							.ShowButton("close");
					}
				}));
			}

			private void SelectAndSaveFile(object p_sender, byte[] p_levelData, byte[] p_levelMeta, int p_removedDuplicatesCount)
			{
				// load file with the level file names
				((MonoBehaviour)p_sender).StartCoroutine(LevelDB.GetLevelListAsync((LevelFile[] p_levelFiles)=>
				{
					if (uMyGUI_PopupManager.Instance != null)
					{
						// show file selection UI with an enter file name field
						string[] levelNames = LevelFile.GetLevelNames(p_levelFiles);
						string[] levelIconPaths = LevelFile.GetLevelIconPaths(p_levelFiles);
						LE_PopupFileSelection filePopup = ((LE_PopupFileSelection)uMyGUI_PopupManager.Instance.ShowPopup(LE_FileSelectionHelpers.POPUP_NAME));
						filePopup.SetFiles(levelNames, levelIconPaths,
							// on level selected -> enter level name in the input field
							(int p_selectedLevelIndex)=>
							{
								filePopup.SaveInput.text = levelNames[p_selectedLevelIndex];
							},
							// on delete button -> delete file, then refresh level list
							(int p_deletedLevelIndex)=>
							{
								filePopup.Hide();
								((MonoBehaviour)p_sender).StartCoroutine(LevelDB.Delete(p_levelFiles[p_deletedLevelIndex], (bool p_isDeleted) => SelectAndSaveFile(p_sender, p_levelData, p_levelMeta, p_removedDuplicatesCount)));
							}, false)
							.SetText("Save Level", "Enter level name or select an existing level.")
							.ShowButton("close")
							.ShowButton("save", ()=>
							{
								if (!string.IsNullOrEmpty(filePopup.SaveInput.text))
								{
									LevelDB.SaveFile(filePopup.SaveInput.text, p_levelData, p_levelMeta, p_removedDuplicatesCount, p_levelFiles,
					                	// update reload level name on success
										(string p_savedFilePath) => m_reloadLevelName = p_savedFilePath,
										// show file selection popup on failure
					                	() => SelectAndSaveFile(p_sender, p_levelData, p_levelMeta, p_removedDuplicatesCount));
								}
								else
								{
									((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(LE_FileSelectionHelpers.POPUP_TEXT)).SetText("Save Level", "The level name cannot be empty!").ShowButton("ok", ()=>
									{
										SelectAndSaveFile(p_sender, p_levelData, p_levelMeta, p_removedDuplicatesCount);
									});
								}
							});
					}
				}));
			}

			private void LoadFile(object p_sender, string p_filePath, System.Action<byte[][]> p_onLoadedCallback)
			{
				m_reloadLevelName = p_filePath;
				// show a loading message
				if (uMyGUI_PopupManager.Instance != null)
				{
					uMyGUI_PopupManager.Instance.ShowPopup(LE_FileSelectionHelpers.POPUP_LOADING);
				}
				// try load level
				((MonoBehaviour)p_sender).StartCoroutine(ExampleGame_LoadSave.LoadRoutineByFilePath(p_filePath, p_onLoadedCallback));
			}
		}
	}
}
#endif
