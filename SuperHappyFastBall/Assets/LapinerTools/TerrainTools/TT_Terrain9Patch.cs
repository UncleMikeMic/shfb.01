using UnityEngine;
using System.Collections;

namespace TT_TerrainTools
{
	public class TT_Terrain9Patch : MonoBehaviour
	{
		public enum EFixMode { AVERAGE, READ_FROM_CENTER, READ_FROM_BORDERS }

		public Terrain Center;
		public Terrain XPositve;
		public Terrain XNegative;
		public Terrain ZPositve;
		public Terrain ZNegative;
		public Terrain XPositiveZPositve;
		public Terrain XNegativeZNegative;
		public Terrain XNegativeZPositve;
		public Terrain XPositiveZNegative;

		public bool IsTerrainDataClonedOnAwake = false;
		public bool IsTerrainDataClonedOnStart = false;

		public void FixAllBorders(EFixMode p_mode)
		{
			FixBorders(p_mode, true, true, true, true, true);
		}

		public void FixBorders(EFixMode p_mode, bool p_isFixCenter, bool p_isFixXPositive, bool p_isFixXNegative, bool p_isFixZPositive, bool p_isFixZNegative)
		{
			float fade;
			switch (p_mode)
			{
				case EFixMode.AVERAGE: fade = 0.5f; break;
				case EFixMode.READ_FROM_CENTER: fade = 0.0f; break;
				case EFixMode.READ_FROM_BORDERS:
				default: fade = 1.0f; break;
			}
			if (p_isFixCenter)
			{
				if (Center != null && XPositve != null) { TT_TerrainHelpers.FixBorders(Center, XPositve, TT_TerrainHelpers.EDirection.RIGHT, TT_TerrainHelpers.EDirection.LEFT, fade); }
				if (Center != null && XNegative != null) { TT_TerrainHelpers.FixBorders(Center, XNegative, TT_TerrainHelpers.EDirection.LEFT, TT_TerrainHelpers.EDirection.RIGHT, fade); }
				if (Center != null && ZPositve != null) { TT_TerrainHelpers.FixBorders(Center, ZPositve, TT_TerrainHelpers.EDirection.TOP, TT_TerrainHelpers.EDirection.BOTTOM, fade); }
				if (Center != null && ZNegative != null) { TT_TerrainHelpers.FixBorders(Center, ZNegative, TT_TerrainHelpers.EDirection.BOTTOM, TT_TerrainHelpers.EDirection.TOP, fade); }
			}

			if (p_isFixXPositive)
			{
				if (XPositve != null && XPositiveZPositve != null) { TT_TerrainHelpers.FixBorders(XPositve, XPositiveZPositve, TT_TerrainHelpers.EDirection.TOP, TT_TerrainHelpers.EDirection.BOTTOM, fade); }
				if (XPositve != null && XPositiveZNegative != null) { TT_TerrainHelpers.FixBorders(XPositve, XPositiveZNegative, TT_TerrainHelpers.EDirection.BOTTOM, TT_TerrainHelpers.EDirection.TOP, fade); }
			}

			if (p_isFixXNegative)
			{
				if (XNegative != null && XNegativeZPositve != null) { TT_TerrainHelpers.FixBorders(XNegative, XNegativeZPositve, TT_TerrainHelpers.EDirection.TOP, TT_TerrainHelpers.EDirection.BOTTOM, fade); }
				if (XNegative != null && XNegativeZNegative != null) { TT_TerrainHelpers.FixBorders(XNegative, XNegativeZNegative, TT_TerrainHelpers.EDirection.BOTTOM, TT_TerrainHelpers.EDirection.TOP, fade); }
			}

			if (p_isFixZPositive)
			{
				if (ZPositve != null && XPositiveZPositve != null) { TT_TerrainHelpers.FixBorders(ZPositve, XPositiveZPositve, TT_TerrainHelpers.EDirection.RIGHT, TT_TerrainHelpers.EDirection.LEFT, fade); }
				if (ZPositve != null && XNegativeZPositve != null) { TT_TerrainHelpers.FixBorders(ZPositve, XNegativeZPositve, TT_TerrainHelpers.EDirection.LEFT, TT_TerrainHelpers.EDirection.RIGHT, fade); }
			}

			if (p_isFixZNegative)
			{
				if (ZNegative != null && XPositiveZNegative != null) { TT_TerrainHelpers.FixBorders(ZNegative, XPositiveZNegative, TT_TerrainHelpers.EDirection.RIGHT, TT_TerrainHelpers.EDirection.LEFT, fade); }
				if (ZNegative != null && XNegativeZNegative != null) { TT_TerrainHelpers.FixBorders(ZNegative, XNegativeZNegative, TT_TerrainHelpers.EDirection.LEFT, TT_TerrainHelpers.EDirection.RIGHT, fade); }
			}
		}

