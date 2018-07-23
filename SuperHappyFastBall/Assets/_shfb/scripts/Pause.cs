using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{

    public Transform menuCanvas;
    public bool paused;

    // Use this for initialization
    void Start()
    {
        paused = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            paused = !paused;
          
            Paused();

        }
    }

    public void Paused()
    { 
        if (paused)
        {
            Time.timeScale = 0;
            menuCanvas.gameObject.SetActive(true);
        }
        else if (!paused)
        {
            menuCanvas.gameObject.SetActive(false);
            Time.timeScale = 1;
        }
    }

}
