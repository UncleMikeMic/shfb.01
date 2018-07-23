using UnityEngine;
using System.Collections;
using LE_LevelEditor.UI;
using LE_LevelEditor.Events;
using LE_LevelEditor.Core;
using LS_LevelStreaming;

// warning generation for Windows
[System.Obsolete("If you run on Windows (Phone) you should think about Unity bugs. If you have changed the resolution or you are using a Unity version below 4.6.1 you might run into problems. Please carefully read the instructions of the function that throws this warning!")]
public class Win_Hacks: System.Attribute{}

namespace LE_LevelEditor.Logic
{
	public class LE_LogicLevel : LE_LogicBase
	{
		private LE_ConfigLevel m_confL;
		private LE_GUI3dTerrain m_GUI3dTerrain;
		private LE_GUI3dObject m_GUI3dObject;
		private bool m_isObliqueFocusCentering;
		private Texture2D[] m_configTextures;
		private Vector2[] m_configTextureSizes;
		private Vector2[] m_configTextureOffsets;

		private Texture2D m_levelIcon = null;

		public LE_LogicLevel(
			LE_ConfigLevel p_confL, LE_GUI3dTerrain p_GUI3dTerrain, LE_GUI3dObject p_GUI3dObject, bool p_isObliqueFocusCentering,
			Texture2D[] p_configTextures, Vector2[] p_configTextureSizes, Vector2[] p_configTextureOffsets)
		{
			m_confL = p_confL;
			m_GUI3dTerrain = p_GUI3dTerrain;
			m_GUI3dObject = p_GUI3dObject;
			m_isObliqueFocusCentering = p_isObliqueFocusCentering;
			m_configTextures = p_configTextures;
			m_configTextureSizes = p_configTextureSizes;
			m_configTextureOffsets = p_configTextureOffsets;
			// register to events
			LE_GUIInterface.Instance.events.OnLevelSaveBtn += OnLevelSaveBtn;
			LE_GUIInterface.Instance.events.OnLevelLoadBtn += OnLevelLoadBtn;
			LE_GUIInterface.Instance.events.OnLevelRenderIconBtn += OnLevelRenderIconBtn;
		}

		public override void Destroy ()
		{
			// unregister from events
			if (LE_GUIInterface.Instance != null)
			{
				LE_GUIInterface.Instance.events.OnLevelSaveBtn -= OnLevelSaveBtn;
				LE_GUIInterface.Instance.events.OnLevelLoadBtn -= OnLevelLoadBtn;
				LE_GUIInterface.Instance.events.OnLevelRenderIconBtn -= OnLevelRenderIconBtn;
			}
			// free memory of the level icon
			if (m_levelIcon != null)
			{
				Object.Destroy(m_levelIcon);
			}
		}

		public override void Update()
		{
		}

// EVENT HANDLERS -----------------------------------------------------------------------------------------------------------------

		private void OnLevelSaveBtn(object p_obj, System.EventArgs p_args)
		{
			SaveLevel();
		}

		private void OnLevelLoadBtn(object p_obj, System.EventArgs p_args)
		{
			if (LE_EventInterface.OnLoad != null)
			{
				LE_EventInterface.OnLoad(this, GetLoadEvent());
			}
			else
			{
				Debug.LogError("LE_LogicLevel: OnLevelLoadBtn: you have to provide an event handler for 'LE_EventInterface.OnLoad' to load a level!");
			}
		}

