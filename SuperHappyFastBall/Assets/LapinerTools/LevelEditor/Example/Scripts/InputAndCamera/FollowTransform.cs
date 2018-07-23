using UnityEngine;
using System.Collections;

public class FollowTransform : MonoBehaviour
{
	public Transform targetTransform;		// Transform to follow
	public bool faceForward = false;		// Match forward vector?
	
	private void Update()
	{
		if (targetTransform != null)
		{
			transform.position = targetTransform.position;
			
			if (faceForward)
			{
				transform.forward = targetTransform.forward;
			}
		}
	}
}
