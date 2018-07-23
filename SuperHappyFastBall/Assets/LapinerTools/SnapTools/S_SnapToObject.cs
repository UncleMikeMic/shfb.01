using UnityEngine;
using System.Collections;
using MyUtility;

namespace S_SnapTools
{
	public class S_SnapToObject : MonoBehaviour
	{
		// global callbacks triggered by all instances (e.g. use to make same changes to all preview instances)
		public static System.EventHandler<S_SnapToObjectBeforePlacementEventArgs> OnGlobalBeforeObjectSnapped;
		public static System.EventHandler<S_SnapToObjectEventArgs> OnGlobalAfterObjectSnapped;
		public static System.EventHandler<S_SnapToObjectEventArgs> OnGlobalPreviewObjectInstantiated;

		private static S_SnapToObject s_openSelection = null;

		// local per instance callbacks (e.g. use to make changes to single objects)
		public System.EventHandler<S_SnapToObjectBeforePlacementEventArgs> OnBeforeObjectSnapped;
		public System.EventHandler<S_SnapToObjectEventArgs> OnAfterObjectSnapped;
		public System.EventHandler<S_SnapToObjectEventArgs> OnPreviewObjectInstantiated;

		private const float ZOOM_OUT_TIME = 0.4f;
		private const float ZOOM_IN_TIME = 0.2f;
		private readonly Vector3 MIN_SCALE = Vector3.one*0.0001f;
		private enum EAnimationState { ZOOM_IN, ZOOM_OUT, IDLE }

		[SerializeField]
		private float PREVIEW_RELATIVE_SCREEN_DISTANCE = 0.3f;
		[SerializeField]
		private float PREVIEW_RELATIVE_SCREEN_CLICK_AREA = 0.2f;
		[SerializeField]
		private float PREVIEW_RELATIVE_MOVE_TO_CAMERA_DISTANCE = 0.5f;

		[SerializeField]
		private S_SnapToObjectPrefab[] m_prefabs = new S_SnapToObjectPrefab[1];
		public S_SnapToObjectPrefab[] Prefabs
		{
			get{ return m_prefabs; }
			set{ m_prefabs = value;}
		}

		[SerializeField]
		private bool m_isDeactivatedAfterSnap = false;
		public bool IsDeactivatedAfterSnap
		{
			get{ return m_isDeactivatedAfterSnap; }
			set{ m_isDeactivatedAfterSnap = value;}
		}
		[SerializeField]
		private bool m_isDestroyedAfterSnap = true;
		public bool IsDestroyedAfterSnap
		{
			get{ return m_isDestroyedAfterSnap; }
			set{ m_isDestroyedAfterSnap = value;}
		}

		[SerializeField]
		private bool m_isSetNameToResourcePath = true;
		public bool IsSetNameToResourcePath
		{
			get{ return m_isSetNameToResourcePath; }
			set{ m_isSetNameToResourcePath = value;}
		}

		[SerializeField]
		private bool m_isDrawUI = true;
		public bool IsDrawUI
		{
			get{ return m_isDrawUI; }
			set{ m_isDrawUI = value;}
		}

		[SerializeField]
		private Material m_uiMaterialLine = null;
		public Material UImaterialLine
		{
			get{ return m_uiMaterialLine; }
			set{ m_uiMaterialLine = value;}
		}

		[SerializeField]
		private Material m_uiMaterialFill = null;
		public Material UImaterialFill
		{
			get{ return m_uiMaterialFill; }
			set{ m_uiMaterialFill = value;}
		}

		private int m_snapCounter = 0;
		public int SnapCounter { get{ return m_snapCounter; } }

		private bool m_isSelectionOpen = false;
		public bool IsPrefabSelectionOpen { get{ return m_isSelectionOpen; } }

		private EAnimationState m_animState = EAnimationState.IDLE;
		private float m_zoomAnimationTime = -1f;

		private Transform m_previewRoot = null;
		public Transform PreviewRoot
		{
			get
			{
				if (m_previewRoot == null)
				{
					m_previewRoot = new GameObject("S_SnapToObject Preview Root").transform;
					m_previewRoot.position = transform.position;
				}
				return m_previewRoot;
			}
		}

		private Camera m_cam = null;
		private Camera Cam
		{
			get
			{
				if (m_cam == null)
				{
					m_cam = Camera.main;
					if (m_cam == null)
					{
						Debug.LogError("S_SnapToObject: could not find main camera!");
					}
				}
				return m_cam;
			}
		}

		public S_SnapToObject()
		{
			if (m_prefabs != null && m_prefabs.Length == 1)
			{
				m_prefabs[0] = new S_SnapToObjectPrefab();
			}
		}

		public void IncSnapCounter()
		{
			// disable self when done
			if (m_isDeactivatedAfterSnap)
			{
				gameObject.SetActive(false);
			}
			// destroy self when done
			else if (m_isDestroyedAfterSnap)
			{
				Destroy(gameObject);
			}
			m_snapCounter++;

		}

