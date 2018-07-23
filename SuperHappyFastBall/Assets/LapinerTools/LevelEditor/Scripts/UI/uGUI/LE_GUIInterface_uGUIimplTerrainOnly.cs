using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using LapinerTools.uMyGUI;

namespace LE_LevelEditor.UI
{
	public class LE_GUIInterface_uGUIimplTerrainOnly : LE_GUIInterface_uGUIimplBase
	{
		private const string POPUP_TEXT = "text";
		private const string POPUP_TEXTURE_PICKER = "texture_picker";

		[SerializeField]
		private float CAM_GIZMO_RIGHT_PIXEL_OFFSET = 265f;
		[SerializeField]
		private float CAM_OBLIQUE_RIGHT_PIXEL_OFFSET = 250f;
		[SerializeField]
		private uMyGUI_TexturePicker TERRAIN_BRUSH_PICKER = null;
		[SerializeField]
		private uMyGUI_TexturePicker TERRAIN_PAINT_TEXTURES_PICKER = null;
		[SerializeField]
		private Texture2D TERRAIN_ADD_PAINT_TEXTURE_ICON = null;
		[SerializeField]
		private Slider TERRAIN_EDIT_BRUSH_SIZE_SLIDER = null;
		[SerializeField]
		private Slider TERRAIN_EDIT_BRUSH_AMOUNT_SLIDER = null;
		[SerializeField]
		private Slider TERRAIN_EDIT_BRUSH_TARGET_VALUE_SLIDER_HEIGHT = null;
		[SerializeField]
		private Slider TERRAIN_EDIT_BRUSH_TARGET_VALUE_SLIDER_TEXTURE = null;
		[SerializeField]
		private Button TERRAIN_READ_PAINT_HEIGHT_BTN = null;
		[SerializeField]
		private RectTransform TERRAIN_MENU = null;
		[SerializeField]
		private string TERRAIN_MENU_FADE_IN_ANIM = "TabRotatedFadeIn";
		[SerializeField]
		private RectTransform TERRAIN_UI_MODE_EDIT = null;

		[SerializeField]
		private AspectRatioFitter LEVEL_ICON_BG = null;
		[SerializeField]
		private RawImage LEVEL_ICON = null;

		[SerializeField]
		private AudioSource BUTTON_CLICK_AUDIO_SOURCE = null;

		private float m_levelIconMissingTextureAspectRatio = 1f;
		private Texture m_levelIconMissingTexture = null;

		private Texture2D[] m_unusedPaintTextures =  new Texture2D[0];

		protected override void Start()
		{
			base.Start();
			
			if (LE_GUIInterface.Instance != null)
			{
				// register delegates
				LE_GUIInterface.Instance.delegates.GetCameraPerspectiveGizmoRightPixelOffset += GetCameraPerspectiveGizmoRightPixelOffset;
				LE_GUIInterface.Instance.delegates.GetObliqueCameraPerspectiveRightPixelOffset += GetObliqueCameraPerspectiveRightPixelOffset;

				// we do not need the width, length, height UI, but we need to inform the level editor that we know it (or we will get warnings)
				LE_GUIInterface.Instance.delegates.SetTerrainWidth += (int p_dummy)=>{};
				LE_GUIInterface.Instance.delegates.SetTerrainLength += (int p_dummy)=>{};
				LE_GUIInterface.Instance.delegates.SetTerrainHeight += (int p_dummy)=>{};

				LE_GUIInterface.Instance.delegates.SetTerrainUIMode += SetTerrainUIMode;
				LE_GUIInterface.Instance.delegates.SetTerrainEditBrushSize += SetTerrainEditBrushSize;
				LE_GUIInterface.Instance.delegates.SetTerrainEditBrushAmount += SetTerrainEditBrushAmount;
				LE_GUIInterface.Instance.delegates.SetTerrainEditBrushTargetValue += SetTerrainEditBrushTargetValue;
				LE_GUIInterface.Instance.delegates.SetTerrainIsReadingPaintHeight += SetTerrainIsReadingPaintHeight;
				SetupTerrainPaintTexturePickerEvents();
				SetupTexturePickerEvents("TERRAIN_BRUSH_PICKER", TERRAIN_BRUSH_PICKER,
					ref LE_GUIInterface.Instance.delegates.SetTerrainBrushes,
					LE_GUIInterface.Instance.OnTerrainBrushChanged,
					ref LE_GUIInterface.Instance.events.OnTerrainBrushChanged);

				LE_GUIInterface.Instance.delegates.SetLevelIcon += SetLevelIcon;
			}
			else
			{
				Debug.LogError("LE_GUIInterface_uGUIimpl: Start: could not find LE_GUIInterface!");
			}

			// save variables
			if (LEVEL_ICON != null) { m_levelIconMissingTexture = LEVEL_ICON.texture; }
			if (LEVEL_ICON_BG != null) { m_levelIconMissingTextureAspectRatio = LEVEL_ICON_BG.aspectRatio; }

			if (BUTTON_CLICK_AUDIO_SOURCE == null)
			{
				Debug.LogWarning("LE_GUIInterface_uGUIimpl: BUTTON_CLICK_AUDIO_SOURCE was not set in the inspector!");
			}
		}