		public void SetNeighbors()
		{
			CrashCheck();

			if (Center != null)
			{
				Center.SetNeighbors(
					XNegative, // left X-
					ZPositve, // top Z+
					XPositve, // right X+
					ZNegative); // bottom Z-
			}
			
			if (XPositve != null)
			{
				XPositve.SetNeighbors(
					Center, // left X-
					XPositiveZPositve, // top Z+
					null, // right X+
					XPositiveZNegative); // bottom Z-
			}
			
			if (XNegative != null)
			{
				XNegative.SetNeighbors(
					null, // left X-
					XNegativeZPositve, // top Z+
					Center, // right X+
					XNegativeZNegative); // bottom Z-
			}

			if (ZPositve != null)
			{
				ZPositve.SetNeighbors(
					XNegativeZPositve, // left X-
					null, // top Z+
					XPositiveZPositve, // right X+
					Center); // bottom Z-
			}
			
			if (ZNegative != null)
			{
				ZNegative.SetNeighbors(
					XNegativeZNegative, // left X-
					Center, // top Z+
					XPositiveZNegative, // right X+
					null); // bottom Z-
			}
			
			if (XPositiveZPositve != null)
			{
				XPositiveZPositve.SetNeighbors(
					ZPositve, // left X-
					null, // top Z+
					null, // right X+
					XPositve); // bottom Z-
			}
			
			if (XNegativeZNegative != null)
			{
				XNegativeZNegative.SetNeighbors(
					null, // left X-
					XNegative, // top Z+
					ZNegative, // right X+
					null); // bottom Z-
			}
			
			if (XNegativeZPositve != null)
			{
				XNegativeZPositve.SetNeighbors(
					null, // left X-
					null, // top Z+
					ZPositve, // right X+
					XNegative); // bottom Z-
			}
			
			if (XPositiveZNegative != null)
			{
				XPositiveZNegative.SetNeighbors(
					ZNegative, // left X-
					XPositve, // top Z+
					null, // right X+
					null); // bottom Z-
			}
		}

		public void RemoveNeighbors()
		{
			if (Center != null) { Center.SetNeighbors(null, null, null, null); }
			if (XPositve != null) { XPositve.SetNeighbors(null, null, null, null); }
			if (XNegative != null) { XNegative.SetNeighbors(null, null, null, null); }
			if (ZPositve != null) { ZPositve.SetNeighbors(null, null, null, null); }
			if (ZNegative != null) { ZNegative.SetNeighbors(null, null, null, null); }
			if (XPositiveZPositve != null) { XPositiveZPositve.SetNeighbors(null, null, null, null); }
			if (XNegativeZNegative != null) { XNegativeZNegative.SetNeighbors(null, null, null, null); }
			if (XNegativeZPositve != null) { XNegativeZPositve.SetNeighbors(null, null, null, null); }
			if (XPositiveZNegative != null) { XPositiveZNegative.SetNeighbors(null, null, null, null); }
		}

