using UnityEngine;
using System.Collections;

namespace LE_LevelEditor.LEInput
{
	public interface LE_IInputHandler
	{
		// cursor
		void SetCursorPosition(Vector3 p_cursorScreenCoords);
		void SetIsCursorAction(bool p_isCursorAction);
		
		// camera movement
		void MoveCamera(Vector3 p_fromScreenCoords, Vector3 p_toScreenCoords);
		void RotateCamera(Vector3 p_fromScreenCoords, Vector3 p_toScreenCoords);
		void RotateCameraAroundPivot(Vector3 p_fromScreenCoords, Vector3 p_toScreenCoords);
	}
}
