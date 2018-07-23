using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace LE_LevelEditor.UI
{
	public class LE_ObjectDragDrop : MonoBehaviour
	{
		[SerializeField]
		private float MAX_IMAGE_SIZE = 150;
		[SerializeField]
		private RectTransform SELECTED_OBJECT_TRANSFORM = null;
		[SerializeField]
		private RawImage SELECTED_OBJECT_IMAGE = null;

		private float m_selectedObjectTransformHeight;
		private LE_ObjectPrefabNode m_selectedNode = null;

		public void OnObjectSelected(LE_ObjectPrefabNode p_objectNode)
		{
			if (m_selectedNode != p_objectNode)
			{
				if (m_selectedNode != null)
				{
					m_selectedNode.HideSelection();
				}
				m_selectedNode = p_objectNode;
				if (SELECTED_OBJECT_TRANSFORM != null)
				{
					SELECTED_OBJECT_TRANSFORM.gameObject.SetActive(true);
					float size = Mathf.Min(MAX_IMAGE_SIZE, m_selectedObjectTransformHeight);
					SELECTED_OBJECT_TRANSFORM.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
					SELECTED_OBJECT_TRANSFORM.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
				}
				else
				{
					Debug.LogError("LE_ObjectDragDrop: OnObjectSelected: SELECTED_OBJECT_TRANSFORM was not set in inspector!");
				}
				if (SELECTED_OBJECT_IMAGE != null)
				{
					if (m_selectedNode.Image != null) { SELECTED_OBJECT_IMAGE.texture = m_selectedNode.Image.texture; }
				}
				else
				{
					Debug.LogError("LE_ObjectDragDrop: OnObjectSelected: SELECTED_OBJECT_IMAGE was not set in inspector!");
				}
			}
		}

		private void Start()
		{
			if (SELECTED_OBJECT_TRANSFORM != null)
			{
				m_selectedObjectTransformHeight = SELECTED_OBJECT_TRANSFORM.rect.height;
			}
			else
			{
				Debug.LogError("LE_ObjectDragDrop: OnObjectSelected: SELECTED_OBJECT_TRANSFORM was not set in inspector!");
			}
		}
	}
}
