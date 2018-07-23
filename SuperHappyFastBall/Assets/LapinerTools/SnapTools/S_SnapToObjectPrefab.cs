using UnityEngine;
using System.Collections;

namespace S_SnapTools
{
	[System.Serializable]
	public class S_SnapToObjectPrefab
	{
		[SerializeField]
		private bool m_isCollidersDisabledInPreview = true;
		public bool IsCollidersDisabledInPreview
		{
			get{ return m_isCollidersDisabledInPreview; }
			set{ m_isCollidersDisabledInPreview = value;}
		}

		[SerializeField]
		private Vector3 m_previewScale = Vector3.one*0.25f;
		public Vector3 PreviewScale
		{
			get{ return m_previewScale; }
			set{ m_previewScale = value;}
		}

		[SerializeField]
		private Vector3 m_previewEulerRotation = Vector3.zero;
		public Vector3 PreviewEulerRotation
		{
			get{ return m_previewEulerRotation; }
			set{ m_previewEulerRotation = value;}
		}

		[SerializeField]
		private Vector3 m_localPosition = Vector3.zero;
		public Vector3 LocalPosition
		{
			get{ return m_localPosition; }
			set{ m_localPosition = value;}
		}

		[SerializeField]
		private Vector3 m_localEulerRotation = Vector3.zero;
		public Vector3 LocalEulerRotation
		{
			get{ return m_localEulerRotation; }
			set{ m_localEulerRotation = value;}
		}

		[SerializeField]
		private Vector3 m_localScale = Vector3.one;
		public Vector3 LocalScale
		{
			get{ return m_localScale; }
			set{ m_localScale = value;}
		}

		[SerializeField]
		private string m_prefabResourcePath;
		public string PrefabResourcePath
		{
			get{ return m_prefabResourcePath; }
			set{ m_prefabResourcePath = value;}
		}

		[System.NonSerialized]
		public GameObject m_currentInstance = null;
		[System.NonSerialized]
		public S_SnapToObjectPreview m_currentInstancePreviewScript = null;
	}
}