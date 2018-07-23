using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class followMouse : MonoBehaviour {


 
    private float actualDistance;
    public bool canMove;

 

    

    // Use this for initialization
    void Start () {

    canMove = true;

    if (canMove == true)
        {
            Vector3 toObjectVector = transform.position - Camera.main.transform.position;
            Vector3 linearDistanceVector = Vector3.Project(toObjectVector, Camera.main.transform.forward);
            actualDistance = linearDistanceVector.magnitude;
        }
     

    }

    public void PaddleCanMove()
    {
        canMove = !canMove;
    }

    // Update is called once per frame
    void Update () {

       


        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = actualDistance;
      
        transform.position = Camera.main.ScreenToWorldPoint(mousePosition);
		
	}
}
