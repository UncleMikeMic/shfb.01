using UnityEngine;
using UnityEditor;
using System.Collections;

namespace LE_LevelEditor.Core
{
	[CustomPropertyDrawer(typeof(LE_EnumFlagsAttribute))]
	public class LE_EnumFlagsPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect p_position, SerializedProperty p_property, GUIContent p_label)
		{
			p_property.intValue = EditorGUI.MaskField(p_position, new GUIContent(p_label.text, ((LE_EnumFlagsAttribute)attribute).m_tooltip), p_property.intValue, p_property.enumNames);
		}
	}
}