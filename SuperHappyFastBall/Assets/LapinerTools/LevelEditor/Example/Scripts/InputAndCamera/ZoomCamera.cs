using UnityEngine;
using System.Collections;

public class ZoomCamera : MonoBehaviour
{
	public Transform origin; // What is considered the origin to the camera
	public float zoom = 0f;
	public float zoomMin = -5f;
	public float zoomMax = 5f;
	public float seekTime = 1f;
	public bool smoothZoomIn = false;

	private Vector3 defaultLocalPosition;
	private float currentZoom;
	private float targetZoom;
	private float zoomVelocity;

	private void Start()
	{
		// The default position is the position that is set in the editor
		defaultLocalPosition = transform.localPosition;
		
		// Default the current zoom to what was set in the editor 
		currentZoom = zoom;
	}
	
	private void Update()
	{
		// The zoom set externally must still be within the min-max range
		zoom = Mathf.Clamp(zoom, zoomMin, zoomMax);
		
		// Only collide with non-Player (8) layers
		var layerMask = ~((1 << 8) | (1 << 2));
		
		RaycastHit hit;
		Vector3 start = origin.position;
		Vector3 zoomedPosition = defaultLocalPosition + transform.parent.InverseTransformDirection(transform.forward * zoom);
		Vector3 end = transform.parent.TransformPoint(zoomedPosition);
		
		// Cast a line from the origin transform to the camera and find out if we hit anything in-between
		if (Physics.Linecast(start, end, out hit, layerMask)) 
		{
			// We hit something, so translate this to a zoom value
			var position = hit.point + transform.TransformDirection( Vector3.forward );
			var difference = position - transform.parent.TransformPoint( defaultLocalPosition );
			targetZoom = difference.magnitude;
		}
		else
			// We didn't hit anything, so the camera should use the zoom set externally
			targetZoom = zoom;
		
		// Clamp target zoom to our min-max range
		targetZoom = Mathf.Clamp( targetZoom, zoomMin, zoomMax );
		
		if ( !smoothZoomIn && ( targetZoom - currentZoom ) > 0 )
		{
			// Snap the current zoom to our target if it is closer. This is useful if
			// some object is between the camera and the origin
			currentZoom = targetZoom;
		}
		else
		{
			// Smoothly seek towards our target zoom value
			currentZoom = Mathf.SmoothDamp(currentZoom, targetZoom, ref zoomVelocity, seekTime);
		}
		
		// Set the position of the camera
		zoomedPosition = defaultLocalPosition + transform.parent.InverseTransformDirection(transform.forward * currentZoom);
		transform.localPosition = zoomedPosition;
	}
}
