using UnityEngine;
using System.IO;
using System.Collections.Generic;
using LE_LevelEditor.UI;
using LS_LevelStreaming;
using LE_LevelEditor.Logic;

namespace LE_LevelEditor.Core
{
	/// <summary>
	/// This static class will allow you to load and save your levels to and from byte arrays.
	/// </summary>
	public static class LE_SaveLoad
	{
		/// <summary>
		/// This class represents the meta data of a level
		/// </summary>
		public class LevelMetaData
		{
			private readonly byte m_version;
			public byte Version { get{ return m_version; } }
			
			private readonly Texture2D m_levelIcon;
			/// <summary>
			/// NEEDS TO BE DESTROYED BY USER!
			/// </summary>
			public Texture2D Icon { get{ return m_levelIcon; } }

			private readonly Dictionary<string, string> m_metaData;
			public Dictionary<string, string> MetaData { get{ return m_metaData; } }

			public LevelMetaData(byte p_version, Texture2D p_levelIcon, KeyValuePair<string, string>[] p_metaData)
			{
				m_version = p_version;
				m_levelIcon = p_levelIcon;
				m_metaData = new Dictionary<string, string>();
				if (p_metaData != null)
				{
					for (int i = 0; i < p_metaData.Length; i++)
					{
						m_metaData.Add(p_metaData[i].Key, p_metaData[i].Value);
					}
				}
			}

			public KeyValuePair<string,string>[] GetMetaDataArray()
			{
				KeyValuePair<string, string>[] result = new KeyValuePair<string, string>[m_metaData.Count];
				int index = 0;
				foreach (KeyValuePair<string, string> item in m_metaData)
				{
					result[index] = item;
					index++;		
				}
				return result;
			}
		}

		public const byte SERIALIZATION_VERSION = 3;
		public static byte[] SUPPORTED_SERIALIZATION_VERSIONS { get{ return new byte[]{1, 2, 3}; } }
		public static bool IsSerializationVersionSupported(byte pSerializationVersion)
		{
			for (int i = 0; i < SUPPORTED_SERIALIZATION_VERSIONS.Length; i++)
			{
				if (SUPPORTED_SERIALIZATION_VERSIONS[i] == pSerializationVersion)
				{
					return true;
				}
			}
			return false;
		}

		public static int RemoveDuplicatesInCurrentLevel()
		{
			int duplicateCount = 0;
			Dictionary<string, List<Vector3>> usedPositions = new Dictionary<string, List<Vector3>>();
			Dictionary<string, List<Quaternion>> usedRotations = new Dictionary<string, List<Quaternion>>();
			Dictionary<string, List<Vector3>> usedScales = new Dictionary<string, List<Vector3>>();
			LE_Object[] objs = Object.FindObjectsOfType<LE_Object>();
			for (int i = 0; i < objs.Length; i++)
			{
				bool isDuplicate = true;
				string objPrefabPath = objs[i].name;
				Vector3 objPos = objs[i].transform.position;
				Quaternion objRor = objs[i].transform.rotation;
				Vector3 objScale = objs[i].transform.lossyScale;

				CheckIfHasDuplicate(usedPositions, objPrefabPath, objPos, ref isDuplicate);
				CheckIfHasDuplicate(usedRotations, objPrefabPath, objRor, ref isDuplicate);
				CheckIfHasDuplicate(usedScales, objPrefabPath, objScale, ref isDuplicate);

				if (isDuplicate)
				{
					duplicateCount++;
					objs[i].name = "duplicate";
					Object.Destroy(objs[i].gameObject);
				}
			}
			return duplicateCount;
		}

		/// <summary>
		/// Saves the current level data to a byte array. The resulting binary data contains: serialization version number,
		/// terrain heightmaps and alphamaps, terrain texture indices, data of every LE_Object in the scene(resource path, position, rotation and scale)
		/// and object snapping meta data.
		/// </summary>
		/// <param name="p_terrainTextures">Needs to know the used terrain textures in order to index them correctly.
		/// This way only the index is saved and not the whole texture.</param>
		public static byte[] SaveCurrentLevelDataToByteArray(Texture2D[] p_terrainTextures)
		{
			// force show all streamed objects to guarantie that no object is deleted, because it is not visible right now
			if (LS_LevelStreamingSceneManager.IsInstanceSet)
			{
				LS_LevelStreamingSceneManager.Instance.ForceShowAllManagedObjects();
			}
			using (MemoryStream byteStream = new MemoryStream())
			{
				using (BinaryWriter stream = new BinaryWriter(byteStream))
				{
					// write version
					stream.Write(SERIALIZATION_VERSION);

					// save terrain
					LE_GUI3dTerrain gui3dTerrain = Object.FindObjectOfType<LE_GUI3dTerrain>();
					Terrain terrain = gui3dTerrain!=null ? gui3dTerrain.TerrainInstance : null;
					stream.Write((int)(terrain!=null?1:0)); // write int (instead of bool for upward compatibility) to indicate if the level has a terrain
					if (terrain != null)
					{
						SaveTerrainHeightmap(stream, terrain.terrainData);
						SaveTerrainAlphamaps(stream, terrain.terrainData, p_terrainTextures);
					}

					// save all LE_Objects
					LE_Object[] objs = Object.FindObjectsOfType<LE_Object>();
					// duplicates were destroyed in this frame, they still exist -> filter them out
					int realObjectCount = objs.Length;
					for (int i = 0; i < objs.Length; i++)
					{
						if (objs[i].name == "duplicate")
						{
							realObjectCount--;
							objs[i] = null;
						}
					}
					stream.Write(realObjectCount); // write number of found LE_Object without duplicates
					// write objects
					for (int i = 0; i < objs.Length; i++)
					{
						if (objs[i] != null)
						{
							SaveLE_Object(stream, objs[i]);
						}
					}

					// write object snap data
					LE_GUI3dObject gui3d = Object.FindObjectOfType<LE_GUI3dObject>();
					if (gui3d != null)
					{
						SaveObjectSnapUIDs(stream, gui3d.SnapPointUIDsToObjUIDs);
					}
					else
					{
						stream.Write((int)0);
						if (realObjectCount > 0)
						{
							Debug.LogWarning("LE_SaveLoad: SaveCurrentLevelDataToByteArray: level has objects, but no LE_GUI3dObject instance was found in the scene! Cannot save references of objects that are snapped to each other!");
						}
					}

					// return the resulting binary array
					stream.Flush();
					byteStream.Flush();
					return byteStream.ToArray();
				}
			}
		}

