using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LE_LevelEditor.Extensions
{
	public class LE_ExtensionDelegate<T>
	{
		private int m_priority;

		private T m_delegate;
		public T Delegate
		{
			get
			{
				if (EqualityComparer<T>.Default.Equals(m_delegate, default(T)))
				{
					Debug.LogError("LE_ExtensionMethod: get Delegate: extension delegate is missing for '" + typeof(T) + "'!");
				}
				return m_delegate;
			}
		}

		public void SetDelegate(int p_priority, T p_delegate)
		{
			if (p_delegate == null || m_delegate == null || m_priority < p_priority)
			{
				m_delegate = p_delegate;
				m_priority = p_priority;
			}
		}
	}
}
