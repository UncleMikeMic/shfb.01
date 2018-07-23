using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Death : MonoBehaviour {

    public GameManager gm;
    public GameObject deathParticles;
  
   

    private void Start()
    {
        gm = GameObject.FindWithTag("gm").GetComponent<GameManager>();
    }
    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("ball"))
        {
            gm.DecrementBalls();
            Destroy(col.gameObject);
            Instantiate(deathParticles, col.transform.position, Quaternion.identity);

            //sfx
            AudioController.Play("ballDeath");
        }
       
    }
}
