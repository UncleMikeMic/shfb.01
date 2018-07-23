using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    public Transform[] spawnLocations;
    public GameObject[] whatToSpawnPrefab;
    public GameObject[] whatToSpawnClone;
    public GameObject spawnParticles;
    public bool hasSpawnParticles = false;
    public float spawnTime;
    private float timeLeft;
    




    void Start()
    {
        
        spawnThing();
        timeLeft = spawnTime;
    }
    void Update()
    {
        timeLeft -= Time.deltaTime;
        if (timeLeft < 0)
        {
            spawnThing();
            timeLeft = spawnTime;

        }
        
    }

    public void spawnThing()
    {
        whatToSpawnClone[0] = Instantiate(whatToSpawnPrefab[0], spawnLocations[0].transform.position, Quaternion.Euler(0, 0, 0)) as GameObject;

        if (hasSpawnParticles == true)
        {
            //particles
            Instantiate(spawnParticles, spawnLocations[0].position, spawnLocations[0].rotation);
        }




    }
   
}
