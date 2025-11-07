using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using TMPro;

public class MenuScripts : MonoBehaviour
{
    public static bool isPaused;
    [SerializeField]
    private GameObject Canvas;

    private void Update()
    {
        if(!isPaused && Input.GetKeyDown(KeyCode.Escape) && 
            SceneManager.GetActiveScene().name == "DungeonTest")
        {
            Canvas.transform.Find("Pause Button").gameObject.SetActive(false);
            Canvas.transform.Find("Pause Panel").gameObject.SetActive(true);

            PauseGame();
            

        }
    }
    public void EasyButton()
    {
        GameState.playOption = GameState.playOptions.NewGame;
        isPaused = false;
        GameState.difficulty = GameState.difficulties.Easy;
        SceneManager.LoadScene("DungeonTest");
        StartGame();
    }

    public void MediumButton()
    {
        GameState.playOption = GameState.playOptions.NewGame;
        isPaused = false;
        GameState.difficulty = GameState.difficulties.Medium;
        SceneManager.LoadScene("DungeonTest");
        StartGame();
    }

    public void HardButton()
    {
        GameState.playOption = GameState.playOptions.NewGame;
        isPaused = false;
        GameState.difficulty = GameState.difficulties.Hard;
        SceneManager.LoadScene("DungeonTest");
        StartGame();
    }

    public void QuitButton()
    {
        Application.Quit();
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");

    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        isPaused = true;
    }

    public void StartGame()
    {
        Time.timeScale = 1;
        isPaused = false;
    }

    public void GameOver(bool isfinished)
    {
        Time.timeScale = 0;
        isPaused = true;
        string title = "";

        if (isfinished) title = "Dungeon Cleared !!";
        else title = "Game Over";

        GameObject gameOverPanel = Canvas.transform.Find("GameOver Panel").gameObject;
        gameOverPanel.SetActive(true);

        TMP_Text panelName = gameOverPanel.transform.Find("Title Text").gameObject.GetComponent<TMP_Text>();
        panelName.text = title;
        TMP_Text scoreText = gameOverPanel.transform.Find("FinalScore Number").gameObject.GetComponent<TMP_Text>();
        PlayScore playScore = GameObject.FindFirstObjectByType<PlayScore>();
        scoreText.text = playScore.target.ToString();
        playScore.CompareHighScore();
    }

    public void GetHighScore(GameObject highScoreText)
    {
        float highScore = PlayerPrefs.GetFloat("HighScore", 0);
        highScoreText.GetComponent<TMP_Text>().text = ((int)highScore).ToString();

    }



    #region // Save&LoadMenu
    public void SaveAndBackToMenu(GameObject dungeonGenerator)
    {
        bspTree bsp = dungeonGenerator.transform.Find("bspManager").GetComponent<bspTree>();

        List<Rect> dungeonCorridors = new List<Rect>(bsp.rootSubDungeon.corridors);

        int[,] boardMap = bsp.boardPositionsFloor;

        SaveManager.instance.SaveGame(bsp);
        Debug.Log("Saved dungeon data.");

        SceneManager.LoadScene("MainMenu");
    }

    public void LoadPrevGame()
    {
        GameState.playOption = GameState.playOptions.LoadGame;
        SceneManager.LoadScene("DungeonTest");

        bspTree dungeonData = SaveManager.instance.Loadgame();

        bspTree bsp = GameObject.Find("bspManager").GetComponent<bspTree>();
        GameObject floorTileMap = GameObject.Find("bspTileMap");
        GameObject wallTileMap = GameObject.Find("wallTileMap");

    }
    #endregion 

}