		private void OnLevelRenderIconBtn(object p_obj, System.EventArgs p_args)
		{
			m_confL.StartCoroutine(DrawLevelIcon());
		}

// LOGIC --------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Call to load a level into the level editor. Use the callbacks in the returned event args to start loading from byte arrays.
		/// Use if loading is needed without using the load button. Learn more:
		/// http://www.freebord-game.com/index.php/multiplatform-runtime-level-editor/documentation/load
		/// </summary>
		public LE_LoadEvent GetLoadEvent()
		{
			LE_LoadEvent loadEventArgs = new LE_LoadEvent((byte[] p_savedLevelData)=>
			// LoadLevelDataFromBytes callback
			{
				Vector3 removedOffset = new Vector3(99999f, -99999f, 99999f);
				// clean up level (remove all LE_Objects)
				LE_Object[] objs = Object.FindObjectsOfType<LE_Object>();
				for (int i = 0; i < objs.Length; i++)
				{
					objs[i].transform.position += removedOffset;
					Object.Destroy(objs[i].gameObject);
				}
				// load level data
				LE_SaveLoadData level = LE_SaveLoad.LoadLevelDataFromByteArray(p_savedLevelData, m_GUI3dTerrain!=null?m_GUI3dTerrain.TERRAIN_LAYER:0, m_configTextures, m_configTextureSizes, m_configTextureOffsets);
				// process all level objects as if they were new
				for (int i = 0; i < level.LevelObjects.Length; i++)
				{
					LE_SaveLoadData.ObjectData obj = level.LevelObjects[i];
					if (obj.Result == LE_SaveLoadData.ObjectData.EResult.INSTANCE)
					{
						// add snapping if needed
						if (m_GUI3dObject != null)
						{
							LE_LogicObjects.AddSnappingScripts(m_GUI3dObject, obj.Instance);
						}
					}
					else if (obj.Result == LE_SaveLoadData.ObjectData.EResult.STREAMED)
					{
						// add snapping if needed once spawned
						LS_ManagedObjectBase managedObj = LS_LevelStreamingSceneManager.Instance.GetManagedObject(obj.StreamedLevelObjectID);
						if (managedObj != null)
						{
							// add snapping if needed
							if (m_GUI3dObject != null)
							{
								managedObj.m_onShow += (object p_object, System.EventArgs p_args)=>
								{
									if (m_GUI3dObject != null && p_object is LS_ManagedObjectInstantiateDestroy)
									{
										LE_LogicObjects.AddSnappingScripts(m_GUI3dObject, ((LS_ManagedObjectInstantiateDestroy)p_object).Instance.GetComponent<LE_Object>());
									}
								};
							}
						}
					}
				}
				// inform listeners that the level is now fully loaded
				if (LE_EventInterface.OnLoadedLevelInEditor != null)
				{
					LE_EventInterface.OnLoadedLevelInEditor(this, System.EventArgs.Empty);
				}
			}, (byte[] p_savedLevelMeta)=>
			// LoadLevelMetaFromBytes callback
			{
				// load level meta
				LE_SaveLoad.LevelMetaData meta = LE_SaveLoad.LoadLevelMetaFromByteArray(p_savedLevelMeta, true);
				m_levelIcon = meta.Icon;
				if (LE_GUIInterface.Instance.delegates.SetLevelIcon != null)
				{
					LE_GUIInterface.Instance.delegates.SetLevelIcon(meta.Icon);
				}
				else if (meta.Icon != null)
				{
					Debug.LogError("LE_LogicLevel: GetLoadEvent: LE_LoadEvent: LoadLevelMetaFromBytes: you level meta seems to contain an icon, but you have not provided the LE_GUIInterface.delegates.SetLevelIcon delegate. Level icon will not be shown!");
				}
			});
			return loadEventArgs;
		}

