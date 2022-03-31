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
    public PauseMenu pauseMenu;

    private matchType matchType;
    public GameState gameState = GameState.Start;

    // Start is called before the first frame update
    void Start()
    {
        // Handle events
        Ball.OnBallLanded += UpdateScore;
        pauseMenu.resume.onClick.AddListener(delegate { ResumeGame(); });

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

                if (Input.GetKeyDown(KeyCode.Return))
                {
                    foreach (GameObject athlete in allAthletes)
                        athlete.GetComponent<Athlete>().controllable = true;
                    Time.timeScale = 1;

                    canvases[(int)gameState].SetActive(false);
                    gameState = GameState.Match;
                    canvases[(int)gameState].SetActive(true);

                }

                break;


            case GameState.Match:

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    foreach (GameObject athlete in allAthletes)
                        athlete.GetComponent<Athlete>().controllable = false;
                    Time.timeScale = 0;

                    canvases[(int)gameState].SetActive(false);
                    gameState = GameState.Paused;
                    canvases[(int)gameState].SetActive(true);

                    canvases[(int)gameState].GetComponentInChildren<Button>().Select();
                }
                else if (leftScore >= 7 || rightScore >= 7)
                {
                    foreach (GameObject athlete in allAthletes)
                        athlete.GetComponent<Athlete>().controllable = false;
                    Time.timeScale = 0;

                    canvases[(int)gameState].SetActive(false);
                    gameState = GameState.End;
                    canvases[(int)gameState].SetActive(true);
                }

                break;

            case GameState.End:

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    canvases[(int)gameState].SetActive(false);
                    gameState = GameState.Paused;
                    canvases[(int)gameState].SetActive(true);
                }

                break;

            case GameState.Paused:

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    ResumeGame();
                }

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

    private void ResumeGame()
    {
        foreach (GameObject athlete in allAthletes)
            athlete.GetComponent<Athlete>().controllable = true;
        Time.timeScale = 1;

        canvases[(int)gameState].SetActive(false);
        gameState = GameState.Match;
        canvases[(int)gameState].SetActive(true);
    }
}
