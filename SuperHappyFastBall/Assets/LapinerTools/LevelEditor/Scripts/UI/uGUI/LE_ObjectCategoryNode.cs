using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MyUtility;

namespace LE_LevelEditor.UI
{
	public class LE_ObjectCategoryNode : MonoBehaviour
	{
		public class SendMessageInitData
		{
			public readonly string m_categoryName;
			public readonly int m_indentLevel;
			public SendMessageInitData(string p_categoryName, int p_indentLevel) { m_categoryName = p_categoryName; m_indentLevel = p_indentLevel; }
		}

		[SerializeField]
		private Text m_text;

		public void uMyGUI_TreeBrowser_InitNode(object p_data)
		{
			if (m_text != null)
			{
				if (p_data is SendMessageInitData)
				{
					SendMessageInitData data = (SendMessageInitData)p_data;
					// category name text
					m_text.text = GetCleanCategoryName(data.m_categoryName);
					// resize text due to indent
					Vector2 offsetMax = m_text.rectTransform.offsetMax;
					offsetMax.x -= data.m_indentLevel*10f;
					m_text.rectTransform.offsetMax = offsetMax;
				}
				else
				{
					Debug.LogError("LE_ObjectCategoryNode: uMyGUI_TreeBrowser_InitNode: expected p_data to be a string! p_data: " + p_data);
				}
			}
			else
			{
				Debug.LogError("LE_ObjectCategoryNode: uMyGUI_TreeBrowser_InitNode: m_text was not set via inspector!");
			}
		}

		public static string GetCleanCategoryName(string p_categoryName)
		{
			if (p_categoryName.Contains("_"))
			{
				return UtilityStrings.InsertSpacesIntoCamelCase(p_categoryName.Substring(0, p_categoryName.IndexOf("_")));
			}
			else
			{
				return UtilityStrings.InsertSpacesIntoCamelCase(p_categoryName);
			}
		}
	}
}
