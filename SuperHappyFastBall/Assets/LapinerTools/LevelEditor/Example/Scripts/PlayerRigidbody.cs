using UnityEngine;
using System.Collections;

namespace LE_LevelEditor.Example
{
	public class PlayerRigidbody : MonoBehaviour
	{
		void Awake ()
		{
			Collider collider = GetComponent<Collider>();
			Collider[] colliders = transform.root.GetComponentsInChildren<Collider>();
			for (int i = 0; i < colliders.Length; i++)
			{
				if (collider != colliders[i])
				{
					Physics.IgnoreCollision(collider, colliders[i]);
				}
			}
		}
	}
}
