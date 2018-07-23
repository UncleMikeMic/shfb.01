#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

namespace LE_LevelEditor.Core
{
	[CustomEditor(typeof(LE_ObjectMap))] 
	public class LE_ObjectMapInspector : Editor 
	{
		private string m_errorMessage = null;

		[MenuItem("Assets/Create/LE_ObjectMap")]
		public static void CreateLE_ObjectMap()
		{
			LE_ObjectMap map = ScriptableObject.CreateInstance<LE_ObjectMap>();
			AssetDatabase.CreateAsset(map, "Assets/ObjectMap.asset");
			AssetDatabase.SaveAssets();
			Selection.activeObject = map;
		}

		public override void OnInspectorGUI()
		{
			LE_ObjectMap targetMap = (LE_ObjectMap)target;
			GUIStyle style = new GUIStyle(EditorStyles.whiteBoldLabel);
			style.wordWrap = true;
			// check and fix prefab paths
			bool isSynchronized = SynchronizePrefabsAndPathes(targetMap);
			// check and fix preview icons
			bool isRendered = !isSynchronized || CheckPreviewIcons(targetMap);
			// check is readable state of meshes
			if (m_errorMessage == null)
			{
				CheckReadableMeshes(targetMap);
			}

			GUI.color = isSynchronized ? Color.green : Color.red;
			EditorGUILayout.LabelField(isSynchronized ? "Resource pathes are all ok :)" : "Prefab resource pathes are not synchronized!", style);
			GUI.color = (isSynchronized && isRendered) ? Color.green : Color.red;
			if (m_errorMessage != null)
			{
				GUI.color = Color.red;
				EditorGUILayout.LabelField(m_errorMessage, style);
			}
			GUI.color = Color.white;

			if (!isRendered && GUILayout.Button("Render Missing Icons"))
			{
				RenderPreviewIcons(targetMap);
			}

			base.OnInspectorGUI();
		}

		private bool SynchronizePrefabsAndPathes(LE_ObjectMap p_map)
		{
			m_errorMessage = null;
			bool isSynchronized = true;
			for (int i = 0; i < p_map.ObjectPrefabs.Length; i++)
			{
				if (p_map.ObjectPrefabs[i] == null)
				{
					m_errorMessage = "ERROR: Object prefab at index '"+i+"' is null! Please remove it or set a reference!";
					isSynchronized = false;
					break;
				}
				else
				{
					string path = AssetDatabase.GetAssetPath(p_map.ObjectPrefabs[i]);
					if (string.IsNullOrEmpty(path))
					{
						m_errorMessage = "ERROR: Object prefab at index '"+i+"' was not found in the AssetDatabase! " +
							"Make sure to place your object prefabs in a 'Resources' folder. Do not link scene objects!";
						isSynchronized = false;
						break;
					}
					else if (path.IndexOf("Resources/") < 0)
					{
						m_errorMessage = "ERROR: Object prefab at index '"+i+"' is not in a 'Resources' folder! " +
						               "Make sure to place your object prefabs in a 'Resources' folder. " +
						               "This is required in order to load them when game is played!";
						isSynchronized = false;
						break;
					}
					else
					{
						// get only resource path
						path = path.Substring(path.IndexOf("Resources/")+10);
						path = path.Substring(0, path.LastIndexOf("."));
						if (p_map.ObjectPrefabResourcePaths.Length != p_map.ObjectPrefabs.Length)
						{
							m_errorMessage = "...synchronizing array length";
							isSynchronized = false;
							p_map.ObjectPrefabResourcePaths = new string[p_map.ObjectPrefabs.Length];
							EditorUtility.SetDirty(p_map);
							AssetDatabase.SaveAssets();
							break;
						}
						else if(p_map.ObjectPrefabResourcePaths[i] != path)
						{
							m_errorMessage = "...synchronizing path of object prefab at index '"+i+"'";
							isSynchronized = false;
							p_map.ObjectPrefabResourcePaths[i] = path;
							EditorUtility.SetDirty(p_map);
							AssetDatabase.SaveAssets();
							break;
						}
					}
				}
			}

			return isSynchronized;
		}

		private bool CheckPreviewIcons(LE_ObjectMap p_map)
		{
			m_errorMessage = null;
			for (int i = 0; i < p_map.ObjectPrefabs.Length; i++)
			{
				if (string.IsNullOrEmpty(p_map.ObjectPrefabs[i].IconPath))
				{
					m_errorMessage = "ERROR: Object prefab '" + p_map.ObjectPrefabs[i].name + "' at index '"+i+"' " +
						"has no icon path set and will not work! Click the button below for automatic preview icon rendering!";
					return false;
				}
			}
			
			return true;
		}

