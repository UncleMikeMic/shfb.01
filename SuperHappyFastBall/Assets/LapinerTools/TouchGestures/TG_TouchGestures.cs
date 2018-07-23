using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace TG_TouchGesture
{
	public class TG_TouchGestures : MonoBehaviour
	{
		private List<TG_TouchGestureBase> m_touchGestures = new List<TG_TouchGestureBase>();

		private static TG_TouchGestures s_instance = null;
		public static bool IsInstanceSet { get{ return s_instance != null; } }
		public static TG_TouchGestures Instance
		{
			get
			{
				if (s_instance == null)
				{
					GameObject go = new GameObject("TG_TouchGestures");
					GameObject.DontDestroyOnLoad(go);
					s_instance = go.AddComponent<TG_TouchGestures>();
				}
				return s_instance;
			}
		}

		public EventHandler<TG_TouchGestureEventArgs> OnGestureDetected;
		
		public void EnableGesture(TG_ETouchGestureType p_type)
		{
			foreach (TG_TouchGestureBase gesture in m_touchGestures)
			{
				if (gesture.Type == p_type)
				{
					return;
				}
			}
			switch (p_type)
			{
				case TG_ETouchGestureType.TAP:
					m_touchGestures.Add(new TG_TouchGestureTap());
					break;
				case TG_ETouchGestureType.PRESS_1_FINGER:
					m_touchGestures.Add(new TG_TouchGesturePress(TG_ETouchGestureType.PRESS_1_FINGER, 1, 0.1f));
					break;
				case TG_ETouchGestureType.PRESS_2_FINGER:
					m_touchGestures.Add(new TG_TouchGesturePress(TG_ETouchGestureType.PRESS_2_FINGER, 2, 0f));
					break;
				case TG_ETouchGestureType.PRESS_3_FINGER:
					m_touchGestures.Add(new TG_TouchGesturePress(TG_ETouchGestureType.PRESS_3_FINGER, 3, 0f));
					break;
				case TG_ETouchGestureType.PRESS_4_FINGER:
					m_touchGestures.Add(new TG_TouchGesturePress(TG_ETouchGestureType.PRESS_4_FINGER, 4, 0f));
					break;
				case TG_ETouchGestureType.ZOOM:
					m_touchGestures.Add(new TG_TouchGestureZoom());
					break;
				default:
					Debug.LogError("TG_TouchGestures: EnableGesture: unknown gesture type '" + p_type + "'!");
					break;
			}
		}
		
		private void Start()
		{
		
		}
		
		private void Update()
		{
			// no need to update if no one is listening
			if (OnGestureDetected != null)
			{
				foreach (TG_TouchGestureBase gesture in m_touchGestures)
				{
					TG_TouchGestureEventArgs detectedGestureEvent = gesture.Update();
					if (detectedGestureEvent != null)
					{
						OnGestureDetected(gesture, detectedGestureEvent);
					}
				}
			}
		}
	}
}