		public void DecSnapCounter ()
		{
			if (m_snapCounter > 0)
			{
				m_snapCounter--;
				if (m_isDeactivatedAfterSnap && m_snapCounter == 0)
				{
					gameObject.SetActive(true);
				}
			}
		}

		public void OpenSelection()
		{
			if (s_openSelection != null)
			{
				s_openSelection.CloseSelection();
			}
			s_openSelection = this;
			m_isSelectionOpen = true;
			m_animState = EAnimationState.ZOOM_OUT;
			m_zoomAnimationTime = 0f;
			// create snap prefab previews
			for (int i=0; i<m_prefabs.Length; i++)
			{
				CreatePrefabPreview(m_prefabs[i]);
			}
		}

		public void CloseSelection()
		{
			if (s_openSelection == this)
			{
				s_openSelection = null;
			}
			m_isSelectionOpen = false;
			// hide shown snap prefab previews
			m_animState = EAnimationState.ZOOM_IN;
			m_zoomAnimationTime = 0f;
		}

		public void PlacePrefab(S_SnapToObjectPrefab p_prefab)
		{
			// instantly close selection
			if (m_isSelectionOpen)
			{
				DestroyAllPreviews();
				if (s_openSelection == this)
				{
					s_openSelection = null;
				}
				m_isSelectionOpen = false;
				m_animState = EAnimationState.IDLE;
				m_zoomAnimationTime = -1f;
			}
			// create game object
			Object resource = Resources.Load(p_prefab.PrefabResourcePath);
			if (resource != null)
			{
				GameObject go = (GameObject)Instantiate(resource);
				if (m_isSetNameToResourcePath)
				{
					go.name = p_prefab.PrefabResourcePath;
				}
				go.transform.localRotation = transform.rotation;
				go.transform.Rotate(p_prefab.LocalEulerRotation);
				go.transform.localScale = Vector3.Scale(p_prefab.LocalScale, transform.lossyScale);
				go.transform.position = transform.position + go.transform.TransformPoint(p_prefab.LocalPosition);
				IncSnapCounter();
				// notify event listeners
				if (OnGlobalAfterObjectSnapped != null) { OnGlobalAfterObjectSnapped(this, new S_SnapToObjectEventArgs(this, go)); }
				if (OnAfterObjectSnapped != null) { OnAfterObjectSnapped(this, new S_SnapToObjectEventArgs(this, go)); }
			}
			else
			{
				Debug.LogError("S_SnapToObject: PlacePrefabInternal: could not find prefab in resources at '" + p_prefab.PrefabResourcePath + "'!");
			}
		}

		private void Start()
		{
			UtilityClickTouchDetector clickTouch = gameObject.AddComponent<UtilityClickTouchDetector>();
			clickTouch.m_onClick += OnClicked;
		}

		private void Update()
		{
			// update preview root
			if (IsPrefabSelectionOpen)
			{
				PreviewRoot.position = transform.position;
			}
			else if (m_previewRoot != null && m_animState == EAnimationState.IDLE)
			{
				Destroy(m_previewRoot.gameObject);
				m_previewRoot = null;
			}
			// handle zoom in and zoom out animation
			if (m_animState == EAnimationState.ZOOM_OUT)
			{
				float fade = UpdateAnimationTimeFadeState(ZOOM_OUT_TIME);
				UpdateZoomAnimation(fade);
			}
			else if (m_animState == EAnimationState.ZOOM_IN)
			{
				float fade = UpdateAnimationTimeFadeState(ZOOM_IN_TIME);
				if (fade < 1f)
				{
					UpdateZoomAnimation(1f - fade);
				}
				else
				{
					DestroyAllPreviews();
				}
			}
			else if (IsPrefabSelectionOpen)
			{
				UpdateZoomAnimation(1f);
			}
		}

		private void DestroyAllPreviews()
		{
			for (int i=0; i<m_prefabs.Length; i++)
			{
				Destroy(m_prefabs[i].m_currentInstance);
			}
		}

		private float UpdateAnimationTimeFadeState(float p_animTime)
		{
			m_zoomAnimationTime += Time.deltaTime;
			float fade;
			if (m_zoomAnimationTime >= p_animTime)
			{
				// zoom out animation is finished
				m_animState = EAnimationState.IDLE;
				m_zoomAnimationTime = -1f;
				fade = 1f;
			}
			else
			{
				fade = m_zoomAnimationTime / p_animTime;
			}
			return fade;
		}

