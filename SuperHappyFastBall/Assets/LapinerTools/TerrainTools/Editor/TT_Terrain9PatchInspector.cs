#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;

namespace TT_TerrainTools
{
	[CustomEditor(typeof(TT_Terrain9Patch))] 
	public class TT_Terrain9PatchInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			TT_Terrain9Patch t9patch = (TT_Terrain9Patch)target;

			if (GUILayout.Button("Set Neighbors"))
			{
				t9patch.SetNeighbors();
			}

			if (GUILayout.Button("Fix Borders AVERAGE"))
			{
				t9patch.FixAllBorders(TT_Terrain9Patch.EFixMode.AVERAGE);
			}

			if (GUILayout.Button("Fix Borders READ_FROM_CENTER"))
			{
				t9patch.FixAllBorders(TT_Terrain9Patch.EFixMode.READ_FROM_CENTER);
			}
		}
	}
}
#endif
