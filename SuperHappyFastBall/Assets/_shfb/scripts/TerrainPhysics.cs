using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LE_LevelEditor.Events;

public class TerrainPhysics : MonoBehaviour {

    public PhysicMaterial bouncy;

    private void Awake()
    {
        //This event is fired when the terrain was created with the "Create Terrain" button in the level editor. You can get the terrain GameObject from the event arguments. 

        LE_EventInterface.OnTerrainCreated += OnTerrainCreated;
    }

    private void OnTerrainCreated(object p_sender, LE_TerrainCreatedEvent p_args)
    {

     //   p_args.collider.material = bouncy;
    }

    void OnDisable()
    {
        LE_EventInterface.OnTerrainCreated -= OnTerrainCreated;
    }

}
