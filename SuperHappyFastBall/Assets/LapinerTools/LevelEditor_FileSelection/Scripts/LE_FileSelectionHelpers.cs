using UnityEngine;
using System.IO;
using System.Collections;
using LapinerTools.uMyGUI;
using LE_LevelEditor.Core;
using LE_LevelEditor.UI;
using LE_LevelEditor.Example;
using LevelFile = LE_LevelEditor.Extensions.LE_ExtensionInterface.FileSelectionExtensionLoader.LevelFile;

namespace LE_LevelEditor.Extensions
{
	public static class LE_FileSelectionHelpers
	{
		public const string POPUP_NAME = "fileselection";
		public const string POPUP_LOADING = "loading";
		public const string POPUP_TEXT = "text";

		public static void SelectLevel(MonoBehaviour p_worker, string p_popupTitle, string p_popupText, System.Action<int, LevelFile[]> p_onSelectedCallback)
		{
			LE_ExtensionInterface.FileSelectionExtensionLoader fileSelect = LE_ExtensionInterface.FileSelectionInstance;
			p_worker.StartCoroutine(fileSelect.LevelDB.GetLevelListAsync((LevelFile[] p_levelFiles)=>
				{
					if (uMyGUI_PopupManager.Instance != null)
					{
						// show file selection UI
						string[] levelNames = LevelFile.GetLevelNames(p_levelFiles);
						string[] levelIconPaths = LevelFile.GetLevelIconPaths(p_levelFiles);
						((LE_PopupFileSelection)uMyGUI_PopupManager.Instance.ShowPopup(LE_FileSelectionHelpers.POPUP_NAME))
							.SetFiles(levelNames, levelIconPaths, (int p_selectedIndex) => p_onSelectedCallback(p_selectedIndex, p_levelFiles), null, true)
							.SetText(p_popupTitle, p_popupText)
							.ShowButton("close");
					}
				}));
		}

		public static Texture2D DownscaleTexture(Texture2D p_tex, int p_maxHeightBeforeDownscale, int p_maxHeightAfterDownscale)
		{
			Texture2D downscaledIcon = p_tex;
			if (p_tex.height > p_maxHeightBeforeDownscale)
			{
				// generate a copy of the icon with mip maps
				Texture2D mipmapIcon = new Texture2D(p_tex.width, p_tex.height, p_tex.format, true, true);
				mipmapIcon.SetPixels(p_tex.GetPixels());
				mipmapIcon.Apply(true);
				int mipMapLevel = 1;
				while ((int)(p_tex.height / Mathf.Pow(2f, mipMapLevel)) > p_maxHeightAfterDownscale) { mipMapLevel++; }
				downscaledIcon = new Texture2D(
					(int)(p_tex.width / Mathf.Pow(2f, mipMapLevel)),
					(int)(p_tex.height / Mathf.Pow(2f, mipMapLevel)),
					mipmapIcon.format, false, true);
				downscaledIcon.SetPixels(mipmapIcon.GetPixels(mipMapLevel));
				downscaledIcon.Apply();
				Object.Destroy(mipmapIcon);
			}
			return downscaledIcon;
		}

#if (!UNITY_WEBPLAYER && !UNITY_WEBGL) || UNITY_EDITOR
		public static void SaveLevel(string p_levelDataFilePath, string p_levelIconFileName, byte[] p_levelData, byte[] p_levelMeta, int p_removedDuplicatesCount)
		{
			// info popup
			System.Action<string> infoPopup = (string p_infoText)=>
			{
				if (uMyGUI_PopupManager.Instance != null)
				{
					if (p_removedDuplicatesCount > 0)
					{
						p_infoText += "\n'" + p_removedDuplicatesCount + "' duplicate object(s) removed before saving\n(duplicate = same: object, position, rotation, scale).";
					}
					((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(POPUP_TEXT)).SetText("Level Saved", p_infoText).ShowButton("ok");	
				}
			};
			
			// save level icon as png
			LE_SaveLoad.LevelMetaData loadedMeta = LE_SaveLoad.LoadLevelMetaFromByteArray(p_levelMeta, true);
			if (loadedMeta.Icon != null)
			{
				Texture2D downScaledIcon = LE_FileSelectionHelpers.DownscaleTexture(loadedMeta.Icon, 255, 128);
				// save to file
				string path = Path.Combine(Application.persistentDataPath, p_levelIconFileName);
				// need to use own implementation of SaveToFile to keep backwards compatibility with MRLE v1.31 (had no UtilityPlatformIO.SaveToFile(..., bytes[]))
				LE_FileSelectionHelpers.SaveToFile(path, downScaledIcon.EncodeToPNG());
				Object.Destroy(loadedMeta.Icon);
				Object.Destroy(downScaledIcon);
			}
			
			// save level
			infoPopup(ExampleGame_LoadSave.SaveByFilePath(p_levelDataFilePath, p_levelData, p_levelMeta));
		}

		public static void SaveToFile(string p_filePath, byte[] p_fileContents)
		{
#if UNITY_METRO && !UNITY_EDITOR
			UnityEngine.Windows.File.WriteAllBytes(p_filePath, p_fileContents);
#else
			using (System.IO.FileStream stream = System.IO.File.Open(p_filePath, System.IO.FileMode.Create))
			{
				stream.Write(p_fileContents, 0, p_fileContents.Length);
			}
#endif
		}
#endif
	}
}