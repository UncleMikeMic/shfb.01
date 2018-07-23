using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using LapinerTools.uMyGUI;
using LE_LevelEditor.Core;

namespace LE_LevelEditor.UI
{
	public class LE_GUIInterface_uGUIimpl : LE_GUIInterface_uGUIimplBase
	{
		private class LeafNodeInstance
		{
			public readonly uMyGUI_Draggable Draggable;
			public readonly RawImage RawImage;
			public LeafNodeInstance(uMyGUI_Draggable p_draggable, RawImage p_rawImage)
			{
				Draggable = p_draggable;
				RawImage = p_rawImage;
			}

			public void SetState(LE_GUIInterface.Delegates.EDraggedObjectState p_state)
			{
				if (RawImage != null)
				{
					switch (p_state)
					{
						case LE_GUIInterface.Delegates.EDraggedObjectState.NOT_PLACEABLE: RawImage.color = Color.red; break;
						case LE_GUIInterface.Delegates.EDraggedObjectState.IN_3D_PREVIEW: RawImage.color = Color.clear; break;
						case LE_GUIInterface.Delegates.EDraggedObjectState.NONE: default: RawImage.color = Color.white; break;
					}
				}
			}
		}

		private const string POPUP_TEXT = "text";
		private const string POPUP_TEXTURE_PICKER = "texture_picker";

		[SerializeField, Tooltip("Enable only if you know what you do. Disable to see what you need to assign.")]
		private bool SUPPRESS_UNASSIGNED_WARNINGS = false;

		[SerializeField]
		private float CAM_GIZMO_RIGHT_PIXEL_OFFSET = 265f;
		[SerializeField]
		private float CAM_OBLIQUE_RIGHT_PIXEL_OFFSET = 250f;

		[SerializeField]
		private InputField TERRAIN_WIDTH_TEXT = null;
		[SerializeField]
		private InputField TERRAIN_LENGTH_TEXT = null;
		[SerializeField]
		private InputField TERRAIN_HEIGHT_TEXT = null;
		[SerializeField]
		private RectTransform TERRAIN_BASE_TEXTURE_PICKER_BG = null;
		[SerializeField]
		private uMyGUI_TexturePicker TERRAIN_BASE_TEXTURE_PICKER = null;
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
		private RectTransform TERRAIN_UI_MODE_CREATE = null;
		[SerializeField]
		private RectTransform TERRAIN_UI_MODE_EDIT = null;

		[SerializeField]
		private uMyGUI_TreeBrowser OBJECT_BROWSER = null;
		[SerializeField]
		private uMyGUI_Draggable OBJECT_PREVIEW_DRAGGABLE = null;
		[SerializeField]
		private RawImage OBJECT_PREVIEW_IMAGE = null;
		[SerializeField]
		private Text OBJECT_PREVIEW_MESSAGE = null;
		[SerializeField]
		private Button OBJECT_FOCUS_SELECTED_BTN = null;
		[SerializeField]
		private Button OBJECT_DUPLICATE_SELECTED_BTN = null;
		[SerializeField]
		private Button OBJECT_FIND_PREFAB_BTN = null;
		[SerializeField]
		private Button OBJECT_DELETE_SELECTED_BTN = null;
		[SerializeField]
		private RectTransform OBJECT_IS_SLEEP_MENU = null;
		[SerializeField]
		private Toggle OBJECT_IS_SLEEP_TOGGLE = null;
		[SerializeField]
		private RectTransform OBJECT_COLOR_MENU = null;
		[SerializeField]
		private uMyGUI_ColorPicker OBJECT_COLOR_PICKER = null;
		[SerializeField]
		private RectTransform OBJECT_VARIATION_MENU = null;
		[SerializeField]
		private uMyGUI_TreeBrowser OBJECT_VARIATION_BROWSER = null;