		private void SetupTerrainPaintTexturePickerEvents()
		{
			if (TERRAIN_PAINT_TEXTURES_PICKER != null)
			{
				// update selection if it is changed (e.g. by clicking on a texture)
				LE_GUIInterface.Instance.events.OnTerrainPaintTextureChanged += (object p_obj, LE_GUIInterface.EventHandlers.IntEventArgs p_args)=>
				{
					if (TERRAIN_PAINT_TEXTURES_PICKER != null) { TERRAIN_PAINT_TEXTURES_PICKER.SetSelection(p_args.Value); }
				};
				
				// handle the clicked event coming back from UI
				TERRAIN_PAINT_TEXTURES_PICKER.ButtonCallback = (int p_selectedIndex)=>
				{
					if (TERRAIN_PAINT_TEXTURES_PICKER != null)
					{
						if (TERRAIN_PAINT_TEXTURES_PICKER.Textures[p_selectedIndex] == TERRAIN_ADD_PAINT_TEXTURE_ICON)
						{
							// user wants to add one more texture to the paint selection
							uMyGUI_Popup popup = ((uMyGUI_PopupTexturePicker)uMyGUI_PopupManager.Instance.ShowPopup(POPUP_TEXTURE_PICKER))
								.SetPicker(m_unusedPaintTextures, -1, (int p_clickedIndex)=>
								           {
									LE_GUIInterface.Instance.OnTerrainPaintTextureAdded(m_unusedPaintTextures[p_clickedIndex]);
								})
									.SetText("Select Texture", "Click on the texture which you want to add to the terrain.")
									.ShowButton("back");
							AddButtonClickSoundsToGeneratedUI(popup.transform as RectTransform);
						}
						else
						{
							// user has selected a paint texture
							LE_GUIInterface.Instance.OnTerrainPaintTextureChanged(p_selectedIndex);
						}
					}
					else
					{
						Debug.LogError("LE_GUIInterface_uGUIimpl: TERRAIN_PAINT_TEXTURES_PICKER is not set in inspector!");
					}
				};
			}
			else
			{
				Debug.LogError("LE_GUIInterface_uGUIimpl: TERRAIN_PAINT_TEXTURES_PICKER is not set in inspector!");
			}
			
			// handle the initialization/update event coming from the level editor
			LE_GUIInterface.Instance.delegates.SetTerrainPaintTextures += (Texture2D[] p_textures, Texture2D[] p_unusedTextures, int p_selectedIndex, bool p_isAddTextureBtn)=>
			{
				if (TERRAIN_PAINT_TEXTURES_PICKER != null)
				{
					m_unusedPaintTextures = p_unusedTextures;
					if (p_isAddTextureBtn && TERRAIN_ADD_PAINT_TEXTURE_ICON != null)
					{
						if (p_textures.Length == 0)
						{
							p_selectedIndex = -1; // make sure that the add new icon is never selected
						}
						// add additional texture to use it as add new texture button
						Texture2D[] texturesWithAddIcon = new Texture2D[p_textures.Length+1];
						System.Array.Copy(p_textures, texturesWithAddIcon, p_textures.Length);
						texturesWithAddIcon[texturesWithAddIcon.Length-1] = TERRAIN_ADD_PAINT_TEXTURE_ICON;
						TERRAIN_PAINT_TEXTURES_PICKER.SetTextures(texturesWithAddIcon, p_selectedIndex);
						// make the add new texture button be rendered with alpha
						RawImage addBtnImg = TERRAIN_PAINT_TEXTURES_PICKER.Instances[texturesWithAddIcon.Length-1].GetComponent<RawImage>();
						if (addBtnImg != null) { addBtnImg.material = null; }
					}
					else
					{
						if (p_isAddTextureBtn)
						{
							Debug.LogError("LE_GUIInterface_uGUIimpl: TERRAIN_ADD_PAINT_TEXTURE_ICON is not set in inspector!");
						}
						// textures cannot be modified any more -> simply pass through the array
						TERRAIN_PAINT_TEXTURES_PICKER.SetTextures(p_textures, p_selectedIndex);
					}
					AddButtonClickSoundsToGeneratedUI(TERRAIN_PAINT_TEXTURES_PICKER.transform as RectTransform);
				}
				else
				{
					Debug.LogError("LE_GUIInterface_uGUIimpl: TERRAIN_PAINT_TEXTURES_PICKER is not set in inspector!");
				}
			};
		}

