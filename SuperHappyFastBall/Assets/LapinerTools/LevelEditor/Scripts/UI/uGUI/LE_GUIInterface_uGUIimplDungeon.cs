using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using LapinerTools.uMyGUI;
using LE_LevelEditor.Core;

namespace LE_LevelEditor.UI
{
	public class LE_GUIInterface_uGUIimplDungeon : LE_GUIInterface_uGUIimplBase
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

		[SerializeField]
		private float CAM_OBLIQUE_RIGHT_PIXEL_OFFSET = 250f;

		[SerializeField]
		private uMyGUI_TreeBrowser OBJECT_BROWSER = null;
		[SerializeField]
		private uMyGUI_Draggable OBJECT_PREVIEW_DRAGGABLE = null;
		[SerializeField]
		private RawImage OBJECT_PREVIEW_IMAGE = null;
		[SerializeField]
		private Text OBJECT_PREVIEW_MESSAGE = null;
		[SerializeField]
		private Button OBJECT_DUPLICATE_SELECTED_BTN = null;
		[SerializeField]
		private Button OBJECT_DELETE_SELECTED_BTN = null;
		[SerializeField]
		private RectTransform OBJECT_COLOR_MENU = null;
		[SerializeField]
		private uMyGUI_ColorPicker OBJECT_COLOR_PICKER = null;
		[SerializeField]
		private RectTransform OBJECT_VARIATION_MENU = null;
		[SerializeField]
		private uMyGUI_TreeBrowser OBJECT_VARIATION_BROWSER = null;

		[SerializeField]
		private AudioSource BUTTON_CLICK_AUDIO_SOURCE = null;

		private Dictionary<uMyGUI_TreeBrowser.Node, LE_Object> m_nodeToObject = new Dictionary<uMyGUI_TreeBrowser.Node, LE_Object>();
		private Dictionary<uMyGUI_TreeBrowser.Node, string> m_nodeToResourcePath = new Dictionary<uMyGUI_TreeBrowser.Node, string>();

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
				LE_GUIInterface.Instance.delegates.GetObliqueCameraPerspectiveRightPixelOffset += GetObliqueCameraPerspectiveRightPixelOffset;

				// we do not need the focus button, but we need to inform the level editor that we know it (or we will get warnings)
				LE_GUIInterface.Instance.delegates.SetIsSelectedObjectFocusBtnInteractable += (bool p_dummy)=>{};
				// we do not need is sleep on start for rigidbodies, because the level will be never loaded in play mode
				// but we need to inform the level editor that we know it (or we will get warnings)
				LE_GUIInterface.Instance.delegates.SetIsSelectedObjectSleepPropertyInteractable += (bool p_dummy)=>{};
				LE_GUIInterface.Instance.delegates.SetSelectedObjectIsSleepOnStartPropertyValue += (bool p_dummy)=>{};
				// we do not need the level icon, but we need to inform the level editor that we know it (or we will get warnings)
				LE_GUIInterface.Instance.delegates.SetLevelIcon += (Texture2D p_icon)=>{};

				LE_GUIInterface.Instance.delegates.SetObjects += SetObjects;
				LE_GUIInterface.Instance.delegates.IsObjectDragged += IsObjectDragged;
				LE_GUIInterface.Instance.delegates.SetDraggableObjectState += SetDraggableObjectState;
				LE_GUIInterface.Instance.delegates.SetDraggableObjectMessage += SetDraggableObjectMessage;
				LE_GUIInterface.Instance.delegates.SetIsSelectedObjectDuplicateBtnInteractable += SetIsSelectedObjectDuplicateBtnInteractable;
				LE_GUIInterface.Instance.delegates.SetIsSelectedObjectDeleteBtnInteractable += SetIsSelectedObjectDeleteBtnInteractable;
				LE_GUIInterface.Instance.delegates.SetIsSelectedObjectColorPropertyInteractable += SetIsSelectedObjectColorPropertyInteractable;
				LE_GUIInterface.Instance.delegates.SetSelectedObjectColorPropertyValue += SetSelectedObjectColorPropertyValue;
				LE_GUIInterface.Instance.delegates.SetSelectedObjectVariationPropertyValue += SetSelectedObjectVariationPropertyValue;

