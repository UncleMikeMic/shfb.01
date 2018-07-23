using UnityEngine;
using System.Collections;
using S_SnapTools;

namespace LE_LevelEditor.UI
{
	public class LE_GUISnapButtonParticleSys : MonoBehaviour
	{
		[SerializeField]
		private Material m_openedMaterial;
		[SerializeField]
		private Material m_closedMaterial;

		private S_SnapToObject m_parent;
		private Renderer m_particleSysRenderer;

		private bool m_isClosed = true;

		private void Start ()
		{
			m_parent = transform.GetComponentInParent<S_SnapToObject>();
			if (m_parent == null)
			{
				Debug.LogError("LE_GUISnapButtonParticleSys: could not find S_SnapToObject in parent.");
				Destroy(this);
				return;
			}
			ParticleSystem particleSys = GetComponent<ParticleSystem>();
			if (particleSys == null)
			{
				Debug.LogError("LE_GUISnapButtonParticleSys: could not find ParticleSystem.");
				Destroy(this);
				return;
			}
			m_particleSysRenderer = particleSys.GetComponent<Renderer>();
			if (m_particleSysRenderer == null)
			{
				Debug.LogError("LE_GUISnapButtonParticleSys: could not find Renderer for ParticleSystem.");
				Destroy(this);
				return;
			}
			m_particleSysRenderer.sharedMaterial = m_closedMaterial;
		}
		
		private void Update ()
		{
			if (m_parent != null && m_particleSysRenderer != null)
			{
				if (m_isClosed && m_parent.IsPrefabSelectionOpen)
				{
					m_isClosed = false;
					m_particleSysRenderer.sharedMaterial = m_openedMaterial;
				}
				else if (!m_isClosed && !m_parent.IsPrefabSelectionOpen)
				{
					m_isClosed = true;
					m_particleSysRenderer.sharedMaterial = m_closedMaterial;
				}
			}
			else
			{
				Destroy(this);
			}
		}
	}
}
