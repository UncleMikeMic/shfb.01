using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveOverTime : MonoBehaviour {

    public float speedX = 0.0f;
    public float speedY = 0.0f;
    public float speedZ = 0.0f;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.Translate(speedX * Time.deltaTime, speedY * Time.deltaTime, speedZ * Time.deltaTime);
	}
}
