/*
using System.IO;
using UnityEngine;

using LE_LevelEditor.Extensions;
using LE_LevelEditor.UI;
using LevelFile = LE_LevelEditor.Extensions.LE_ExtensionInterface.FileSelectionExtensionLoader.LevelFile;

using LapinerTools.Steam;
using LapinerTools.Steam.UI;
using LapinerTools.Steam.Data;

using LapinerTools.uMyGUI;

/// <summary>
/// Add this example script to your level editor scene.
/// It will change the level folder structure of the saved levels to match Steam Workshop requirements.
/// Press F1 while running the scene to upload a level.
/// </summary>
public class MRLE_SteamWorkshop_ESI_Example : MonoBehaviour
{
	private void Awake()
	{
		LE_LevelEditor.Extensions.LE_ExtensionInterface.FileSelectionInstance.LevelDB = new LE_LevelEditor.Extensions.LE_LevelDatabase_SteamWorkshop();
	}
	
	void Update()
	{
		if (Input.GetKeyUp(KeyCode.F1))
		{
			LE_FileSelectionHelpers.SelectLevel(this, "Upload Level", "Which level do you want to upload to Steam Workshop?", (int p_selectedLevelIndex, LevelFile[] p_levelFiles)=>
				{
					// get Steam item from folder
					string levelFolder = Path.GetDirectoryName(p_levelFiles[p_selectedLevelIndex].PathData);
					WorkshopItemUpdate itemUpdate = SteamWorkshopMain.Instance.GetItemUpdateFromFolder(levelFolder);
					if (itemUpdate == null) // if the level was already uploaded, then the upload popup will be prefilled with the data used for last upload 
					{
						// level was never uploaded => prefil the upload dialog
						itemUpdate = new WorkshopItemUpdate();
						itemUpdate.Name = p_levelFiles[p_selectedLevelIndex].Name;
						itemUpdate.IconPath = p_levelFiles[p_selectedLevelIndex].PathIcon;
						itemUpdate.ContentPath = levelFolder;
					}
					// show the Steam Workshop item upload popup
					((SteamWorkshopPopupUpload)uMyGUI_PopupManager.Instance.ShowPopup("steam_ugc_upload")).UploadUI.SetItemData(itemUpdate);
				});
		}
	}
}
*/
