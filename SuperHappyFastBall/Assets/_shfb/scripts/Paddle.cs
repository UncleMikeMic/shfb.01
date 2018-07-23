using UnityEngine;
using System.Collections;

public class Paddle : MonoBehaviour
{
    public float freedom = 22.5F;
    public bool canMove;

    private void Start()
    {
        canMove = true;
    }

    public void PaddleCanMove()
    {
        canMove = !canMove;
    }


    void Update()
    {
        //Check if left mousebutton is down or a finger is touching the screen
        if (Input.GetMouseButton(0) && canMove == true)
        {
            //Create a ray from camera to playfield
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane p = new Plane(Vector3.up, transform.position);

            //Shoot the ray to know where the click/tap found place in 3D
            float distance;
            if (p.Raycast(mouseRay, out distance))
            {
                //Use current position as starting point
                Vector3 position = transform.position;
                //The player can only move the paddle in the x axis, so don't use the y and z
                position.x = mouseRay.GetPoint(distance).x; //GetPoint gives us the position in 3D
                //Apply the new position
                transform.position = position;
            }
            else
            {
                //The ray missed
            }

            //Make sure the paddle stays inside the level
            Vector3 limitedPosition = transform.position;
            if (Mathf.Abs(limitedPosition.x) > freedom)
            {
                //Paddle is outside the level so move it back in
                limitedPosition.x = Mathf.Clamp(transform.position.x, -freedom, freedom);
                transform.position = limitedPosition;
            }
        }

        
        
    }
}