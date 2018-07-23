using UnityEngine;
using System.Collections;

namespace LE_LevelEditor.UI
{
	[RequireComponent(typeof(Animation))]
	public class LE_UndoRedoBtnAnimator : MonoBehaviour
	{
		public enum Type { UNDO, REDO }

		[SerializeField]
		private Type m_type = Type.UNDO; 

		private Animation m_animation;
		private bool m_isAvailable = true;

		private void Start()
		{
			m_animation = GetComponent<Animation>();
			if (m_animation == null)
			{
				Debug.LogError("LE_UndoRedoBtnAnimator: missing Animation component!");
				enabled = false;
				return;
			}
		}

		private void Update()
		{
			bool isAvailable = false;
			switch (m_type)
			{
				case Type.UNDO: isAvailable = LE_LevelEditorMain.Instance.IsUndoable;
					break;
				case Type.REDO: isAvailable = LE_LevelEditorMain.Instance.IsRedoable;
					break;
			}

			if (m_isAvailable != isAvailable)
			{
				m_isAvailable = isAvailable;
				m_animation.Play(m_isAvailable ? "UndoRedoRotatedFadeIn" : "UndoRedoRotatedFadeOut");
			}
		}
	}
}
