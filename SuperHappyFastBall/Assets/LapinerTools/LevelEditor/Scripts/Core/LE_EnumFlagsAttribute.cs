using UnityEngine;
using System.Collections;

namespace LE_LevelEditor.Core
{
	public class LE_EnumFlagsAttribute : PropertyAttribute
	{
		public readonly string m_tooltip;
		public LE_EnumFlagsAttribute(string p_tooltip)
		{
			m_tooltip = p_tooltip;
		}
	}
}