		private void SaveLevel()
		{
			if (LE_EventInterface.OnSave != null)
			{
				// collect level meta data, which depends on the game's implementation
				LE_CollectMetaDataEvent collectMetaData = new LE_CollectMetaDataEvent();
				if (LE_EventInterface.OnCollectMetaDataBeforeSave != null)
				{
					LE_EventInterface.OnCollectMetaDataBeforeSave(this, collectMetaData);
				}
				// save
				int removedDuplicatesCount = 0;
				if (m_confL.IsRemoveDuplicatesOnSave)
				{
					removedDuplicatesCount = LE_SaveLoad.RemoveDuplicatesInCurrentLevel();
				}
				LE_SaveEvent saveEventArgs = new LE_SaveEvent(
					LE_SaveLoad.SaveCurrentLevelDataToByteArray(m_configTextures),
					LE_SaveLoad.SaveCurrentLevelMetaToByteArray(m_levelIcon, collectMetaData.GetCollectedMetaData()),
					removedDuplicatesCount);
				LE_EventInterface.OnSave(this, saveEventArgs);
			}
			else
			{
				Debug.LogError("LE_LogicLevel: OnLevelLoadBtn: you have to provide an event handler for 'LE_EventInterface.OnSave' to save a level!");
			}
		}

#if UNITY_METRO || UNITY_METRO_8_1 || UNITY_WP_8_1 || UNITY_WP8
		[Win_Hacks]
#endif
		private IEnumerator DrawLevelIcon()
		{
			yield return new WaitForEndOfFrame();
#if UNITY_METRO_8_1 || UNITY_WP_8_1
	#if IS_MY_MOBILE
			// please ignore this, or delete it if you have a IS_MY_MOBILE directive in your built
			// A MESSAGE FOR MYSELF:
			... copy paste too much huh?
	#endif
			// Level Icon Preview Bug
			// On Windows 8.1 and Windows Phone 8.1 the resolution must be set to the default/native resolution
			// before the level preview icon can be rendered. The commented code below is taken from my game Mad Snowboarding.
			// Comment in the code below and replace MyConst.Resolution.DEF_X with the width of the resolution, which was set
			// before you have changed the resolution. Do the same for MyConst.Resolution.DEF_Y with height.
			// >>>> ALSO COMMENT IN THE CODE BELOW! Search for "Level Icon Preview Bug" again!!! <<<<
			// bool isResolutionChanged = false;
			// Resolution backupResolution = Screen.currentResolution;
			// if (MyConst.Resolution.DEF_X != Screen.width || MyConst.Resolution.DEF_Y != Screen.height)
			// {
			// 	isResolutionChanged = true;
			// 	Screen.SetResolution(MyConst.Resolution.DEF_X, MyConst.Resolution.DEF_Y, Screen.fullScreen);
			// }
			//yield return new WaitForEndOfFrame();
#endif
			if (m_levelIcon == null || m_levelIcon.width != m_confL.LevelIconWidth || m_levelIcon.height != m_confL.LevelIconHeight)
			{
				Object.Destroy(m_levelIcon);
				m_levelIcon = new Texture2D(m_confL.LevelIconWidth, m_confL.LevelIconHeight, TextureFormat.RGB24, false, true);
			}
// This is another bug with Unity for Windows Phone 8. It must have been fixed in Unity 4.6.1. Anyway you should make a test
// with the Unity version that you use. If this bug occurs, then the level icon can be rendered only in ScreenOrientation.LandscapeLeft.
// What happens if you render the level icon in another orientation is half random, but with the code below the icon is rendered right on
// most devices. Make a test of your build, if you see randomly rotated parts of the screen instead of the rendered level icon in some
// orientations try to comment in this code.
//#if UNITY_WP8
//			if (Screen.orientation != ScreenOrientation.LandscapeLeft && LE_GUIInterface.Instance.delegates.ShowWP8RenderLevelIconBugDialog != null)
//			{
//				LE_GUIInterface.Instance.delegates.ShowWP8RenderLevelIconBugDialog();
//			}
//			else
//			{
//				Debug.LogError("LE_LogicLevel: DrawLevelIcon: you have commented in the WP8 bug code, but have not provided the LE_GUIInterface.delegates.ShowWP8RenderLevelIconBugDialog delegate. Set it to show an error message to your players. An example text can be found in the LE_GUIInterface.cs file in the comments of ShowWP8RenderLevelIconBugDialog.");
//			}
//
//			Rect originalPixelRect = Camera.main.pixelRect;
//			Rect pixelRect;
//			if (Screen.orientation == ScreenOrientation.LandscapeLeft)
//			{
//				pixelRect = new Rect(Screen.height-m_confL.LevelIconWidth, 0, m_confL.LevelIconWidth, m_confL.LevelIconHeight);
//			}
//			else
//			{
//				float xFix = 0.5f*(float)m_confL.LevelIconWidth*(Camera.main.pixelWidth/(float)Screen.width);
//				pixelRect = new Rect(Screen.height-m_confL.LevelIconWidth+xFix, Screen.height-m_confL.LevelIconHeight, m_confL.LevelIconWidth, m_confL.LevelIconHeight);
//			}
//			Camera.main.pixelRect = pixelRect;
//			if (!Camera.main.orthographic && m_isObliqueFocusCentering) { LE_LevelEditorMain.SetObliqueFocusProjectionMatrix(false); }
//			Camera.main.Render();
//			Rect readPixels = new Rect(0, 0, pixelRect.height, pixelRect.width);
//			Texture2D wpHackTex = new Texture2D(m_confL.LevelIconHeight, m_confL.LevelIconWidth, TextureFormat.RGB24, false, true);
//			wpHackTex.ReadPixels(readPixels, 0, 0);
//			wpHackTex.Apply(false);
//			Color[] hackPixels = wpHackTex.GetPixels();
//			Color[] rotatedPixels = new Color[hackPixels.Length];
//			for (int i = 0; i < m_confL.LevelIconWidth; i++)
//			{
//				for (int j = 0; j < m_confL.LevelIconHeight; j++)
//				{
//					int rotIndex;
//					if (Screen.orientation == ScreenOrientation.LandscapeLeft)
//					{
//						rotIndex = j*m_confL.LevelIconWidth+(m_confL.LevelIconWidth-1)-i;
//					}
//					else
//					{
//						rotIndex = (m_confL.LevelIconHeight-1)*m_confL.LevelIconWidth-j*m_confL.LevelIconWidth+i;
//					}
//					int hackIndex = i*m_confL.LevelIconHeight+j;
//					if (rotIndex < 0 || rotIndex >= hackPixels.Length || hackIndex >= hackPixels.Length)
//					{
//						Debug.LogError("i " + i + " j " + j + " len " + hackPixels.Length + " rot " + rotIndex + " hack " + hackIndex);
//					}
//					rotatedPixels[rotIndex] = hackPixels[hackIndex];
//				}
//			}
//			Object.Destroy(wpHackTex);
//			m_levelIcon.SetPixels(rotatedPixels);
//#else
			Rect originalPixelRect = Camera.main.pixelRect;
			Rect pixelRect = new Rect(0, 0, m_confL.LevelIconWidth, m_confL.LevelIconHeight);
			Camera.main.pixelRect = pixelRect;
			if (!Camera.main.orthographic && m_isObliqueFocusCentering) { LE_LevelEditorMain.SetObliqueFocusProjectionMatrix(false); }
			Camera.main.Render();
			m_levelIcon.ReadPixels(pixelRect, 0, 0, false);
//#endif
			m_levelIcon.Apply(false);
			Camera.main.pixelRect = originalPixelRect;
			if (!Camera.main.orthographic && m_isObliqueFocusCentering) { LE_LevelEditorMain.SetObliqueFocusProjectionMatrix(true); }
			// notify listeners that the level icon was changed
			if (LE_GUIInterface.Instance.delegates.SetLevelIcon != null)
			{
				LE_GUIInterface.Instance.delegates.SetLevelIcon(m_levelIcon);
			}
			else
			{
				Debug.LogError("LE_LogicLevel: DrawLevelIcon: you seem to have a 'Render Level Icon' button, but you have not provided the LE_GUIInterface.delegates.SetLevelIcon delegate. Level icon will not be shown!");
			}
			// notify listeners that the level data was changed
			if (LE_EventInterface.OnChangeLevelData != null)
			{
				LE_EventInterface.OnChangeLevelData(m_levelIcon, new LE_LevelDataChangedEvent(LE_ELevelDataChangeType.LEVEL_ICON));
			}
			
//#if UNITY_METRO_8_1 || UNITY_WP_8_1
			// Level Icon Preview Bug
			// simply comment this in if you need it
			// the code below will set the resolution back to the value you have used
			// if (isResolutionChanged)
			// {
			//	Screen.SetResolution(backupResolution.width, backupResolution.height, Screen.fullScreen);
			// }
			// yield return new WaitForEndOfFrame();
//#endif
		}
	}
}