		private void RenderPreviewIcons(LE_ObjectMap p_map)
		{
			for (int i = 0; i < p_map.ObjectPrefabs.Length; i++)
			{
				if (string.IsNullOrEmpty(p_map.ObjectPrefabs[i].IconPath))
				{
					GameObject go = ((LE_ObjectMap)target).ObjectPrefabs[i].gameObject;
					string iconFilePath = AssetDatabase.GetAssetPath(go);
					iconFilePath = iconFilePath.Replace("Assets/", "");
					iconFilePath = iconFilePath.Substring(0, iconFilePath.IndexOf(go.name));
					string iconFolderName = iconFilePath.Substring(iconFilePath.IndexOf("Resources/")+10);
					if (iconFolderName.StartsWith("Objects/"))
					{
						iconFolderName = iconFolderName.Replace("Objects/", "");
					}
					iconFilePath = iconFilePath.Substring(0, iconFilePath.IndexOf("Resources/")+10) + "ObjectIcons/" + iconFolderName;
					if (!iconFilePath.EndsWith("/"))
					{
						iconFilePath += "/";
					}
					Texture2D preview = AssetPreview.GetAssetPreview(go);
					if (preview != null)
					{
						Color[] pixels = preview.GetPixels();
						// try find background color
						Color borderBGColor = new Color(0.32156862745f, 0.32156862745f, 0.32156862745f);
						if (pixels.Length > 0)
						{
							borderBGColor = pixels[0];
							if (borderBGColor != pixels[pixels.Length-1])
							{
								Debug.LogWarning("LE_ObjectMapInspector: RenderPreviewIcons: internal error! The background of the generated icon is not completely cleared!");
							}
						}
						// add transparency
						for (int j = 0; j < pixels.Length; j++)
						{
							Color pixelColor = pixels[j];
							if ((pixelColor.r == 0.32156862745f &&
							     pixelColor.g == 0.32156862745f &&
							     pixelColor.b == 0.32156862745f) ||
							    (pixelColor.r == borderBGColor.r &&
								 pixelColor.g == borderBGColor.g &&
								 pixelColor.b == borderBGColor.b))
							{
								pixels[j].a = 0f;
							}
						}
						preview.SetPixels(pixels);
						// save to file
						byte[] pngBytes = preview.EncodeToPNG();
						bool isExists = Directory.Exists(Application.dataPath + "/" + iconFilePath);
						if(!isExists)
						{
							Directory.CreateDirectory(Application.dataPath + "/" + iconFilePath);
						}
						iconFilePath += go.name + ".png";
						using (FileStream pngFileStream = File.Open(Application.dataPath + "/" + iconFilePath, FileMode.Create))
						{
							using (BinaryWriter writer = new BinaryWriter(pngFileStream))
							{
								writer.Write(pngBytes);
								writer.Flush();
								pngFileStream.Close();
							}
						}
						AssetDatabase.ImportAsset("Assets/" + iconFilePath);
						TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath("Assets/" + iconFilePath);
						if (textureImporter != null)
						{
							textureImporter.alphaIsTransparency = true;
							textureImporter.mipmapEnabled = false;
#if UNITY_5_5_OR_NEWER
							textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
#else
							textureImporter.textureFormat = TextureImporterFormat.ARGB32;
#endif
							textureImporter.textureType = TextureImporterType.GUI;
						}
						else
						{
							Debug.LogError("Failed to setup texture import settings for '" + p_map.ObjectPrefabs[i].name + "'!");
						}
						AssetDatabase.ImportAsset("Assets/" + iconFilePath);
						p_map.ObjectPrefabs[i].IconPath = "ObjectIcons/" + iconFolderName + go.name;
						EditorUtility.SetDirty(p_map.ObjectPrefabs[i]);
					}
					else
					{
						Debug.LogError("Failed to generate preview for object prefab '" + p_map.ObjectPrefabs[i].name + "' at index '"+i+"'!");
					}
				}
			}
		}

		private bool CheckReadableMeshes(LE_ObjectMap p_map)
		{
			m_errorMessage = null;
			for (int i = 0; i < p_map.ObjectPrefabs.Length; i++)
			{
				MeshFilter[] meshFilters = p_map.ObjectPrefabs[i].GetComponentsInChildren<MeshFilter>(true);
				SkinnedMeshRenderer[] smrs = p_map.ObjectPrefabs[i].GetComponentsInChildren<SkinnedMeshRenderer>(true);
				for (int j = 0; j < meshFilters.Length; j++)
				{
					if (meshFilters[j].sharedMesh != null && !meshFilters[j].sharedMesh.isReadable)
					{
						m_errorMessage = "ERROR: Object prefab '" + p_map.ObjectPrefabs[i].name + "' at index '"+i+"' " +
							"has a not readable mesh! Please enable the 'Read/Write Enabled' option in the mesh importer settings of '" + meshFilters[j].name + "'!";
						return false;
					}
				}
				for (int j = 0; j < smrs.Length; j++)
				{
					if (smrs[j].sharedMesh != null && !smrs[j].sharedMesh.isReadable)
					{
						m_errorMessage = "ERROR: Object prefab '" + p_map.ObjectPrefabs[i].name + "' at index '"+i+"' " +
							"has a not readable mesh! Please enable the 'Read/Write Enabled' option in the mesh importer settings of '" + meshFilters[j].name + "'!";
						return false;
					}
				}
			}
			
			return false;
		}
	}
}
#endif
