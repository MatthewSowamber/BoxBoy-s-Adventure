using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public static Manager instance;
    public static int coins, lives;
    public enum DoorKeyColours { Red, Blue, Yellow};
    public static bool redKey, blueKey, yellowKey;
    public static Vector3 lastCheckPoint;
    public static bool gamePaused;
    static GameUI gameUI;
    public Transform[] cheatPoints;

    private void Awake()
    {
        if(instance==null)
        { 
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        { 
            Destroy(gameObject);
        }
        gameUI = FindObjectOfType<GameUI>();
        lives = 3;
        coins = 0;
        gameUI.UpdateCoins();
        gameUI.UpdateLives();
    }

    public static void AddCoins(int coinValue)
    {
        coins += coinValue;
        if(coins >= 100) 
        {
            coins -= 100;
            AddLives(1);
        }
        gameUI.UpdateCoins();
    }

    public static void AddLives(int lifeValue) 
    {
        lives += lifeValue;
        if (lifeValue == -1)
        {
            FindObjectOfType<AudioManager>().AudioTrigger(AudioManager.SoundFXCat.Death, Vector3.zero,1f);
        }
        if(lives < 0)
        {
            gameUI.CheckGameState(GameUI.GameState.GameOver);
        }
        else 
        {
            gameUI.UpdateLives();
        }
    }

    public static void KeyPickup(DoorKeyColours keyColour)
    {
        switch (keyColour)
        {
            case DoorKeyColours.Red:
                redKey = true;
                break;
            case DoorKeyColours.Blue:
                blueKey = true;
                break;
            case DoorKeyColours.Yellow:
                yellowKey = true;
                break;
        }
        gameUI.UpdateKey(keyColour);
    }

    public static void UpdateCheckPoints(GameObject flag)
    {
        lastCheckPoint = flag.transform.position;
        CheckPoint[] allCheckPoints = FindObjectsOfType<CheckPoint>();
        foreach (CheckPoint cp in allCheckPoints)
        {
            if(cp != flag.GetComponent<CheckPoint>())
            {
                cp.LowerFlag(); 
            }
        }
    }

    private void Update()
    {
        CheatModes();
    }

    void CheatModes()
    {
        if (Input.GetKeyDown(KeyCode.R)) KeyPickup(DoorKeyColours.Red);
        if (Input.GetKeyDown(KeyCode.B)) KeyPickup(DoorKeyColours.Blue);
        if (Input.GetKeyDown(KeyCode.Y)) KeyPickup(DoorKeyColours.Yellow);
        if (Input.GetKeyDown(KeyCode.L)) AddLives(1);
        if(cheatPoints[0] == null)
        {
            cheatPoints[0] = GameObject.Find("flagpole 1").transform;
        }
        if(Input.GetKeyDown(KeyCode.Alpha1)) GameObject.FindGameObjectWithTag("Player").transform.position = cheatPoints[0].position;
        if (cheatPoints[1] == null)
        {
            cheatPoints[1] = GameObject.Find("flagpole 2").transform;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) GameObject.FindGameObjectWithTag("Player").transform.position = cheatPoints[1].position;
        if (cheatPoints[2] == null)
        {
            cheatPoints[2] = GameObject.Find("flagpole 3").transform;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)) GameObject.FindGameObjectWithTag("Player").transform.position = cheatPoints[2].position;
    }
}
