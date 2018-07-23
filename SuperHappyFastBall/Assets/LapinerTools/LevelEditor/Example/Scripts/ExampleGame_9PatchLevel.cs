using UnityEngine;
using System.Collections;
using LE_LevelEditor.Events;
using LE_LevelEditor.Core;
using LE_LevelEditor.Logic;
using TT_TerrainTools;

namespace LE_LevelEditor.Example
{
	public class ExampleGame_9PatchLevel : MonoBehaviour
	{
		private TT_Terrain9Patch m_patchGroup;
		private bool m_doFix = true;

		private void Start()
		{
			m_patchGroup = FindObjectOfType<TT_Terrain9Patch>();
			LE_EventInterface.OnChangeLevelData += OnChangeLevelData;
		}

		private void LateUpdate()
		{
			if (m_doFix)
			{
				m_doFix = false;
				m_patchGroup.FixAllBorders(TT_Terrain9Patch.EFixMode.READ_FROM_BORDERS);
			}
		}

		private void OnDestroy()
		{
			LE_EventInterface.OnChangeLevelData -= OnChangeLevelData;
		}

		private void OnChangeLevelData(object p_obj, System.EventArgs p_args)
		{
			if (p_obj is LE_TerrainManager ||
			    p_obj is GameObject && ((GameObject)p_obj).GetComponent<Terrain>() != null ||
			    p_obj is LE_LogicTerrain)
			{
				m_doFix = true;
			}
		}
	}
}