		/// <summary>
		/// Saves the current level meta data to a byte array. The resulting binary data contains: serialization version number,
		/// the meta data dictionary and the level icon.
		/// </summary>
		/// <param name="p_levelIcon">Level preview icon.</param>
		/// <param name="p_metaData">Meta data as a string-string dictionary content array.</param>
		public static byte[] SaveCurrentLevelMetaToByteArray(Texture2D p_levelIcon, KeyValuePair<string, string>[] p_metaData)
		{
			using (MemoryStream byteStream = new MemoryStream())
			{
				using (BinaryWriter stream = new BinaryWriter(byteStream))
				{
					// write version
					stream.Write(SERIALIZATION_VERSION);

					// write meta data parameters
					stream.Write(p_metaData.Length);
					for (int i = 0; i < p_metaData.Length; i++)
					{
						stream.Write(p_metaData[i].Key);
						stream.Write(p_metaData[i].Value);
					}

					// write icon, keep it last so that this part of the file can be skipped when no icon is needed
					if (p_levelIcon != null)
					{
						byte[] levelIconBytes = p_levelIcon.EncodeToPNG();
						stream.Write(levelIconBytes.Length);
						stream.Write(levelIconBytes);
					}
					else
					{
						stream.Write((int)0);
					}
					
					// return the resulting binary array
					stream.Flush();
					byteStream.Flush();
					return byteStream.ToArray();
				}
			}
		}

		/// <summary>
		/// Loads the level data from a byte array. Objects are instantiated, terrains are created. The level is fully loaded when this method is finished.
		/// The data of the loaded level is returned. Use it to find the loaded terrain and object instances.
		/// </summary>
		public static LE_SaveLoadData LoadLevelDataFromByteArray(byte[] p_byteArray, int p_terrainLayer, Texture2D[] p_terrainTextures, Vector2[] p_terrainTextureSizes, Vector2[] p_terrainTextureOffsets)
		{
			if (!CheckParameters("LoadLevelDataFromByteArray", p_byteArray, p_terrainTextures, p_terrainTextureSizes, p_terrainTextureOffsets))
			{
				return null;
			}

			using (MemoryStream byteStream = new MemoryStream(p_byteArray))
			{
				using (BinaryReader stream = new BinaryReader(byteStream))
				{
					byte version = stream.ReadByte();
					CheckVersion("LoadLevelDataFromByteArray", version);

					// clear cached data of the streamed scene manager
					LS_LevelStreamingSceneManager.Instance.RemoveAllManagedObjects();
					// clear cached data of the 3d object gui if it exists
					LE_GUI3dObject gui3d = Object.FindObjectOfType<LE_GUI3dObject>();
					if (gui3d != null)
					{
						gui3d.ClearLevelData();
					}

					// load terrain
					GameObject terrainGO = null;
					int terrainCount = stream.ReadInt32();
					if (terrainCount > 0)
					{
						TerrainData terrainData = LoadTerrainData(stream, p_terrainTextures, p_terrainTextureSizes, p_terrainTextureOffsets);
						terrainGO = LE_LogicTerrain.CreateOrRecycleTerrain(terrainData, p_terrainLayer);

						// backwards compatibility loading (still only the first terrain is instantiated -> levels could break (see error message below))
						if (terrainCount > 1)
						{
							// read from stream, otherwise loading would break (only read no game object is created...)
							for (int i = 1; i < terrainCount; i++) { Object.Destroy(LoadTerrainData(stream, p_terrainTextures, p_terrainTextureSizes, p_terrainTextureOffsets)); }
							Debug.LogError("LE_SaveLoad: LoadLevelDataFromByteArray: you have multiple terrains in this level! " +
								"This happened due to a bug in the older version of MR Level Editor! The new implementation will load only the first terrain. " +
								"Please contact me if you have problems with this change: " +
								"http://forum.unity3d.com/threads/multiplatform-runtime-level-editor-any-one-interested.250920/");
						}
					}
					else
					{
						LE_LogicTerrain.DestroyOrResetTerrain();
					}

					// load all LE_Objects
					bool isInEditor = gui3d != null;
					int objectsCount = stream.ReadInt32();
					LE_SaveLoadData.ObjectData[] levelObjects = new LE_SaveLoadData.ObjectData[objectsCount];
					for (int i = 0; i < objectsCount; i++)
					{
						if (version == 1)
						{
							LE_Object loadedObj = LoadLE_Object_V1(isInEditor, stream);
							if (loadedObj != null)
							{
								levelObjects[i] = new LE_SaveLoadData.ObjectData(loadedObj);
							}
							else
							{
								levelObjects[i] = new LE_SaveLoadData.ObjectData("Failed to load object!");
							}
						}
						else if (version == 2)
						{
							levelObjects[i] = LoadLE_Object_V2(isInEditor, stream, gui3d);
						}
						else // version == 3
						{
							levelObjects[i] = LoadLE_Object_V3(isInEditor, stream, gui3d);
						}
					}

					if (version != 1)
					{
						// load object snap data
						Dictionary<string, int> snapPointUIDsToObjUIDs = LoadObjectSnapUIDs(stream);
						if (gui3d != null)
						{
							gui3d.SetSnapPointUIDsToObjUIDs(snapPointUIDsToObjUIDs);
						}
					}

					return new LE_SaveLoadData(version, terrainGO, levelObjects);
				}
			}
		}

