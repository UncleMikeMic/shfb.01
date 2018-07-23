using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LE_LevelEditor.Core
{
	[System.Serializable]
	public class LE_ObjectVariationMaterial : LE_ObjectVariationBase
	{
		[System.Serializable]
		public class Materials
		{
			public Material[] m_materials;
			public Materials(Material[] p_materials) { m_materials = p_materials; }
		}

		[SerializeField]
		private string m_name = "Enter variation name here..";
		public string Name
		{
			get{ return m_name; }
			set{ m_name = value; }
		}

		[SerializeField]
		private Renderer[] m_renderers = new Renderer[0];
		public Renderer[] Renderers
		{
			get{ return m_renderers; }
			set{ m_renderers = value; }
		}

		[SerializeField]
		private Materials[] m_rendererMaterials = new Materials[0];
		public Materials[] RendererMaterials
		{
			get{ return m_rendererMaterials; }
			set{ m_rendererMaterials = value; }
		}

		public LE_ObjectVariationMaterial(string p_name)
		{
			m_name = p_name;
		}

		public override string GetName()
		{
			return m_name;
		}

		public override void Apply(LE_Object p_object)
		{
			if (m_renderers == null || m_rendererMaterials == null || m_renderers.Length != m_rendererMaterials.Length)
			{
				Debug.LogWarning("LE_ObjectVariationMaterial: Apply: the 'Renderers' array must be of same length as the 'RendererMaterials' array! The materials of the renderer at index i will be overwritten the 'RendererMaterials' at index i!");
			}

			// hide selection if there is any
			bool isSelected = p_object.IsSelected;
			if (isSelected)
			{
				p_object.IsSelected = false;
				// selection state would be applied at the end of the frame, but we need it right now
				p_object.ApplySelectionState();
			}

			// apply material changes
			for (int i = 0; i < m_renderers.Length && i < m_rendererMaterials.Length; i++)
			{
				Renderer renderer = m_renderers[i];
				Materials materials = m_rendererMaterials[i];
				if (renderer != null && materials != null && materials.m_materials != null)
				{
					renderer.materials = materials.m_materials;
				}
			}

			// restore selection if there was any
			if (isSelected)
			{
				p_object.IsSelected = true;
				// selection state would be applied at the end of the frame, but we need it right now
				p_object.ApplySelectionState();
			}
		}

		public bool LoadNFixReferences(LE_Object p_object)
		{
			bool isChanged = false;

			Renderer[] currRenderers = p_object.GetComponentsInChildren<Renderer>(true);

			// check for nullpointers
			for (int i = m_renderers.Length-1; i >= 0; i--)
			{
				if (m_renderers[i] == null)
				{
					RemoveRendererAt(i);
					isChanged = true;
				}
			}
			
			// check if renderers where removed
			for (int i = m_renderers.Length-1; i >= 0; i--)
			{
				if (System.Array.IndexOf(currRenderers, m_renderers[i]) < 0)
				{
					RemoveRendererAt(i);
					isChanged = true;
				}
			}
			
			// check if renderers added
			for (int i = 0; i < currRenderers.Length; i++)
			{
				if (System.Array.IndexOf(m_renderers, currRenderers[i]) < 0 &&
				    currRenderers[i].GetComponentInParent<LE_ObjectEditHandle>() == null) // don't select edit handles
				{
					AddRenderer(currRenderers[i]);
					isChanged = true;
				}
			}

			return isChanged;
		}

		private void AddRenderer(Renderer p_renderer)
		{
			List<Renderer> changedRenderers = new List<Renderer>(m_renderers);
			List<Materials> changedRendererMaterials = new List<Materials>(m_rendererMaterials);

			changedRenderers.Add(p_renderer);
			changedRendererMaterials.Add(new Materials(p_renderer.sharedMaterials));
			
			m_renderers = changedRenderers.ToArray();
			m_rendererMaterials = changedRendererMaterials.ToArray();
		}

		private void RemoveRendererAt(int p_index)
		{
			List<Renderer> changedRenderers = new List<Renderer>(m_renderers);
			List<Materials> changedRendererMaterials = new List<Materials>(m_rendererMaterials);

			changedRenderers.RemoveAt(p_index);
			changedRendererMaterials.RemoveAt(p_index);

			m_renderers = changedRenderers.ToArray();
			m_rendererMaterials = changedRendererMaterials.ToArray();
		}
	}
}
