using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class randomBrickSpawner : MonoBehaviour {

    public Transform brickPrefab;
    public int howManyBricks = 200;
    public float xMin = -25f;
    public float xMax = 25f;
    public float yMin = 2f;
    public float yMax = 9f;
    public float zMin = -20f;
    public float zMax = 25f;

    void Awake()
    {
        for (int i = 0; i < howManyBricks; i++)
        {
            Vector3 position = new Vector3(Random.Range(xMin, xMax), Random.Range(yMin, yMax), Random.Range(zMin, zMax));
            Instantiate(brickPrefab, position, Random.rotation);
        }
    }
  
}
