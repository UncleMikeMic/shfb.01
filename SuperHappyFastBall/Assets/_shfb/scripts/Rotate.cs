using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour {

    public float rotationSpeedX = 0f;
    public float rotationSpeedY = 0f;
    public float rotationSpeedZ = 0f;

    public bool randomSpeed = false;

    public float xMax = 0f;
    public float yMax = 0f;
    public float zMax = 0f;

    void Awake ()
    {
        if(randomSpeed == true)
        {
            rotationSpeedX = Random.Range(-xMax, xMax);
            rotationSpeedY = Random.Range(-yMax, yMax);
            rotationSpeedZ = Random.Range(-zMax, zMax);
        }
    }
    // Update is called once per frame
    void Update () {
        transform.Rotate(Time.deltaTime * rotationSpeedX, Time.deltaTime * rotationSpeedY, Time.deltaTime * rotationSpeedZ);
    }
}