		private void UpdateZoomAnimation(float p_fade)
		{
			if (Cam != null)
			{
				Vector3 screenPosRoot = Cam.WorldToScreenPoint(transform.position);
				float screenAngleStepSize = 360f / (float)m_prefabs.Length;
				for (int i=0; i<m_prefabs.Length; i++)
				{
					GameObject go = m_prefabs[i].m_currentInstance;
					if (go != null && m_prefabs[i].m_currentInstancePreviewScript != null)
					{
						Vector3 targetPosition = (float)Screen.height*PREVIEW_RELATIVE_SCREEN_DISTANCE*new Vector3(
							Mathf.Cos(Mathf.Deg2Rad*screenAngleStepSize*(float)i*p_fade),
							Mathf.Sin(Mathf.Deg2Rad*screenAngleStepSize*(float)i*p_fade),
							0f);
						targetPosition = Cam.ScreenToWorldPoint(screenPosRoot+targetPosition);
						targetPosition = targetPosition*(1f-PREVIEW_RELATIVE_MOVE_TO_CAMERA_DISTANCE) + Cam.transform.position*PREVIEW_RELATIVE_MOVE_TO_CAMERA_DISTANCE;
						go.transform.localScale = Vector3.Lerp(MIN_SCALE, m_prefabs[i].PreviewScale*m_prefabs[i].m_currentInstancePreviewScript.WorldRadius, p_fade);
						go.transform.position = Vector3.Lerp(transform.position, targetPosition, p_fade);

						Quaternion rotation = Quaternion.LookRotation(Cam.transform.position-go.transform.position, Vector3.up);
						rotation *= Quaternion.Euler(m_prefabs[i].PreviewEulerRotation);
						go.transform.rotation = rotation;
					}
				}
			}
		}

		private void OnClicked()
		{
			if (!m_isSelectionOpen)
			{
				OpenSelection();
			}
			else
			{
				CloseSelection();
			}
		}

		private void OnDisable()
		{
			if (m_previewRoot != null)
			{
				Destroy(m_previewRoot.gameObject);
				m_previewRoot = null;
			}
			DestroyAllPreviews();
			if (s_openSelection == this)
			{
				s_openSelection = null;
			}
			m_isSelectionOpen = false;
			m_animState = EAnimationState.IDLE;
			m_zoomAnimationTime = -1f;
		}

		private void CreatePrefabPreview(S_SnapToObjectPrefab p_prefab)
		{
			if (p_prefab.m_currentInstance != null)
			{
				Debug.LogWarning("S_SnapToObject: CreatePrefabPreview: there was already an instance of this prefab '" + p_prefab.m_currentInstance.name + "'! Old instance will be destroyed...");
				Destroy(p_prefab.m_currentInstance);
			}
			if (p_prefab.PrefabResourcePath != null)
			{
				Object resource = Resources.Load(p_prefab.PrefabResourcePath);
				if (resource != null)
				{
					p_prefab.m_currentInstance = (GameObject)Instantiate(resource);
					p_prefab.m_currentInstance.transform.parent = PreviewRoot;
					p_prefab.m_currentInstance.transform.localPosition = Vector3.zero;
					p_prefab.m_currentInstance.transform.localScale = Vector3.one;
					// disable colliders if allowed, they could block mouse events otherwise
					if (p_prefab.IsCollidersDisabledInPreview)
					{
						Collider[] colliders = p_prefab.m_currentInstance.GetComponentsInChildren<Collider>();
						for (int i=0; i<colliders.Length; i++)
						{
							colliders[i].enabled = false;
						}
					}
					// scale down prefab for zoom out animation
					p_prefab.m_currentInstance.transform.localScale = MIN_SCALE;
					// add preview handling script
					p_prefab.m_currentInstancePreviewScript = p_prefab.m_currentInstance.AddComponent<S_SnapToObjectPreview>();
					p_prefab.m_currentInstancePreviewScript.Init(m_isDrawUI, PREVIEW_RELATIVE_SCREEN_CLICK_AREA, ()=>
					{
						PlacePrefabInternal(p_prefab);
					}, m_uiMaterialLine, m_uiMaterialFill);
					// notify listeners
					if (OnGlobalPreviewObjectInstantiated != null) { OnGlobalPreviewObjectInstantiated(this, new S_SnapToObjectEventArgs(this, p_prefab.m_currentInstance)); }
					if (OnPreviewObjectInstantiated != null) { OnPreviewObjectInstantiated(this, new S_SnapToObjectEventArgs(this, p_prefab.m_currentInstance)); }
				}
				else
				{
					Debug.LogError("S_SnapToObject: CreatePrefabPreview: could not find prefab in resources at '" + p_prefab.PrefabResourcePath + "'!");
				}
			}
			else
			{
				Debug.LogError("S_SnapToObject: CreatePrefabPreview: no prefab assigned!");
			}
		}

		private void PlacePrefabInternal(S_SnapToObjectPrefab p_prefab)
		{
			S_SnapToObjectBeforePlacementEventArgs beforePlacementArgs = new S_SnapToObjectBeforePlacementEventArgs(this, p_prefab);
			if (OnGlobalBeforeObjectSnapped != null) { OnGlobalBeforeObjectSnapped(this, beforePlacementArgs); }
			if (OnBeforeObjectSnapped != null) { OnBeforeObjectSnapped(this, beforePlacementArgs); }

			// if prefab placement is delayed, then the entity, which has changed beforePlacementArgs.IsDelayedPlacePrefab to true
			// is responsible for calling PlacePrefab later. This might be needed if further popups or UI is shown before placement.
			if (!beforePlacementArgs.IsDelayedPlacePrefab)
			{
				PlacePrefab(p_prefab);
			}
		}
	}
}
