using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using LapinerTools.uMyGUI;
using UnityEngine.EventSystems;

namespace LE_LevelEditor.UI
{
	public class LE_PopupFileSelection : uMyGUI_PopupText
	{
		[SerializeField]
		protected uMyGUI_TreeBrowser m_filePicker;

		[SerializeField]
		protected InputField m_saveInput;
		public InputField SaveInput { get{ return m_saveInput; } }

		public override void Show ()
		{
			base.Show ();
			if (m_saveInput != null)
			{
				m_saveInput.text = "";
			}
		}

		public override void Hide()
		{
			base.Hide ();
			if (m_filePicker != null)
			{
				m_filePicker.Clear();
				m_filePicker.OnLeafNodeClick = null;
				m_filePicker.gameObject.SetActive(false);
			}
		}

		public virtual LE_PopupFileSelection SetFiles(string[] p_names, string[] p_iconPaths, System.Action<int> p_onSelectedCallback, System.Action<int> p_onDeleteCallback)
		{
			return SetFiles(p_names, p_iconPaths, p_onSelectedCallback, p_onDeleteCallback, true);
		}

		public virtual LE_PopupFileSelection SetFiles(string[] p_names, string[] p_iconPaths, System.Action<int> p_onSelectedCallback, System.Action<int> p_onDeleteCallback, bool p_isCloseOnClick)
		{
			if (m_filePicker != null)
			{
				m_filePicker.gameObject.SetActive(true);

				uMyGUI_TreeBrowser.Node[] browserNodes = new uMyGUI_TreeBrowser.Node[p_names.Length];
				for (int i = 0; i < p_names.Length; i++)
				{
					string nodeText = p_names[i];
					string nodeIconPath = p_iconPaths.Length > i ? p_iconPaths[i] : "";
					LE_LevelFileNode.SendMessageInitData nodeData = new LE_LevelFileNode.SendMessageInitData(i, nodeText, nodeIconPath, false);
					if (p_onDeleteCallback != null)
					{
						nodeData.m_onDelete = () => p_onDeleteCallback(nodeData.m_id);
					}
					browserNodes[i] = new uMyGUI_TreeBrowser.Node(nodeData, null);
				}
				m_filePicker.OnLeafNodeClick += (object p_object, uMyGUI_TreeBrowser.NodeClickEventArgs p_args)=>
				{
					LE_LevelFileNode.SendMessageInitData data = (LE_LevelFileNode.SendMessageInitData)p_args.ClickedNode.SendMessageData;
					p_onSelectedCallback(data.m_id);
					if (m_audioSources.Length > 0 && m_audioSources[0] != null)
					{
						m_audioSources[0].Play();
					}
					if (p_isCloseOnClick)
					{
						Hide();
					}
				};
				m_filePicker.BuildTree(browserNodes);
			}
			return this;
		}
	}
}
