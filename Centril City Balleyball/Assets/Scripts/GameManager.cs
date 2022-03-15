using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public enum matchType
{
    Singles,
    Doubles
}

public enum GameState
{
    Start,
    Match,
    End,
    Paused
}

public class GameManager : MonoBehaviour
{
    public GameObject[] canvases;
    public TMP_Text leftScoreText;
    public TMP_Text rightScoreText;
    private int leftScore = 0;
    private int rightScore = 0;


    public GameObject[] allAthletes;
    public GameObject ball;

    private matchType matchType;
    private GameState gameState = GameState.Start;

    // Start is called before the first frame update
    void Start()
    {
        // Handle the event
        Ball.OnBallLanded += UpdateScore;

        // Find the ball and athletes
        ball = GameObject.FindGameObjectWithTag("Ball");
        allAthletes = GameObject.FindGameObjectsWithTag("Athlete");

        // Determine game type
        if (allAthletes.Length == 2)
            matchType = matchType.Singles;
        else if (allAthletes.Length == 4)
            matchType = matchType.Doubles;
        
        // All athletes and ball should ignore collisions
        for (int i = 0; i < allAthletes.Length; i++)
        {
            Physics2D.IgnoreCollision(ball.GetComponent<Collider2D>(), allAthletes[i].GetComponent<Collider2D>());

            for (int n = 0; n < allAthletes.Length; n++)
                if (n != i)
                    Physics2D.IgnoreCollision(allAthletes[n].GetComponent<Collider2D>(), allAthletes[i].GetComponent<Collider2D>());
        }

        // Freeze time
        Time.timeScale = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // Check game state
        switch (gameState)
        {
            case GameState.Start:

                if (Input.anyKeyDown)
                {
                    canvases[(int)gameState].SetActive(false);
                    gameState = GameState.Match;
                    canvases[(int)gameState].SetActive(true);
                    Time.timeScale = 1;
                }

                break;

            case GameState.Match:
                break;

            case GameState.End:
                break;

            case GameState.Paused:
                break;
        }
    }


    // Event handler
    private void UpdateScore()
    {
        // Right side TECHNICALLY has an advantage, here
        if (ball.transform.position.x < 0)
        {
            rightScore++;
            rightScoreText.text = rightScore.ToString();
        }
        else
        {
            leftScore++;
            leftScoreText.text = leftScore.ToString();
        }
    }
}