		[SerializeField]
		private AspectRatioFitter LEVEL_ICON_BG = null;
		[SerializeField]
		private RawImage LEVEL_ICON = null;

		[SerializeField]
		private AudioSource BUTTON_CLICK_AUDIO_SOURCE = null;

		private float m_levelIconMissingTextureAspectRatio = 1f;
		private Texture m_levelIconMissingTexture = null;

		private Dictionary<uMyGUI_TreeBrowser.Node, LE_Object> m_nodeToObject = new Dictionary<uMyGUI_TreeBrowser.Node, LE_Object>();
		private Dictionary<uMyGUI_TreeBrowser.Node, string> m_nodeToResourcePath = new Dictionary<uMyGUI_TreeBrowser.Node, string>();

		private Texture2D[] m_unusedPaintTextures =  new Texture2D[0];

		private List<LeafNodeInstance> m_treeBrowserLeafNodes = new List<LeafNodeInstance>(); // leaf node draggables
		private LeafNodeInstance m_draggedTreeBrowserLeafNode = null;

		private int m_usedVariationIndex = -10;
		private LE_ObjectVariationBase[] m_variations = null;

		protected override void Start()
		{
			base.Start();

			if (LE_GUIInterface.Instance != null)
			{
				// register delegates
				LE_GUIInterface.Instance.delegates.GetCameraPerspectiveGizmoRightPixelOffset += GetCameraPerspectiveGizmoRightPixelOffset;
				LE_GUIInterface.Instance.delegates.GetObliqueCameraPerspectiveRightPixelOffset += GetObliqueCameraPerspectiveRightPixelOffset;

				LE_GUIInterface.Instance.delegates.SetTerrainWidth += SetTerrainWidth;
				LE_GUIInterface.Instance.delegates.SetTerrainLength += SetTerrainLength;
				LE_GUIInterface.Instance.delegates.SetTerrainHeight += SetTerrainHeight;
				LE_GUIInterface.Instance.delegates.SetTerrainUIMode += SetTerrainUIMode;
				LE_GUIInterface.Instance.delegates.SetTerrainEditBrushSize += SetTerrainEditBrushSize;
				LE_GUIInterface.Instance.delegates.SetTerrainEditBrushAmount += SetTerrainEditBrushAmount;
				LE_GUIInterface.Instance.delegates.SetTerrainEditBrushTargetValue += SetTerrainEditBrushTargetValue;
				LE_GUIInterface.Instance.delegates.SetTerrainIsReadingPaintHeight += SetTerrainIsReadingPaintHeight;
				SetupTerrainPaintTexturePickerEvents();
				LE_GUIInterface.Instance.delegates.SetTerrainBaseTextures += (Texture2D[] p_textures, int p_selectedIndex)=>
				{
					// enable base texture selection UI if terrain base texture selection is enabled
					TERRAIN_BASE_TEXTURE_PICKER_BG.gameObject.SetActive(true);
				};
				SetupTexturePickerEvents("TERRAIN_TEXTURE_PICKER", TERRAIN_BASE_TEXTURE_PICKER,
					ref LE_GUIInterface.Instance.delegates.SetTerrainBaseTextures,
					LE_GUIInterface.Instance.OnTerrainBaseTextureChanged,
					ref LE_GUIInterface.Instance.events.OnTerrainBaseTextureChanged);
				SetupTexturePickerEvents("TERRAIN_BRUSH_PICKER", TERRAIN_BRUSH_PICKER,
					ref LE_GUIInterface.Instance.delegates.SetTerrainBrushes,
					LE_GUIInterface.Instance.OnTerrainBrushChanged,
					ref LE_GUIInterface.Instance.events.OnTerrainBrushChanged);

				LE_GUIInterface.Instance.delegates.SetObjects += SetObjects;
				LE_GUIInterface.Instance.delegates.IsObjectDragged += IsObjectDragged;
				LE_GUIInterface.Instance.delegates.SetDraggableObjectState += SetDraggableObjectState;
				LE_GUIInterface.Instance.delegates.SetDraggableObjectMessage += SetDraggableObjectMessage;
				LE_GUIInterface.Instance.delegates.SetIsSelectedObjectFocusBtnInteractable += SetIsSelectedObjectFocusBtnInteractable;
				LE_GUIInterface.Instance.delegates.SetIsSelectedObjectDuplicateBtnInteractable += SetIsSelectedObjectDuplicateBtnInteractable;
				LE_GUIInterface.Instance.delegates.SetIsSelectedPrefabFindBtnInteractable += SetIsSelectedPrefabFindBtnInteractable;
				LE_GUIInterface.Instance.delegates.SetIsSelectedObjectDeleteBtnInteractable += SetIsSelectedObjectDeleteBtnInteractable;
				LE_GUIInterface.Instance.delegates.SetIsSelectedObjectSleepPropertyInteractable += SetIsSelectedObjectSleepPropertyInteractable;
				LE_GUIInterface.Instance.delegates.SetSelectedObjectIsSleepOnStartPropertyValue += SetSelectedObjectIsSleepOnStartPropertyValue;
				LE_GUIInterface.Instance.delegates.SetIsSelectedObjectColorPropertyInteractable += SetIsSelectedObjectColorPropertyInteractable;
				LE_GUIInterface.Instance.delegates.SetSelectedObjectColorPropertyValue += SetSelectedObjectColorPropertyValue;
				LE_GUIInterface.Instance.delegates.SetSelectedObjectVariationPropertyValue += SetSelectedObjectVariationPropertyValue;

				LE_GUIInterface.Instance.delegates.SetLevelIcon += SetLevelIcon;

				LE_GUIInterface.Instance.delegates.ShowPopupConfirmDeleteObject += ShowPopupConfirmDeleteObject;

				// register callbacks
				if (OBJECT_COLOR_PICKER != null)
				{
					OBJECT_COLOR_PICKER.m_onChanged += (object p_obj, uMyGUI_ColorPicker.ColorEventArgs p_args)=>{ LE_GUIInterface.Instance.OnSelectedObjectColorChanged(p_args.Value); };
				}
			}

			// save variables
			if (LEVEL_ICON != null) { m_levelIconMissingTexture = LEVEL_ICON.texture; }
			if (LEVEL_ICON_BG != null) { m_levelIconMissingTextureAspectRatio = LEVEL_ICON_BG.aspectRatio; }

			// generate some warnings
			if (!SUPPRESS_UNASSIGNED_WARNINGS && OBJECT_PREVIEW_DRAGGABLE == null)
			{
				Debug.LogError("LE_GUIInterface_uGUIimpl: OBJECT_PREVIEW_DRAGGABLE was not set in the inspector -> drag&drop will not work!");
			}
			if (!SUPPRESS_UNASSIGNED_WARNINGS && OBJECT_PREVIEW_IMAGE == null)
			{
				Debug.LogWarning("LE_GUIInterface_uGUIimpl: OBJECT_PREVIEW_IMAGE was not set in the inspector!");
			}
			if (!SUPPRESS_UNASSIGNED_WARNINGS && OBJECT_PREVIEW_MESSAGE == null)
			{
				Debug.LogWarning("LE_GUIInterface_uGUIimpl: OBJECT_PREVIEW_MESSAGE was not set in the inspector!");
			}
			if (!SUPPRESS_UNASSIGNED_WARNINGS && OBJECT_FOCUS_SELECTED_BTN == null)
			{
				Debug.LogWarning("LE_GUIInterface_uGUIimpl: OBJECT_FOCUS_SELECTED_BTN was not set in the inspector!");
			}
			if (!SUPPRESS_UNASSIGNED_WARNINGS && OBJECT_DUPLICATE_SELECTED_BTN == null)
			{
				Debug.LogWarning("LE_GUIInterface_uGUIimpl: OBJECT_DUPLICATE_SELECTED_BTN was not set in the inspector!");
			}
			if (!SUPPRESS_UNASSIGNED_WARNINGS && OBJECT_FIND_PREFAB_BTN == null)
			{
				Debug.LogWarning("LE_GUIInterface_uGUIimpl: OBJECT_FIND_PREFAB_BTN was not set in the inspector!");
			}
			if (!SUPPRESS_UNASSIGNED_WARNINGS && OBJECT_DELETE_SELECTED_BTN == null)
			{
				Debug.LogWarning("LE_GUIInterface_uGUIimpl: OBJECT_DELETE_SELECTED_BTN was not set in the inspector!");
			}
			if (!SUPPRESS_UNASSIGNED_WARNINGS && OBJECT_IS_SLEEP_MENU == null)
			{
				Debug.LogWarning("LE_GUIInterface_uGUIimpl: OBJECT_IS_SLEEP_MENU was not set in the inspector!");
			}
			if (!SUPPRESS_UNASSIGNED_WARNINGS && OBJECT_IS_SLEEP_TOGGLE == null)
			{
				Debug.LogWarning("LE_GUIInterface_uGUIimpl: OBJECT_IS_SLEEP_TOGGLE was not set in the inspector!");
			}
			if (!SUPPRESS_UNASSIGNED_WARNINGS && OBJECT_COLOR_MENU == null)
			{
				Debug.LogWarning("LE_GUIInterface_uGUIimpl: OBJECT_COLOR_MENU was not set in the inspector!");
			}
			if (!SUPPRESS_UNASSIGNED_WARNINGS && OBJECT_COLOR_PICKER == null)
			{
				Debug.LogWarning("LE_GUIInterface_uGUIimpl: OBJECT_COLOR_PICKER was not set in the inspector!");
			}
			if (!SUPPRESS_UNASSIGNED_WARNINGS && BUTTON_CLICK_AUDIO_SOURCE == null)
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

		private void SetTerrainWidth(int p_width)
		{
			if (TERRAIN_WIDTH_TEXT != null)
			{
				TERRAIN_WIDTH_TEXT.text = p_width.ToString();
			}
			else
			{
				Debug.LogError("LE_GUIInterface_uGUIimpl: SetTerrainWidth: TERRAIN_WIDTH_TEXT is not set in inspector!");
			}
		}

		private void SetTerrainLength(int p_length)
		{
			if (TERRAIN_LENGTH_TEXT != null)
			{
				TERRAIN_LENGTH_TEXT.text = p_length.ToString();
			}
			else
			{
				Debug.LogError("LE_GUIInterface_uGUIimpl: SetTerrainWidth: TERRAIN_LENGTH_TEXT is not set in inspector!");
			}	
		}

		private void SetTerrainHeight(int p_height)
		{
			if (TERRAIN_HEIGHT_TEXT != null)
			{
				TERRAIN_HEIGHT_TEXT.text = p_height.ToString();
			}
			else
			{
				Debug.LogError("LE_GUIInterface_uGUIimpl: SetTerrainWidth: TERRAIN_HEIGHT_TEXT is not set in inspector!");
			}
		}

		private void SetTerrainUIMode(LE_GUIInterface.Delegates.ETerrainUIMode p_mode)
		{
			if (TERRAIN_UI_MODE_CREATE != null)
			{
				bool isShowCreateUI = p_mode == LE_GUIInterface.Delegates.ETerrainUIMode.CREATE;
				TERRAIN_UI_MODE_CREATE.gameObject.SetActive(isShowCreateUI);
			}
			else
			{
				Debug.LogError("LE_GUIInterface_uGUIimpl: SetTerrainUIMode: TERRAIN_UI_MODE_CREATE is not set in inspector!");
			}

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

		private void SetObjects(LE_ObjectMap p_objectMap)
		{
			if (OBJECT_BROWSER != null)
			{
				OBJECT_BROWSER.OnInnerNodeClick += (object p_object, uMyGUI_TreeBrowser.NodeClickEventArgs p_args)=>
				{
					AddButtonClickSoundsToGeneratedUI(OBJECT_BROWSER.transform as RectTransform);
				};
				OBJECT_BROWSER.OnLeafNodePointerDown += (object p_object, uMyGUI_TreeBrowser.NodeClickEventArgs p_args)=>
				{
					LE_Object selectedObj;
					string selectedResourcePath;
					if (!m_nodeToObject.TryGetValue(p_args.ClickedNode, out selectedObj))
					{
						Debug.LogError("LE_GUIInterface_uGUIimpl: could not find selected node's object!");
					}
					if (!m_nodeToResourcePath.TryGetValue(p_args.ClickedNode, out selectedResourcePath))
					{
						Debug.LogError("LE_GUIInterface_uGUIimpl: could not find selected node's path!");
					}
					if (selectedObj != null && selectedResourcePath != null)
					{
						LE_GUIInterface.Instance.OnObjectSelectDraggable(selectedObj, selectedResourcePath);
					}
				};
				OBJECT_BROWSER.OnNodeInstantiate += (object p_object, uMyGUI_TreeBrowser.NodeInstantiateEventArgs p_args)=>
				{
					// drag for leaf items
					if (p_args.Node.Children == null || p_args.Node.Children.Length == 0)
					{
						// use GetComponentsInChildren instead of GetComponentInChildren as this is the only way to get inactive components in all Unity versions
						uMyGUI_Draggable[] draggable = p_args.Instance.GetComponentsInChildren<uMyGUI_Draggable>(true);
						if (draggable.Length > 0 && draggable[0] != null)
						{
							RawImage[] image = draggable[0].GetComponentsInChildren<RawImage>(true);
							m_treeBrowserLeafNodes.Add(new LeafNodeInstance(draggable[0], image.Length > 0 ? image[0] : null));
						}
					}
				};
				OBJECT_BROWSER.BuildTree(SetObjectsRecursive(p_objectMap, 0));
				AddButtonClickSoundsToGeneratedUI(OBJECT_BROWSER.transform as RectTransform);
			}
			else
			{
				Debug.LogError("LE_GUIInterface_uGUIimpl: SetObjects: OBJECT_BROWSER is not set in inspector!");
			}
		}

		private uMyGUI_TreeBrowser.Node[] SetObjectsRecursive(LE_ObjectMap p_objectMap, int p_indentLevel)
		{
			List<uMyGUI_TreeBrowser.Node> nodes = new List<uMyGUI_TreeBrowser.Node>();
			// inner nodes
			for (int i = 0; i < p_objectMap.SubObjectMaps.Length; i++)
			{
				if (p_objectMap.SubObjectMaps[i] != null)
				{
					LE_ObjectCategoryNode.SendMessageInitData data = new LE_ObjectCategoryNode.SendMessageInitData(p_objectMap.SubObjectMaps[i].name, p_indentLevel);
					nodes.Add(new uMyGUI_TreeBrowser.Node(data, SetObjectsRecursive(p_objectMap.SubObjectMaps[i], p_indentLevel+1)));
				}
				else
				{
					Debug.LogError("LE_GUIInterface_uGUIimpl: SetObjectsRecursive: sub object map from map '" + p_objectMap.name + "'" +
					               " at index '" + i + "' is null!");
				}
			}
			// leafs
			for (int i = 0; i < p_objectMap.ObjectPrefabs.Length; i++)
			{
				if (p_objectMap.ObjectPrefabs[i] != null)
				{
					LE_ObjectPrefabNode.SendMessageInitData data = new LE_ObjectPrefabNode.SendMessageInitData(p_objectMap.ObjectPrefabs[i], p_indentLevel);
					uMyGUI_TreeBrowser.Node node = new uMyGUI_TreeBrowser.Node(data, null);
					nodes.Add(node);
					m_nodeToObject.Add(node, p_objectMap.ObjectPrefabs[i]);
					m_nodeToResourcePath.Add(node, p_objectMap.ObjectPrefabResourcePaths[i]);
				}
				else
				{
					Debug.LogError("LE_GUIInterface_uGUIimpl: SetObjectsRecursive: object from map '" + p_objectMap.name + "'" +
					               " at index '" + i + "' is null!");
				}
			}
			return nodes.ToArray();
		}

		private bool IsObjectDragged()
		{
			// check if the big item preview image is dragged
			if (OBJECT_PREVIEW_DRAGGABLE != null && OBJECT_PREVIEW_DRAGGABLE.IsDragged) { return true; }

			// check if a leaf node of the tree browser is dragged
			LeafNodeInstance oldInstance = m_draggedTreeBrowserLeafNode;
			m_draggedTreeBrowserLeafNode = null;
			for (int i = m_treeBrowserLeafNodes.Count-1; i>=0; i--)
			{
				uMyGUI_Draggable draggable = m_treeBrowserLeafNodes[i].Draggable;
				if (draggable == null)
				{
					m_treeBrowserLeafNodes.RemoveAt(i); // this tree browser leaf instance does not exist any more (category was closed)
				}
				else if (draggable.IsDragged)
				{
					m_draggedTreeBrowserLeafNode = m_treeBrowserLeafNodes[i];
				}
			}
			if (oldInstance != null && oldInstance != m_draggedTreeBrowserLeafNode)
			{
				// remove effects from old dragged instance
				oldInstance.SetState(LE_GUIInterface.Delegates.EDraggedObjectState.NONE);
			}
			if (m_draggedTreeBrowserLeafNode != null)
			{
				return true; // leaf is dragged
			}

			return false;
		}

		private void SetDraggableObjectState(LE_GUIInterface.Delegates.EDraggedObjectState p_state)
		{
			if (m_draggedTreeBrowserLeafNode != null)
			{
				m_draggedTreeBrowserLeafNode.SetState(p_state);
				p_state = LE_GUIInterface.Delegates.EDraggedObjectState.NONE; // no effects on the big preview image if the tree browser is used to drag
			}
			if (OBJECT_PREVIEW_IMAGE != null)
			{
				switch (p_state)
				{
					case LE_GUIInterface.Delegates.EDraggedObjectState.NOT_PLACEABLE:
						OBJECT_PREVIEW_IMAGE.color = Color.red;
						break;
					case LE_GUIInterface.Delegates.EDraggedObjectState.IN_3D_PREVIEW:
						OBJECT_PREVIEW_IMAGE.color = Color.clear;
						break;
					case LE_GUIInterface.Delegates.EDraggedObjectState.NONE:
					default:
						OBJECT_PREVIEW_IMAGE.color = Color.white;
						break;
				}
			}
		}

		private void SetDraggableObjectMessage(string p_message)
		{
			if (OBJECT_PREVIEW_MESSAGE != null) { OBJECT_PREVIEW_MESSAGE.text = p_message; }
		}

		private void SetIsSelectedObjectFocusBtnInteractable(bool p_isInteractable)
		{
			if (OBJECT_FOCUS_SELECTED_BTN != null)
			{
				OBJECT_FOCUS_SELECTED_BTN.interactable = p_isInteractable;
			}
		}

		private void SetIsSelectedObjectDuplicateBtnInteractable(bool p_isInteractable)
		{
			if (OBJECT_DUPLICATE_SELECTED_BTN != null)
			{
				OBJECT_DUPLICATE_SELECTED_BTN.interactable = p_isInteractable;
			}
		}

		private void SetIsSelectedPrefabFindBtnInteractable(bool p_isInteractable)
		{
			if (OBJECT_FIND_PREFAB_BTN != null)
			{
				OBJECT_FIND_PREFAB_BTN.interactable = p_isInteractable;
			}
		}

		private void SetIsSelectedObjectDeleteBtnInteractable(bool p_isInteractable)
		{
			if (OBJECT_DELETE_SELECTED_BTN != null)
			{
				OBJECT_DELETE_SELECTED_BTN.interactable = p_isInteractable;
			}
		}

		private void SetIsSelectedObjectSleepPropertyInteractable(bool p_isInteractable)
		{
			if (OBJECT_IS_SLEEP_MENU != null)
			{
				OBJECT_IS_SLEEP_MENU.gameObject.SetActive(p_isInteractable);
			}
		}

		private void SetSelectedObjectIsSleepOnStartPropertyValue(bool p_isSleepOnStart)
		{
			if (OBJECT_IS_SLEEP_TOGGLE != null)
			{
				OBJECT_IS_SLEEP_TOGGLE.isOn = p_isSleepOnStart;
			}
		}

		private void SetIsSelectedObjectColorPropertyInteractable(bool p_isInteractable)
		{
			if (OBJECT_COLOR_MENU != null)
			{
				OBJECT_COLOR_MENU.gameObject.SetActive(p_isInteractable);
			}
		}

		private void SetSelectedObjectColorPropertyValue(Color p_color)
		{
			if (OBJECT_COLOR_PICKER != null)
			{
				OBJECT_COLOR_PICKER.PickedColor = p_color;
			}
		}

		private void SetSelectedObjectVariationPropertyValue(int p_usedVariationIndex, LE_ObjectVariationBase[] p_variations)
		{
			if (m_usedVariationIndex == p_usedVariationIndex && m_variations == p_variations) { return; } // nothing has changed
			m_usedVariationIndex = p_usedVariationIndex;
			m_variations = p_variations;

			if (OBJECT_VARIATION_BROWSER != null)
			{
				// reset variation browser
				OBJECT_VARIATION_BROWSER.Clear();
				OBJECT_VARIATION_BROWSER.OnLeafNodePointerDown = null;

				// refill variation browser if needed
				bool isInteractable = p_variations != null && p_variations.Length > 1;
				if (isInteractable)
				{
					uMyGUI_TreeBrowser.Node[] browserNodes = new uMyGUI_TreeBrowser.Node[p_variations.Length];
					for (int i = 0; i < p_variations.Length; i++)
					{
						browserNodes[i] = new uMyGUI_TreeBrowser.Node(new LE_TextPrefabNode.SendMessageInitData(i, p_variations[i].GetName(), p_usedVariationIndex == i), null);
					}
					OBJECT_VARIATION_BROWSER.OnLeafNodePointerDown += (object p_object, uMyGUI_TreeBrowser.NodeClickEventArgs p_args)=>
					{
						LE_TextPrefabNode.SendMessageInitData data = (LE_TextPrefabNode.SendMessageInitData)p_args.ClickedNode.SendMessageData;
						LE_GUIInterface.Instance.OnSelectedObjectVariationIndexChanged(data.m_id);
					};
					OBJECT_VARIATION_BROWSER.BuildTree(browserNodes);
				}

				if (OBJECT_VARIATION_MENU != null)
				{
					OBJECT_VARIATION_MENU.gameObject.SetActive(isInteractable);
				}
			}
			else
			{
				Debug.LogError("LE_GUIInterface_uGUIimpl: SetSelectedObjectVariationPropertyValue: OBJECT_VARIATION_BROWSER is not set in inspector!");
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

		private void ShowPopupConfirmDeleteObject(System.Action<bool> p_confirmCallback)
		{
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup(uMyGUI_PopupManager.POPUP_TEXT))
				.SetText("Delete Object", "Do you really want to delete this object?")
					.ShowButton("yes", ()=>p_confirmCallback(true))
					.ShowButton("no", ()=>p_confirmCallback(false));
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
