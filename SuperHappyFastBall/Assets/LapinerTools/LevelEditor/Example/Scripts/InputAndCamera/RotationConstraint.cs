using UnityEngine;
using System.Collections;

public class RotationConstraint : MonoBehaviour
{
	public enum EConstraintAxis { X = 0, Y = 1, Z = 2 }
	
	public EConstraintAxis axis; 			// Rotation around this axis is constrained
	public float min;						// Relative value in degrees
	public float max;						// Relative value in degrees
	private Vector3 rotateAround;
	private Quaternion minQuaternion;
	private Quaternion maxQuaternion;
	private float range;

	private void Start()
	{
		// Set the axis that we will rotate around
		switch ( axis )
		{
			case EConstraintAxis.X:
				rotateAround = Vector3.right;
				break;
				
			case EConstraintAxis.Y:
				rotateAround = Vector3.up;
				break;
				
			case EConstraintAxis.Z:
				rotateAround = Vector3.forward;
				break;
		}
		
		// Set the min and max rotations in quaternion space
		Quaternion axisRotation = Quaternion.AngleAxis(transform.localRotation.eulerAngles[(int)axis], rotateAround);
		minQuaternion = axisRotation * Quaternion.AngleAxis(min, rotateAround);
		maxQuaternion = axisRotation * Quaternion.AngleAxis(max, rotateAround);
		range = max - min;
	}

	// We use LateUpdate to grab the rotation from the Transform after all Updates from
	// other scripts have occured
	private void LateUpdate()
	{
		// We use quaternions here, so we don't have to adjust for euler angle range [ 0, 360 ]
		Quaternion localRotation = transform.localRotation;
		Quaternion axisRotation = Quaternion.AngleAxis(localRotation.eulerAngles[(int)axis], rotateAround);
		float angleFromMin = Quaternion.Angle(axisRotation, minQuaternion);
		float angleFromMax = Quaternion.Angle(axisRotation, maxQuaternion);
		
		if (angleFromMin <= range && angleFromMax <= range)
		{
			return; // within range
		}
		else
		{		
			// Let's keep the current rotations around other axes and only
			// correct the axis that has fallen out of range.
			var euler = localRotation.eulerAngles;			
			if ( angleFromMin > angleFromMax )
				euler[(int)axis] = maxQuaternion.eulerAngles[(int)axis];
			else
				euler[(int)axis] = minQuaternion.eulerAngles[(int)axis];
			
			transform.localEulerAngles = euler;		
		}
	}
}