		private void SetupTexturePickerEvents(string p_errorPickerName, uMyGUI_TexturePicker p_texPicker, ref System.Action<Texture2D[], int> r_setTexturesDelegate, System.Action<int> p_buttonCallback, ref System.EventHandler<LE_GUIInterface.EventHandlers.IntEventArgs> r_onChangedEvent)
		{
			if (p_texPicker != null)
			{
				r_setTexturesDelegate += (Texture2D[] p_textures, int p_selectedIndex)=>
				{
					p_texPicker.SetTextures(p_textures, p_selectedIndex);
					AddButtonClickSoundsToGeneratedUI(p_texPicker.transform as RectTransform);
				};
				p_texPicker.ButtonCallback = p_buttonCallback;
				r_onChangedEvent += (object p_obj, LE_GUIInterface.EventHandlers.IntEventArgs p_args)=>
				{
					if (p_texPicker != null) { p_texPicker.SetSelection(p_args.Value); } // update selection if it is changed (e.g. by clicking on a texture)
				};
			}
			else
			{
				Debug.LogError("LE_GUIInterface_uGUIimpl: " + p_errorPickerName + " is not set in inspector!");
			}
		}

		private float GetCameraPerspectiveGizmoRightPixelOffset()
		{
			return CAM_GIZMO_RIGHT_PIXEL_OFFSET*(Screen.height/m_transform.rect.height);
		}

		private float GetObliqueCameraPerspectiveRightPixelOffset()
		{
			return CAM_OBLIQUE_RIGHT_PIXEL_OFFSET*(Screen.height/m_transform.rect.height);
		}

		private void SetTerrainUIMode(LE_GUIInterface.Delegates.ETerrainUIMode p_mode)
		{
			if (TERRAIN_UI_MODE_EDIT != null)
			{
				bool isShowEditUI = p_mode == LE_GUIInterface.Delegates.ETerrainUIMode.EDIT;
				if (isShowEditUI)
				{
					TERRAIN_UI_MODE_EDIT.gameObject.SetActive(true); // activate before message
					TERRAIN_UI_MODE_EDIT.gameObject.SendMessage("uMyGUI_OnActivateTab",SendMessageOptions.DontRequireReceiver);
				}
				else
				{
					TERRAIN_UI_MODE_EDIT.gameObject.SendMessage("uMyGUI_OnDeactivateTab",SendMessageOptions.DontRequireReceiver);
					TERRAIN_UI_MODE_EDIT.gameObject.SetActive(false); // disable after message (NOT BEFORE!)
				}
			}
			else
			{
				Debug.LogError("LE_GUIInterface_uGUIimpl: SetTerrainUIMode: TERRAIN_UI_MODE_EDIT is not set in inspector!");
			}
			if (TERRAIN_MENU != null && !string.IsNullOrEmpty(TERRAIN_MENU_FADE_IN_ANIM) && TERRAIN_MENU.gameObject.activeInHierarchy)
			{
				Animation anim = TERRAIN_MENU.GetComponent<Animation>();
				if (anim != null) { anim.Play(TERRAIN_MENU_FADE_IN_ANIM); }
			}
			else if (TERRAIN_MENU == null)
			{
				Debug.LogError("LE_GUIInterface_uGUIimpl: SetTerrainUIMode: TERRAIN_MENU is not set in inspector!");
			}
		}

		private void SetTerrainEditBrushSize(float p_brushSize)
		{
			if (TERRAIN_EDIT_BRUSH_SIZE_SLIDER != null)
			{
				TERRAIN_EDIT_BRUSH_SIZE_SLIDER.value = p_brushSize;
			}
			else
			{
				Debug.LogError("LE_GUIInterface_uGUIimpl: SetTerrainEditBrushSize: TERRAIN_EDIT_BRUSH_SIZE_SLIDER is not set in inspector!");
			}
		}

		private void SetTerrainEditBrushAmount(float p_brushAmount)
		{
			if (TERRAIN_EDIT_BRUSH_AMOUNT_SLIDER != null)
			{
				TERRAIN_EDIT_BRUSH_AMOUNT_SLIDER.value = p_brushAmount;
			}
			else
			{
				Debug.LogError("LE_GUIInterface_uGUIimpl: SetTerrainEditBrushAmount: TERRAIN_EDIT_BRUSH_AMOUNT_SLIDER is not set in inspector!");
			}
		}