		/// <summary>
		/// Loads the level data from a byte array. Objects are NOT instantiated and terrains are NOT created. The level is NOT loaded.
		/// The terrain data is loaded, but no terrain object is created. However, you must DESTROY the TERRAIN DATA when you do not need it anymore.
		/// Use this method to preview the level contents without creating them.
		/// </summary>
		public static LE_SaveLoadDataPeek PeekLevelDataFromByteArray(byte[] p_byteArray, Texture2D[] p_terrainTextures, Vector2[] p_terrainTextureSizes, Vector2[] p_terrainTextureOffsets)
		{
			if (!CheckParameters("PeekLevelDataFromByteArray", p_byteArray, p_terrainTextures, p_terrainTextureSizes, p_terrainTextureOffsets))
			{
				return null;
			}
			
			using (MemoryStream byteStream = new MemoryStream(p_byteArray))
			{
				using (BinaryReader stream = new BinaryReader(byteStream))
				{
					byte version = stream.ReadByte();
					CheckVersion("PeekLevelDataFromByteArray", version);

					// load terrain
					TerrainData terrainData =  null;
					int terrainCount = stream.ReadInt32();
					if (terrainCount > 0)
					{
						terrainData = LoadTerrainData(stream, p_terrainTextures, p_terrainTextureSizes, p_terrainTextureOffsets);

						// backwards compatibility loading (still only the first terrain is instantiated -> levels could break (see error message below))
						if (terrainCount > 1)
						{
							// read from stream, otherwise loading would break (only read no game object is created...)
							for (int i = 1; i < terrainCount; i++) { Object.Destroy(LoadTerrainData(stream, p_terrainTextures, p_terrainTextureSizes, p_terrainTextureOffsets)); }
							Debug.LogError("LE_SaveLoad: PeekLevelDataFromByteArray: you have multiple terrains in this level! " +
							               "This happened due to a bug in the older version of MR Level Editor! The new implementation will load only the first terrain. " +
							               "Please contact me if you have problems with this change: " +
							               "http://forum.unity3d.com/threads/multiplatform-runtime-level-editor-any-one-interested.250920/");
						}
					}

					// load all LE_Objects
					int objectsCount = stream.ReadInt32();

					// the rest of the stream is ignored

					return new LE_SaveLoadDataPeek(version, terrainData, objectsCount);
				}
			}
		}

		/// <summary>
		/// Loads the level meta data from a byte array.
		/// </summary>
		public static LevelMetaData LoadLevelMetaFromByteArray(byte[] p_byteArray, bool p_isLevelIconLoaded)
		{
			using (MemoryStream byteStream = new MemoryStream(p_byteArray))
			{
				using (BinaryReader stream = new BinaryReader(byteStream))
				{
					byte version = stream.ReadByte();
					CheckVersion("LoadLevelMetaFromByteArray", version);

					// load meta data parameters
					int metaDataLength = stream.ReadInt32();
					KeyValuePair<string, string>[] metaData = new KeyValuePair<string, string>[metaDataLength];
					for (int i = 0; i < metaDataLength; i++)
					{
						string metaKey = stream.ReadString();
						string metaValue = stream.ReadString();
						metaData[i] = new KeyValuePair<string, string>(metaKey, metaValue);
					}

					// load icon only if needed
					Texture2D levelIcon = null;
					if (p_isLevelIconLoaded)
					{
						int levelIconBytesLength = stream.ReadInt32();
						if (levelIconBytesLength > 0)
						{
							Texture2D tempTex = new Texture2D(4,4);
							tempTex.LoadImage(stream.ReadBytes(levelIconBytesLength));
							levelIcon = new Texture2D(tempTex.width, tempTex.height, TextureFormat.RGB24, false, true);
							levelIcon.SetPixels(tempTex.GetPixels());
							levelIcon.Apply(false);
							Object.Destroy(tempTex);
						}
					}

					return new LevelMetaData(version, levelIcon, metaData);
				}
			}
		}

