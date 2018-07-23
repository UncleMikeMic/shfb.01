using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LE_LevelEditor.Core
{
	public class LE_TerrainTextureConfig : ScriptableObject
	{
#if UNITY_EDITOR
		[MenuItem("Assets/Create/LE_TerrainTextureConfig")]
		public static void CreateLE_TerrainTextureConfig()
		{
			LE_TerrainTextureConfig terraintTextureConf = ScriptableObject.CreateInstance<LE_TerrainTextureConfig>();
			AssetDatabase.CreateAsset(terraintTextureConf, "Assets/TerrainTextureConfig.asset");
			AssetDatabase.SaveAssets();
			Selection.activeObject = terraintTextureConf;
		}
#endif

		[SerializeField]
		private Texture2D[] m_terrainTextures = new Texture2D[0];
		public Texture2D[] TERRAIN_TEXTURES { get{ return m_terrainTextures; } }
		[SerializeField]
		private Vector2[] m_terrainTextureSizes = new Vector2[0];
		public Vector2[] TERRAIN_TEXTURE_SIZES { get{ return m_terrainTextureSizes; } }
		[SerializeField]
		private Vector2[] m_terrainTextureOffsets = new Vector2[0];
		public Vector2[] TERRAIN_TEXTURE_OFFSETS { get{ return m_terrainTextureOffsets; } }
	}
}