		public void CrashCheck()
		{
			bool isCrashPossible = false;
			int terrainInstances = 1;
			int resolution = -1;
			if (Center != null) { terrainInstances++; isCrashPossible |= resolution!=-1&&resolution!=Center.terrainData.heightmapResolution; resolution = Center.terrainData.heightmapResolution; }
			if (XPositve != null) { terrainInstances++; isCrashPossible |= resolution!=-1&&resolution!=XPositve.terrainData.heightmapResolution; resolution = XPositve.terrainData.heightmapResolution; }
			if (XNegative != null) { terrainInstances++; isCrashPossible |= resolution!=-1&&resolution!=XNegative.terrainData.heightmapResolution; resolution = XNegative.terrainData.heightmapResolution; }
			if (ZPositve != null) { terrainInstances++; isCrashPossible |= resolution!=-1&&resolution!=ZPositve.terrainData.heightmapResolution; resolution = ZPositve.terrainData.heightmapResolution; }
			if (ZNegative != null) { terrainInstances++; isCrashPossible |= resolution!=-1&&resolution!=ZNegative.terrainData.heightmapResolution; resolution = ZNegative.terrainData.heightmapResolution; }
			if (XPositiveZPositve != null) { terrainInstances++; isCrashPossible |= resolution!=-1&&resolution!=XPositiveZPositve.terrainData.heightmapResolution; resolution = XPositiveZPositve.terrainData.heightmapResolution; }
			if (XNegativeZNegative != null) { terrainInstances++; isCrashPossible |= resolution!=-1&&resolution!=XNegativeZNegative.terrainData.heightmapResolution; resolution = XNegativeZNegative.terrainData.heightmapResolution; }
			if (XNegativeZPositve != null) { terrainInstances++; isCrashPossible |= resolution!=-1&&resolution!=XNegativeZPositve.terrainData.heightmapResolution; resolution = XNegativeZPositve.terrainData.heightmapResolution; }
			if (XPositiveZNegative != null) { terrainInstances++; isCrashPossible |= resolution!=-1&&resolution!=XPositiveZNegative.terrainData.heightmapResolution; resolution = XPositiveZNegative.terrainData.heightmapResolution; }
			isCrashPossible = terrainInstances >= 3 && isCrashPossible;
			if (isCrashPossible)
			{
				Debug.LogWarning(
					"The Unity Engine has a 'Terrain.SetNeighbors' critical bug that leads to a crash in the editor and at runtime!\n" +
					"Your terrain constellation seems to allow this bug. However, I have not found a 100% reproduction way. I only " +
					"know that if the terrains have different 'heightmapResolution', then the crash is possible (not guaranteed only possible).\n" +
					"The next code line will break the execution, remove it if you encounter no problems. I have decided to insert this line to " +
					"make sure that you will see this warning and know that there is a possible issue. Also the current implementation of the " +
					"TT_TerrainTools does not allow your terrains to have different heightmapResolutions if you want to snap the terrain borders " +
					"to each other.");
				// remove this line if your implementation does not crash
				Debug.Break();
				// this lines will disable all terrains to prevent the crash before the editor can pause the game
				if (Center != null) { Center.enabled = false; }
				if (XPositve != null) { XPositve.enabled = false; }
				if (XNegative != null) { XNegative.enabled = false; }
				if (ZPositve != null) { ZPositve.enabled = false; }
				if (ZNegative != null) { ZNegative.enabled = false; }
				if (XPositiveZPositve != null) { XPositiveZPositve.enabled = false; }
				if (XNegativeZNegative != null) { XNegativeZNegative.enabled = false; }
				if (XNegativeZPositve != null) { XNegativeZPositve.enabled = false; }
				if (XPositiveZNegative != null) { XPositiveZNegative.enabled = false; }
				// the terrains will be enabled again in the next frame to show you if there is a crash or not
				StartCoroutine(CrashCheckReenable());
			}
		}

		public IEnumerator CrashCheckReenable()
		{
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			if (Center != null) { Center.enabled = true; }
			if (XPositve != null) { XPositve.enabled = true; }
			if (XNegative != null) { XNegative.enabled = true; }
			if (ZPositve != null) { ZPositve.enabled = true; }
			if (ZNegative != null) { ZNegative.enabled = true; }
			if (XPositiveZPositve != null) { XPositiveZPositve.enabled = true; }
			if (XNegativeZNegative != null) { XNegativeZNegative.enabled = true; }
			if (XNegativeZPositve != null) { XNegativeZPositve.enabled = true; }
			if (XPositiveZNegative != null) { XPositiveZNegative.enabled = true; }
		}
		
		private void Awake()
		{
			if (IsTerrainDataClonedOnAwake)
			{
				CloneTerrainData();
			}
		}

		private void Start()
		{
			if (IsTerrainDataClonedOnAwake && IsTerrainDataClonedOnStart)
			{
				Debug.LogError("TT_Terrain9Patch: IsTerrainDataClonedOnAwake and IsTerrainDataClonedOnStart are set to true! Terrain data was cloned on awake...");
			}
			else if (IsTerrainDataClonedOnStart)
			{
				CloneTerrainData();
			}
			SetNeighbors();
		}

		private void CloneTerrainData()
		{
			CloneTerrainData(Center);
			CloneTerrainData(XPositve);
			CloneTerrainData(XNegative);
			CloneTerrainData(ZPositve);
			CloneTerrainData(ZNegative);
			CloneTerrainData(XPositiveZPositve);
			CloneTerrainData(XNegativeZNegative);
			CloneTerrainData(XNegativeZPositve);
			CloneTerrainData(XPositiveZNegative);
		}

		private void OnDestroy()
		{
			RemoveNeighbors();
		}

		private void CloneTerrainData(Terrain p_terrain)
		{
			if (p_terrain != null)
			{
				p_terrain.enabled = false;
				p_terrain.terrainData = (TerrainData)Instantiate(p_terrain.terrainData);
				if (p_terrain.GetComponent<TerrainCollider>() != null)
				{
					p_terrain.GetComponent<TerrainCollider>().terrainData = p_terrain.terrainData;
				}
				p_terrain.Flush();
				p_terrain.enabled = true;
			}
		}
	}
}