		/// <summary>
		/// This method will destroy editing scripts. This will guarantee that the level will not be edited and improve performance.
		/// </summary>
		public static void DisableLevelEditing(LE_SaveLoadData p_loadedLevel)
		{
			// destroy all level object scripts for performance optimization
			for (int i = 0; i < p_loadedLevel.LevelObjects.Length; i++)
			{
				LE_SaveLoadData.ObjectData obj = p_loadedLevel.LevelObjects[i];
				// The result code INSTANCE says that the object was loaded successfully and is already instantiated
				if (obj.Result == LE_SaveLoadData.ObjectData.EResult.INSTANCE)
				{
					// destroy LE_Object from instance
					GameObject.Destroy(obj.Instance);
				}
				// The result code STREAMED says that the object was loaded successfully, but it is a streamed object.
				// Therefore, the object will be managed by LS_LevelStreamingSceneManager
				else if (obj.Result == LE_SaveLoadData.ObjectData.EResult.STREAMED)
				{
					// destroy once the object is spawned
					LS_ManagedObjectBase managedObj = LS_LevelStreamingSceneManager.Instance.GetManagedObject(obj.StreamedLevelObjectID);
					if (managedObj != null)
					{
						managedObj.m_onShow += (object p_object, System.EventArgs p_args)=>
						{
							// LS_LevelStreaming is one of my own frameworks, for the level editor I have used only LS_ManagedObjectInstantiateDestroy
							// as management implementation, however we still need to convert the returned managed object to LS_ManagedObjectInstantiateDestroy
							if (p_object is LS_ManagedObjectInstantiateDestroy)
							{
								GameObject.Destroy(((LS_ManagedObjectInstantiateDestroy)p_object).Instance.GetComponent<LE_Object>());
							}
						};
					}
				}
			}
		}

		private static bool CheckParameters(string p_methodName, byte[] p_byteArray, Texture2D[] p_terrainTextures, Vector2[] p_terrainTextureSizes, Vector2[] p_terrainTextureOffsets)
		{
			if (p_byteArray == null || p_terrainTextures == null || p_terrainTextureSizes == null || p_terrainTextureOffsets == null)
			{
				Debug.LogError("LE_SaveLoad: " + p_methodName + ": one of the passed arrays is null!\n" +
				               "(p_byteArray==null) = " + (p_byteArray==null) + "\n" +
				               "(p_terrainTextures==null) = " + (p_terrainTextures==null) + "\n" +
				               "(p_terrainTextureSizes==null) = " + (p_terrainTextureSizes==null) + "\n" +
				               "(p_terrainTextureOffsets==null) = " + (p_terrainTextureOffsets==null));
				return false;
			}
			else
			{
				return true;
			}
		}

		private static bool CheckVersion(string p_methodName, byte p_version)
		{
			if (!IsSerializationVersionSupported(p_version))
			{
				Debug.LogError("LE_SaveLoad: " + p_methodName + ": serialization version '"+p_version+"' " +
				               "is not supported! Level loading might fail! Supported version ('1', '2').");
				return false;
			}
			else
			{
				return true;
			}
		}
		
		private static void CheckIfHasDuplicate<T>(Dictionary<string, List<T>> p_existingParameters, string p_prefabPath, T p_parameter, ref bool p_isDuplicate)
		{
			List<T> existingParametersForPath;
			if (p_existingParameters.TryGetValue(p_prefabPath, out existingParametersForPath))
			{
				if (!existingParametersForPath.Contains(p_parameter))
				{
					p_isDuplicate = false;
					existingParametersForPath.Add(p_parameter);
				}
			}
			else
			{
				p_isDuplicate = false;
				existingParametersForPath = new List<T>();
				existingParametersForPath.Add(p_parameter);
				p_existingParameters.Add(p_prefabPath, existingParametersForPath);
			}
		}

		private static void SaveTerrainHeightmap(BinaryWriter p_stream, TerrainData p_terrainData)
		{
			// general heightmap data
			p_stream.Write(p_terrainData.heightmapResolution);
			SaveVector(p_stream, p_terrainData.size);
			//SaveVector(p_stream, p_terrainData.heightmapScale);
			p_stream.Write(p_terrainData.heightmapWidth);
			p_stream.Write(p_terrainData.heightmapHeight);
			// heightmap
			float[,] heightmap = p_terrainData.GetHeights(0,0,p_terrainData.heightmapWidth,p_terrainData.heightmapHeight);
			for (int x = 0; x < p_terrainData.heightmapWidth; x++)
			{
				for (int y = 0; y < p_terrainData.heightmapHeight; y++)
				{
					p_stream.Write(heightmap[x,y]);
				}
			}
		}

		private static TerrainData LoadTerrainData(BinaryReader p_stream, Texture2D[] p_terrainTextures, Vector2[] p_terrainTextureSizes, Vector2[] p_terrainTextureOffsets)
		{
			TerrainData terrainData = new TerrainData();
			LoadTerrainHeightmap(p_stream, terrainData);
			LoadTerrainAlphamaps(p_stream, terrainData, p_terrainTextures, p_terrainTextureSizes, p_terrainTextureOffsets);
			return terrainData;
		}
		
		private static void LoadTerrainHeightmap(BinaryReader p_stream, TerrainData p_terrainData)
		{
			p_terrainData.heightmapResolution = p_stream.ReadInt32();
			p_terrainData.size = LoadVector(p_stream);
			int heightmapWidth = p_stream.ReadInt32();
			int heightmapHeight = p_stream.ReadInt32();
			// heightmap
			float[,] heightmap = new float[heightmapWidth, heightmapHeight];
			for (int x = 0; x < heightmapWidth; x++)
			{
				for (int y = 0; y < heightmapHeight; y++)
				{
					heightmap[x,y] = p_stream.ReadSingle();
				}
			}
			p_terrainData.SetHeights(0, 0, heightmap);
		}

