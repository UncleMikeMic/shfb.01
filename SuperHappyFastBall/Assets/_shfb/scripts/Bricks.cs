using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bricks : MonoBehaviour {

    public GameManager gm;
    public int brickPoints = 1;
    public GameObject brickParticles;
    public int brickSound = 0;

   

    private void Start()
    {
        gm = GameObject.FindWithTag("gm").GetComponent<GameManager>();
    }
    void OnCollisionExit (Collision col)
    {
        if (col.gameObject.CompareTag("ball"))
        {
            //sfx
            AudioController.Play("brick" + brickSound);

            //create particles and destroy brick
            Instantiate(brickParticles, transform.position, Quaternion.identity);
            Destroy(gameObject);

            //update score and coins in game manager
            gm.score += brickPoints;
            gm.coins += brickPoints;
            gm.IncrementScore();
            gm.DecrementBricks();
        }

        

    }
}
