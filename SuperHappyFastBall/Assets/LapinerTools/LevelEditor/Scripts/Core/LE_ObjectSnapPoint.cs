using UnityEngine;
using System.Collections;
using S_SnapTools;

namespace LE_LevelEditor.Core
{
	[System.Serializable]
	public class LE_ObjectSnapPoint
	{
		private const string BUTTON_VISUALS_NAME = "LE_ObjectSnapPoint.Button.Visuals";

		[SerializeField]
		private Transform m_point = null;
		public Transform Point
		{
			get{ return m_point; }
			set{ m_point = value;}
		}

		[SerializeField]
		private S_SnapToObjectPrefab[] m_prefabs = new S_SnapToObjectPrefab[1];
		public S_SnapToObjectPrefab[] Prefabs
		{
			get{ return m_prefabs; }
			set{ m_prefabs = value;}
		}

		[SerializeField]
		private Vector3 m_snapButtonLocalOffset = Vector3.zero;
		public Vector3 SnapButtonLocalOffset
		{
			get{ return m_snapButtonLocalOffset; }
			set{ m_snapButtonLocalOffset = value;}
		}

		[SerializeField]
		private float m_snapButtonSize = 1f;
		public float SnapButtonSize
		{
			get{ return m_snapButtonSize; }
			set{ m_snapButtonSize = value;}
		}

		private S_SnapToObject m_snapSysInstance = null;
		public S_SnapToObject SnapSystemInstance
		{
			get{ return m_snapSysInstance; }
			set{ m_snapSysInstance = value;}
		}

		public LE_ObjectSnapPoint()
		{
			if (m_prefabs != null && m_prefabs.Length == 1)
			{
				m_prefabs[0] = new S_SnapToObjectPrefab();
			}
		}

		public S_SnapToObject InstatiateSnapSystem(GameObject p_buttonVisuals, bool p_isDrawUI, Material p_materialLine, Material p_materialFill)
		{
			// check params
			if (m_point == null)
			{
				Debug.LogError("LE_ObjectSnapPoint: InstatiateSnapSystem: the Transform property 'Point' is null! Set it in the inspector!");
				return null;
			}
			if (m_prefabs == null || m_prefabs.Length == 0)
			{
				Debug.LogError("LE_ObjectSnapPoint: InstatiateSnapSystem: the Prefabs property array is null or has no entries!");
				return null;
			}
			// instantiate S_SnapToObject system
			if (m_snapSysInstance != null)
			{
				Debug.LogError("LE_ObjectSnapPoint: InstatiateSnapSystem: the system was already initialized!");
				GameObject.Destroy(m_snapSysInstance);
			}
			m_snapSysInstance = Point.gameObject.AddComponent<S_SnapToObject>();
			m_snapSysInstance.Prefabs = Prefabs;
			m_snapSysInstance.IsDeactivatedAfterSnap = true;
			m_snapSysInstance.IsDestroyedAfterSnap = false;
			m_snapSysInstance.IsDrawUI = p_isDrawUI;
			m_snapSysInstance.UImaterialLine = p_materialLine;
			m_snapSysInstance.UImaterialFill = p_materialFill;
			// create the snap button
			SphereCollider btnCollider = Point.gameObject.AddComponent<SphereCollider>();
			btnCollider.radius = SnapButtonSize*0.5f;
			btnCollider.center = SnapButtonLocalOffset;
			// place snap button visuals at the right place
			p_buttonVisuals.name = BUTTON_VISUALS_NAME; // name has to be constant to allow destruction
			if (p_buttonVisuals != null)
			{
				p_buttonVisuals.transform.parent = Point;
				p_buttonVisuals.transform.localPosition = m_snapButtonLocalOffset;
				p_buttonVisuals.transform.localScale = Vector3.one*m_snapButtonSize;
				p_buttonVisuals.transform.localRotation = Quaternion.identity;
			}
			else
			{
				Debug.LogWarning("LE_ObjectSnapPoint: InstatiateSnapSystem: p_buttonVisuals is null! The snap button will be invisible for the user!");
			}
			return m_snapSysInstance;
		}

		public static void DestroySnapSystem(S_SnapToObject p_system)
		{
			if (p_system != null)
			{
				GameObject.Destroy(p_system);
				GameObject.Destroy(p_system.GetComponent<SphereCollider>());
				Transform[] transforms = p_system.GetComponentsInChildren<Transform>(true);
				for (int i = 0; i < transforms.Length; i++)
				{
					if (transforms[i].name == BUTTON_VISUALS_NAME)
					{
						GameObject.Destroy(transforms[i].gameObject);
					}
				}
			}
		}
	}
}