		private static void SaveTerrainAlphamaps(BinaryWriter p_stream, TerrainData p_terrainData, Texture2D[] p_terrainTextures)
		{
			// general alphamap data
			p_stream.Write(p_terrainData.alphamapResolution);
			p_stream.Write(p_terrainData.alphamapWidth);
			p_stream.Write(p_terrainData.alphamapHeight);
			p_stream.Write(p_terrainData.alphamapLayers);
			// texture indices for each alphamap layer
			for (int i = 0; i < p_terrainData.splatPrototypes.Length; i++)
			{
				int terrainTextureIndex = -1;
				for (int j = 0; j < p_terrainTextures.Length; j++)
				{
					if (p_terrainData.splatPrototypes[i].texture == p_terrainTextures[j])
					{
						terrainTextureIndex = j;
						break;
					}
				}
				if (terrainTextureIndex == -1)
				{
					Debug.LogError("LE_SaveLoad: SaveTerrainAlphamaps: terrain uses a texture that is not contained in the " +
					               "texture array passed to SaveCurrentLevelDataToByteArray! The texture '"+p_terrainData.splatPrototypes[i].texture.name+
					               "' coult not be indexed and will not be loaded correctly!");
				}
				p_stream.Write(terrainTextureIndex);
			}
			// alphamaps
			float[,,] alphamaps = p_terrainData.GetAlphamaps(0,0,p_terrainData.alphamapWidth,p_terrainData.alphamapHeight);
			for (int x = 0; x < p_terrainData.alphamapWidth; x++)
			{
				for (int y = 0; y < p_terrainData.alphamapHeight; y++)
				{
					for (int z = 0; z < p_terrainData.alphamapLayers; z++)
					{
						p_stream.Write(alphamaps[x,y,z]);
					}
				}
			}
		}

		private static void LoadTerrainAlphamaps(BinaryReader p_stream, TerrainData p_terrainData, Texture2D[] p_terrainTextures, Vector2[] p_terrainTextureSizes, Vector2[] p_terrainTextureOffsets)
		{
			// general alphamap data
			p_terrainData.alphamapResolution = p_stream.ReadInt32();
			int alphamapWidth = p_stream.ReadInt32();
			int alphamapHeight = p_stream.ReadInt32();
			int alphamapLayers = p_stream.ReadInt32();
			// link textures from indices for each alphamap layer
			SplatPrototype[] splats = new SplatPrototype[alphamapLayers];
			for (int i = 0; i < alphamapLayers; i++)
			{
				splats[i] = new SplatPrototype();
				int terrainTextureIndex = p_stream.ReadInt32();
				if (terrainTextureIndex == -1)
				{
					Debug.LogError("LE_SaveLoad: LoadTerrainAlphamaps: terrain used a texture that was not contained in the " +
					               "texture array passed to SaveCurrentLevelDataToByteArray! A texture coult not be indexed " +
					               "when the level was saved and will not be loaded!");
				}
				else if (terrainTextureIndex < 0 || terrainTextureIndex >= p_terrainTextures.Length)
				{
					Debug.LogError("LE_SaveLoad: LoadTerrainAlphamaps: the passed texture set seems to differ from the " +
					               "texture set passed to SaveCurrentLevelDataToByteArray! Texture index '"+terrainTextureIndex+"' " +
					               "is out of bounds! Texture will not be loaded!");
				}
				else
				{
					splats[i].texture = p_terrainTextures[terrainTextureIndex];
					splats[i].tileSize = p_terrainTextureSizes[terrainTextureIndex];
					splats[i].tileOffset = p_terrainTextureOffsets[terrainTextureIndex];
				}
			}
			p_terrainData.splatPrototypes = splats;
			// alphamaps
			float[,,] alphamaps = new float[alphamapWidth, alphamapHeight, alphamapLayers];
			for (int x = 0; x < alphamapWidth; x++)
			{
				for (int y = 0; y < alphamapHeight; y++)
				{
					for (int z = 0; z < alphamapLayers; z++)
					{
						alphamaps[x,y,z] = p_stream.ReadSingle();
					}
				}
			}
			p_terrainData.SetAlphamaps(0, 0, alphamaps);
		}

		private static void SaveLE_Object(BinaryWriter p_stream, LE_Object p_object)
		{
			// write UID
			p_stream.Write(p_object.UID);
			// write resource path
			p_stream.Write(p_object.name);
			// write transformation
			SaveVector(p_stream, p_object.transform.position);
			SaveQuaternion(p_stream, p_object.transform.rotation);
			SaveVector(p_stream, p_object.transform.localScale);
			// write IsRigidbodySleepingStart property
			p_stream.Write(p_object.IsRigidbodySleepingStart);
			// write color property if there is one
			p_stream.Write(p_object.IsWithColorProperty);
			if (p_object.IsWithColorProperty)
			{
				SaveColorNoAlpha(p_stream, p_object.ColorProperty);
			}
			// write variation
			p_stream.Write(p_object.VariationsDefaultIndex); // default index contains the currently used index
		}

