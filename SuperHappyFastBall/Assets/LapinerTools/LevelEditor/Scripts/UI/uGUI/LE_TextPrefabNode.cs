using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using LE_LevelEditor.Core;
using MyUtility;

namespace LE_LevelEditor.UI
{
	public class LE_TextPrefabNode : MonoBehaviour
	{
		public class SendMessageInitData
		{
			public readonly int m_id;
			public readonly string m_text;
			public readonly bool m_isSelected;
			public SendMessageInitData(int p_id, string p_text, bool p_isSelected) { m_id = p_id; m_text = p_text; m_isSelected = p_isSelected; }
		}

		[SerializeField]
		private Text m_text;

		[SerializeField]
		private Image m_selectionImage;

		public void uMyGUI_TreeBrowser_InitNode(object p_data)
		{
			if (m_text != null)
			{
				if (p_data is SendMessageInitData)
				{
					SendMessageInitData data = (SendMessageInitData)p_data;
					// text
					m_text.text = data.m_text;
					// initial selection
					if (data.m_isSelected)
					{
						ShowSelection();
					}
				}
				else
				{
					Debug.LogError("LE_TextPrefabNode: uMyGUI_TreeBrowser_InitNode: expected p_data to be a LE_TextPrefabNode.SendMessageInitData! p_data: " + p_data);
				}
			}
			else
			{
				Debug.LogError("LE_TextPrefabNode: uMyGUI_TreeBrowser_InitNode: m_text was not set via inspector!");
			}
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
	}
}
