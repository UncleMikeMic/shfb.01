using UnityEngine;
using System.Collections;

namespace LE_LevelEditor.LEInput
{
	[System.Flags]
	public enum LE_EKeyCombo
	{
		FOCUS = 1,
		DUPLICATE = 2,
		UNDO = 4,
		REDO = 8,
	}
}