		private static LE_SaveLoadData.ObjectData LoadLE_Object_V3(bool p_isInEditor, BinaryReader p_stream, LE_GUI3dObject p_gui3d)
		{
			// read UID
			int UID = p_stream.ReadInt32();
			LE_Object.ReportUsedUID(UID);
			// read resource path and instantiate the prefab
			string resourcePath = p_stream.ReadString();
			// read transformation
			Vector3 pos = LoadVector(p_stream);
			Quaternion rot = LoadQuaternion(p_stream);
			Vector3 scale = LoadVector(p_stream);
			// read is IsRigidbodySleepingStart
			bool isRigidbodySleepingStart = p_stream.ReadBoolean();
			// read color property if there is one
			bool isWithColorProperty = p_stream.ReadBoolean();
			Color colorProperty = Color.clear;
			if (isWithColorProperty) { colorProperty = LoadColorNoAlpha(p_stream); }
			// read variation property
			int variationIndex = p_stream.ReadInt32();
			// load resource prefab
			Object resource = Resources.Load(resourcePath);
			GameObject go = null;
			if (resource != null)
			{
				// check if this object is streamed
				LE_Object resourceObj = ((GameObject)resource).GetComponent<LE_Object>();
				if (resourceObj.IsLevelStreaming)
				{
					// register prefab to a global manager if it is streamed
					LS_ManagedObjectInstantiateDestroy streamedObj = new LS_ManagedObjectInstantiateDestroy(
						resourcePath,
						pos,
						rot,
						scale,
						new LS_DistanceUpdatedTrigger(
						resourceObj.LevelStreamingUpdateFrequency,
						Camera.main.transform,
						p_gui3d!=null?resourceObj.LevelStreamingInstantiateDistanceInEditor:resourceObj.LevelStreamingInstantiateDistance,
						p_gui3d!=null?resourceObj.LevelStreamingDestroyDistanceInEditor:resourceObj.LevelStreamingDestroyDistance,
						p_gui3d!=null), // always visible in editor when camera mode is orthographic
						(int p_streamObjID, GameObject p_instance)=>
						{
						LE_Object levelObject = LoadLE_Object_V3(p_isInEditor, p_instance, UID, resourcePath, isRigidbodySleepingStart, isWithColorProperty, colorProperty, variationIndex);
						if (p_instance != null && levelObject != null && !levelObject.IsTransformationCachedOnDestroyWhenLevelStreaming)
						{
							p_instance.transform.position = pos;
							p_instance.transform.rotation = rot;
							p_instance.transform.localScale = scale;
						}
					},
					null);
					int streamedObjectID = LS_LevelStreamingSceneManager.Instance.AddManagedObject(streamedObj);
					return new LE_SaveLoadData.ObjectData(streamedObjectID);
				}
				else
				{
					// instantiate prefab if it is not streamed
					go = (GameObject)Object.Instantiate(resource);
					if (go != null)
					{
						go.transform.position = pos;
						go.transform.rotation = rot;
						go.transform.localScale = scale;
						return new LE_SaveLoadData.ObjectData(LoadLE_Object_V3(p_isInEditor, go, UID, resourcePath, isRigidbodySleepingStart, isWithColorProperty, colorProperty, variationIndex));
					}
					else
					{
						Debug.LogError("LE_SaveLoad: LoadLE_Object: resource path '"+resourcePath+"' is not valid! " +
						               "Only GameObject prefabs can be loaded!");
						return new LE_SaveLoadData.ObjectData("Resource path '"+resourcePath+"' is not valid!");
					}
				}
			}
			else
			{
				Debug.LogError("LE_SaveLoad: LoadLE_Object: resource path '"+resourcePath+"' is not found! " +
				               "Have you renamed or removed your object prefab after the level was saved?");
				return new LE_SaveLoadData.ObjectData("Resource path '"+resourcePath+"' is not found!");
			}
		}
		
		private static LE_Object LoadLE_Object_V3(bool p_isInEditor, GameObject p_instance, int p_UID, string p_resourcePath,
		                                          bool p_isRigidbodySleepingStart, bool p_isWithColorProperty, Color p_colorProperty, int p_variationIndex)
		{
			LE_Object levelObject = null;
			if (p_instance != null)
			{
				p_instance.name = p_resourcePath;
				levelObject = p_instance.GetComponent<LE_Object>();
				if (levelObject != null)
				{
					// set UID
					levelObject.UID = p_UID;
					// apply only if desired
					if (levelObject.IsRigidbodySleepingStartEditable)
					{
						levelObject.IsRigidbodySleepingStart = p_isRigidbodySleepingStart;
					}
					// send rigidbodies to sleep if needed (by property or always in editor)
					if (p_isInEditor || levelObject.IsRigidbodySleepingStart)
					{
						Rigidbody[] rigidbodies = levelObject.GetComponentsInChildren<Rigidbody>();
						for (int i = 0; i < rigidbodies.Length; i++)
						{
							Rigidbody body = rigidbodies[i];
							body.Sleep();
							if (p_isInEditor && !body.isKinematic)
							{
								// make sure that objects will never move in editor
								body.isKinematic = true;
							}
						}
					}
					
					// set color property if there is one
					if (p_isWithColorProperty)
					{
						levelObject.ColorProperty = p_colorProperty;
						if (!levelObject.IsWithColorProperty)
						{
							Debug.LogWarning("LE_SaveLoad: LoadLE_Object: object at path '"+p_resourcePath+"' has no color property, " +
							                 "but there is a color set in the saved data! Color will be applied from saved data!");
						}
					}
					else if (levelObject.IsWithColorProperty)
					{
						Debug.LogWarning("LE_SaveLoad: LoadLE_Object: object at path '"+p_resourcePath+"' has a color property, " +
						                 "but no color was found in the saved data!");
					}

					// set variation
					LE_LogicObjects.ApplyVariation(levelObject, p_variationIndex);
				}
				else
				{
					Debug.LogError("LE_SaveLoad: LoadLE_Object: prefab under resource path '"+p_resourcePath+"' has no LE_Object attached!");
				}
			}
			return levelObject;
		}

