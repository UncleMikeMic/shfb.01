using UnityEngine;
using System.Collections;

public class Paddle : MonoBehaviour
{
    public float freedom = 22.5F;
    public bool canMove;

    [Header ("bumper stuff")]
    public GameManager gm;
    public int paddlePoints = 1;
    public int paddleSound = 0;

    [Header("lerpScale")]
    public bool lerpScaleOn = true;
    Vector3 originalScale;
    private Vector3 newScale;
    public float scaleMultiplier = 1.1f;
    private bool repeatable;
    public float lerpSpeed = 10f;
    public float lerpDuration = .5f;

    void OnEnable()
    {
        originalScale = transform.localScale;
        newScale = originalScale * scaleMultiplier;
    }

    private void Start()
    {
        //connect to the game manager script and store as variable "gm"
        gm = GameObject.FindWithTag("gm").GetComponent<GameManager>();
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
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("ball"))
        {
            //sfx
            AudioController.Play("paddle" + paddleSound);

            if (lerpScaleOn == true)
            {
                StartCoroutine("LerpScale");
            }

            //raise score and coins in the game manager by set amount for this object
            gm.score += paddlePoints;
            gm.coins += paddlePoints;

            //updates scoreboard
            gm.IncrementScore();
        }
    }

    IEnumerator LerpScale()
    {
        repeatable = true;
        while (repeatable)
        {
            yield return RepeatLerp(originalScale, newScale, lerpDuration);
            yield return RepeatLerp(newScale, originalScale, lerpDuration);
            repeatable = false;
        }
    }

    public IEnumerator RepeatLerp(Vector3 a, Vector3 b, float time)
    {
        float i = 0.0f;
        float rate = (1.0f / time) * lerpSpeed;
        while (i < 1.0f)
        {
            i += Time.deltaTime * rate;
            transform.localScale = Vector3.Lerp(a, b, i);
            yield return null;
        }
    }
}