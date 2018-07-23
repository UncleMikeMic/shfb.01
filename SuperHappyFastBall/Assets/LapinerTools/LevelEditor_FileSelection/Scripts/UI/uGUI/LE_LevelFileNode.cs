using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using LE_LevelEditor.Core;
using MyUtility;

namespace LE_LevelEditor.UI
{
	public class LE_LevelFileNode : MonoBehaviour
	{
		public const int MAX_STREAMS = 4;
		public static int s_streamCount = 0;

		public class SendMessageInitData
		{
			public readonly int m_id;
			public readonly string m_text;
			public readonly string m_iconPath;
			public readonly bool m_isSelected;
			public System.Action m_onDelete = null;
			public SendMessageInitData(int p_id, string p_text, string p_iconPath, bool p_isSelected) { m_id = p_id; m_text = p_text; m_iconPath = p_iconPath; m_isSelected = p_isSelected; }
		}

		[SerializeField]
		private Text m_text;

		[SerializeField]
		private Image m_selectionImage;

		[SerializeField]
		private RawImage m_iconImage;
		
		[SerializeField]
		private Image m_iconOverlay;

		[SerializeField]
		private Button m_deleteBtn;
		
		private bool m_isIconLoading = false;
		private bool m_isIconLoaded = false;

		public void uMyGUI_TreeBrowser_InitNode(object p_data)
		{
			if (m_text != null)
			{
				if (p_data is SendMessageInitData)
				{
					SendMessageInitData data = (SendMessageInitData)p_data;
					// text
					m_text.text = data.m_text;
					// try load icon
					StartCoroutine(LoadLevelIcon(data.m_iconPath));
					// delete button click handler
					if (m_deleteBtn != null)
					{
						if (data.m_onDelete != null)
						{
							m_deleteBtn.onClick.AddListener(() => data.m_onDelete());
						}
						else
						{
							m_deleteBtn.transform.parent.gameObject.SetActive(false);
						}
					}
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

		private void OnDestroy()
		{
			if (m_isIconLoaded && m_iconImage != null) { Destroy(m_iconImage.texture); }
			m_isIconLoaded = false;
			if (m_isIconLoading)
			{
				s_streamCount--;
			}
			m_isIconLoading = false;
			if (m_deleteBtn != null)
			{
				m_deleteBtn.onClick.RemoveAllListeners();
			}
		}

		private IEnumerator LoadLevelIcon(string p_iconPath)
		{
			if (m_iconImage != null)
			{
				float startTime = Time.realtimeSinceStartup;
				// wait for icon to be fully loaded
				WWW iconWWW = null;
				WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();
				while (iconWWW == null || !iconWWW.isDone)
				{
					// start loading icon (after a little delay (wait for the popup animation to finish))
					if (iconWWW == null && Time.realtimeSinceStartup - startTime >= 0.65f && s_streamCount < MAX_STREAMS)
					{
						iconWWW = new WWW(UtilityPlatformIO.FixFilePath(p_iconPath));
						s_streamCount++;
						m_isIconLoading = true;
					}
					// rotate loading indicator
					if (m_iconImage != null) { m_iconImage.transform.eulerAngles = 140f * Vector3.back * Time.realtimeSinceStartup; }
					if (m_iconOverlay != null) { m_iconOverlay.transform.rotation = Quaternion.identity; }
					yield return frameEnd;
				}

				m_isIconLoading = false;
				s_streamCount--;

				// show icon
				if (string.IsNullOrEmpty(iconWWW.error) && iconWWW.texture != null)
				{
					if (m_iconImage != null)
					{
						// reset icon rotation
						m_iconImage.transform.rotation = Quaternion.identity;
						if (m_iconOverlay != null) { m_iconOverlay.transform.rotation = Quaternion.identity; }
						// set texture
						if (iconWWW.texture.width > iconWWW.texture.height)
						{
							float aspectChangeFactor = (float)iconWWW.texture.width / (float)iconWWW.texture.height;
							float aspectCorrectedWidth = m_iconImage.rectTransform.rect.width;
							aspectCorrectedWidth *= aspectChangeFactor; 
							m_iconImage.rectTransform.pivot = new Vector2(0f, m_iconImage.rectTransform.pivot.y);
							m_iconImage.rectTransform.localPosition -= Vector3.right * m_iconImage.rectTransform.rect.width * 0.5f;
							m_iconImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, aspectCorrectedWidth);
						}
						m_iconImage.texture = iconWWW.texture;
					}
					m_isIconLoaded = true;
				}
				else
				{
					// hide icon on error (not found)
					if (m_iconImage != null) { m_iconImage.enabled = false; }
				}
			}
			else
			{
				yield return null;
			}
		}
	}
}
