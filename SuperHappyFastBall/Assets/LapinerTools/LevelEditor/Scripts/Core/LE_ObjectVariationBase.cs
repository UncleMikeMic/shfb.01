using UnityEngine;
using System.Collections;

namespace LE_LevelEditor.Core
{
	[System.Serializable]
	public abstract class LE_ObjectVariationBase
	{
		/// <summary>
		/// Returns the name of this variations. It will be shown to the player/user.
		/// </summary>
		public abstract string GetName();

		/// <summary>
		/// Apply this variation to the object passed as parameter.
		/// Called when the user selects a variation.
		/// </summary>
		public abstract void Apply(LE_Object p_object);
	}
}