		private void SetTerrainEditBrushTargetValue(float p_brushTargetValue)
		{
			if (TERRAIN_EDIT_BRUSH_TARGET_VALUE_SLIDER_HEIGHT != null)
			{
				TERRAIN_EDIT_BRUSH_TARGET_VALUE_SLIDER_HEIGHT.value = p_brushTargetValue;
			}
			else
			{
				Debug.LogError("LE_GUIInterface_uGUIimpl: SetTerrainEditBrushTargetValue: TERRAIN_EDIT_BRUSH_TARGET_VALUE_SLIDER_HEIGHT is not set in inspector!");
			}

			if (TERRAIN_EDIT_BRUSH_TARGET_VALUE_SLIDER_TEXTURE != null)
			{
				TERRAIN_EDIT_BRUSH_TARGET_VALUE_SLIDER_TEXTURE.value = p_brushTargetValue;
			}
			else
			{
				Debug.LogError("LE_GUIInterface_uGUIimpl: SetTerrainEditBrushTargetValue: TERRAIN_EDIT_BRUSH_TARGET_VALUE_SLIDER_TEXTURE is not set in inspector!");
			}
		}

		private void SetTerrainIsReadingPaintHeight(bool p_isReadingPaintHeight)
		{
			if (TERRAIN_READ_PAINT_HEIGHT_BTN != null)
			{
				TERRAIN_READ_PAINT_HEIGHT_BTN.interactable = !p_isReadingPaintHeight;
				Text text = TERRAIN_READ_PAINT_HEIGHT_BTN.transform.GetChild(0).GetComponent<Text>();
				text.text = p_isReadingPaintHeight ? "Click On Terrain To Read Height" : "Read Terrain Paint Height";
				text.color = p_isReadingPaintHeight ? Color.red : (Color.white*0.196f + Color.black);
			}
			else
			{
				Debug.LogError("LE_GUIInterface_uGUIimpl: SetTerrainIsReadingPaintHeight: TERRAIN_READ_PAINT_HEIGHT_BTN is not set in inspector!");
			}
		}

		private void SetLevelIcon (Texture2D p_levelIcon)
		{
			if (LEVEL_ICON_BG != null)
			{
				if (p_levelIcon != null)
				{
					LEVEL_ICON_BG.aspectRatio = (float)p_levelIcon.width / (float)p_levelIcon.height;
				}
				else
				{
					LEVEL_ICON_BG.aspectRatio = m_levelIconMissingTextureAspectRatio;
				}
			}
			else
			{
				Debug.LogError("LE_GUIInterface_uGUIimpl: SetLevelIcon: LEVEL_ICON_BG is not set in inspector!");
			}

			if (LEVEL_ICON != null)
			{
				if (p_levelIcon != null)
				{
					LEVEL_ICON.texture = p_levelIcon;
				}
				else
				{
					LEVEL_ICON.texture = m_levelIconMissingTexture;
				}
			}
			else
			{
				Debug.LogError("LE_GUIInterface_uGUIimpl: SetLevelIcon: LEVEL_ICON is not set in inspector!");
			}
		}

		private void AddButtonClickSoundsToGeneratedUI(RectTransform p_root)
		{
			if (p_root != null && BUTTON_CLICK_AUDIO_SOURCE != null)
			{
				// find all button elements and add the needed event script
				Button[] buttons = p_root.GetComponentsInChildren<Button>(true);
				for (int i = 0; i < buttons.Length; i++)
				{
					// make sure that the sound will not be played double
					buttons[i].onClick.RemoveListener(OnButtonClickSound);
					// add listener (again if it was added already)
					buttons[i].onClick.AddListener(OnButtonClickSound);
				}
				// find all toggle elements and add the needed event script
				Toggle[] toggles = p_root.GetComponentsInChildren<Toggle>(true);
				for (int i = 0; i < toggles.Length; i++)
				{
					// make sure that the sound will not be played double
					toggles[i].onValueChanged.RemoveListener(OnToggleClickSound);
					// add listener (again if it was added already)
					toggles[i].onValueChanged.AddListener(OnToggleClickSound);
				}
			}
		}

		private void OnButtonClickSound()
		{
			if (BUTTON_CLICK_AUDIO_SOURCE != null) { BUTTON_CLICK_AUDIO_SOURCE.Play(); }
		}

		private void OnToggleClickSound(bool p_isOn)
		{
			if (BUTTON_CLICK_AUDIO_SOURCE != null) { BUTTON_CLICK_AUDIO_SOURCE.Play(); }
		}
	}
}
