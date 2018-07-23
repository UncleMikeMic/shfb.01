using UnityEngine;
using System.Collections;
using LE_LevelEditor.Core;

namespace LE_LevelEditor
{
	public class LE_ConfigTerrain : MonoBehaviour
	{
		// terrain related references
		[SerializeField, Tooltip(
			"Scene object with a 'Projector' component that will be used to project the terrain edit brush. " +
			"Can be 'null' if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is 'false'.")]
		private Projector m_brushProjector = null;
		/// <summary>
		/// Scene object with a 'Projector' component that will be used to project the terrain edit brush. 
		/// Can be 'null' if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is 'false'.
		/// </summary>
		public Projector BrushProjector { get { return m_brushProjector; } }

		[SerializeField, Tooltip("Array with selectable brush textures.")]
		private Texture2D[] m_brushes = new Texture2D[0];
		/// <summary>
		/// Array with selectable brush textures. Learn more:
		/// http://www.freebord-game.com/index.php/multiplatform-runtime-level-editor/documentation/terrain-brushes
		/// </summary>
		public Texture2D[] Brushes { get { return m_brushes; } }

		[SerializeField, Tooltip("Terrain texture configuration must be assigned! Assign an empty asset if needed...")]
		private LE_TerrainTextureConfig m_terrainTextureConfig = null;
		/// <summary>
		/// Terrain texture configuration must be assigned! Assign an empty asset if needed. Learn more:
		/// http://www.freebord-game.com/index.php/multiplatform-runtime-level-editor/documentation/terrain-textures
		/// </summary>
		public LE_TerrainTextureConfig TerrainTextureConfig { get { return m_terrainTextureConfig; } }

		[SerializeField, Tooltip("Set a predefined Unity terrain scene object.")]
		private Terrain m_customDefaultTerrain = null;
		/// <summary>
		/// Set a predefined Unity terrain scene object. Learn more:
		/// http://www.freebord-game.com/index.php/multiplatform-runtime-level-editor/documentation/custom-terrain
		/// </summary>
		public Terrain CustomDefaultTerrain { get { return m_customDefaultTerrain; } }

		// terrain values
		[SerializeField, Tooltip("Created/loaded terrains will be moved to this layer. Raycast layer for terrain editing and terrain snapped object placement.")]
		private int m_terrainLayer = 28;
		/// <summary>
		/// Created/loaded terrains will be moved to this layer. Raycast layer for terrain editing and terrain snapped object placement.
		/// </summary>
		public int TerrainLayer { get { return m_terrainLayer; } }

		[SerializeField, Tooltip(
			"Float array with increasing size limits. Must have the same length as 'HeightmapResolutions'. When a terrain is created, " +
			"then the used resolution index (from 'HeightmapResolutions') will be the highest index, that has a smaller or equal size " +
			"(in 'HeightmapResolutionSizes') than the terrain width and length. In the example on the right a terrain with 200 units " +
			"size would get a heightmap resolution of 65 and a terrain with a size of 300 would have a resolution of 129.")]
		private float[] m_heightmapResolutionSizes = new float[0];
		/// <summary>
		/// Float array with increasing size limits. Must have the same length as 'HeightmapResolutions'. When a terrain is created, 
		/// then the used resolution index (from 'HeightmapResolutions') will be the highest index, that has a smaller or equal size 
		/// (in 'HeightmapResolutionSizes') than the terrain width and length. In the example on the right a terrain with 200 units 
		/// size would get a heightmap resolution of 65 and a terrain with a size of 300 would have a resolution of 129.
		/// </summary>
		public float[] HeightmapResolutionSizes { get { return m_heightmapResolutionSizes; } }

		[SerializeField, Tooltip("Int array with increasing heightmap resolutions. Must have the same length as 'HeightmapResolutionSizes'. Values must be power of two plus 1.")]
		private int[] m_heightmapResolutions = new int[0];
		/// <summary>
		/// Int array with increasing heightmap resolutions. Must have the same length as 'HeightmapResolutionSizes'. Values must be power of two plus 1.
		/// </summary>
		public int[] HeightmapResolutions { get { return m_heightmapResolutions; } }

		[SerializeField, Tooltip(
			"Float array with increasing size limits. Must have the same length as 'AlphamapResolutions'. When a terrain is created, " +
			"then the used resolution index (from 'AlphamapResolutions') will be the highest index, that has a smaller or equal size " +
			"(in 'AlphamapResolutionSizes') than the terrain width and length. In the example on the right a terrain with 200 units " +
			"size would get a heightmap resolution of 64 and a terrain with a size of 300 would have a resolution of 128.")]
		private float[] m_alphamapResolutionSizes = new float[0];
		/// <summary>
		/// Float array with increasing size limits. Must have the same length as 'AlphamapResolutions'. When a terrain is created, then the used resolution index (from 'AlphamapResolutions') will be the highest index, that has a smaller or equal size (in 'AlphamapResolutionSizes') than the terrain width and length. In the example on the right a terrain with 200 units size would get a heightmap resolution of 64 and a terrain with a size of 300 would have a resolution of 128.
		/// </summary>
		public float[] AlphamapResolutionSizes { get { return m_alphamapResolutionSizes; } }

		[SerializeField, Tooltip("Int array with increasing alphamap resolutions. Must have the same length as 'AlphamapResolutionSizes'. Values must be power of two.")]
		private int[] m_alphamapResolutions = new int[0];
		/// <summary>
		/// Int array with increasing alphamap resolutions. Must have the same length as 'AlphamapResolutionSizes'. Values must be power of two.
		/// </summary>
		public int[] AlphamapResolutions { get { return m_alphamapResolutions; } }

		[SerializeField, Tooltip("The maximal count of textures that a terrain can have. It will be not possible to add more textures when the limit is reached.")]
		private int m_maxTextureCount = 3;
		/// <summary>
		/// The maximal count of textures that a terrain can have. It will be not possible to add more textures when the limit is reached.
		/// </summary>
		public int MaxTextureCount { get { return m_maxTextureCount; } }

		[SerializeField, Tooltip("Player can choose a base texture for his terrain if enabled. The first texture from the 'TerrainTextureConfig' will be used if disabled.")]
		private bool m_isBaseTextureSelection = true;
		/// <summary>
		/// Player can choose a base texture for his terrain if enabled.
		/// The first texture from the TerrainTextureConfig will be used if disabled.
		/// </summary>
		public bool IsBaseTextureSelection { get { return m_isBaseTextureSelection; } }


		// initial terrain values
		[SerializeField, Tooltip("The default width value in the create terrain menu.")]
		private int m_initialWidth = 500;
		/// <summary>
		/// The default width value in the create terrain menu.
		/// </summary>
		public int InitialWidth { get { return m_initialWidth; } }

		[SerializeField, Tooltip("The default height value in the create terrain menu.")]
		private int m_initialHeight = 250;
		/// <summary>
		/// The default height value in the create terrain menu.
		/// </summary>
		public int InitialHeight { get { return m_initialHeight; } }

		[SerializeField, Tooltip("The default length value in the create terrain menu.")]
		private int m_initialLength = 500;
		/// <summary>
		/// The default length value in the create terrain menu.
		/// </summary>
		public int InitialLength { get { return m_initialLength; } }
	}
}
