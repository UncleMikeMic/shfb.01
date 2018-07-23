using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainFinder : MonoBehaviour {

    public PhysicMaterial bouncy;
    public GameObject terrain;
    public bool matSet = false;
    
        // Use this for initialization
    void Update () {

        if (matSet == false)
        {
            terrain = GameObject.Find("Terrain");
            Collider terColl = terrain.GetComponent<Collider>();
            terColl.material = bouncy;
            matSet = true;
        }
       
            }
	
}
