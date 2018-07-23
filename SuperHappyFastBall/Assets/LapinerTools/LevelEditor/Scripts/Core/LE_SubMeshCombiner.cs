using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LE_LevelEditor.Core
{
	public class LE_SubMeshCombiner
	{
		private const string MESH_NAME_POSTFIX = "_combSubMesh";

		private readonly Material m_material;
		private readonly Mesh m_mesh;
		private readonly Renderer m_renderer;

		private bool m_isCombinedSubMeshVisible = false;
		private bool m_isCombinedSubMeshGenerated = false;

		public LE_SubMeshCombiner(Material p_material, Mesh p_mesh, Renderer p_renderer)
		{
			m_material = p_material;
			m_mesh = p_mesh;
			m_renderer = p_renderer;
			if (m_mesh != null)
			{
				// check if a combined sub mesh is needed
				m_isCombinedSubMeshGenerated =
					m_mesh.subMeshCount <= 1 || // no need to generate the exactly same mesh
					m_mesh.name.EndsWith(MESH_NAME_POSTFIX); // no need to generate the mesh multiple times
				// check if a combined material is already added and needs to be removed
				if (m_isCombinedSubMeshGenerated &&
				    m_renderer.sharedMaterials[m_renderer.sharedMaterials.Length-1] != null &&
				    m_renderer.sharedMaterials[m_renderer.sharedMaterials.Length-1].name == m_material.name &&
				    m_renderer.sharedMaterials[m_renderer.sharedMaterials.Length-1].shader.name == m_material.shader.name)
				{
					m_isCombinedSubMeshVisible = true;
					HideCombinedSubMesh();
				}
			}
			else
			{
				Debug.LogError("LE_SubMeshCombiner: p_mesh is null!");
			}
		}

		public void GenerateCombinedSubMesh()
		{
			if (m_mesh != null && !m_isCombinedSubMeshGenerated)
			{
				m_isCombinedSubMeshGenerated = true;
				m_mesh.name += MESH_NAME_POSTFIX;
				List<int> triangles = new List<int>();
				// collect triangle indicies from all submeshes into one list
				for (int i = 0; i < m_mesh.subMeshCount; i++)
				{
					triangles.AddRange(m_mesh.GetTriangles(i));
				}
				m_mesh.subMeshCount = m_mesh.subMeshCount+1;
				m_mesh.SetTriangles(triangles.ToArray(), m_mesh.subMeshCount-1);
			}
		}

		public void ShowCombinedSubMesh()
		{
			if (m_mesh != null && !m_isCombinedSubMeshVisible)
			{
				if (m_renderer != null)
				{
					GenerateCombinedSubMesh();
					Material[] materials = new Material[m_renderer.sharedMaterials.Length+1];
					System.Array.Copy(m_renderer.sharedMaterials, materials, m_renderer.sharedMaterials.Length);
					materials[materials.Length-1] = m_material;
					m_renderer.sharedMaterials = materials;
					m_isCombinedSubMeshVisible = true;
					m_mesh.RecalculateBounds();
				}
				else
				{
					Debug.LogError("LE_SubMeshCombiner: ShowCombinedSubMesh: lost reference to renderer!");
				}
			}
		}

		public void HideCombinedSubMesh()
		{
			if (m_mesh != null && m_isCombinedSubMeshVisible)
			{
				if (m_renderer != null)
				{
					m_isCombinedSubMeshVisible = false;
					Material[] materials = new Material[m_renderer.sharedMaterials.Length-1];
					System.Array.Copy(m_renderer.sharedMaterials, materials, materials.Length);
					m_renderer.sharedMaterials = materials;
					m_mesh.RecalculateBounds();
				}
				else
				{
					Debug.LogError("LE_SubMeshCombiner: HideCombinedSubMesh: lost reference to renderer!");
				}
			}
		}
	}
}