		private static LE_SaveLoadData.ObjectData LoadLE_Object_V2(bool p_isInEditor, BinaryReader p_stream, LE_GUI3dObject p_gui3d)
		{
			// read UID
			int UID = p_stream.ReadInt32();
			LE_Object.ReportUsedUID(UID);
			// read resource path and instantiate the prefab
			string resourcePath = p_stream.ReadString();
			// read transformation
			Vector3 pos = LoadVector(p_stream);
			Quaternion rot = LoadQuaternion(p_stream);
			Vector3 scale = LoadVector(p_stream);
			// read is IsRigidbodySleepingStart
			bool isRigidbodySleepingStart = p_stream.ReadBoolean();
			// read color property if there is one
			bool isWithColorProperty = p_stream.ReadBoolean();
			Color colorProperty = Color.clear;
			if (isWithColorProperty) { colorProperty = LoadColorNoAlpha(p_stream); }
			// load resource prefab
			Object resource = Resources.Load(resourcePath);
			GameObject go = null;
			if (resource != null)
			{
				// check if this object is streamed
				LE_Object resourceObj = ((GameObject)resource).GetComponent<LE_Object>();
				if (resourceObj.IsLevelStreaming)
				{
					// register prefab to a global manager if it is streamed
					LS_ManagedObjectInstantiateDestroy streamedObj = new LS_ManagedObjectInstantiateDestroy(
						resourcePath,
						pos,
						rot,
						scale,
						new LS_DistanceUpdatedTrigger(
							resourceObj.LevelStreamingUpdateFrequency,
							Camera.main.transform,
							p_gui3d!=null?resourceObj.LevelStreamingInstantiateDistanceInEditor:resourceObj.LevelStreamingInstantiateDistance,
							p_gui3d!=null?resourceObj.LevelStreamingDestroyDistanceInEditor:resourceObj.LevelStreamingDestroyDistance,
							p_gui3d!=null), // always visible in editor when camera mode is orthographic
						(int p_streamObjID, GameObject p_instance)=>
						{
							LE_Object levelObject = LoadLE_Object_V2(p_isInEditor, p_instance, UID, resourcePath, isRigidbodySleepingStart, isWithColorProperty, colorProperty);
							if (p_instance != null && levelObject != null && !levelObject.IsTransformationCachedOnDestroyWhenLevelStreaming)
							{
								p_instance.transform.position = pos;
								p_instance.transform.rotation = rot;
								p_instance.transform.localScale = scale;
							}
						},
						null);
					int streamedObjectID = LS_LevelStreamingSceneManager.Instance.AddManagedObject(streamedObj);
					return new LE_SaveLoadData.ObjectData(streamedObjectID);
				}
				else
				{
					// instantiate prefab if it is not streamed
					go = (GameObject)Object.Instantiate(resource);
					if (go != null)
					{
						go.transform.position = pos;
						go.transform.rotation = rot;
						go.transform.localScale = scale;
						return new LE_SaveLoadData.ObjectData(LoadLE_Object_V2(p_isInEditor, go, UID, resourcePath, isRigidbodySleepingStart, isWithColorProperty, colorProperty));
					}
					else
					{
						Debug.LogError("LE_SaveLoad: LoadLE_Object: resource path '"+resourcePath+"' is not valid! " +
						               "Only GameObject prefabs can be loaded!");
						return new LE_SaveLoadData.ObjectData("Resource path '"+resourcePath+"' is not valid!");
					}
				}
			}
			else
			{
				Debug.LogError("LE_SaveLoad: LoadLE_Object: resource path '"+resourcePath+"' is not found! " +
				               "Have you renamed or removed your object prefab after the level was saved?");
				return new LE_SaveLoadData.ObjectData("Resource path '"+resourcePath+"' is not found!");
			}
		}

		private static LE_Object LoadLE_Object_V2(bool p_isInEditor, GameObject p_instance, int p_UID, string p_resourcePath,
			bool p_isRigidbodySleepingStart, bool p_isWithColorProperty, Color p_colorProperty)
		{
			LE_Object levelObject = null;
			if (p_instance != null)
			{
				p_instance.name = p_resourcePath;
				levelObject = p_instance.GetComponent<LE_Object>();
				if (levelObject != null)
				{
					// set UID
					levelObject.UID = p_UID;
					// apply only if desired
					if (levelObject.IsRigidbodySleepingStartEditable)
					{
						levelObject.IsRigidbodySleepingStart = p_isRigidbodySleepingStart;
					}
					// send rigidbodies to sleep if needed (by property or always in editor)
					if (p_isInEditor || levelObject.IsRigidbodySleepingStart)
					{
						Rigidbody[] rigidbodies = levelObject.GetComponentsInChildren<Rigidbody>();
						for (int i = 0; i < rigidbodies.Length; i++)
						{
							Rigidbody body = rigidbodies[i];
							body.Sleep();
							if (p_isInEditor && !body.isKinematic)
							{
								// make sure that objects will never move in editor
								body.isKinematic = true;
							}
						}
					}

					// set color property if there is one
					if (p_isWithColorProperty)
					{
						levelObject.ColorProperty = p_colorProperty;
						if (!levelObject.IsWithColorProperty)
						{
							Debug.LogWarning("LE_SaveLoad: LoadLE_Object: object at path '"+p_resourcePath+"' has no color property, " +
							                 "but there is a color set in the saved data! Color will be applied from saved data!");
						}
					}
					else if (levelObject.IsWithColorProperty)
					{
						Debug.LogWarning("LE_SaveLoad: LoadLE_Object: object at path '"+p_resourcePath+"' has a color property, " +
							"but no color was found in the saved data!");
					}
				}
				else
				{
					Debug.LogError("LE_SaveLoad: LoadLE_Object: prefab under resource path '"+p_resourcePath+"' has no LE_Object attached!");
				}
			}
			return levelObject;
		}

