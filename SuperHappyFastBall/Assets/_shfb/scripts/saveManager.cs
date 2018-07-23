using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LE_LevelEditor.Events;

public class saveManager : MonoBehaviour {

    [Header("coins")]
    public float coins = 0.0f;
    public TextMeshProUGUI coinsText;



    void Awake () {
        
        //find coinSave using easy save 3 asset, or set it to 0 if it doesn't already have a value
        if (ES3.KeyExists("coinSave"))
        {
            coins = ES3.Load<float>("coinSave");

        }
      

        //display coins on UI
        coinsText.text = "Coins:" + coins.ToString();

        // Register for the load event, which is called when the level is loaded

        LE_EventInterface.OnObjectPlaced += OnObjectPlaced;

        // Register for the load event, which is called when the level is loaded

        LE_EventInterface.OnObjectDragged += OnObjectDragged;

    }

    private void OnObjectDragged(object p_sender, LE_ObjectDragEvent p_args)
    {
        // in this example we will check if the cursor position (the place where the object will be placed)
        // is too far away from the center of the level (outside a square with side length 200)
        // you can replace this with the check that you need to perform
        // take a look at the other members of LE_ObjectDragEvent
        // for example the object prefab is also passed within the event args

        if (coins < 1)
        {
            // tell the level editor that this object cannot be placed at the current cursor position

            p_args.IsObjectPlaceable = false;
            // check if there is already a message that will be shown to the player
            // this can be the case if some other listener of this event has added a message
            // or if the instance count of this objects has reached its maximum

            if (p_args.Message.Length > 0)
            {
                // add a line break if the message is not empty

                p_args.Message += "\n";
            }
            // add your message here in this example the object is simply out of bounds

            p_args.Message += "You don't have enough coins!";
        }
    }

    private void OnObjectPlaced(object p_sender, LE_ObjectPlacedEvent p_args)
    {
          float price = p_args.Object.GetComponent<objectCost>().cost;
        coins-= price;
        //display coins on UI
        coinsText.text = "Coins:" + coins.ToString();
        //save coins to easy save 3 
        ES3.Save<float>("coinSave", coins);
    }

    void OnDisable()
    {
        LE_EventInterface.OnObjectPlaced -= OnObjectPlaced;
        LE_EventInterface.OnObjectDragged -= OnObjectDragged;

    }

}
