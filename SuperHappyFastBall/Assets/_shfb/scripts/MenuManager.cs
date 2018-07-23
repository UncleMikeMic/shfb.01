using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour {

    [Header("coins")]
    public float coins = 0.0f;
    public TextMeshProUGUI coinsText;

    void Awake()
    {

        //find coinSave using easy save 3 asset, or set it to 0 if it doesn't already have a value
        if (ES3.KeyExists("coinSave"))
        {
            coins = ES3.Load<float>("coinSave");

        }
        else
        {
            coins = 0f;
        }

        //display coins on UI
        coinsText.text = "Coins:" + coins.ToString();
    }

    // Use this for initialization
    void Start () {
        //play theme music
        AudioController.PlayMusic("themeMusic");

    


    }

    // Update is called once per frame
    void Update () {
		
	}

    public void LoadEditor()
    {
        SceneManager.LoadScene("LE_ExampleEditor", LoadSceneMode.Single);
    }

    public void LoadLevel1()
    {
        Debug.Log("loadLevel1");
        SceneManager.LoadScene("level1", LoadSceneMode.Single);
    }

    public void LoadLevel2()
    {
        SceneManager.LoadScene("level2", LoadSceneMode.Single);
    }

    public void LoadLevel3()
    {
        SceneManager.LoadScene("level3", LoadSceneMode.Single);
    }

    public void LoadLevel4()
    {
        SceneManager.LoadScene("level4", LoadSceneMode.Single);
    }



    //quit
    public void OnApplicationQuit()
    {
        Debug.Log("has quit game");
        Application.Quit();
    }
}