		private static LE_Object LoadLE_Object_V1(bool p_isInEditor, BinaryReader p_stream)
		{
			LE_Object levelObject = null;
			// read resource path and instantiate the prefab
			string resourcePath = p_stream.ReadString();
			Object resource = Resources.Load(resourcePath);
			GameObject go = null;
			if (resource != null)
			{
				go = (GameObject)Object.Instantiate(resource);
			}
			// read transformation
			Vector3 pos = LoadVector(p_stream);
			Quaternion rot = LoadQuaternion(p_stream);
			Vector3 scale = LoadVector(p_stream);
			if (go != null)
			{
				go.name = resourcePath;
				go.transform.position = pos;
				go.transform.rotation = rot;
				go.transform.localScale = scale;
				
				// send rigidbodies to sleep if needed
				levelObject = go.GetComponent<LE_Object>();
				if (levelObject != null && (p_isInEditor || levelObject.IsRigidbodySleepingStart))
				{
					Rigidbody[] rigidbodies = levelObject.GetComponentsInChildren<Rigidbody>();
					for (int i = 0; i < rigidbodies.Length; i++)
					{
						Rigidbody body = rigidbodies[i];
						body.Sleep();
						if (p_isInEditor && !body.isKinematic)
						{
							// make sure that objects will never move in editor
							body.isKinematic = true;
						}
					}
				}
				
				// read color property if there is one
				bool isWithColorProperty = p_stream.ReadBoolean();
				if (isWithColorProperty)
				{
					levelObject.ColorProperty = LoadColorNoAlpha(p_stream);
					if (!levelObject.IsWithColorProperty)
					{
						Debug.LogWarning("LE_SaveLoad: LoadLE_Object: object at path '"+resourcePath+"' has no color property, " +
						                 "but there is a color set in the saved data! Color will be applied from saved data!");
					}
				}
				else if (levelObject.IsWithColorProperty)
				{
					Debug.LogWarning("LE_SaveLoad: LoadLE_Object: object at path '"+resourcePath+"' has a color property, " +
					                 "but no color was found in the saved data!");
				}
			}
			else
			{
				// skip rest of the stream data
				if (p_stream.ReadBoolean())
				{
					LoadColorNoAlpha(p_stream);
				}
				Debug.LogError("LE_SaveLoad: LoadLE_Object: resource path '"+resourcePath+"' is not valid! " +
				               "Have you renamed or removed your object prefab after the level was saved?");
			}
			return levelObject;
		}

		private static void SaveObjectSnapUIDs(BinaryWriter p_stream, Dictionary<string, int> p_snapPointUIDsToObjUIDs)
		{
			p_stream.Write(p_snapPointUIDsToObjUIDs.Count);
			foreach (KeyValuePair<string, int> reference in p_snapPointUIDsToObjUIDs)
			{
				p_stream.Write(reference.Key);
				p_stream.Write(reference.Value);
			}
		}

		private static Dictionary<string, int> LoadObjectSnapUIDs(BinaryReader p_stream)
		{
			Dictionary<string, int> snapPointUIDsToObjUIDs = new Dictionary<string, int>();
			int count = p_stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				snapPointUIDsToObjUIDs.Add(p_stream.ReadString(), p_stream.ReadInt32());
			}
			return snapPointUIDsToObjUIDs;
		}

		private static void SaveColorNoAlpha(BinaryWriter p_stream, Color p_color)
		{
			p_stream.Write(p_color.r);
			p_stream.Write(p_color.g);
			p_stream.Write(p_color.b);
		}

		private static Color LoadColorNoAlpha(BinaryReader p_stream)
		{
			return new Color(
				p_stream.ReadSingle(),
				p_stream.ReadSingle(),
				p_stream.ReadSingle());
		}

		private static void SaveVector(BinaryWriter p_stream, Vector3 p_vector)
		{
			p_stream.Write(p_vector.x);
			p_stream.Write(p_vector.y);
			p_stream.Write(p_vector.z);
		}

		private static Vector3 LoadVector(BinaryReader p_stream)
		{
			return new Vector3(
				p_stream.ReadSingle(),
				p_stream.ReadSingle(),
				p_stream.ReadSingle());
		}

		private static void SaveQuaternion(BinaryWriter p_stream, Quaternion p_quaternion)
		{
			p_stream.Write(p_quaternion.x);
			p_stream.Write(p_quaternion.y);
			p_stream.Write(p_quaternion.z);
			p_stream.Write(p_quaternion.w);
		}

		private static Quaternion LoadQuaternion(BinaryReader p_stream)
		{
			return new Quaternion(
				p_stream.ReadSingle(),
				p_stream.ReadSingle(),
				p_stream.ReadSingle(),
				p_stream.ReadSingle());
		}
	}
}