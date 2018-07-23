using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using LE_LevelEditor.Core;
using MyUtility;

namespace LE_LevelEditor.UI
{
	public class LE_ObjectPrefabNode : MonoBehaviour, IScrollHandler
	{
		public class SendMessageInitData
		{
			public readonly LE_Object m_object;
			public readonly int m_indentLevel;
			public SendMessageInitData(LE_Object p_object, int p_indentLevel) { m_object = p_object; m_indentLevel = p_indentLevel; }
		}

		[SerializeField]
		private Text m_text;

		[SerializeField]
		private RawImage m_image;
		public RawImage Image { get{ return m_image; } }

		[SerializeField]
		private Image m_selectionImage;

		private ScrollRect m_parentScroller = null;

		public void uMyGUI_TreeBrowser_InitNode(object p_data)
		{
			if (m_text != null && m_image != null)
			{
				if (p_data is SendMessageInitData)
				{
					SendMessageInitData data = (SendMessageInitData)p_data;
					// image
					Texture2D iconTex = (Texture2D)Resources.Load(data.m_object.IconPath);
					if (iconTex != null)
					{
						m_image.texture = iconTex;
					}
					else
					{
						Debug.LogError("LE_ObjectPrefabNode: uMyGUI_TreeBrowser_InitNode: object '" + data.m_object.name + "'" +
						               " has an invalid icon resource path! Path: '"+data.m_object.IconPath+"'");
					}
					// name
					m_text.text = UtilityStrings.InsertSpacesIntoCamelCase(data.m_object.name);
					// resize text due to indent
					Vector2 offsetMax = m_text.rectTransform.offsetMax;
					offsetMax.x -= data.m_indentLevel*10f;
					m_text.rectTransform.offsetMax = offsetMax;
				}
				else if (p_data is LE_ObjectCategoryNode.SendMessageInitData)
				{
					LE_ObjectCategoryNode.SendMessageInitData data = (LE_ObjectCategoryNode.SendMessageInitData)p_data;
					Debug.LogError("LE_ObjectPrefabNode: uMyGUI_TreeBrowser_InitNode: the category '" + data.m_categoryName + "' is empty and will not be shown correctly!");
					m_image.gameObject.SetActive(false);
					m_text.text = LE_ObjectCategoryNode.GetCleanCategoryName(data.m_categoryName);
				}
				else
				{
					Debug.LogError("LE_ObjectPrefabNode: uMyGUI_TreeBrowser_InitNode: expected p_data to be a LE_ObjectPrefabNode.SendMessageInitData! p_data: " + p_data);
				}
			}
			else
			{
				Debug.LogError("LE_ObjectPrefabNode: uMyGUI_TreeBrowser_InitNode: m_text or m_image were not set via inspector!");
			}
		}

		public void OnSelected()
		{
			LE_ObjectDragDrop dragAndDrop = GetComponentInParent<LE_ObjectDragDrop>();
			if (dragAndDrop != null)
			{
				dragAndDrop.OnObjectSelected(this);
			}
			else
			{
				Debug.LogError("LE_ObjectPrefabNode: OnSelected: could not find LE_ObjectDragDrop in parents!");
			}
			ShowSelection();
		}

		public void ShowSelection()
		{
			if (m_selectionImage != null) { m_selectionImage.color = Color.green; }
			if (m_text != null) { m_text.color = Color.green; }
		}

		public void HideSelection()
		{
			if (m_selectionImage != null) { m_selectionImage.color = Color.white; }
			if (m_text != null) { m_text.color = Color.white; }
		}

		public void OnScroll(PointerEventData data)
		{
			// try to find the parent ScrollRect
			if (m_parentScroller == null)
			{
				m_parentScroller = GetComponentInParent<ScrollRect>();
			}

			// cannot do anything without a parent ScrollRect -> return
			if (m_parentScroller == null)
			{
				return;
			}

			// forward the scroll event data to the parent
			m_parentScroller.OnScroll(data);
		}
	}
}
