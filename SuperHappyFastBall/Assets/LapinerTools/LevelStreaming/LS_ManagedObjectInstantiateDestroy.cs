using UnityEngine;
using System.Collections;

namespace LS_LevelStreaming
{
	public class LS_ManagedObjectInstantiateDestroy : LS_ManagedObjectBase
	{
		protected readonly string m_resourcePath;
		protected Vector3 m_position;
		protected Quaternion m_rotation;
		protected Vector3 m_scale;
		protected readonly LS_ITriggerByUpdatedPosition m_updatedTrigger;
		protected readonly System.Action<int, GameObject> m_onInstantiated;
		protected readonly System.Action<int, GameObject> m_onDestroyed;

		protected GameObject m_instance;
		public GameObject Instance { get{ return m_instance; } }

		public LS_ManagedObjectInstantiateDestroy(string p_resourcePath, Vector3 p_position, Quaternion p_rotation, Vector3 p_scale, LS_ITriggerByUpdatedPosition p_trigger, System.Action<int, GameObject> p_onInstantiated, System.Action<int, GameObject> p_onDestroyed)
			: base(p_trigger)
		{
			m_updatedTrigger = p_trigger;
			m_resourcePath = p_resourcePath;
			m_position = p_position;
			m_rotation = p_rotation;
			m_scale = p_scale;
			m_onInstantiated = p_onInstantiated;
			m_onDestroyed = p_onDestroyed;

			m_isVisible = false;
		}

		public override void Update()
		{
			m_updatedTrigger.Update(m_instance==null?m_position:m_instance.transform.position);
			base.Update();
			// check if object was destroyed from outside
			if (m_isVisible && m_instance == null)
			{
				LS_LevelStreamingSceneManager.Instance.RemoveManagedObjectAtFrameEnd(ID);
			}
		}

		protected override void Hide ()
		{
			if (m_instance != null)
			{
				m_position = m_instance.transform.position;
				m_rotation = m_instance.transform.rotation;
				m_scale = m_instance.transform.localScale;
				GameObject.Destroy(m_instance);
				if (m_onDestroyed != null)
				{
					m_onDestroyed(ID, m_instance);
				}
				m_instance = null;
			}
			else
			{
				Debug.LogError("LS_ManagedObjectInstantiateDestroy: Hide: object instance is already destroyed!");
			}
		}

		protected override void Show ()
		{
			if (m_instance != null)
			{
				GameObject.Destroy(m_instance);
				Debug.LogError("LS_ManagedObjectInstantiateDestroy: Show: object instance was already instantiated!");
			}
			Object resource = Resources.Load(m_resourcePath);
			if (resource != null)
			{
				m_instance = (GameObject)GameObject.Instantiate(resource, m_position, m_rotation);
				m_instance.transform.localScale = m_scale;
				if (m_onInstantiated != null)
				{
					m_onInstantiated(ID, m_instance);
				}
			}
			else
			{
				Debug.LogError("LS_ManagedObjectInstantiateDestroy: Show: object prefab not found in resources at '" + m_resourcePath + "'!");
			}
		}
	}
}
