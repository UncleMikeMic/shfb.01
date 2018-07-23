using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(AudioSource))]

public class GameManager : MonoBehaviour {
    [Header("sart sequence")]
    public float startTimeSpent = 0.0f;
    public float startTimeAllowed = 8.0f;
    // public Text startTimeText;
    public TextMeshProUGUI startTimeText;
    public GameObject startTimeTextObj;
    public GameObject instructionsText;
    private bool started;

    [Header("scoreboard")]
    //score/time vars
    public float timeSpent = 0.0f;
    private float timeAllowed;
    public float gameTime;
    public TextMeshProUGUI timeText;
    public int score = 0;
    public TextMeshProUGUI scoreText;
    //brick counter vars
    [HideInInspector]
    public GameObject[] brickCount;
    private int bricks;
    public TextMeshProUGUI bricksText;

    [Header("coins")]
    public float coins = 0.0f;
    public TextMeshProUGUI coinsText;



    [Header("ball")]
    // ball spawn vars
    public Transform ballSpawnLocation;
    public Rigidbody ballPrefab;
    Rigidbody ballInstance;
    public float spawnTime;
    private float timeLeft;
    public float launchForce;
    public float horizontalOffset;
    public GameObject ballSpawnParticles;

    //ball text vars
    int balls = 0;
    public TextMeshProUGUI ballsText;



    [Header("game over")]
    //game over vars
    public Transform gameOverCanvas;
    public Transform youWinCanvas;

    public bool canSpawn;

    [Header("pause")]
    //pause vars
    public Transform menuCanvas;
    public bool paused = false;
    //access paddle script for pause
    public Paddle paddle;



    void Awake()
    {
        //game is active
        paused = false;

        //I had to add this, because things were fucky when I loaded a level after going to the main menu
        //it might have something to do with easy save 3
        Time.timeScale = 1;

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
    void Start() {
        //game is active
        paused = false;

        //finds paddle script on paddle and follower for pausing
        paddle = GameObject.FindWithTag("paddle").GetComponent<Paddle>();


        //play theme music
        AudioController.PlayMusic("themeMusic");

        //set vars
        startTimeTextObj.SetActive(true);
        instructionsText.SetActive(true);
        started = false;
        canSpawn = true;

        timeLeft = startTimeAllowed;
        timeAllowed = gameTime + startTimeAllowed;


    }

    // Update is called once per frame
    void Update() {

     

        //start sequence timer
        if (started == false)
        {
            startTimeSpent += Time.deltaTime;
            startTimeText.text = "Starting in:" + Mathf.Floor(startTimeAllowed - startTimeSpent).ToString();

            //check how many bricks there are, display on scoreboard, if < 1 you win
            brickCount = GameObject.FindGameObjectsWithTag("brick");
            bricks = brickCount.Length;
            bricksText.text = "Bricks:" + bricks;
        }
        if (startTimeSpent + 1 > startTimeAllowed)
        {
        //    gameOverCanvas.gameObject.SetActive(false);
            instructionsText.SetActive(false);
            startTimeTextObj.SetActive(false);
            started = true;
           

        }



        //pause button
        if (Input.GetKeyDown(KeyCode.Escape))
        {


            Paused();

        }



        // timer
        if (canSpawn == true)
        {
            timeSpent += Time.deltaTime;
            timeText.text = "Time:" + Mathf.Floor(timeAllowed - timeSpent).ToString();
        }
        if (timeSpent + 1 > timeAllowed && canSpawn ==  true)
        {
            GameOver();
        }

        // ball spawn timer
        timeLeft -= Time.deltaTime;
        if (timeLeft < 1 && canSpawn == true)
        {
            spawnBall();
        }
        // game ends if you don't have any balls
        if (balls < 1 && started == true && canSpawn == true)
        {

            GameOver();

        }
    }

    //Save any game data that needs to persist between levels
    public void SaveGameData()
    {
        //save coins to easy save 3 
        ES3.Save<float>("coinSave", coins);
    }

    //pause
    public void Paused()
    {

        SaveGameData();
        paused = !paused;
        paddle.PaddleCanMove();




        if (paused)
        {
            Time.timeScale = 0;
            menuCanvas.gameObject.SetActive(true);
            canSpawn = false;


        }
        else if (!paused)
        {
            menuCanvas.gameObject.SetActive(false);
            Time.timeScale = 1;
            canSpawn = true;

        }
    }

    public void IncrementScore()
    {
        if (canSpawn == true)
        {

            //update scoreboard (the .ToString("D3") makes sure there's always 3 digits, so the ui doesn't move)
            scoreText.text = "Score:" + score.ToString("D3");

            //display coins on UI
            coinsText.text = "Coins:" + coins.ToString();
        }
    }



    public void spawnBall()
    {
        //  spawn ball
        ballInstance = Instantiate(ballPrefab, ballSpawnLocation.position, ballSpawnLocation.rotation) as Rigidbody;
        //make ball move
        ballInstance.AddForce(ballSpawnLocation.forward * launchForce + ballSpawnLocation.right * Random.Range(-horizontalOffset, horizontalOffset));

        //sfx
        AudioController.Play("ballSpawn");

        //particles
        Instantiate(ballSpawnParticles, ballSpawnLocation.position, ballSpawnLocation.rotation);

        //reset ball spawn timre
        timeLeft = spawnTime;
        //increment balls on scoreboard
        balls++;
        ballsText.text = "Balls:" + balls.ToString("D3");

    }
    //calls from death script (on front(death) wall) decreases number of balls on scoreboard when one is destroyed
    public void DecrementBalls()
    {
        if (canSpawn == true)
        {
            balls--;
            ballsText.text = "Balls:" + balls.ToString("D3");
           
        }
    }

    public void DecrementBricks()
    {
        //check how many bricks there are, display on scoreboard, if < 1 you win
        bricks--;
        bricksText.text = "Bricks:" + bricks;

        if (bricks < 1 && started == true)
        {
            YouWin();
        }
    }


    // you win, if all bricks gone
    public void YouWin()
    {
        SaveGameData();
        canSpawn = false;
        youWinCanvas.gameObject.SetActive(true);
        AudioController.Play("youWin");

    }

    //game over.  When time is up, or your out of balls.
    public void GameOver()
    {
        SaveGameData();
        canSpawn = false;
        //sfx
       AudioController.Play("gameOver");

       
     
        gameOverCanvas.gameObject.SetActive(true);
       
        

    }

    //function for the replay button
    public void Replay()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    //function for the main menu button
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("mainMenu", LoadSceneMode.Single);
    }

    //function for the quit button
    public void OnApplicationQuit()
    {
        SaveGameData();
        Debug.Log("has quit game");
        Application.Quit();
    }
 


}
