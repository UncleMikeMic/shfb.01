using UnityEngine;
using System.Collections;

namespace LE_LevelEditor
{
	public class LE_ConfigLevel : MonoBehaviour
	{
		// level values
		[SerializeField, Tooltip(
			"If enabled, then objects that have an equal resource path, position, rotation and scale are detected as duplicate. " +
			"Only one duplicate is saved, the others are destroyed. Users often generate duplicates by clicking the 'duplicate' button or using Ctrl+D.")]
		private bool m_isRemoveDuplicatesOnSave = true;
		/// <summary>
		/// If enabled, then objects that have an equal resource path, position, rotation and scale are detected as duplicate. 
		/// Only one duplicate is saved, the others are destroyed. Users often generate duplicates by clicking the 'duplicate' button or using Ctrl+D.
		/// </summary>
		public bool IsRemoveDuplicatesOnSave { get { return m_isRemoveDuplicatesOnSave; } }

		// level icon values
		[SerializeField, Tooltip("The width of the level icon texture that is captured when the 'Render Level Icon' button is clicked.")]
		private int m_levelIconWidth = 256;
		/// <summary>
		/// The width of the level icon texture that is captured when the 'Render Level Icon' button is clicked.
		/// </summary>
		public int LevelIconWidth { get { return m_levelIconWidth; } }
		[SerializeField, Tooltip("The height of the level icon texture that is captured when the 'Render Level Icon' button is clicked.")]
		private int m_levelIconHeight = 256;
		/// <summary>
		/// The height of the level icon texture that is captured when the 'Render Level Icon' button is clicked.
		/// </summary>
		public int LevelIconHeight { get { return m_levelIconHeight; } }
	}
}
