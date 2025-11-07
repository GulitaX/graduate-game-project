using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayScore : MonoBehaviour
{
    public TMP_Text scoreUI;
    public float highscore;
    public int countFPS;
    public float target;
    protected float score { get; private set; }
    void Start()
    {
        score = target = 0f;
        scoreUI.text = score.ToString();
        highscore = PlayerPrefs.GetFloat("HighScore", 0);
      
    }
    public void UpdateGameScore(float addedScore)
    {
        switch (GameState.difficulty)
        {
            case (GameState.difficulties.Easy):
                addedScore *= 1f;
                break;

            case (GameState.difficulties.Medium):
                addedScore *= 1.5f;
                break;

            case (GameState.difficulties.Hard):
                addedScore *= 2f;
                break;

        }
        target += addedScore;
        
        StartCoroutine(CountScoreUp());
    }

    public void CompareHighScore()
    {
        if(score > PlayerPrefs.GetFloat("HighScore", 0))
        {
            PlayerPrefs.SetFloat("HighScore", score);

            highscore = score;
        }
    }

    IEnumerator CountScoreUp()
    {
        while (score < target)
        {
            score = Mathf.MoveTowards(score, target, countFPS * Time.deltaTime);
            scoreUI.text = ((int)score).ToString();
            yield return null;
        }
    }
}