				LE_GUIInterface.Instance.delegates.ShowPopupConfirmDeleteObject += ShowPopupConfirmDeleteObject;

				// register callbacks
				if (OBJECT_COLOR_PICKER != null)
				{
					OBJECT_COLOR_PICKER.m_onChanged += (object p_obj, uMyGUI_ColorPicker.ColorEventArgs p_args)=>{ LE_GUIInterface.Instance.OnSelectedObjectColorChanged(p_args.Value); };
				}
			}
			else
			{
				Debug.LogError("LE_GUIInterface_uGUIimplDungeon: Start: could not find LE_GUIInterface!");
			}

			// generate some warnings
			if (OBJECT_PREVIEW_DRAGGABLE == null)
			{
				Debug.LogError("LE_GUIInterface_uGUIimplDungeon: OBJECT_PREVIEW_DRAGGABLE was not set in the inspector -> drag&drop will not work!");
			}
			if (OBJECT_PREVIEW_IMAGE == null)
			{
				Debug.LogWarning("LE_GUIInterface_uGUIimplDungeon: OBJECT_PREVIEW_IMAGE was not set in the inspector!");
			}
			if (OBJECT_PREVIEW_MESSAGE == null)
			{
				Debug.LogWarning("LE_GUIInterface_uGUIimplDungeon: OBJECT_PREVIEW_MESSAGE was not set in the inspector!");
			}
			if (OBJECT_DUPLICATE_SELECTED_BTN == null)
			{
				Debug.LogWarning("LE_GUIInterface_uGUIimplDungeon: OBJECT_DUPLICATE_SELECTED_BTN was not set in the inspector!");
			}
			if (OBJECT_DELETE_SELECTED_BTN == null)
			{
				Debug.LogWarning("LE_GUIInterface_uGUIimplDungeon: OBJECT_DELETE_SELECTED_BTN was not set in the inspector!");
			}
			if (OBJECT_COLOR_MENU == null)
			{
				Debug.LogWarning("LE_GUIInterface_uGUIimplDungeon: OBJECT_COLOR_MENU was not set in the inspector!");
			}
			if (OBJECT_COLOR_PICKER == null)
			{
				Debug.LogWarning("LE_GUIInterface_uGUIimplDungeon: OBJECT_COLOR_PICKER was not set in the inspector!");
			}
			if (BUTTON_CLICK_AUDIO_SOURCE == null)
			{
				Debug.LogWarning("LE_GUIInterface_uGUIimplDungeon: BUTTON_CLICK_AUDIO_SOURCE was not set in the inspector!");
			}
		}

		private float GetObliqueCameraPerspectiveRightPixelOffset()
		{
			return CAM_OBLIQUE_RIGHT_PIXEL_OFFSET*(Screen.height/m_transform.rect.height);
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
						Debug.LogError("LE_GUIInterface_uGUIimplDungeon: could not find selected node's object!");
					}
					if (!m_nodeToResourcePath.TryGetValue(p_args.ClickedNode, out selectedResourcePath))
					{
						Debug.LogError("LE_GUIInterface_uGUIimplDungeon: could not find selected node's path!");
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
				Debug.LogError("LE_GUIInterface_uGUIimplDungeon: SetObjects: OBJECT_BROWSER is not set in inspector!");
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
					Debug.LogError("LE_GUIInterface_uGUIimplDungeon: SetObjectsRecursive: sub object map from map '" + p_objectMap.name + "'" +
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
					Debug.LogError("LE_GUIInterface_uGUIimplDungeon: SetObjectsRecursive: object from map '" + p_objectMap.name + "'" +
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

		private void SetIsSelectedObjectDuplicateBtnInteractable(bool p_isInteractable)
		{
			if (OBJECT_DUPLICATE_SELECTED_BTN != null)
			{
				OBJECT_DUPLICATE_SELECTED_BTN.interactable = p_isInteractable;
			}
		}

		private void SetIsSelectedObjectDeleteBtnInteractable(bool p_isInteractable)
		{
			if (OBJECT_DELETE_SELECTED_BTN != null)
			{
				OBJECT_DELETE_SELECTED_BTN.interactable = p_isInteractable;
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
